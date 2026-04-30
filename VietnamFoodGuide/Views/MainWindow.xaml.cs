using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VietnamFoodGuide.Models;
using VietnamFoodGuide.Services;

namespace VietnamFoodGuide.Views
{
    public partial class MainWindow : Window
    {
        private readonly ApiFoodService apiService = new ApiFoodService();
        private readonly FoodService fallbackService = new FoodService();
        private readonly FavoritesApiService favoritesService = new FavoritesApiService();
        private readonly LanguageService lang = LanguageService.Instance;
        private List<FoodItem> allFoods;
        private string currentCategory = "Tất cả";

        public MainWindow()
        {
            InitializeComponent();
            InitializeLanguage();
            LoadData();
            
            // Subscribe to language changes
            lang.LanguageChanged += (s, e) => UpdateUILanguage();
            
            // Check QR scan status and show notification if needed
            CheckQRScanStatus();
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
            if (RbPho != null)
            {
                RbPho.Content = lang["cat_pho"];
            }
            if (RbBun != null)
            {
                RbBun.Content = lang["cat_bun"];
            }
            if (RbCom != null)
            {
                RbCom.Content = lang["cat_com"];
            }
            if (RbBanhMi != null)
            {
                RbBanhMi.Content = lang["cat_banh_mi"];
            }
            if (RbBanhKhac != null)
            {
                RbBanhKhac.Content = lang["cat_banh_khac"];
            }
            if (RbThucUong != null)
            {
                RbThucUong.Content = lang["cat_thuc_uong"];
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
                // Open QR Scanner Window
                var qrWindow = new QRScannerWindow();
                qrWindow.Owner = this;
                qrWindow.ShowDialog();
                
                // After QR scanner closes, check status again
                CheckQRScanStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở QR Scanner: {ex.Message}", "Lỗi", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
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
                // Thử load từ API trước (MySQL qua XAMPP)
                allFoods = await apiService.LoadFoodsAsync();
                
                if (allFoods == null || allFoods.Count == 0)
                {
                    // Fallback về JSON local
                    allFoods = fallbackService.LoadFoods();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load từ API: {ex.Message}");
                MessageBox.Show($"{lang["connection_error"]}. {lang["offline_mode"]}", 
                                lang["connection_lost"], MessageBoxButton.OK, MessageBoxImage.Warning);
                allFoods = fallbackService.LoadFoods();
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
            }
        }

        private async void ToggleFavoriteInList(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is FoodItem food)
            {
                if (App.CurrentApiUser == null)
                {
                    MessageBox.Show("Vui lòng đăng nhập để sử dụng tính năng yêu thích!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                try
                {
                    btn.IsEnabled = false; // Tránh bấm nhiều lần
                    
                    if (food.IsFavorite)
                    {
                        // Đang thích -> Bỏ thích
                        var result = await favoritesService.RemoveFavoriteAsync(App.CurrentApiUser.Id, food.Id);
                        if (result.success)
                        {
                            food.IsFavorite = false;
                        }
                    }
                    else
                    {
                        // Chưa thích -> Thêm thích
                        var result = await favoritesService.AddFavoriteAsync(App.CurrentApiUser.Id, food.Id);
                        if (result.success)
                        {
                            food.IsFavorite = true;
                        }
                    }
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
                // Open QR Scanner Window
                var qrWindow = new QRScannerWindow();
                qrWindow.Owner = this;
                qrWindow.ShowDialog();
                
                // After QR scanner closes, check status again
                CheckQRScanStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở QR Scanner: {ex.Message}", "Lỗi", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenFavorites(object sender, RoutedEventArgs e)
        {
            if (App.CurrentApiUser == null)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem danh sách yêu thích!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var favWin = new FavoritesWindow();
            favWin.ShowDialog();
        }

        private void OpenProfile(object sender, RoutedEventArgs e)
        {
            if (App.CurrentApiUser != null)
            {
                var dialog = new AccountDialog(App.CurrentApiUser.Username);
                if (dialog.ShowDialog() == true && dialog.ShouldLogout)
                {
                    App.CurrentApiUser = null;
                    var lang = LanguageService.Instance;
                    if (TxtUserStatus != null)
                        TxtUserStatus.Text = $"👤 {lang["guest"]}";
                    MessageBox.Show(lang["logout_success"], lang["account"], 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
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
