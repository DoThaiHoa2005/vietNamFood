using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    public class FavoritesApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = AppConfig.ApiBaseUrl;

        public FavoritesApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
        }

        /// <summary>
        /// Thêm yêu thích
        /// </summary>
        public async Task<(bool success, string message)> AddFavoriteAsync(int userId, int foodId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[AddFavorite] API Call - UserId: {userId}, FoodId: {foodId}");
                
                var payload = new
                {
                    userId = userId,
                    foodId = foodId
                };

                string json = JsonSerializer.Serialize(payload);
                System.Diagnostics.Debug.WriteLine($"[AddFavorite] Request JSON: {json}");
                System.Diagnostics.Debug.WriteLine($"[AddFavorite] API URL: {_baseUrl}?action=addFavorite");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}?action=addFavorite", content);
                string responseText = await response.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"[AddFavorite] Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[AddFavorite] Response Body: {responseText}");

                // Check if response is HTML (error page)
                if (responseText.TrimStart().StartsWith("<") || responseText.Contains("<!DOCTYPE"))
                {
                    System.Diagnostics.Debug.WriteLine($"[AddFavorite] ERROR: API returned HTML instead of JSON");
                    return (false, "Lỗi kết nối API. Vui lòng kiểm tra:\n1. XAMPP đã bật chưa?\n2. Ngrok URL còn hoạt động không?\n3. Database đã tạo chưa?");
                }

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = JsonSerializer.Deserialize<ApiResponse>(responseText);
                        return (result?.success == true, result?.message ?? "Thêm yêu thích thành công");
                    }
                    catch (JsonException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AddFavorite] JSON Parse Error: {ex.Message}");
                        return (false, $"Lỗi parse JSON: {ex.Message}");
                    }
                }
                else
                {
                    return (false, $"Lỗi API: {response.StatusCode} - {responseText}");
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AddFavorite] HTTP Exception: {ex.Message}");
                return (false, $"Lỗi kết nối: Không thể kết nối đến API. Kiểm tra XAMPP và ngrok.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AddFavorite] Exception: {ex.Message}");
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa yêu thích
        /// </summary>
        public async Task<(bool success, string message)> RemoveFavoriteAsync(int userId, int foodId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}?action=removeFavorite&userId={userId}&foodId={foodId}");
                string responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse>(responseText);
                    return (result?.success == true, result?.message ?? "Xóa yêu thích thành công");
                }
                else
                {
                    return (false, $"Lỗi API: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra có yêu thích không
        /// </summary>
        public async Task<bool> IsFavoriteAsync(int userId, int foodId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}?action=isFavorite&userId={userId}&foodId={foodId}");
                string responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse>(responseText);
                    return result?.success == true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking favorite: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Lấy danh sách yêu thích của user
        /// </summary>
        public async Task<List<FoodItem>> GetUserFavoritesAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}?action=getUserFavorites&userId={userId}");
                string responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<FavoritesResponse>(responseText);
                    return result?.data ?? new List<FoodItem>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting favorites: {ex.Message}");
            }
            return new List<FoodItem>();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ApiResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class FavoritesResponse
    {
        public bool success { get; set; }
        public List<FoodItem> data { get; set; }
    }
}