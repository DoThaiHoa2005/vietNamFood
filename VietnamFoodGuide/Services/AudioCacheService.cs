using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service quản lý download & cache audio files cho từng quán ăn
    /// Download lần đầu khi đăng nhập → Lưu offline → Phát khi không có mạng
    /// </summary>
    public class AudioCacheService
    {
        private static AudioCacheService _instance;
        private static readonly object _lock = new object();
        
        private readonly HttpClient _httpClient;
        private readonly string _audioCacheDir;
        private System.Windows.Media.MediaPlayer _mediaPlayer;
        private bool _isPlaying = false;
        private System.Windows.Threading.DispatcherTimer _progressTimer;
        
        public event Action OnPlaybackStarted;
        public event Action OnPlaybackCompleted;
        public event Action<string> OnPlaybackError;
        public event Action<TimeSpan, TimeSpan> OnPlaybackProgress; // (current, total)
        
        public static AudioCacheService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AudioCacheService();
                        }
                    }
                }
                return _instance;
            }
        }
        
        private AudioCacheService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            // Thư mục cache audio: AppData/VietnamFoodGuide/AudioCache
            _audioCacheDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VietnamFoodGuide",
                "AudioCache"
            );
            
            if (!Directory.Exists(_audioCacheDir))
            {
                Directory.CreateDirectory(_audioCacheDir);
            }
            
            // Khởi tạo MediaPlayer trên UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                _mediaPlayer = new System.Windows.Media.MediaPlayer();
                _mediaPlayer.MediaEnded += (s, e) =>
                {
                    _isPlaying = false;
                    _progressTimer?.Stop();
                    OnPlaybackCompleted?.Invoke();
                    Debug.WriteLine("✅ [AudioCache] Playback completed");
                };
                _mediaPlayer.MediaFailed += (s, e) =>
                {
                    _isPlaying = false;
                    _progressTimer?.Stop();
                    OnPlaybackError?.Invoke(e.ErrorException?.Message ?? "Media playback failed");
                    Debug.WriteLine($"❌ [AudioCache] Playback failed: {e.ErrorException?.Message}");
                };
                
                // Timer để cập nhật progress
                _progressTimer = new System.Windows.Threading.DispatcherTimer();
                _progressTimer.Interval = TimeSpan.FromMilliseconds(100);
                _progressTimer.Tick += (s, e) =>
                {
                    if (_mediaPlayer.NaturalDuration.HasTimeSpan)
                    {
                        OnPlaybackProgress?.Invoke(_mediaPlayer.Position, _mediaPlayer.NaturalDuration.TimeSpan);
                    }
                };
            });
            
            Debug.WriteLine($"✅ [AudioCache] Initialized. Cache dir: {_audioCacheDir}");
        }
        
        /// <summary>
        /// Lấy đường dẫn file cache cho một quán ăn + ngôn ngữ
        /// Format: {foodId}_{lang}.mp3
        /// </summary>
        private string GetCacheFilePath(int foodId, string lang)
        {
            return Path.Combine(_audioCacheDir, $"{foodId}_{lang}.mp3");
        }
        
        /// <summary>
        /// Kiểm tra audio đã được cache chưa
        /// </summary>
        public bool HasCachedAudio(int foodId, string lang)
        {
            string path = GetCacheFilePath(foodId, lang);
            return File.Exists(path) && new FileInfo(path).Length > 0;
        }
        
        /// <summary>
        /// Lấy URL audio phù hợp theo ngôn ngữ
        /// </summary>
        public string GetAudioUrl(FoodItem food, string lang)
        {
            if (food == null) return null;
            
            switch (lang)
            {
                case "en": return food.AudioUrl_EN;
                case "zh": return food.AudioUrl_CN;
                default: return food.AudioUrl_VI;
            }
        }
        
        /// <summary>
        /// Kiểm tra xem quán ăn có audio cho ngôn ngữ hiện tại không
        /// </summary>
        public bool HasAudioForLanguage(FoodItem food, string lang)
        {
            string url = GetAudioUrl(food, lang);
            return !string.IsNullOrWhiteSpace(url);
        }
        
        /// <summary>
        /// Download tất cả audio files cho danh sách quán ăn (gọi khi đăng nhập lần đầu)
        /// </summary>
        public async Task DownloadAllAudioAsync(List<FoodItem> foods, Action<int, int> progressCallback = null)
        {
            if (foods == null || foods.Count == 0) return;
            
            int total = 0;
            int downloaded = 0;
            
            // Đếm tổng số file cần download
            foreach (var food in foods)
            {
                string[] langs = { "vi", "en", "zh" };
                foreach (var lang in langs)
                {
                    string url = GetAudioUrl(food, lang);
                    if (!string.IsNullOrWhiteSpace(url) && !HasCachedAudio(food.Id, lang))
                    {
                        total++;
                    }
                }
            }
            
            if (total == 0)
            {
                Debug.WriteLine("✅ [AudioCache] All audio files already cached");
                return;
            }
            
            Debug.WriteLine($"📥 [AudioCache] Need to download {total} audio files...");
            
            foreach (var food in foods)
            {
                string[] langs = { "vi", "en", "zh" };
                foreach (var lang in langs)
                {
                    string url = GetAudioUrl(food, lang);
                    if (!string.IsNullOrWhiteSpace(url) && !HasCachedAudio(food.Id, lang))
                    {
                        await DownloadAudioAsync(food.Id, lang, url);
                        downloaded++;
                        progressCallback?.Invoke(downloaded, total);
                    }
                }
            }
            
            Debug.WriteLine($"✅ [AudioCache] Downloaded {downloaded}/{total} audio files");
        }
        
        /// <summary>
        /// Download 1 file audio và lưu vào cache
        /// </summary>
        public async Task<bool> DownloadAudioAsync(int foodId, string lang, string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return false;
                
                // Nếu URL là relative, thêm base URL
                if (!url.StartsWith("http"))
                {
                    url = $"{AppConfig.ApiBaseUrl.Replace("api.php", "")}{url.TrimStart('/')}";
                }
                
                Debug.WriteLine($"📥 [AudioCache] Downloading: Food {foodId} ({lang}) from {url}");
                
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var audioData = await response.Content.ReadAsByteArrayAsync();
                    string cachePath = GetCacheFilePath(foodId, lang);
                    File.WriteAllBytes(cachePath, audioData);
                    Debug.WriteLine($"✅ [AudioCache] Saved: {cachePath} ({audioData.Length} bytes)");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"❌ [AudioCache] Download failed: HTTP {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [AudioCache] Download error: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Phát audio từ cache (offline) hoặc download rồi phát (online)
        /// Trả về true nếu phát thành công (có file audio), false nếu không có audio
        /// </summary>
        public async Task<bool> PlayAudioAsync(FoodItem food, string lang)
        {
            if (food == null || food.Id <= 0) return false;
            
            string audioUrl = GetAudioUrl(food, lang);
            
            // Không có audio URL cho ngôn ngữ này
            if (string.IsNullOrWhiteSpace(audioUrl)) return false;
            
            // Kiểm tra cache trước
            string cachePath = GetCacheFilePath(food.Id, lang);
            
            if (!File.Exists(cachePath))
            {
                // Chưa cache → thử download
                bool isOnline = await NetworkService.IsInternetAvailableAsync();
                if (isOnline)
                {
                    bool downloaded = await DownloadAudioAsync(food.Id, lang, audioUrl);
                    if (!downloaded) return false;
                }
                else
                {
                    Debug.WriteLine($"⚠️ [AudioCache] Offline and no cache for Food {food.Id} ({lang})");
                    return false;
                }
            }
            
            // Phát file từ cache
            PlayFromFile(cachePath);
            return true;
        }
        
        /// <summary>
        /// Phát file audio từ đường dẫn local
        /// </summary>
        private void PlayFromFile(string filePath)
        {
            Stop();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _isPlaying = true;
                    OnPlaybackStarted?.Invoke();
                    _mediaPlayer.Open(new Uri(filePath));
                    
                    // ⚡ Tăng tốc độ phát lên 1.25x (nhanh hơn 25% nhưng vẫn nghe rõ)
                    _mediaPlayer.SpeedRatio = 1.25;
                    
                    _mediaPlayer.Play();
                    _progressTimer?.Start();
                    Debug.WriteLine($"▶️ [AudioCache] Playing: {filePath} (Speed: 1.25x)");
                }
                catch (Exception ex)
                {
                    _isPlaying = false;
                    Debug.WriteLine($"❌ [AudioCache] Play error: {ex.Message}");
                    OnPlaybackError?.Invoke(ex.Message);
                }
            });
        }
        
        /// <summary>
        /// Dừng phát audio
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying) return;
            
            _isPlaying = false;
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _progressTimer?.Stop();
                    _mediaPlayer.Stop();
                    Debug.WriteLine("⏹️ [AudioCache] Stopped");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ [AudioCache] Stop error: {ex.Message}");
                }
            });
            
            OnPlaybackCompleted?.Invoke();
        }
        
        public bool IsPlaying => _isPlaying;
        
        /// <summary>
        /// Xóa tất cả audio cache
        /// </summary>
        public void ClearCache()
        {
            try
            {
                if (Directory.Exists(_audioCacheDir))
                {
                    foreach (var file in Directory.GetFiles(_audioCacheDir, "*.mp3"))
                    {
                        File.Delete(file);
                    }
                    Debug.WriteLine("🗑️ [AudioCache] Cache cleared");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [AudioCache] Clear cache error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Lấy kích thước cache (bytes)
        /// </summary>
        public long GetCacheSize()
        {
            long totalSize = 0;
            try
            {
                if (Directory.Exists(_audioCacheDir))
                {
                    foreach (var file in Directory.GetFiles(_audioCacheDir, "*.mp3"))
                    {
                        totalSize += new FileInfo(file).Length;
                    }
                }
            }
            catch { }
            return totalSize;
        }
    }
}
