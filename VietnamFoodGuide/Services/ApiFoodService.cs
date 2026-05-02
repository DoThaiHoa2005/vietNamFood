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
        private readonly SQLiteFoodService _sqliteService; // Fallback về SQLite thay vì JSON

        public ApiFoodService(string apiBaseUrl = null)
        {
            _apiBaseUrl = apiBaseUrl ?? AppConfig.ApiBaseUrl;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            _sqliteService = new SQLiteFoodService(); // Backup với SQLite
        }

        public async Task<List<FoodItem>> LoadFoodsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ApiFoodService] Loading foods from API: {_apiBaseUrl}?action=foods");
                
                // Gọi API để lấy dữ liệu từ MySQL
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}?action=foods");
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiFoodService] API returned status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine("[ApiFoodService] Fallback to SQLite");
                    return _sqliteService.LoadFoods();
                }

                var json = await response.Content.ReadAsStringAsync();
                
                // Check if response is HTML (error page)
                if (json.TrimStart().StartsWith("<") || json.Contains("<!DOCTYPE"))
                {
                    System.Diagnostics.Debug.WriteLine("[ApiFoodService] ERROR: API returned HTML instead of JSON");
                    System.Diagnostics.Debug.WriteLine("[ApiFoodService] Possible causes:");
                    System.Diagnostics.Debug.WriteLine("  1. XAMPP is not running");
                    System.Diagnostics.Debug.WriteLine("  2. Ngrok URL expired");
                    System.Diagnostics.Debug.WriteLine("  3. Database not created");
                    System.Diagnostics.Debug.WriteLine("[ApiFoodService] Fallback to SQLite");
                    return _sqliteService.LoadFoods();
                }
                
                System.Diagnostics.Debug.WriteLine($"[ApiFoodService] API Response (first 200 chars): {json.Substring(0, Math.Min(200, json.Length))}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var foods = JsonSerializer.Deserialize<List<ApiFood>>(json, options);

                if (foods == null || foods.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[ApiFoodService] API returned empty list, fallback to SQLite");
                    return _sqliteService.LoadFoods();
                }

                System.Diagnostics.Debug.WriteLine($"[ApiFoodService] Successfully loaded {foods.Count} foods from API");
                
                // Convert từ ApiFood (database format) sang FoodItem (app format)
                var foodItems = ConvertToFoodItems(foods);
                
                // Sync dữ liệu từ API về SQLite để dùng offline
                _sqliteService.SyncFromAPI(foodItems);
                
                return foodItems;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiFoodService] HTTP Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("[ApiFoodService] Cannot connect to API. Check XAMPP and ngrok.");
                System.Diagnostics.Debug.WriteLine("[ApiFoodService] Fallback to SQLite");
                return _sqliteService.LoadFoods();
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiFoodService] JSON Parse Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("[ApiFoodService] API response is not valid JSON");
                System.Diagnostics.Debug.WriteLine("[ApiFoodService] Fallback to SQLite");
                return _sqliteService.LoadFoods();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiFoodService] Unexpected Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("[ApiFoodService] Fallback to SQLite");
                return _sqliteService.LoadFoods();
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
                    Id = apiFood.Id,
                    Name = apiFood.Name,
                    City = apiFood.City,
                    Category = apiFood.Category,
                    Image = apiFood.ImagePath,
                    DescriptionVI = apiFood.Description_VI,
                    DescriptionEN = apiFood.Description_EN,
                    DescriptionCN = apiFood.Description_CN,
                    Latitude = apiFood.Latitude,
                    Longitude = apiFood.Longitude,
                    Rating = apiFood.Rating,
                    // Map new fields for Geofence & Narration
                    Radius = apiFood.Radius,
                    Priority = apiFood.Priority,
                    AudioUrl = apiFood.AudioUrl,
                    NarrationScript = apiFood.NarrationScript,
                    CooldownMinutes = apiFood.CooldownMinutes
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
            // New fields for Geofence & Narration
            public double Radius { get; set; }
            public int Priority { get; set; }
            public string AudioUrl { get; set; }
            public string NarrationScript { get; set; }
            public int CooldownMinutes { get; set; }
        }
    }
}
