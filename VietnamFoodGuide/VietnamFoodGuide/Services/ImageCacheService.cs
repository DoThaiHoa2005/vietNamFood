using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service để cache ảnh từ URL server về local AppData
    /// Hỗ trợ offline-first: dùng cached image khi không có mạng
    /// </summary>
    public class ImageCacheService
    {
        private static ImageCacheService _instance;
        private static readonly object _lock = new object();
        
        private readonly string _cacheDirectory;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _cacheMap; // URL -> LocalPath

        public static ImageCacheService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ImageCacheService();
                        }
                    }
                }
                return _instance;
            }
        }

        private ImageCacheService()
        {
            // Cache directory: AppData/VietnamFoodGuide/ImageCache/
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _cacheDirectory = Path.Combine(appData, "VietnamFoodGuide", "ImageCache");
            
            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
                System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Created cache directory: {_cacheDirectory}");
            }

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VietnamFoodGuide/1.0");

            _cacheMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// Lấy đường dẫn cached của ảnh. Nếu chưa có, download về cache.
        /// </summary>
        /// <param name="imageUrl">URL hoặc path của ảnh</param>
        /// <param name="forceDownload">Bắt buộc download lại dù đã có cache</param>
        /// <returns>Đường dẫn local của ảnh (cached hoặc original)</returns>
        public async Task<string> GetCachedPathAsync(string imageUrl, bool forceDownload = false)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return imageUrl;

            // Nếu không phải URL web, trả về nguyên path
            if (!imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return imageUrl;
            }

            // Kiểm tra cache map trong memory
            if (!forceDownload && _cacheMap.ContainsKey(imageUrl))
            {
                string cachedPath = _cacheMap[imageUrl];
                if (File.Exists(cachedPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Using cached image: {cachedPath}");
                    return cachedPath;
                }
            }

            // Tính cache path từ URL
            string localPath = GetCacheFilePath(imageUrl);

            // Nếu file đã tồn tại và không bắt buộc download lại
            if (!forceDownload && File.Exists(localPath))
            {
                _cacheMap[imageUrl] = localPath;
                System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Using existing cache: {localPath}");
                return localPath;
            }

            // Download ảnh về cache
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Downloading image: {imageUrl}");
                
                byte[] imageData = await _httpClient.GetByteArrayAsync(imageUrl);
                
                // .NET Framework 4.8 doesn't have WriteAllBytesAsync, use synchronous version
                File.WriteAllBytes(localPath, imageData);
                
                _cacheMap[imageUrl] = localPath;
                System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Downloaded and cached: {localPath}");
                
                return localPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Error downloading image: {ex.Message}");
                
                // Nếu download thất bại nhưng có cache cũ, dùng cache cũ
                if (File.Exists(localPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Using old cache after download failed");
                    return localPath;
                }
                
                // Nếu không có cache, trả về URL gốc (để converter xử lý)
                return imageUrl;
            }
        }

        /// <summary>
        /// Pre-download nhiều ảnh cùng lúc (dùng khi có mạng)
        /// </summary>
        public async Task<int> PreDownloadImagesAsync(List<string> imageUrls, IProgress<(int current, int total, string url)> progress = null)
        {
            int downloaded = 0;
            int total = imageUrls.Count;

            for (int i = 0; i < imageUrls.Count; i++)
            {
                string url = imageUrls[i];
                
                if (string.IsNullOrEmpty(url))
                    continue;

                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    progress?.Report((i + 1, total, url));
                    
                    string localPath = await GetCachedPathAsync(url, forceDownload: false);
                    
                    if (File.Exists(localPath))
                    {
                        downloaded++;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Error pre-downloading {url}: {ex.Message}");
                }

                // Delay nhỏ để không quá tải server
                await Task.Delay(100);
            }

            System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Pre-downloaded {downloaded}/{total} images");
            return downloaded;
        }

        /// <summary>
        /// Tính cache file path từ URL (dùng MD5 hash)
        /// </summary>
        private string GetCacheFilePath(string url)
        {
            // Hash URL thành tên file
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(url));
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                
                // Lấy extension từ URL
                string extension = Path.GetExtension(url.Split('?')[0]); // Bỏ query string
                if (string.IsNullOrEmpty(extension) || extension.Length > 5)
                {
                    extension = ".jpg"; // Default extension
                }
                
                return Path.Combine(_cacheDirectory, hash + extension);
            }
        }

        /// <summary>
        /// Xóa cache cũ (files không được truy cập trong X ngày)
        /// </summary>
        public void CleanOldCache(int daysOld = 30)
        {
            try
            {
                var files = Directory.GetFiles(_cacheDirectory);
                int deleted = 0;
                DateTime threshold = DateTime.Now.AddDays(-daysOld);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastAccessTime < threshold)
                    {
                        File.Delete(file);
                        deleted++;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Cleaned {deleted} old cache files");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Error cleaning cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy kích thước cache hiện tại (MB)
        /// </summary>
        public double GetCacheSizeMB()
        {
            try
            {
                var files = Directory.GetFiles(_cacheDirectory);
                long totalBytes = 0;

                foreach (var file in files)
                {
                    totalBytes += new FileInfo(file).Length;
                }

                return totalBytes / (1024.0 * 1024.0);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Xóa toàn bộ cache
        /// </summary>
        public void ClearAllCache()
        {
            try
            {
                var files = Directory.GetFiles(_cacheDirectory);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                
                _cacheMap.Clear();
                System.Diagnostics.Debug.WriteLine("[ImageCacheService] Cleared all cache");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCacheService] Error clearing cache: {ex.Message}");
            }
        }
    }
}
