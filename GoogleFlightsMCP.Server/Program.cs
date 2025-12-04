using GoogleFlightsMCP.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IFlightSearchService, FlightSearchService>();

// CORS configuration - Note: In production, restrict to specific origins for security
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Allow all origins for testing
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // Production: Restrict to specific origins
            // TODO: Update with your production domains
            policy.WithOrigins("https://yourdomain.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

// MCP Server endpoints for Google Flights
app.MapGet("/", () => new
{
    name = "Google Flights MCP Server",
    version = "1.0.0",
    description = "MCP Server for searching Google Flights"
})
.WithName("GetServerInfo")
.WithOpenApi();

app.MapGet("/api/flights/search", async (
    string origin,
    string destination,
    string departureDate,
    string? returnDate,
    int passengers,
    string cabinClass,
    IFlightSearchService flightService) =>
{
    try
    {
        var results = await flightService.SearchFlightsAsync(
            origin,
            destination,
            departureDate,
            returnDate,
            passengers,
            cabinClass);
        
        return Results.Ok(results);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("SearchFlights")
.WithOpenApi();

app.MapPost("/api/mcp/search", async (FlightSearchRequest request, IFlightSearchService flightService) =>
{
    try
    {
        var results = await flightService.SearchFlightsAsync(
            request.Origin,
            request.Destination,
            request.DepartureDate,
            request.ReturnDate,
            request.Passengers,
            request.CabinClass);
        
        return Results.Ok(new
        {
            tool = "search_flights",
            content = results
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("MCPSearchFlights")
.WithOpenApi();

app.Run();

public record FlightSearchRequest(
    string Origin,
    string Destination,
    string DepartureDate,
    string? ReturnDate,
    int Passengers,
    string CabinClass
);
