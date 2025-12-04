using FluentAssertions;
using GoogleFlightsApi.Models;
using GoogleFlightsApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GoogleFlightsApi.Tests.Services;

public class FlightSearchServiceTests
{
    private readonly Mock<ILogger<FlightSearchService>> _loggerMock;
    private readonly FlightSearchService _service;

    public FlightSearchServiceTests()
    {
        _loggerMock = new Mock<ILogger<FlightSearchService>>();
        var httpClient = new HttpClient();
        _service = new FlightSearchService(_loggerMock.Object, httpClient);
    }

    [Fact]
    public async Task SearchFlightsAsync_ValidRequest_ReturnsFlights()
    {
        // Arrange
        var request = new FlightSearchRequest
        {
            Origin = "JFK",
            Destination = "LAX",
            DepartureDate = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"),
            Passengers = 1,
            CabinClass = "economy"
        };

        // Act
        var result = await _service.SearchFlightsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Origin.Should().Be("JFK");
        result.Destination.Should().Be("LAX");
        result.Flights.Should().NotBeEmpty();
        result.SearchUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SearchFlightsAsync_PastDepartureDate_ThrowsArgumentException()
    {
        // Arrange
        var request = new FlightSearchRequest
        {
            Origin = "JFK",
            Destination = "LAX",
            DepartureDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
            Passengers = 1,
            CabinClass = "economy"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SearchFlightsAsync(request));
    }

    [Fact]
    public async Task SearchFlightsAsync_InvalidCabinClass_ThrowsArgumentException()
    {
        // Arrange
        var request = new FlightSearchRequest
        {
            Origin = "JFK",
            Destination = "LAX",
            DepartureDate = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"),
            Passengers = 1,
            CabinClass = "invalid"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SearchFlightsAsync(request));
    }

    [Fact]
    public async Task SearchFlightsAsync_WithReturnDate_ReturnsRoundTripFlights()
    {
        // Arrange
        var request = new FlightSearchRequest
        {
            Origin = "JFK",
            Destination = "LAX",
            DepartureDate = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"),
            ReturnDate = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd"),
            Passengers = 2,
            CabinClass = "business"
        };

        // Act
        var result = await _service.SearchFlightsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ReturnDate.Should().NotBeNull();
        result.Passengers.Should().Be(2);
        result.CabinClass.Should().Be("business");
    }
}
