using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service xác thực người dùng qua XAMPP API
    /// </summary>
    public class ApiAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ApiAuthService(string apiBaseUrl = null)
        {
            _apiBaseUrl = apiBaseUrl ?? AppConfig.ApiBaseUrl;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            System.Diagnostics.Debug.WriteLine($"[ApiAuthService] Initialized with URL: {_apiBaseUrl}");
        }

        /// <summary>
        /// Đăng nhập qua API
        /// </summary>
        public async Task<(bool Success, UserInfo User, string ErrorMessage)> LoginAsync(string username, string password)
        {
            try
            {
                // Tự động khởi tạo/fix users trước khi login
                await InitializeUsersAsync();

                // Tạo request body
                var loginData = new
                {
                    username = username,
                    password = password
                };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gọi API login
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}?action=login", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse response
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<LoginResponse>(responseJson, options);

                    if (result?.Success == true && result.User != null)
                    {
                        // Lưu token vào UserInfo
                        result.User.Token = result.Token;
                        
                        // Parse ExpiresAt
                        if (!string.IsNullOrEmpty(result.ExpiresAt))
                        {
                            if (DateTime.TryParse(result.ExpiresAt, out DateTime expiresAt))
                            {
                                result.User.ExpiresAt = expiresAt;
                            }
                        }
                        
                        return (true, result.User, null);
                    }
                }

                // Parse error message
                try
                {
                    var errorResult = JsonSerializer.Deserialize<ErrorResponse>(responseJson);
                    return (false, null, errorResult?.Error ?? "Đăng nhập thất bại");
                }
                catch
                {
                    return (false, null, "Đăng nhập thất bại");
                }
            }
            catch (HttpRequestException)
            {
                return (false, null, "Không thể kết nối đến server. Hãy kiểm tra XAMPP.");
            }
            catch (TaskCanceledException)
            {
                return (false, null, "Kết nối timeout. Hãy kiểm tra XAMPP.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Tự động khởi tạo hoặc fix password cho users
        /// </summary>
        private async Task InitializeUsersAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}?action=init_users", null);
                // Không cần xử lý response, chỉ cần gọi để init
            }
            catch
            {
                // Bỏ qua lỗi, vì có thể database chưa có table Users
            }
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        public async Task<(bool Success, string Message)> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var registerData = new
                {
                    username = username,
                    email = email,
                    password = password
                };

                var json = JsonSerializer.Serialize(registerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"[Register] Calling API: {_apiBaseUrl}?action=register");
                System.Diagnostics.Debug.WriteLine($"[Register] Request: {json}");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}?action=register", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[Register] Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[Register] Response: {responseJson}");

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<RegisterResponse>(responseJson, options);
                    return (result?.Success == true, result?.Message ?? "Đăng ký thành công");
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var errorResult = JsonSerializer.Deserialize<ErrorResponse>(responseJson, options);
                    return (false, errorResult?.Error ?? $"Đăng ký thất bại (HTTP {response.StatusCode})");
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Register] HttpRequestException: {ex}");
                return (false, "Không thể kết nối đến server. Hãy kiểm tra XAMPP.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Register] Exception: {ex}");
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        // Response models
        private class LoginResponse
        {
            public bool Success { get; set; }
            public UserInfo User { get; set; }
            public string Token { get; set; }  // Session token
            public string ExpiresAt { get; set; }  // Thời gian hết hạn
            public string Message { get; set; }
        }

        private class RegisterResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public object UserId { get; set; } // Có thể là string hoặc int
        }

        private class ErrorResponse
        {
            public string Error { get; set; }
        }
    }

    /// <summary>
    /// Thông tin user từ API
    /// </summary>
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }  // Session token
        public DateTime? ExpiresAt { get; set; }  // Thời gian hết hạn

        public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
    }
}
