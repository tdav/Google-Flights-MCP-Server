using Serilog;
using System.Text.Json;

namespace GoogleFlightsMcp;

class Program
{
    static async Task Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Google Flights MCP Server starting...");

            var mcpServer = new McpServer();
            await mcpServer.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

public class McpServer
{
    public async Task RunAsync()
    {
        Log.Information("MCP Server initialized");
        Log.Information("Listening for MCP protocol messages on STDIN...");

        // Simple MCP protocol implementation
        // In a real implementation, this would parse JSON-RPC messages from STDIN
        
        var serverInfo = new
        {
            name = "Google Flights MCP Server",
            version = "1.0.0",
            description = "MCP Server for searching Google Flights",
            capabilities = new
            {
                tools = new[]
                {
                    new
                    {
                        name = "search_flights",
                        description = "Search for flights between two airports",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                origin = new { type = "string", description = "Origin airport code (e.g., JFK)" },
                                destination = new { type = "string", description = "Destination airport code (e.g., LAX)" },
                                departureDate = new { type = "string", description = "Departure date (YYYY-MM-DD)" },
                                returnDate = new { type = "string", description = "Return date (YYYY-MM-DD, optional)" },
                                passengers = new { type = "integer", description = "Number of passengers (1-9)" },
                                cabinClass = new { type = "string", description = "Cabin class (economy, premium_economy, business, first)" }
                            },
                            required = new[] { "origin", "destination", "departureDate", "passengers", "cabinClass" }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(serverInfo, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        Console.WriteLine(json);
        Log.Information("Server info sent to STDOUT");

        // Keep the server running
        await Task.Delay(Timeout.Infinite);
    }
}

