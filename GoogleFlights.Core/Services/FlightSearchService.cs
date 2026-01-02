using Serilog;
using GoogleFlights.Core.Models;
using GoogleFlights.Core.Helpers;
using Microsoft.Playwright;
using AngleSharp.Html.Parser;

namespace GoogleFlights.Core.Services;

/// <summary>
/// Service for searching flights on Google Flights
/// </summary>
public class FlightSearchService : IFlightSearchService, IDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private readonly SemaphoreSlim _lock = new(1, 1);

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
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            Locale = "en-US",
            TimezoneId = "America/New_York",
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        var page = await context.NewPageAsync();
        var flights = new List<Flight>();

        try 
        {
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            
            // Handle Google cookie consent dialog
            await AcceptCookieConsentAsync(page);

            // Wait for results. We accept either "Best departing flights" or "Other departing flights" containers
            try 
            {
                await Task.Delay(1000);
                await page.WaitForSelectorAsync("div[jsname='IWWDBc'], div[jsname='YdtKid']", new PageWaitForSelectorOptions { Timeout = 45000 });
                await Task.Delay(2000);
            }
            catch (TimeoutException)
            {
                // Try to check if there's an error message or no flights available
                var noFlightsMessage = await page.QuerySelectorAsync("div[class*='error'], div[class*='no-results']");
                if (noFlightsMessage != null)
                {
                    Log.Warning("No flights found or error on page. URL: {Url}", url);
                }
                else
                {
                    Log.Warning("Timeout waiting for flight results. URL: {Url}", url);
                }
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

    /// <summary>
    /// Handles Google cookie consent dialog if present
    /// </summary>
    private async Task AcceptCookieConsentAsync(IPage page)
    {
        try
        {
            // Common selectors for Google consent dialogs
            var consentSelectors = new[]
            {
                "button[aria-label='Accept all']",
                "button:has-text('Accept all')",
                "button:has-text('I agree')",
                "button:has-text('Agree')",
                "[aria-label='Accept all']",
                "form[action*='consent'] button",
                "#L2AGLb", // Google consent button ID
                "button[jsname='b3VHJd']" // Another common Google consent button
            };

            foreach (var selector in consentSelectors)
            {
                try
                {
                    var consentButton = await page.QuerySelectorAsync(selector);
                    if (consentButton != null)
                    {
                        await consentButton.ClickAsync();
                        Log.Information("Accepted cookie consent using selector: {Selector}", selector);
                        // Wait for page to update after consent
                        await page.WaitForTimeoutAsync(2000);
                        break;
                    }
                }
                catch
                {
                    // Continue to next selector
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not handle cookie consent dialog");
        }
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
                ChromiumSandbox = true,
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

            var baseUrl = "https://www.google.com/travel/flights";

            

            var queryParts = new List<string>
            {
                $"from {flightData.Origin}",
                $"to {flightData.Destination}",
                $"on {flightData.DepartureDate}"
            };

    

            if (flightData.TripType == TripType.RoundTrip && !string.IsNullOrEmpty(flightData.ReturnDate))
            {
                queryParts.Add($"returning {flightData.ReturnDate}");
            }

            

            // Cabin class
            var cabinClass = GetCabinClassName(flightData.SeatType).Replace("_", " ");
            if (flightData.SeatType != SeatType.Economy)
            {
                queryParts.Add($"{cabinClass} class");
            }
                       

            // Passengers
            if (flightData.Passengers.Total > 1)
            {
                queryParts.Add($"{flightData.Passengers.Total} passengers");
            }
    

            var queryString = string.Join(" ", queryParts);

            // Use Uri.EscapeDataString for cleaner encoding of spaces and special chars

            var encodedQuery = Uri.EscapeDataString(queryString);

            

            return $"{baseUrl}?q={encodedQuery}&curr=USD&hl=en";

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

    }

    