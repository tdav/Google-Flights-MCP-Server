using System.Text.Json;
using GoogleFlightsApi.Data;
using GoogleFlightsApi.Data.Entities;
using GoogleFlightsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GoogleFlightsApi.Services;

public class SearchHistoryService : ISearchHistoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SearchHistoryService> _logger;

    public SearchHistoryService(
        ApplicationDbContext context,
        ILogger<SearchHistoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveSearchAsync(
        int clientInfoId,
        FlightSearchRequest request,
        FlightSearchResponse response)
    {
        // Serialize flights to JSON
        var flightsJson = JsonSerializer.Serialize(response.Flights);

        var searchHistory = new SearchHistory
        {
            ClientInfoId = clientInfoId,
            Origin = request.Origin,
            Destination = request.Destination,
            DepartureDate = DateTime.Parse(request.DepartureDate).ToUniversalTime(),
            ReturnDate = string.IsNullOrWhiteSpace(request.ReturnDate) 
                ? null 
                : DateTime.Parse(request.ReturnDate).ToUniversalTime(),
            Passengers = request.Passengers,
            CabinClass = request.CabinClass,
            SearchedAt = DateTime.UtcNow,
            SearchUrl = response.SearchUrl,
            FlightsJson = flightsJson
        };

        _context.SearchHistories.Add(searchHistory);
        
        // Update client search count
        var client = await _context.ClientInfos.FindAsync(clientInfoId);
        if (client != null)
        {
            client.SearchCount++;
            _context.ClientInfos.Update(client);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Search saved for client {ClientId}: {Origin} -> {Destination} with {FlightCount} results",
            clientInfoId, request.Origin, request.Destination, response.Flights.Count);
    }

    public async Task<List<SearchHistoryDto>> GetSearchHistoryAsync(
        string ipAddress, 
        int pageNumber = 1, 
        int pageSize = 10)
    {
        var query = _context.SearchHistories
            .Include(s => s.ClientInfo)
            .Where(s => s.ClientInfo.IpAddress == ipAddress)
            .OrderByDescending(s => s.SearchedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var searches = await query.ToListAsync();

        return searches.Select(s => new SearchHistoryDto
        {
            Id = s.Id,
            Origin = s.Origin,
            Destination = s.Destination,
            DepartureDate = s.DepartureDate,
            ReturnDate = s.ReturnDate,
            Passengers = s.Passengers,
            CabinClass = s.CabinClass,
            SearchedAt = s.SearchedAt,
            SearchUrl = s.SearchUrl,
            IpAddress = s.ClientInfo.IpAddress,
            Flights = string.IsNullOrEmpty(s.FlightsJson) 
                ? new List<FlightDto>() 
                : JsonSerializer.Deserialize<List<FlightDto>>(s.FlightsJson) ?? new List<FlightDto>()
        }).ToList();
    }

    public async Task<List<SearchHistoryDto>> GetAllSearchHistoryAsync(
        int pageNumber = 1, 
        int pageSize = 50)
    {
        var query = _context.SearchHistories
            .Include(s => s.ClientInfo)
            .OrderByDescending(s => s.SearchedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var searches = await query.ToListAsync();

        return searches.Select(s => new SearchHistoryDto
        {
            Id = s.Id,
            Origin = s.Origin,
            Destination = s.Destination,
            DepartureDate = s.DepartureDate,
            ReturnDate = s.ReturnDate,
            Passengers = s.Passengers,
            CabinClass = s.CabinClass,
            SearchedAt = s.SearchedAt,
            SearchUrl = s.SearchUrl,
            IpAddress = s.ClientInfo.IpAddress,
            Flights = string.IsNullOrEmpty(s.FlightsJson) 
                ? new List<FlightDto>() 
                : JsonSerializer.Deserialize<List<FlightDto>>(s.FlightsJson) ?? new List<FlightDto>()
        }).ToList();
    }
}