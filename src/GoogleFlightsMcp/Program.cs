using Serilog;
using GoogleFlightsMcp.Mcp;

namespace GoogleFlightsMcp;

class Program
{
    static async Task Main(string[] args)
    {
        // Configure Serilog to write to stderr to avoid interfering with MCP protocol on stdout
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("Google Flights MCP Server starting...");
            Log.Information("Version: 1.0.0");
            Log.Information("Protocol: JSON-RPC 2.0 over STDIO");

            var mcpServer = new McpServer();
            await mcpServer.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            Environment.ExitCode = 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

