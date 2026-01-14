using System.Globalization;

namespace WoopiAiHub.Domain.Utils
{
    /// <summary>
    /// Helper class for date parsing operations
    /// </summary>
    public static class DateHelper
    {
        /// <summary>
        /// Converts a string date in "yyyy-MM-dd" format to DateTime?.
        /// </summary>
        /// <param name="date">The date string to parse</param>
        /// <returns>Parsed DateTime or null if the string is null or empty</returns>
        public static DateTime? ParseDate(string? date)
        {
            if (string.IsNullOrEmpty(date))
                return null;

            return DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
