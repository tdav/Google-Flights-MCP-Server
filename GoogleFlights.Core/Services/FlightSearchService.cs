using Serilog;
using GoogleFlights.Core.Models;
using GoogleFlights.Core.Helpers;
using Microsoft.Playwright;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;

namespace GoogleFlights.Core.Services;

/// <summary>
/// Service for searching flights on Google Flights
/// </summary>
public class FlightSearchService : IFlightSearchService, IDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly Dictionary<string, string> AirportCodes = new()
    {
        // North America
        { "JFK", "/m/02_286" },
        { "LAX", "/m/030qb3t" },
        { "ORD", "/m/01_d4" },
        { "DFW", "/m/030k2v" },
        { "SFO", "/m/0d6lp" },
        { "SEA", "/m/0d9jr" },
        { "MIA", "/m/0f2v0" },
        { "BOS", "/m/01cx_" },
        { "ATL", "/m/013yq" },
        { "LAS", "/m/0cv3w" },
        { "PHX", "/m/0d35y" },
        { "DEN", "/m/02cft" },
        { "IAH", "/m/03ksg" },
        { "MSP", "/m/0fpzwf" },
        { "DTW", "/m/0fvwg" },
        { "EWR", "/m/0cc56" },
        { "MCO", "/m/0fxmq" },
        
        // Europe
        { "LHR", "/m/04jpl" },
        { "CDG", "/m/05qtj" },
        { "FRA", "/m/0jxgx" },
        { "AMS", "/m/0k3p" },
        { "MAD", "/m/056_y" },
        { "BCN", "/m/01f62" },
        { "FCO", "/m/06c62" },
        { "MUC", "/m/0727_" },
        { "IST", "/m/09949" },
        { "LGW", "/m/065y4w7" },
        { "ZRH", "/m/08g5vq" },
        { "VIE", "/m/05qx6" },
        
        // Asia
        { "DXB", "/m/01f08r" },
        { "HKG", "/m/03h64r" },
        { "NRT", "/m/0f4t4" },
        { "SIN", "/m/02p24c" },
        { "ICN", "/m/0cyzn" },
        { "BKK", "/m/0dl9t8" },
        { "DEL", "/m/03l8mx" },
        { "PEK", "/m/0dq_7" },
        { "PVG", "/m/0j5nb" },
        { "HND", "/m/0gx_x" },
        
        // Middle East & Central Asia
        { "TAS", "/m/0fsmy" },  // Tashkent
        { "DOH", "/m/01_8q3" },
        
        // Australia & Oceania
        { "SYD", "/m/06y57" },
        { "MEL", "/m/0chghy" },
        { "AKL", "/m/0ctyv" },
        
        // South America
        { "GRU", "/m/0fphj" },
        { "EZE", "/m/0132jd" },
        { "GIG", "/m/02k0l1" },
        
        // Africa
        { "JNB", "/m/04g8v" },
        { "CAI", "/m/0cwd8" },
    };

    public async Task<FlightResult> SearchFlightsAsync(FlightData flightData)
    {
        ValidateFlightData(flightData);

        var searchUrl = BuildGoogleFlightsUrl(flightData);
        
        Log.Information(
            "Searching flights: {Origin} -> {Destination} on {DepartureDate}",
            flightData.Origin, 
            flightData.Destination, 
            flightData.DepartureDate);

        var flights = await FetchFlightsAsync(searchUrl, flightData);

        return new FlightResult
        {
            Origin = flightData.Origin,
            Destination = flightData.Destination,
            DepartureDate = flightData.DepartureDate,
            ReturnDate = flightData.ReturnDate,
            Passengers = flightData.Passengers.Total,
            CabinClass = GetCabinClassName(flightData.SeatType),
            Flights = flights,
            SearchUrl = searchUrl,
            TripType = flightData.TripType,
            SearchedAt = DateTime.UtcNow
        };
    }

    private async Task<List<Flight>> FetchFlightsAsync(string url, FlightData flightData)
    {
        await InitializePlaywrightAsync();

        // Use a new context for each request to avoid shared state/cookies issues
        var context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        });

        var page = await context.NewPageAsync();
        var flights = new List<Flight>();

        try 
        {
            await page.GotoAsync(url);
            
            // Wait for results. We accept either "Best departing flights" or "Other departing flights" containers
            try 
            {
                await page.WaitForSelectorAsync("div[jsname='IWWDBc'], div[jsname='YdtKid']", new PageWaitForSelectorOptions { Timeout = 30000 });
            }
            catch (TimeoutException)
            {
                Log.Warning("Timeout waiting for flight results. URL: {Url}", url);
                return flights;
            }

            var content = await page.ContentAsync();
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(content);

            var flightElements = document.QuerySelectorAll("div[jsname='IWWDBc'], div[jsname='YdtKid']");
            
            foreach (var flightGroup in flightElements)
            {
                // Select list items within the group, filtering out hidden or irrelevant ones if necessary
                var listItems = flightGroup.QuerySelectorAll("ul.Rk10dc li");
                foreach (var item in listItems)
                {
                    try 
                    {
                        // Check if it's a valid flight card (basic check for name)
                        var nameElement = item.QuerySelector("div.sSHqwe.tPgKwe.ogfYpf span");
                        if (nameElement == null) continue;

                        var name = nameElement.TextContent.Trim();
                        
                        var times = item.QuerySelectorAll("span.mv1WYe div");
                        var departureTime = times.Length > 0 ? times[0].TextContent.Trim() : "";
                        var arrivalTime = times.Length > 1 ? times[1].TextContent.Trim() : "";
                        
                        var duration = item.QuerySelector("li div.Ak5kof div")?.TextContent.Trim() ?? "";
                        var stops = item.QuerySelector(".BbR8Ec .ogfYpf")?.TextContent.Trim() ?? "0";
                        var priceText = item.QuerySelector(".YMlIz.FpEdX")?.TextContent.Trim() ?? "0";

                        // Parse price
                        decimal price = 0;
                        if (!string.IsNullOrEmpty(priceText))
                        {
                            var cleanPrice = new string(priceText.Where(c => char.IsDigit(c) || c == '.').ToArray());
                            decimal.TryParse(cleanPrice, out price);
                        }

                        // Parse stops
                        int stopsCount = 0;
                        if (stops.Contains("Nonstop", StringComparison.OrdinalIgnoreCase))
                        {
                            stopsCount = 0;
                        }
                        else
                        {
                             var stopsPart = stops.Split(' ')[0];
                             int.TryParse(stopsPart, out stopsCount);
                        }

                        flights.Add(new Flight
                        {
                            Airline = name,
                            FlightNumber = "N/A", // Not scraped currently
                            DepartureTime = departureTime,
                            ArrivalTime = arrivalTime,
                            Duration = duration,
                            Stops = stopsCount,
                            Price = price,
                            Currency = "USD", // Assumed from URL param curr=USD
                            Origin = flightData.Origin,
                            Destination = flightData.Destination,
                            DepartureDate = flightData.DepartureDate
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error parsing flight item");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during Playwright execution");
        }
        finally
        {
            await context.CloseAsync();
        }

        return flights;
    }

    private async Task InitializePlaywrightAsync()
    {
        if (_browser != null) return;

        await _lock.WaitAsync();
        try
        {
            if (_browser != null) return;

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        _browser?.DisposeAsync().AsTask().Wait();
        _playwright?.Dispose();
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }

    public string BuildGoogleFlightsUrl(FlightData flightData)
    {
        // Get Google codes for airports
        var originCode = GetGoogleCode(flightData.Origin);
        var destCode = GetGoogleCode(flightData.Destination);

        // Base URL
        var baseUrl = "https://www.google.ca/travel/flights";
        
        // Build TFS parameter based on trip type
        string tfsParam;
        if (flightData.TripType == TripType.RoundTrip && !string.IsNullOrEmpty(flightData.ReturnDate))
        {
            tfsParam = BuildRoundTripTfsParameter(
                flightData.DepartureDate,
                flightData.ReturnDate!,
                originCode,
                destCode);
        }
        else
        {
            tfsParam = BuildOneWayTfsParameter(
                flightData.DepartureDate,
                originCode,
                destCode);
        }

        // Build URL with parameters
        var url = $"{baseUrl}/search?tfs={tfsParam}&hl=en&curr=USD";
        
        // Add passenger count if more than 1
        if (flightData.Passengers.Total > 1)
        {
            url += $"&adults={flightData.Passengers.Adults}";
            if (flightData.Passengers.Children > 0)
                url += $"&children={flightData.Passengers.Children}";
            if (flightData.Passengers.Infants > 0)
                url += $"&infants={flightData.Passengers.Infants}";
        }
        
        // Add cabin class if not economy
        var cabinClassCode = (int)flightData.SeatType;
        if (cabinClassCode > 0)
            url += $"&c={cabinClassCode}";

        return url;
    }

    public bool ValidateFlightData(FlightData flightData)
    {
        if (string.IsNullOrWhiteSpace(flightData.Origin))
            throw new ArgumentException("Origin airport is required");

        if (string.IsNullOrWhiteSpace(flightData.Destination))
            throw new ArgumentException("Destination airport is required");

        if (flightData.Origin.Equals(flightData.Destination, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Origin and destination must be different");

        if (string.IsNullOrWhiteSpace(flightData.DepartureDate))
            throw new ArgumentException("Departure date is required");

        if (!DateHelper.IsValidDate(flightData.DepartureDate))
            throw new ArgumentException("Departure date must be in YYYY-MM-DD format");

        if (!DateHelper.IsInFuture(flightData.DepartureDate))
            throw new ArgumentException("Departure date must be in the future or today");

        if (flightData.TripType == TripType.RoundTrip)
        {
            if (string.IsNullOrWhiteSpace(flightData.ReturnDate))
                throw new ArgumentException("Return date is required for round trip");

            if (!DateHelper.IsValidDate(flightData.ReturnDate))
                throw new ArgumentException("Return date must be in YYYY-MM-DD format");

            if (!DateHelper.IsReturnDateValid(flightData.DepartureDate, flightData.ReturnDate))
                throw new ArgumentException("Return date must be after or equal to departure date");
        }

        if (!flightData.Passengers.IsValid())
            throw new ArgumentException("Invalid passenger count");

        return true;
    }

    private string GetGoogleCode(string airportCode)
    {
        var code = airportCode.ToUpper();
        if (AirportCodes.TryGetValue(code, out var googleCode))
        {
            return googleCode;
        }
        
        // Log warning for unsupported airport code
        Log.Warning("Airport code {Code} not found in database, using fallback format", code);
        
        // Fallback format - may not always work but provides basic functionality
        return $"/m/{code.ToLower()}";
    }

    private decimal GetCabinPriceMultiplier(SeatType seatType)
    {
        return seatType switch
        {
            SeatType.Economy => 1.0m,
            SeatType.PremiumEconomy => 1.5m,
            SeatType.Business => 3.0m,
            SeatType.First => 5.0m,
            _ => 1.0m
        };
    }

    private string GetCabinClassName(SeatType seatType)
    {
        return seatType switch
        {
            SeatType.Economy => "economy",
            SeatType.PremiumEconomy => "premium_economy",
            SeatType.Business => "business",
            SeatType.First => "first",
            _ => "economy"
        };
    }

   
    private string BuildRoundTripTfsParameter(
        string departureDate,
        string returnDate,
        string originCode,
        string destCode)
    {
        // Format: flight=from:origin,to:dest,departure:YYYY-MM-DD;flight=from:dest,to:origin,departure:YYYY-MM-DD
        return $"flight=from:{originCode},to:{destCode},departure:{departureDate};flight=from:{destCode},to:{originCode},departure:{returnDate}";
    }

    private string BuildOneWayTfsParameter(
        string departureDate,
        string originCode,
        string destCode)
    {
        // Format: flight=from:origin,to:dest,departure:YYYY-MM-DD
        return $"flight=from:{originCode},to:{destCode},departure:{departureDate}";
    }
}