using FluentAssertions;
using GoogleFlightsApi.Data;
using GoogleFlightsApi.Data.Entities;
using GoogleFlightsApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GoogleFlightsApi.Tests.Services;

public class ClientTrackingServiceTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task TrackClientAsync_NewClient_CreatesClientInfo()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var logger = new Mock<ILogger<ClientTrackingService>>();
        var service = new ClientTrackingService(context, logger.Object);

        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act
        var result = await service.TrackClientAsync(ipAddress, userAgent);

        // Assert
        result.Should().NotBeNull();
        result.IpAddress.Should().Be(ipAddress);
        result.UserAgent.Should().Be(userAgent);
        result.SearchCount.Should().Be(0);

        var savedClient = await context.ClientInfos.FirstOrDefaultAsync(c => c.IpAddress == ipAddress);
        savedClient.Should().NotBeNull();
    }

    [Fact]
    public async Task TrackClientAsync_ExistingClient_UpdatesLastSeen()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var logger = new Mock<ILogger<ClientTrackingService>>();
        var service = new ClientTrackingService(context, logger.Object);

        var ipAddress = "192.168.1.1";
        var existingClient = new ClientInfo
        {
            IpAddress = ipAddress,
            UserAgent = "Old Agent",
            FirstSeen = DateTime.UtcNow.AddDays(-1),
            LastSeen = DateTime.UtcNow.AddDays(-1),
            SearchCount = 5
        };

        context.ClientInfos.Add(existingClient);
        await context.SaveChangesAsync();

        // Act
        var result = await service.TrackClientAsync(ipAddress, "New Agent");

        // Assert
        result.Should().NotBeNull();
        result.IpAddress.Should().Be(ipAddress);
        result.UserAgent.Should().Be("New Agent");
        result.SearchCount.Should().Be(5);
        result.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
