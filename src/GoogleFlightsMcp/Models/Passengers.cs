namespace GoogleFlightsMcp.Models;

/// <summary>
/// Represents passenger information for a flight search
/// </summary>
public class Passengers
{
    /// <summary>
    /// Number of adult passengers (12+ years)
    /// </summary>
    public int Adults { get; set; } = 1;

    /// <summary>
    /// Number of children (2-11 years)
    /// </summary>
    public int Children { get; set; } = 0;

    /// <summary>
    /// Number of infants (under 2 years)
    /// </summary>
    public int Infants { get; set; } = 0;

    /// <summary>
    /// Total number of passengers
    /// </summary>
    public int Total => Adults + Children + Infants;

    /// <summary>
    /// Validates passenger counts
    /// </summary>
    public bool IsValid()
    {
        return Adults >= 1 && Adults <= 9 && 
               Children >= 0 && Children <= 9 && 
               Infants >= 0 && Infants <= 9 &&
               Total <= 9;
    }
}
