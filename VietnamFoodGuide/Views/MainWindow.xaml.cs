using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VietnamFoodGuide.Models;
using VietnamFoodGuide.Services;

namespace VietnamFoodGuide.Views
{
    public partial class MainWindow : Window
    {
        private readonly ApiFoodService apiService = new ApiFoodService();
        private readonly FavoritesApiService favoritesService = new FavoritesApiService();
        private readonly LanguageService lang = LanguageService.Instance;
        private List<FoodItem> allFoods;
        private string currentCategory = "Tất cả";

        public MainWindow()
        {
            InitializeComponent();
            InitializeLanguage();
            LoadData();
            
            // Bắt đầu background sync
            BackgroundSyncService.Instance.Start();
            
            // Subscribe to language changes
            lang.LanguageChanged += (s, e) => UpdateUILanguage();
            
            // Check QR scan status and show notification if needed
            CheckQRScanStatus();
            
            // Apply saved theme
            ThemeService.Instance.ApplyTheme();
            UpdateToggleUI(ThemeService.Instance.IsDarkMode);
        }

        private void InitializeLanguage()
        {
            // Set ComboBox to current language
            var currentLang = lang.CurrentLanguage;
            foreach (ComboBoxItem item in CmbLanguage.Items)
            {
                if (item.Tag.ToString() == currentLang)
                {
                    CmbLanguage.SelectedItem = item;
                    break;
                }
            }
            
            // If no match, default to Vietnamese
            if (CmbLanguage.SelectedItem == null)
            {
                CmbLanguage.SelectedIndex = 0;
            }
            
            UpdateUILanguage();
        }

        // ===== DARK MODE =====
        private void DarkModeToggle_Click(object sender, MouseButtonEventArgs e)
        {
            ThemeService.Instance.Toggle();
            UpdateToggleUI(ThemeService.Instance.IsDarkMode);
        }

        private void UpdateToggleUI(bool isDark)
        {
            if (ToggleThumb == null) return;

            if (isDark)
            {
                // Dark ON → thumb right, bright green
                ToggleThumb.HorizontalAlignment = HorizontalAlignment.Right;
                ToggleThumb.Margin = new Thickness(0, 0, 2, 0);
                ToggleTrack.Background = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)); // dark green active
                TxtDarkModeIcon.Text = "🌙";
                TxtDarkModeIcon.HorizontalAlignment = HorizontalAlignment.Left;
                TxtDarkModeIcon.Margin = new Thickness(3, 0, 0, 0);
            }
            else
            {
                // Light OFF → thumb left, muted green
                ToggleThumb.HorizontalAlignment = HorizontalAlignment.Left;
                ToggleThumb.Margin = new Thickness(2, 0, 0, 0);
                ToggleTrack.Background = new SolidColorBrush(Color.FromArgb(0x60, 0xFF, 0xFF, 0xFF)); // translucent on red bar
                TxtDarkModeIcon.Text = "☀️";
                TxtDarkModeIcon.HorizontalAlignment = HorizontalAlignment.Right;
                TxtDarkModeIcon.Margin = new Thickness(0, 0, 3, 0);
            }
        }

        private void UpdateUILanguage()
        {
            // Update user status
            if (TxtUserStatus != null)
            {
                if (App.CurrentApiUser != null)
                {
                    TxtUserStatus.Text = $"👤 {App.CurrentApiUser.Username}";
                }
                else
                {
                    TxtUserStatus.Text = $"👤 {lang["guest"]}";
                }
            }
            
            // Update header subtitle
            if (TxtAppSubtitle != null)
            {
                TxtAppSubtitle.Text = lang["app_subtitle"];
            }
            
            // Update search placeholder
            if (TxtSearchPlaceholder != null)
            {
                TxtSearchPlaceholder.Text = lang["search_placeholder"];
            }
            
            // Update categories label
            if (TxtCategoriesLabel != null)
            {
                TxtCategoriesLabel.Text = lang["categories"];
            }
            
            // Update category buttons
            if (RbAllCategories != null)
            {
                RbAllCategories.Content = lang["all_categories"];
            }
            if (RbHaiSan != null)
            {
                RbHaiSan.Content = lang["cat_haisan"];
            }
            if (RbOc != null)
            {
                RbOc.Content = lang["cat_oc"];
            }
            if (RbBun != null)
            {
                RbBun.Content = lang["cat_bun"];
            }
            if (RbNuong != null)
            {
                RbNuong.Content = lang["cat_nuong"];
            }
            if (RbLauNuong != null)
            {
                RbLauNuong.Content = lang["cat_lau_nuong"];
            }
            
            // Update empty state
            if (TxtNoResults != null)
            {
                TxtNoResults.Text = lang["no_results"];
            }
            if (TxtTryDifferentSearch != null)
            {
                TxtTryDifferentSearch.Text = lang["try_different_search"];
            }
            
            // Update bottom navigation
            if (TxtNavHome != null)
            {
                TxtNavHome.Text = lang["home"];
            }
            if (TxtNavQRScanner != null)
            {
                TxtNavQRScanner.Text = lang.CurrentLanguage == "vi" ? "Quét QR" :
                                       lang.CurrentLanguage == "en" ? "QR Scan" : "扫码";
            }
            if (TxtNavFavorites != null)
            {
                TxtNavFavorites.Text = lang["favorites"];
            }
            if (TxtNavAccount != null)
            {
                TxtNavAccount.Text = lang["account"];
            }
            
            // Update QR notification banner
            if (TxtQRTitle != null)
            {
                TxtQRTitle.Text = lang.CurrentLanguage == "vi" ? 
                    "📱 Quét mã QR để trải nghiệm đầy đủ" :
                    lang.CurrentLanguage == "en" ?
                    "📱 Scan QR for Full Experience" :
                    "📱 扫描二维码获得完整体验";
            }
            if (TxtQRMessage != null)
            {
                TxtQRMessage.Text = lang.CurrentLanguage == "vi" ?
                    "Mở khóa tất cả tính năng!" :
                    lang.CurrentLanguage == "en" ?
                    "Unlock all features!" :
                    "解锁所有功能！";
            }
            if (BtnScanQR != null)
            {
                BtnScanQR.Content = lang.CurrentLanguage == "vi" ?
                    "Quét" :
                    lang.CurrentLanguage == "en" ?
                    "Scan" :
                    "扫描";
            }
            
            // Update "View Details" button text for all food items
            if (allFoods != null)
            {
                foreach (var food in allFoods)
                {
                    food.ViewDetailsText = lang["view_details"];
                }
            }
            
            // Refresh food list to update any displayed text
            if (allFoods != null)
            {
                ApplyFilters();
            }
        }

        private void CheckQRScanStatus()
        {
            try
            {
                var storageService = new StorageService();
                bool hasScanned = storageService.HasScannedQR();
                
                if (!hasScanned)
                {
                    // Show QR notification banner
                    QRNotificationBanner.Visibility = Visibility.Visible;
                    QRBannerSpacer.Visibility = Visibility.Visible;
                }
                else
                {
                    // Hide QR notification banner
                    QRNotificationBanner.Visibility = Visibility.Collapsed;
                    QRBannerSpacer.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking QR status: {ex.Message}");
                // If error, don't show banner
                QRNotificationBanner.Visibility = Visibility.Collapsed;
                QRBannerSpacer.Visibility = Visibility.Collapsed;
            }
        }

        private void ScanQR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open QR Scanner Window and close MainWindow
                var qrWindow = new QRScannerWindow();
                qrWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageDialog.ShowError($"Lỗi mở QR Scanner: {ex.Message}", "Lỗi");
            }
        }

        private void DismissQR_Click(object sender, RoutedEventArgs e)
        {
            // Hide the banner (user can scan later from Account section)
            QRNotificationBanner.Visibility = Visibility.Collapsed;
            QRBannerSpacer.Visibility = Visibility.Collapsed;
        }

        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbLanguage.SelectedItem is ComboBoxItem selected)
            {
                var newLang = selected.Tag.ToString();
                lang.CurrentLanguage = newLang;
            }
        }

        private async void LoadData()
        {
            try
            {
                // Load từ API (tự động fallback về SQLite nếu API lỗi)
                allFoods = await apiService.LoadFoodsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load dữ liệu: {ex.Message}");
                MessageDialog.ShowWarning($"{lang["connection_error"]}. {lang["offline_mode"]}", 
                                lang["connection_lost"]);
                allFoods = new List<FoodItem>(); // Empty list nếu lỗi hoàn toàn
            }

            // Set ViewDetailsText for all items
            if (allFoods != null)
            {
                foreach (var food in allFoods)
                {
                    food.ViewDetailsText = lang["view_details"];
                }
            }

            FoodList.ItemsSource = allFoods;
            UpdateEmptyState();

            // Nếu đã đăng nhập, load trạng thái yêu thích
            if (App.CurrentApiUser != null)
            {
                await SyncFavoritesAsync();
            }
        }

        private async System.Threading.Tasks.Task SyncFavoritesAsync()
        {
            try
            {
                if (App.CurrentApiUser == null || allFoods == null) return;

                var favorites = await favoritesService.GetUserFavoritesAsync(App.CurrentApiUser.Id);
                if (favorites != null)
                {
                    var favIds = new HashSet<int>(favorites.Select(f => f.Id));
                    foreach (var food in allFoods)
                    {
                        food.IsFavorite = favIds.Contains(food.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error syncing favorites: {ex.Message}");
            }
        }

        private void SearchFood(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterCategory(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton btn)
            {
                // Use Tag instead of Content for filtering (Tag contains Vietnamese category name)
                currentCategory = btn.Tag?.ToString() ?? "Tất cả";
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            if (allFoods == null) return;

            string keyword = SearchBox.Text.ToLower().Trim();

            var result = allFoods.Where(f => {
                bool matchesKeyword = string.IsNullOrEmpty(keyword) ||
                                     (f.Name != null && f.Name.ToLower().Contains(keyword)) ||
                                     (f.City != null && f.City.ToLower().Contains(keyword));

                bool matchesCategory = true;
                if (currentCategory != "Tất cả")
                {
                    string categoryName = currentCategory;
                    int spaceIndex = currentCategory.IndexOf(' ');

                    if (spaceIndex >= 0)
                    {
                        categoryName = currentCategory.Substring(spaceIndex + 1).Trim();
                    }

                    matchesCategory = f.Category != null &&
                                      f.Category.ToLower().Contains(categoryName.ToLower());
                }

                return matchesKeyword && matchesCategory;
            }).ToList();

            FoodList.ItemsSource = result;
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            var items = FoodList.ItemsSource as List<FoodItem>;
            EmptyState.Visibility = (items == null || items.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OpenDetail(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is FoodItem selectedFood)
            {
                FoodDetailWindow detailWindow = new FoodDetailWindow(selectedFood);
                detailWindow.Show();
                this.Close(); // Đóng MainWindow khi mở FoodDetailWindow
            }
        }

        private async void ToggleFavoriteInList(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is FoodItem food)
            {
                if (App.CurrentApiUser == null)
                {
                    MessageDialog.ShowInformation("Vui lòng đăng nhập để sử dụng tính năng yêu thích!", "Thông báo");
                    return;
                }

                try
                {
                    btn.IsEnabled = false; // Tránh bấm nhiều lần
                    
                    var storageService = new StorageService();
                    bool isOnline = await NetworkService.IsInternetAvailableAsync();
                    
                    if (food.IsFavorite)
                    {
                        // Đang thích -> Bỏ thích (Offline-First)
                        storageService.RemoveFavorite(food.Name);
                        food.IsFavorite = false;
                        
                        if (isOnline)
                        {
                            _ = favoritesService.RemoveFavoriteAsync(App.CurrentApiUser.Id, food.Id);
                        }
                    }
                    else
                    {
                        // Chưa thích -> Thêm thích (Offline-First)
                        storageService.AddFavorite(food);
                        food.IsFavorite = true;
                        
                        if (isOnline)
                        {
                            var result = await favoritesService.AddFavoriteAsync(App.CurrentApiUser.Id, food.Id);
                            if (result.success)
                            {
                                var sqliteFavorites = new SQLiteFavoritesService();
                                sqliteFavorites.MarkAsSynced(App.CurrentApiUser.Id, food.Id);
                            }
                        }
                    }
                    
                    // Kích hoạt sync background
                    if (isOnline)
                    {
                        _ = BackgroundSyncService.Instance.SyncNowAsync();
                    }
                    
                    // Refresh FoodList to update UI
                    FoodList.Items.Refresh();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error toggling favorite in list: {ex.Message}");
                }
                finally
                {
                    btn.IsEnabled = true;
                }
            }
        }

        private void CategoryScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            if (e.Delta < 0)
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 40);
            else if (e.Delta > 0)
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 40);

            e.Handled = true;
        }

        private void FocusSearch(object sender, RoutedEventArgs e)
        {
            SearchBox?.Focus();
            SearchBox?.SelectAll();
        }

        private void OpenQRScanner(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open QR Scanner Window and close MainWindow
                var qrWindow = new QRScannerWindow();
                qrWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageDialog.ShowError($"Lỗi mở QR Scanner: {ex.Message}", "Lỗi");
            }
        }

        private void OpenFavorites(object sender, RoutedEventArgs e)
        {
            if (App.CurrentApiUser == null)
            {
                MessageDialog.ShowInformation("Vui lòng đăng nhập để xem danh sách yêu thích!", "Thông báo");
                return;
            }
            var favWin = new FavoritesWindow();
            favWin.Show();
            this.Close();
        }

        private void OpenProfile(object sender, RoutedEventArgs e)
        {
            if (App.CurrentApiUser != null)
            {
                var dialog = new AccountDialog(App.CurrentApiUser.Username);
                if (dialog.ShowDialog() == true && dialog.ShouldLogout)
                {
                    // Logout: Xóa UserTracking và Session
                    var userToLogout = App.CurrentApiUser;
                    if (userToLogout != null)
                    {
                        _ = System.Threading.Tasks.Task.Run(async () =>
                        {
                            try
                            {
                                using (var client = new System.Net.Http.HttpClient())
                                {
                                    client.Timeout = TimeSpan.FromSeconds(5);
                                    
                                    // ✅ Gửi cả token và userId để xóa session và UserTracking
                                    var payload = System.Text.Json.JsonSerializer.Serialize(new { 
                                        token = App.SessionToken,
                                        userId = userToLogout.Id 
                                    });
                                    var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                                    await client.PostAsync($"{VietnamFoodGuide.Services.AppConfig.ApiBaseUrl}?action=logout", content);
                                    System.Diagnostics.Debug.WriteLine($"✅ [Logout] Đã xóa session và UserTracking cho user {userToLogout.Username}");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ [Logout] Lỗi logout: {ex.Message}");
                            }
                        });
                    }

                    // Clear current user
                    App.CurrentApiUser = null;
                    App.SessionToken = null;
                    
                    // Show login window immediately
                    var login = new LoginWindow(App.DbContext);
                    if (login.ShowDialog() == true)
                    {
                        // User logged in successfully
                        if (TxtUserStatus != null && App.CurrentApiUser != null)
                            TxtUserStatus.Text = $"👤 {App.CurrentApiUser.Username}";
                    }
                    else
                    {
                        // User cancelled login - update status to guest
                        var lang = LanguageService.Instance;
                        if (TxtUserStatus != null)
                            TxtUserStatus.Text = $"👤 {lang["guest"]}";
                    }
                }
            }
            else
            {
                var login = new LoginWindow(App.DbContext);
                if (login.ShowDialog() == true)
                {
                    if (TxtUserStatus != null && App.CurrentApiUser != null)
                        TxtUserStatus.Text = $"👤 {App.CurrentApiUser.Username}";
                }
            }
        }
    }
}
