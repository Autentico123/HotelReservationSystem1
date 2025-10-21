using System.Globalization;

namespace HotelReservationSystem1.Helpers
{
    public static class CurrencyHelper
    {
        // Philippine Peso symbol using Unicode escape sequence
        public const string CurrencySymbol = "\u20B1"; // ?
        public const string CurrencyCode = "PHP";
        
        /// <summary>
        /// Format amount as Philippine Peso
        /// </summary>
        public static string FormatPeso(decimal amount)
        {
            return $"{CurrencySymbol}{amount:N2}";
        }
        
        /// <summary>
        /// Format amount as Philippine Peso with custom format
        /// </summary>
        public static string FormatPeso(decimal amount, string format)
        {
            return $"{CurrencySymbol}{amount.ToString(format)}";
        }
        
        /// <summary>
        /// Format amount as Philippine Peso without decimals
        /// </summary>
        public static string FormatPesoWhole(decimal amount)
        {
            return $"{CurrencySymbol}{amount:N0}";
        }
    }
}
