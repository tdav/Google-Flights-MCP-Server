using System.ComponentModel.DataAnnotations;

namespace GoogleFlightsApi.Models;

public class FlightSearchRequest
{
    [Required]
    [StringLength(10, MinimumLength = 3)]
    public string Origin { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 3)]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public string DepartureDate { get; set; } = string.Empty;

    public string? ReturnDate { get; set; }

    [Required]
    [Range(1, 9)]
    public int Passengers { get; set; } = 1;

    [Required]
    public string CabinClass { get; set; } = "economy";
}
