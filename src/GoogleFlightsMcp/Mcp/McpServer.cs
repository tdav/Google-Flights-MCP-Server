using System.Text.Json;
using GoogleFlightsMcp.Tools;
using Serilog;

namespace GoogleFlightsMcp.Mcp;

/// <summary>
/// MCP Server implementation for Google Flights
/// </summary>
public class McpServer
{
    private readonly FlightTools _flightTools;
    private readonly JsonSerializerOptions _jsonOptions;

    public McpServer()
    {
        _flightTools = new FlightTools();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Runs the MCP server, listening on STDIN and writing to STDOUT
    /// </summary>
    public async Task RunAsync()
    {
        Log.Information("Google Flights MCP Server starting...");
        Log.Information("Listening for JSON-RPC messages on STDIN");

        try
        {
            while (true)
            {
                var line = await Console.In.ReadLineAsync();
                
                if (line == null)
                {
                    Log.Information("STDIN closed, shutting down");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var request = JsonSerializer.Deserialize<McpMessage>(line, _jsonOptions);
                    if (request == null)
                    {
                        Log.Warning("Failed to parse message: {Line}", line);
                        continue;
                    }

                    var response = await HandleRequestAsync(request);
                    if (response != null)
                    {
                        var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                        Console.WriteLine(responseJson);
                        await Console.Out.FlushAsync();
                    }
                }
                catch (JsonException ex)
                {
                    Log.Error(ex, "JSON parsing error: {Message}", ex.Message);
                    SendError(null, -32700, "Parse error", ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error handling request: {Message}", ex.Message);
                    SendError(null, -32603, "Internal error", ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error in MCP server");
            throw;
        }
    }

    private async Task<McpMessage?> HandleRequestAsync(McpMessage request)
    {
        Log.Information("Received request: {Method} (id: {Id})", request.Method, request.Id);

        try
        {
            return request.Method switch
            {
                "initialize" => HandleInitialize(request),
                "tools/list" => HandleToolsList(request),
                "tools/call" => await HandleToolCallAsync(request),
                "ping" => HandlePing(request),
                _ => CreateErrorResponse(request.Id, -32601, $"Method not found: {request.Method}")
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing method {Method}: {Message}", request.Method, ex.Message);
            return CreateErrorResponse(request.Id, -32603, "Internal error", ex.Message);
        }
    }

    private McpMessage HandleInitialize(McpMessage request)
    {
        Log.Information("Initializing MCP server");
        
        var serverInfo = new McpServerInfo
        {
            Name = "Google Flights MCP Server",
            Version = "1.0.0",
            ProtocolVersion = "2024-11-05",
            Capabilities = new McpCapabilities
            {
                Tools = new Dictionary<string, bool> { { "listChanged", false } }
            }
        };

        return new McpMessage
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = serverInfo
        };
    }

    private McpMessage HandleToolsList(McpMessage request)
    {
        Log.Information("Listing available tools");

        var tools = _flightTools.GetToolDefinitions();
        var result = new { tools };

        return new McpMessage
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = result
        };
    }

    private async Task<McpMessage> HandleToolCallAsync(McpMessage request)
    {
        if (request.Params == null)
        {
            return CreateErrorResponse(request.Id, -32602, "Invalid params");
        }

        try
        {
            var paramsJson = JsonSerializer.Serialize(request.Params, _jsonOptions);
            var toolCall = JsonSerializer.Deserialize<McpToolCall>(paramsJson, _jsonOptions);

            if (toolCall == null || string.IsNullOrEmpty(toolCall.Name))
            {
                return CreateErrorResponse(request.Id, -32602, "Invalid tool call");
            }

            Log.Information("Executing tool: {ToolName}", toolCall.Name);

            var result = await _flightTools.ExecuteToolAsync(toolCall.Name, toolCall.Arguments);

            return new McpMessage
            {
                JsonRpc = "2.0",
                Id = request.Id,
                Result = result
            };
        }
        catch (ArgumentException ex)
        {
            Log.Warning(ex, "Invalid arguments for tool call");
            return CreateErrorResponse(request.Id, -32602, "Invalid params", ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing tool");
            return CreateErrorResponse(request.Id, -32603, "Internal error", ex.Message);
        }
    }

    private McpMessage HandlePing(McpMessage request)
    {
        return new McpMessage
        {
            JsonRpc = "2.0",
            Id = request.Id,
            Result = new { status = "ok", timestamp = DateTime.UtcNow }
        };
    }

    private McpMessage CreateErrorResponse(object? id, int code, string message, string? data = null)
    {
        return new McpMessage
        {
            JsonRpc = "2.0",
            Id = id,
            Error = new McpError
            {
                Code = code,
                Message = message,
                Data = data
            }
        };
    }

    private void SendError(object? id, int code, string message, string? data = null)
    {
        var error = CreateErrorResponse(id, code, message, data);
        var errorJson = JsonSerializer.Serialize(error, _jsonOptions);
        Console.WriteLine(errorJson);
        Console.Out.Flush();
    }
}
