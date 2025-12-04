using GoogleFlightsApi.Models;

namespace GoogleFlightsApi.Services;

public interface ISearchHistoryService
{
    Task SaveSearchAsync(
        int clientInfoId,
        FlightSearchRequest request,
        FlightSearchResponse response);
    
    Task<List<SearchHistoryDto>> GetSearchHistoryAsync(string ipAddress, int pageNumber = 1, int pageSize = 10);
    Task<List<SearchHistoryDto>> GetAllSearchHistoryAsync(int pageNumber = 1, int pageSize = 50);
}
