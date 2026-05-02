using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace VietnamFoodGuide.Helpers.Validators
{
    /// <summary>
    /// Input validation utilities
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate if food name is valid
        /// </summary>
        public static bool IsValidFoodName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return name.Length >= 2 && name.Length <= 200;
        }

        /// <summary>
        /// Validate if rating is in valid range (0-5)
        /// </summary>
        public static bool IsValidRating(double rating)
        {
            return rating >= 0 && rating <= 5;
        }

        /// <summary>
        /// Validate if latitude is valid (-90 to 90)
        /// </summary>
        public static bool IsValidLatitude(double latitude)
        {
            return latitude >= -90 && latitude <= 90;
        }

        /// <summary>
        /// Validate if longitude is valid (-180 to 180)
        /// </summary>
        public static bool IsValidLongitude(double longitude)
        {
            return longitude >= -180 && longitude <= 180;
        }

        /// <summary>
        /// Validate if coordinates are valid
        /// </summary>
        public static bool IsValidCoordinates(double latitude, double longitude)
        {
            return IsValidLatitude(latitude) && IsValidLongitude(longitude);
        }

        /// <summary>
        /// Validate if search keyword is valid
        /// </summary>
        public static bool IsValidSearchKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return false;

            return keyword.Length >= 1 && keyword.Length <= 500;
        }

        /// <summary>
        /// Validate if category is valid
        /// </summary>
        public static bool IsValidCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return false;

            var validCategories = new[] { "Hải sản", "Ốc", "Bún", "Nướng", "Lẩu & Nướng" };
            return validCategories.Contains(category);
        }
    }
}
