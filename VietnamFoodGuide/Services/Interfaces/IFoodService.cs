using System.Collections.Generic;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services.Interfaces
{
    /// <summary>
    /// Contract for food data operations
    /// </summary>
    public interface IFoodService
    {
        /// <summary>
        /// Load all foods from data source
        /// </summary>
        List<FoodItem> LoadFoods();

        /// <summary>
        /// Search foods by keyword
        /// </summary>
        List<FoodItem> SearchByKeyword(string keyword);

        /// <summary>
        /// Filter foods by category
        /// </summary>
        List<FoodItem> FilterByCategory(string category);

        /// <summary>
        /// Get food by ID
        /// </summary>
        FoodItem GetById(string id);
    }
}
