using System.Globalization;

namespace GoogleFlights.Core.Helpers;

/// <summary>
/// Helper class for price-related operations
/// </summary>
public static class PriceHelper
{
    /// <summary>
    /// Formats a price with currency symbol
    /// </summary>
    public static string FormatPrice(decimal price, string currency)
    {
        return currency.ToUpper() switch
        {
            "USD" => $"${price:N2}",
            "EUR" => $"€{price:N2}",
            "GBP" => $"£{price:N2}",
            "UZS" => $"{price:N0} UZS",
            "RUB" => $"{price:N2} ₽",
            "CNY" => $"¥{price:N2}",
            "JPY" => $"¥{price:N0}",
            _ => $"{price:N2} {currency}"
        };
    }

    /// <summary>
    /// Formats price for display in search results
    /// </summary>
    public static string FormatPriceForDisplay(decimal price, string currency, int passengers)
    {
        var totalPrice = price * passengers;
        var perPersonPrice = FormatPrice(price, currency);
        
        if (passengers > 1)
        {
            var total = FormatPrice(totalPrice, currency);
            return $"{perPersonPrice} per person ({total} total for {passengers} passengers)";
        }
        
        return perPersonPrice;
    }

    /// <summary>
    /// Converts price from one currency to another (simplified conversion)
    /// </summary>
    public static decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
    {
        // Simplified conversion rates (in production, use real-time rates)
        var conversionRates = new Dictionary<string, decimal>
        {
            { "USD", 1.0m },
            { "EUR", 0.85m },
            { "GBP", 0.73m },
            { "UZS", 12800m },
            { "RUB", 95m },
            { "CNY", 7.2m },
            { "JPY", 150m }
        };

        if (!conversionRates.TryGetValue(fromCurrency.ToUpper(), out var fromRate))
            return amount;

        if (!conversionRates.TryGetValue(toCurrency.ToUpper(), out var toRate))
            return amount;

        var usdAmount = amount / fromRate;
        return usdAmount * toRate;
    }

    /// <summary>
    /// Rounds price to appropriate precision based on currency
    /// </summary>
    public static decimal RoundPrice(decimal price, string currency)
    {
        return currency.ToUpper() switch
        {
            "UZS" or "JPY" or "KRW" => Math.Round(price, 0),
            _ => Math.Round(price, 2)
        };
    }

    /// <summary>
    /// Parses a price string to decimal
    /// </summary>
    public static decimal ParsePrice(string priceString)
    {
        // Remove currency symbols and spaces
        var cleanPrice = priceString
            .Replace("$", "")
            .Replace("€", "")
            .Replace("£", "")
            .Replace("¥", "")
            .Replace("₽", "")
            .Replace("UZS", "")
            .Replace(",", "")
            .Trim();

        if (decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
        {
            return price;
        }

        throw new ArgumentException($"Invalid price format: {priceString}");
    }

    /// <summary>
    /// Compares prices and returns the cheaper one
    /// </summary>
    public static decimal GetCheaperPrice(decimal price1, decimal price2)
    {
        return Math.Min(price1, price2);
    }

    /// <summary>
    /// Calculates price difference as percentage
    /// </summary>
    public static decimal CalculatePriceDifferencePercent(decimal price1, decimal price2)
    {
        if (price1 == 0) return 0;
        return ((price2 - price1) / price1) * 100;
    }
}
