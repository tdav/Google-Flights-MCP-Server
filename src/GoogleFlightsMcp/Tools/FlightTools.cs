using System.Text.Json;
using GoogleFlightsMcp.Models;
using GoogleFlightsMcp.Services;
using GoogleFlightsMcp.Mcp;
using GoogleFlightsMcp.Helpers;
using Serilog;

namespace GoogleFlightsMcp.Tools;

/// <summary>
/// Flight search tools for MCP
/// </summary>
public class FlightTools
{
    private readonly IFlightSearchService _flightSearchService;

    public FlightTools()
    {
        _flightSearchService = new FlightSearchService();
    }

    /// <summary>
    /// Gets tool definitions for MCP
    /// </summary>
    public List<McpTool> GetToolDefinitions()
    {
        return new List<McpTool>
        {
            new McpTool
            {
                Name = "search_flights",
                Description = "Search for flights between two airports on Google Flights. Supports both one-way and round-trip searches.",
                InputSchema = new McpToolSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, McpProperty>
                    {
                        {
                            "origin", new McpProperty
                            {
                                Type = "string",
                                Description = "Origin airport code (e.g., TAS for Tashkent, JFK for New York)"
                            }
                        },
                        {
                            "destination", new McpProperty
                            {
                                Type = "string",
                                Description = "Destination airport code (e.g., JFK, LAX, LHR)"
                            }
                        },
                        {
                            "departureDate", new McpProperty
                            {
                                Type = "string",
                                Description = "Departure date in YYYY-MM-DD format (e.g., 2026-01-09)"
                            }
                        },
                        {
                            "returnDate", new McpProperty
                            {
                                Type = "string",
                                Description = "Return date in YYYY-MM-DD format (optional for one-way trips)"
                            }
                        },
                        {
                            "passengers", new McpProperty
                            {
                                Type = "integer",
                                Description = "Number of passengers (1-9)",
                                Minimum = 1,
                                Maximum = 9,
                                Default = 1
                            }
                        },
                        {
                            "cabinClass", new McpProperty
                            {
                                Type = "string",
                                Description = "Cabin class",
                                Enum = new List<string> { "economy", "premium_economy", "business", "first" },
                                Default = "economy"
                            }
                        },
                        {
                            "tripType", new McpProperty
                            {
                                Type = "string",
                                Description = "Trip type: one-way or round-trip",
                                Enum = new List<string> { "one_way", "round_trip" },
                                Default = "round_trip"
                            }
                        }
                    },
                    Required = new List<string> { "origin", "destination", "departureDate" }
                }
            },
            new McpTool
            {
                Name = "get_flight_url",
                Description = "Generate a Google Flights search URL for the specified parameters without searching.",
                InputSchema = new McpToolSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, McpProperty>
                    {
                        {
                            "origin", new McpProperty
                            {
                                Type = "string",
                                Description = "Origin airport code"
                            }
                        },
                        {
                            "destination", new McpProperty
                            {
                                Type = "string",
                                Description = "Destination airport code"
                            }
                        },
                        {
                            "departureDate", new McpProperty
                            {
                                Type = "string",
                                Description = "Departure date (YYYY-MM-DD)"
                            }
                        },
                        {
                            "returnDate", new McpProperty
                            {
                                Type = "string",
                                Description = "Return date (YYYY-MM-DD, optional)"
                            }
                        },
                        {
                            "passengers", new McpProperty
                            {
                                Type = "integer",
                                Description = "Number of passengers",
                                Default = 1
                            }
                        },
                        {
                            "cabinClass", new McpProperty
                            {
                                Type = "string",
                                Description = "Cabin class",
                                Default = "economy"
                            }
                        }
                    },
                    Required = new List<string> { "origin", "destination", "departureDate" }
                }
            }
        };
    }

    /// <summary>
    /// Executes a tool with given arguments
    /// </summary>
    public async Task<McpToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object>? arguments)
    {
        try
        {
            Log.Information("Executing tool: {ToolName} with arguments: {Arguments}", 
                toolName, 
                JsonSerializer.Serialize(arguments));

            return toolName switch
            {
                "search_flights" => await SearchFlightsAsync(arguments),
                "get_flight_url" => GetFlightUrl(arguments),
                _ => CreateErrorResult($"Unknown tool: {toolName}")
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing tool {ToolName}", toolName);
            return CreateErrorResult($"Error: {ex.Message}");
        }
    }

    private async Task<McpToolResult> SearchFlightsAsync(Dictionary<string, object>? arguments)
    {
        if (arguments == null)
        {
            return CreateErrorResult("Missing required arguments");
        }

        try
        {
            var flightData = ParseFlightData(arguments);
            var result = await _flightSearchService.SearchFlightsAsync(flightData);

            // Format the result as a readable text response
            var responseText = FormatFlightResult(result);

            return new McpToolResult
            {
                Content = new List<McpContent>
                {
                    new McpContent
                    {
                        Type = "text",
                        Text = responseText
                    }
                },
                IsError = false
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in SearchFlightsAsync");
            return CreateErrorResult(ex.Message);
        }
    }

    private McpToolResult GetFlightUrl(Dictionary<string, object>? arguments)
    {
        if (arguments == null)
        {
            return CreateErrorResult("Missing required arguments");
        }

        try
        {
            var flightData = ParseFlightData(arguments);
            var url = _flightSearchService.BuildGoogleFlightsUrl(flightData);

            var responseText = $"Google Flights Search URL:\n{url}\n\n" +
                             $"Route: {flightData.Origin} → {flightData.Destination}\n" +
                             $"Departure: {flightData.DepartureDate}\n";

            if (!string.IsNullOrEmpty(flightData.ReturnDate))
            {
                responseText += $"Return: {flightData.ReturnDate}\n";
            }

            responseText += $"Passengers: {flightData.Passengers.Total}\n" +
                          $"Cabin Class: {flightData.SeatType}\n";

            return new McpToolResult
            {
                Content = new List<McpContent>
                {
                    new McpContent
                    {
                        Type = "text",
                        Text = responseText
                    }
                },
                IsError = false
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in GetFlightUrl");
            return CreateErrorResult(ex.Message);
        }
    }

    private FlightData ParseFlightData(Dictionary<string, object> arguments)
    {
        var origin = GetStringArgument(arguments, "origin");
        var destination = GetStringArgument(arguments, "destination");
        var departureDate = GetStringArgument(arguments, "departureDate");
        var returnDate = GetStringArgument(arguments, "returnDate", null);
        var passengers = GetIntArgument(arguments, "passengers", 1);
        var cabinClass = GetStringArgument(arguments, "cabinClass", "economy");
        var tripType = GetStringArgument(arguments, "tripType", "round_trip");

        var flightData = new FlightData
        {
            Origin = origin,
            Destination = destination,
            DepartureDate = DateHelper.FormatDate(departureDate),
            ReturnDate = returnDate != null ? DateHelper.FormatDate(returnDate) : null,
            Passengers = new Passengers { Adults = passengers },
            SeatType = ParseSeatType(cabinClass),
            TripType = tripType == "one_way" ? TripType.OneWay : TripType.RoundTrip
        };

        return flightData;
    }

    private string FormatFlightResult(FlightResult result)
    {
        var text = $"# Flight Search Results\n\n";
        text += $"**Route:** {result.Origin} → {result.Destination}\n";
        text += $"**Departure:** {result.DepartureDate}\n";
        
        if (!string.IsNullOrEmpty(result.ReturnDate))
        {
            text += $"**Return:** {result.ReturnDate}\n";
        }
        
        text += $"**Passengers:** {result.Passengers}\n";
        text += $"**Cabin Class:** {result.CabinClass}\n";
        text += $"**Search URL:** {result.SearchUrl}\n\n";

        text += $"## Available Flights ({result.Flights.Count} options)\n\n";

        for (int i = 0; i < result.Flights.Count; i++)
        {
            var flight = result.Flights[i];
            text += $"### Flight {i + 1}: {flight.Airline} {flight.FlightNumber}\n";
            text += $"- **Departure:** {flight.DepartureTime}\n";
            text += $"- **Arrival:** {flight.ArrivalTime}\n";
            text += $"- **Duration:** {flight.Duration}\n";
            text += $"- **Stops:** {flight.Stops}\n";
            text += $"- **Price:** {PriceHelper.FormatPrice(flight.Price, flight.Currency)} per person\n";
            
            if (result.Passengers > 1)
            {
                var totalPrice = flight.Price * result.Passengers;
                text += $"- **Total Price:** {PriceHelper.FormatPrice(totalPrice, flight.Currency)} for {result.Passengers} passengers\n";
            }
            
            text += "\n";
        }

        text += $"\n*Search completed at: {result.SearchedAt:yyyy-MM-dd HH:mm:ss} UTC*\n";

        return text;
    }

    private SeatType ParseSeatType(string cabinClass)
    {
        return cabinClass.ToLower() switch
        {
            "economy" => SeatType.Economy,
            "premium_economy" => SeatType.PremiumEconomy,
            "business" => SeatType.Business,
            "first" => SeatType.First,
            _ => SeatType.Economy
        };
    }

    private string GetStringArgument(Dictionary<string, object> arguments, string key, string? defaultValue = null)
    {
        if (arguments.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
            {
                return jsonElement.GetString() ?? defaultValue ?? string.Empty;
            }
            return value?.ToString() ?? defaultValue ?? string.Empty;
        }
        return defaultValue ?? throw new ArgumentException($"Missing required argument: {key}");
    }

    private int GetIntArgument(Dictionary<string, object> arguments, string key, int defaultValue = 0)
    {
        if (arguments.TryGetValue(key, out var value))
        {
            if (value is JsonElement jsonElement)
            {
                return jsonElement.TryGetInt32(out var intValue) ? intValue : defaultValue;
            }
            if (value is int intVal)
            {
                return intVal;
            }
            if (int.TryParse(value?.ToString(), out var parsed))
            {
                return parsed;
            }
        }
        return defaultValue;
    }

    private McpToolResult CreateErrorResult(string errorMessage)
    {
        return new McpToolResult
        {
            Content = new List<McpContent>
            {
                new McpContent
                {
                    Type = "text",
                    Text = $"Error: {errorMessage}"
                }
            },
            IsError = true
        };
    }
}
