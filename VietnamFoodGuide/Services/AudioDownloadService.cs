using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service tự động download file audio từ server
    /// Chạy khi đăng nhập và sync các file mới
    /// </summary>
    public class AudioDownloadService
    {
        private readonly string _audioFolder;
        private readonly HttpClient _httpClient;
        private readonly ApiFoodService _apiService;

        public AudioDownloadService()
        {
            _audioFolder = Path.Combine(AppContext.BaseDirectory, "Assets", "Audio");
            
            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(_audioFolder))
            {
                Directory.CreateDirectory(_audioFolder);
            }

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _apiService = new ApiFoodService();
        }

        /// <summary>
        /// Download tất cả file audio từ server (chạy khi đăng nhập)
        /// </summary>
        public async Task<AudioDownloadResult> DownloadAllAudioAsync(Action<int, int, string> progressCallback = null)
        {
            var result = new AudioDownloadResult();
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"🚀 [AudioDownload] Bắt đầu download audio...");
                progressCallback?.Invoke(0, 0, "🔄 Đang tải danh sách quán ăn...");

                // Load tất cả foods từ database
                var foods = await _apiService.LoadFoodsAsync();
                
                if (foods == null || foods.Count == 0)
                {
                    result.Message = "Không có quán ăn nào";
                    System.Diagnostics.Debug.WriteLine($"⚠️ [AudioDownload] {result.Message}");
                    return result;
                }

                int total = 0;
                int current = 0;

                // Đếm tổng số file cần download
                foreach (var food in foods)
                {
                    if (!string.IsNullOrEmpty(food.AudioUrl_VI)) total++;
                    if (!string.IsNullOrEmpty(food.AudioUrl_EN)) total++;
                    if (!string.IsNullOrEmpty(food.AudioUrl_CN)) total++;
                }

                System.Diagnostics.Debug.WriteLine($"📝 [AudioDownload] Tìm thấy {foods.Count} quán, {total} file audio");
                progressCallback?.Invoke(0, total, $"📝 Tìm thấy {total} file audio");

                if (total == 0)
                {
                    result.Message = "Không có file audio nào trên server";
                    System.Diagnostics.Debug.WriteLine($"⚠️ [AudioDownload] {result.Message}");
                    return result;
                }

                // Download từng file
                foreach (var food in foods)
                {
                    // Download VI
                    if (!string.IsNullOrEmpty(food.AudioUrl_VI))
                    {
                        current++;
                        string message = $"[{current}/{total}] {food.Name} (VI)";
                        progressCallback?.Invoke(current, total, message);
                        
                        bool downloaded = await DownloadSingleAudioAsync(food.AudioUrl_VI, food.Id, "VI", food.Name);
                        if (downloaded)
                        {
                            result.DownloadedCount++;
                            System.Diagnostics.Debug.WriteLine($"✅ [AudioDownload] Downloaded: {food.Name} VI");
                        }
                        else
                        {
                            result.SkippedCount++;
                            System.Diagnostics.Debug.WriteLine($"⏭️ [AudioDownload] Skipped: {food.Name} VI (already exists)");
                        }
                    }

                    // Download EN
                    if (!string.IsNullOrEmpty(food.AudioUrl_EN))
                    {
                        current++;
                        string message = $"[{current}/{total}] {food.Name} (EN)";
                        progressCallback?.Invoke(current, total, message);
                        
                        bool downloaded = await DownloadSingleAudioAsync(food.AudioUrl_EN, food.Id, "EN", food.Name);
                        if (downloaded)
                        {
                            result.DownloadedCount++;
                            System.Diagnostics.Debug.WriteLine($"✅ [AudioDownload] Downloaded: {food.Name} EN");
                        }
                        else
                        {
                            result.SkippedCount++;
                            System.Diagnostics.Debug.WriteLine($"⏭️ [AudioDownload] Skipped: {food.Name} EN (already exists)");
                        }
                    }

                    // Download CN
                    if (!string.IsNullOrEmpty(food.AudioUrl_CN))
                    {
                        current++;
                        string message = $"[{current}/{total}] {food.Name} (CN)";
                        progressCallback?.Invoke(current, total, message);
                        
                        bool downloaded = await DownloadSingleAudioAsync(food.AudioUrl_CN, food.Id, "CN", food.Name);
                        if (downloaded)
                        {
                            result.DownloadedCount++;
                            System.Diagnostics.Debug.WriteLine($"✅ [AudioDownload] Downloaded: {food.Name} CN");
                        }
                        else
                        {
                            result.SkippedCount++;
                            System.Diagnostics.Debug.WriteLine($"⏭️ [AudioDownload] Skipped: {food.Name} CN (already exists)");
                        }
                    }
                }

                result.IsSuccess = true;
                result.Message = $"✅ Hoàn thành! Downloaded: {result.DownloadedCount}, Skipped: {result.SkippedCount}";
                System.Diagnostics.Debug.WriteLine($"🎉 [AudioDownload] {result.Message}");
                progressCallback?.Invoke(total, total, result.Message);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"❌ Lỗi: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ [AudioDownload] Exception: {ex.Message}");
                progressCallback?.Invoke(0, 0, result.Message);
            }

            return result;
        }

        /// <summary>
        /// Download 1 file audio từ server
        /// </summary>
        private async Task<bool> DownloadSingleAudioAsync(string audioUrl, int foodId, string language, string foodName)
        {
            try
            {
                // Sanitize food name for filename
                string safeFoodName = "";
                if (!string.IsNullOrEmpty(foodName))
                {
                    safeFoodName = foodName
                        .Replace(" ", "_")
                        .Replace("/", "_")
                        .Replace("\\", "_")
                        .Replace(":", "_")
                        .Replace("*", "_")
                        .Replace("?", "_")
                        .Replace("\"", "_")
                        .Replace("<", "_")
                        .Replace(">", "_")
                        .Replace("|", "_");
                    safeFoodName = "_" + safeFoodName;
                }

                string fileName = $"{foodId}{safeFoodName}_{language}.mp3";
                string localPath = Path.Combine(_audioFolder, fileName);

                // Kiểm tra file đã tồn tại chưa
                if (File.Exists(localPath))
                {
                    System.Diagnostics.Debug.WriteLine($"⏭️ [AudioDownload] File already exists: {fileName}");
                    return false; // Skipped
                }

                // Tạo URL đầy đủ
                string fullUrl = audioUrl;
                if (!audioUrl.StartsWith("http"))
                {
                    // Relative URL → Convert to absolute
                    fullUrl = $"{AppConfig.ApiBaseUrl.Replace("/api.php", "")}{audioUrl}";
                }

                System.Diagnostics.Debug.WriteLine($"📥 [AudioDownload] Downloading: {fullUrl}");

                // Download file
                var response = await _httpClient.GetAsync(fullUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    File.WriteAllBytes(localPath, bytes);
                    
                    System.Diagnostics.Debug.WriteLine($"✅ [AudioDownload] Saved: {localPath} ({bytes.Length / 1024} KB)");
                    return true; // Downloaded
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [AudioDownload] HTTP {response.StatusCode}: {fullUrl}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [AudioDownload] Error downloading {audioUrl}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra và download các file audio mới (chạy định kỳ)
        /// </summary>
        public async Task<AudioDownloadResult> SyncNewAudioAsync()
        {
            System.Diagnostics.Debug.WriteLine($"🔄 [AudioDownload] Syncing new audio files...");
            return await DownloadAllAudioAsync();
        }

        /// <summary>
        /// Xóa tất cả file audio local (để re-download)
        /// </summary>
        public void ClearLocalAudio()
        {
            try
            {
                if (Directory.Exists(_audioFolder))
                {
                    var files = Directory.GetFiles(_audioFolder, "*.mp3");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                    System.Diagnostics.Debug.WriteLine($"🗑️ [AudioDownload] Deleted {files.Length} audio files");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [AudioDownload] Error clearing audio: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Kết quả download audio
    /// </summary>
    public class AudioDownloadResult
    {
        public bool IsSuccess { get; set; }
        public int DownloadedCount { get; set; }
        public int SkippedCount { get; set; }
        public string Message { get; set; }
    }
}
