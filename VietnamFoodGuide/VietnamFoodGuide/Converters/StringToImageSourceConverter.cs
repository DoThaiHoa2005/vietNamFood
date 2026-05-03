using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace VietnamFoodGuide.Converters
{
    /// <summary>
    /// Converter để chuyển đổi string path thành ImageSource
    /// Hỗ trợ 4 loại path: URL web, đường dẫn tuyệt đối, tương đối, và pack resource
    /// </summary>
    public class StringToImageSourceConverter : IValueConverter
    {
        private static BitmapImage _placeholderImage;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value?.ToString()))
            {
                System.Diagnostics.Debug.WriteLine("[ImageConverter] ❌ Value is null or empty");
                return LoadPlaceholder();
            }

            string path = value.ToString().Trim();
            System.Diagnostics.Debug.WriteLine($"[ImageConverter] 📥 Input path: {path}");

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;

                // ✅ CASE 1: URL từ server (http:// hoặc https://)
                if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                    path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"[ImageConverter] 🌐 Loading from URL: {path}");
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                }
                // ✅ CASE 2: Đường dẫn tuyệt đối local (C:\..., D:\...)
                else if (Path.IsPathRooted(path) && File.Exists(path))
                {
                    System.Diagnostics.Debug.WriteLine($"[ImageConverter] 📂 Loading from absolute path: {path}");
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                }
                // ✅ CASE 3: Đường dẫn tương đối → chuyển thành tuyệt đối
                else if (!path.StartsWith("pack://"))
                {
                    string cleanPath = path.TrimStart('/', '\\');
                    string fullPath = Path.Combine(AppContext.BaseDirectory, cleanPath);
                    
                    System.Diagnostics.Debug.WriteLine($"[ImageConverter] 🔍 Checking relative path: {fullPath}");
                    
                    if (File.Exists(fullPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"[ImageConverter] ✅ File found! Loading: {fullPath}");
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ImageConverter] ⚠️ File not found at: {fullPath}");
                        System.Diagnostics.Debug.WriteLine($"[ImageConverter] 📦 Fallback to pack resource: {cleanPath}");
                        bitmap.UriSource = new Uri($"pack://application:,,,/{cleanPath}", UriKind.Absolute);
                    }
                }
                else
                {
                    // Đã là pack:// URI
                    System.Diagnostics.Debug.WriteLine($"[ImageConverter] 📦 Already pack URI: {path}");
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                }

                bitmap.EndInit();
                bitmap.Freeze(); // Thread-safe
                System.Diagnostics.Debug.WriteLine($"[ImageConverter] ✅ Image loaded successfully");
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageConverter] ❌ Error loading image: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ImageConverter] ❌ Stack trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"[ImageConverter] ❌ Path: {path}");
                
                // ✅ FALLBACK: Trả về placeholder image
                return LoadPlaceholder();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Load placeholder image khi ảnh chính không load được
        /// </summary>
        private static BitmapImage LoadPlaceholder()
        {
            if (_placeholderImage != null)
                return _placeholderImage;

            try
            {
                _placeholderImage = new BitmapImage();
                _placeholderImage.BeginInit();
                _placeholderImage.CacheOption = BitmapCacheOption.OnLoad;
                
                // Thử load placeholder từ Assets
                string placeholderPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "placeholder.png");
                if (File.Exists(placeholderPath))
                {
                    _placeholderImage.UriSource = new Uri(placeholderPath, UriKind.Absolute);
                }
                else
                {
                    // Fallback: tạo placeholder đơn giản (1x1 gray pixel)
                    _placeholderImage = null;
                    return null;
                }
                
                _placeholderImage.EndInit();
                _placeholderImage.Freeze();
                
                System.Diagnostics.Debug.WriteLine("[ImageConverter] Placeholder loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageConverter] Error loading placeholder: {ex.Message}");
                _placeholderImage = null;
            }

            return _placeholderImage;
        }
    }
}
