namespace GoogleFlightsApi.Models;

public static class AirportCodes
{
    public static readonly Dictionary<string, string> Codes = new()
    {
        // North America
        { "JFK", "/m/02_286" },
        { "LAX", "/m/030qb3t" },
        { "ORD", "/m/01_d4" },
        { "DFW", "/m/030k2v" },
        { "SFO", "/m/0d6lp" },
        { "SEA", "/m/0d9jr" },
        { "MIA", "/m/0f2v0" },
        { "BOS", "/m/01cx_" },
        { "ATL", "/m/013yq" },
        { "LAS", "/m/0cv3w" },
        { "PHX", "/m/0d35y" },
        { "DEN", "/m/02cft" },
        { "IAH", "/m/03ksg" },
        { "MSP", "/m/0fpzwf" },
        { "DTW", "/m/0fvwg" },
        { "EWR", "/m/0cc56" },
        { "MCO", "/m/0fxmq" },
        
        // Europe
        { "LHR", "/m/04jpl" },
        { "CDG", "/m/05qtj" },
        { "FRA", "/m/0jxgx" },
        { "AMS", "/m/0k3p" },
        { "MAD", "/m/056_y" },
        { "BCN", "/m/01f62" },
        { "FCO", "/m/06c62" },
        { "MUC", "/m/0727_" },
        { "IST", "/m/09949" },
        { "LGW", "/m/065y4w7" },
        { "ZRH", "/m/08g5vq" },
        { "VIE", "/m/05qx6" },
        
        // Asia
        { "DXB", "/m/01f08r" },
        { "HKG", "/m/03h64r" },
        { "NRT", "/m/0f4t4" },
        { "SIN", "/m/02p24c" },
        { "ICN", "/m/0cyzn" },
        { "BKK", "/m/0dl9t8" },
        { "DEL", "/m/03l8mx" },
        { "PEK", "/m/0dq_7" },
        { "PVG", "/m/0j5nb" },
        { "HND", "/m/0gx_x" },
        
        // Middle East & Central Asia
        { "TAS", "/m/0fsmy" },  // Tashkent
        { "DOH", "/m/01_8q3" },
        
        // Australia & Oceania
        { "SYD", "/m/06y57" },
        { "MEL", "/m/0chghy" },
        { "AKL", "/m/0ctyv" },
        
        // South America
        { "GRU", "/m/0fphj" },
        { "EZE", "/m/0132jd" },
        { "GIG", "/m/02k0l1" },
        
        // Africa
        { "JNB", "/m/04g8v" },
        { "CAI", "/m/0cwd8" },
    };

    public static string? GetGoogleCode(string airportCode)
    {
        if (Codes.TryGetValue(airportCode.ToUpper(), out var code))
            return code;
        return null;
    }
}
