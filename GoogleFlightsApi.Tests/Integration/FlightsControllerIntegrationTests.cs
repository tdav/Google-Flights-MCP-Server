using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GoogleFlights.Core.Models;
using GoogleFlights.Core.Services;
using GoogleFlightsApi.Data;
using GoogleFlightsApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GoogleFlightsApi.Tests.Integration;

public class FlightsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FlightsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SearchFlights_ValidRequest_ReturnsOk()
    {
        // Arrange
        var origin = "TAS";
        var destination = "JFK";
        var departureDate = "2026-01-09";
        var returnDate = "2026-02-15";
        var passengers = 1;
        var cabinClass = "economy";

        var expectedFlightResult = new FlightResult
        {
            Origin = origin,
            Destination = destination,
            DepartureDate = departureDate,
            ReturnDate = returnDate,
            Passengers = passengers,
            CabinClass = cabinClass,
            Flights = new List<Flight>
            {
                new Flight
                {
                    Airline = "Test Airline",
                    Price = 1000,
                    Currency = "USD",
                    DepartureTime = "10:00",
                    ArrivalTime = "20:00",
                    Duration = "10h"
                }
            },
            SearchUrl = "https://google.com/test"
        };

        var mockService = new Mock<IFlightSearchService>();
        mockService.Setup(s => s.SearchFlightsAsync(It.Is<FlightData>(d => 
            d.Origin == origin && 
            d.Destination == destination && 
            d.DepartureDate == departureDate)))
            .ReturnsAsync(expectedFlightResult);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's DbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using an in-memory database for testing.
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IFlightSearchService>(_ => mockService.Object);
            });
        }).CreateClient();

        // Act
        var url = $"/api/flights/search?origin={origin}&destination={destination}&departureDate={departureDate}&returnDate={returnDate}&passengers={passengers}&cabinClass={cabinClass}";
        var response = await client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FlightSearchResponse>();
        
        result.Should().NotBeNull();
        result!.Origin.Should().Be(origin);
        result.Destination.Should().Be(destination);
        result.Flights.Should().HaveCount(1);
        result.Flights[0].Airline.Should().Be("Test Airline");
        
        mockService.Verify(s => s.SearchFlightsAsync(It.IsAny<FlightData>()), Times.Once);
    }
}
