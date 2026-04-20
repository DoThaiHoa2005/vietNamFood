using System;
using System.Collections.Generic;
using System.Linq;

namespace VietnamFoodGuide.Helpers.Extensions
{
    /// <summary>
    /// Extension methods for string operations
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Check if string is null, empty, or whitespace
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Truncate string to max length
        /// </summary>
        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (value == null) return null;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - suffix.Length) + suffix;
        }

        /// <summary>
        /// Remove special characters, keep only alphanumeric
        /// </summary>
        public static string RemoveSpecialCharacters(this string value)
        {
            if (value == null) return null;
            return System.Text.RegularExpressions.Regex.Replace(value, @"[^a-zA-Z0-9\s]", "");
        }

        /// <summary>
        /// Convert to title case
        /// </summary>
        public static string ToTitleCase(this string value)
        {
            if (value == null) return null;
            var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Check if contains text (case-insensitive)
        /// </summary>
        public static bool ContainsIgnoreCase(this string value, string search)
        {
            if (value == null || search == null) return false;
            return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
