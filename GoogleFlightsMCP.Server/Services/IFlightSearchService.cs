namespace GoogleFlightsMCP.Server.Services;

public interface IFlightSearchService
{
    Task<FlightSearchResult> SearchFlightsAsync(
        string origin,
        string destination,
        string departureDate,
        string? returnDate,
        int passengers,
        string cabinClass);
}

public record FlightSearchResult(
    string Origin,
    string Destination,
    string DepartureDate,
    string? ReturnDate,
    int Passengers,
    string CabinClass,
    List<Flight> Flights,
    string SearchUrl
);

public record Flight(
    string Airline,
    string FlightNumber,
    string DepartureTime,
    string ArrivalTime,
    string Duration,
    int Stops,
    decimal Price,
    string Currency
);
