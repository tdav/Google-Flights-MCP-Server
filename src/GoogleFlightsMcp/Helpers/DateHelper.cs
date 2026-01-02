using System.Globalization;

namespace GoogleFlightsMcp.Helpers;

/// <summary>
/// Helper class for date-related operations
/// </summary>
public static class DateHelper
{
    /// <summary>
    /// Formats a date to YYYY-MM-DD format for Google Flights
    /// </summary>
    public static string FormatDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Formats a date string to YYYY-MM-DD format
    /// </summary>
    public static string FormatDate(string dateString)
    {
        if (DateTime.TryParse(dateString, out var date))
        {
            return FormatDate(date);
        }
        return dateString;
    }

    /// <summary>
    /// Validates if a date string is in valid format
    /// </summary>
    public static bool IsValidDate(string dateString)
    {
        return DateTime.TryParseExact(
            dateString, 
            "yyyy-MM-dd", 
            CultureInfo.InvariantCulture, 
            DateTimeStyles.None, 
            out _);
    }

    /// <summary>
    /// Checks if a date is in the future
    /// </summary>
    public static bool IsInFuture(string dateString)
    {
        if (DateTime.TryParse(dateString, out var date))
        {
            return date.Date >= DateTime.Today;
        }
        return false;
    }

    /// <summary>
    /// Checks if return date is after departure date
    /// </summary>
    public static bool IsReturnDateValid(string departureDate, string returnDate)
    {
        if (DateTime.TryParse(departureDate, out var depDate) && 
            DateTime.TryParse(returnDate, out var retDate))
        {
            return retDate.Date >= depDate.Date;
        }
        return false;
    }

    /// <summary>
    /// Gets the number of days between two dates
    /// </summary>
    public static int GetDaysBetween(string startDate, string endDate)
    {
        if (DateTime.TryParse(startDate, out var start) && 
            DateTime.TryParse(endDate, out var end))
        {
            return (int)(end.Date - start.Date).TotalDays;
        }
        return 0;
    }

    /// <summary>
    /// Converts date to Google Flights format for URL encoding
    /// </summary>
    public static string ToGoogleFlightsFormat(string dateString)
    {
        if (DateTime.TryParse(dateString, out var date))
        {
            return date.ToString("yyyy-MM-dd");
        }
        throw new ArgumentException($"Invalid date format: {dateString}");
    }
}
