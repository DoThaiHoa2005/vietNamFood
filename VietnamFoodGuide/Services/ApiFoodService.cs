using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service lấy dữ liệu từ API (XAMPP MySQL) thay vì foods.json
    /// </summary>
    public class ApiFoodService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly FoodService _fallbackService; // Fallback về JSON nếu API lỗi

        public ApiFoodService(string apiBaseUrl = null)
        {
            _apiBaseUrl = apiBaseUrl ?? AppConfig.ApiBaseUrl;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            _fallbackService = new FoodService(); // Backup
        }

        public async Task<List<FoodItem>> LoadFoodsAsync()
        {
            try
            {
                // Gọi API để lấy dữ liệu từ MySQL
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}?action=foods");
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("API không phản hồi, dùng foods.json");
                    return _fallbackService.LoadFoods();
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var foods = JsonSerializer.Deserialize<List<ApiFood>>(json, options);

                // Convert từ ApiFood (database format) sang FoodItem (app format)
                return ConvertToFoodItems(foods);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load từ API: {ex.Message}");
                // Fallback về JSON local
                return _fallbackService.LoadFoods();
            }
        }

        private List<FoodItem> ConvertToFoodItems(List<ApiFood> apiFoods)
        {
            var result = new List<FoodItem>();
            
            if (apiFoods == null) return result;

            foreach (var apiFood in apiFoods)
            {
                result.Add(new FoodItem
                {
                    Id = apiFood.Id,  // Thêm Id mapping
                    Name = apiFood.Name,
                    City = apiFood.City,
                    Category = apiFood.Category,
                    Image = apiFood.ImagePath,
                    DescriptionVI = apiFood.Description_VI,
                    DescriptionEN = apiFood.Description_EN,
                    DescriptionCN = apiFood.Description_CN,
                    Latitude = apiFood.Latitude,
                    Longitude = apiFood.Longitude,
                    Rating = apiFood.Rating
                });
            }

            return result;
        }

        // Model cho dữ liệu từ API (khớp với database schema)
        private class ApiFood
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string City { get; set; }
            public string Category { get; set; }
            public string Description_VI { get; set; }
            public string Description_EN { get; set; }
            public string Description_CN { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Rating { get; set; }
            public string ImagePath { get; set; }
        }
    }
}
