using GoogleFlightsMCP.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IFlightSearchService, FlightSearchService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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
