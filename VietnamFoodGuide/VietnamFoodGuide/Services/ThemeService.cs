using System;
using System.Windows;
using System.Windows.Media;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service quản lý Dark/Light Mode toàn ứng dụng
    /// </summary>
    public class ThemeService
    {
        private static ThemeService _instance;
        public static ThemeService Instance => _instance ?? (_instance = new ThemeService());

        private bool _isDarkMode = false;

        /// <summary>
        /// Trạng thái Dark Mode hiện tại
        /// </summary>
        public bool IsDarkMode => _isDarkMode;

        /// <summary>
        /// Event kích hoạt khi theme thay đổi
        /// </summary>
        public event EventHandler<bool> ThemeChanged;

        private ThemeService()
        {
            // Load saved preference
            try
            {
                string saved = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "theme.dat");
                if (System.IO.File.Exists(saved))
                {
                    _isDarkMode = System.IO.File.ReadAllText(saved).Trim() == "dark";
                }
            }
            catch { }
        }

        /// <summary>
        /// Chuyển đổi theme và cập nhật toàn bộ app
        /// </summary>
        public void Toggle()
        {
            _isDarkMode = !_isDarkMode;
            ApplyTheme();
            SavePreference();
            ThemeChanged?.Invoke(this, _isDarkMode);
        }

        /// <summary>
        /// Áp dụng theme hiện tại vào Application.Resources
        /// </summary>
        public void ApplyTheme()
        {
            var res = Application.Current.Resources;

            if (_isDarkMode)
            {
                // === DARK MODE ===
                res["BackgroundColor"]    = Color.FromRgb(0x12, 0x12, 0x1A);
                res["SurfaceColor"]       = Color.FromRgb(0x1E, 0x1E, 0x2E);
                res["TextPrimary"]        = Color.FromRgb(0xE8, 0xE8, 0xF0);
                res["TextSecondary"]      = Color.FromRgb(0xA0, 0xA0, 0xB8);
                res["BorderColor"]        = Color.FromRgb(0x33, 0x33, 0x4E);

                res["BackgroundBrush"]    = new SolidColorBrush(Color.FromRgb(0x12, 0x12, 0x1A));
                res["SurfaceBrush"]       = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));
                res["TextPrimaryBrush"]   = new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xF0));
                res["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xB8));
                res["BorderBrush"]        = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x4E));
            }
            else
            {
                // === LIGHT MODE ===
                res["BackgroundColor"]    = Color.FromRgb(0xF8, 0xF9, 0xFA);
                res["SurfaceColor"]       = Color.FromRgb(0xFF, 0xFF, 0xFF);
                res["TextPrimary"]        = Color.FromRgb(0x1D, 0x35, 0x57);
                res["TextSecondary"]      = Color.FromRgb(0x55, 0x55, 0x55);
                res["BorderColor"]        = Color.FromRgb(0xE0, 0xE0, 0xE0);

                res["BackgroundBrush"]    = new SolidColorBrush(Color.FromRgb(0xF8, 0xF9, 0xFA));
                res["SurfaceBrush"]       = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                res["TextPrimaryBrush"]   = new SolidColorBrush(Color.FromRgb(0x1D, 0x35, 0x57));
                res["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                res["BorderBrush"]        = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
            }

            // Force update all open windows
            foreach (Window w in Application.Current.Windows)
            {
                w.Background = (SolidColorBrush)res["BackgroundBrush"];
            }
        }

        private void SavePreference()
        {
            try
            {
                string saved = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "theme.dat");
                System.IO.File.WriteAllText(saved, _isDarkMode ? "dark" : "light");
            }
            catch { }
        }
    }
}
