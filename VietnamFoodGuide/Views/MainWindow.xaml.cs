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
        private List<FoodItem> allFoods;
        private string currentCategory = "Tất cả";

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
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
                MessageBox.Show("Không thể kết nối tới Server. App sẽ chạy ở chế độ Offline (Dữ liệu cục bộ).\nLưu ý: Tính năng Yêu thích sẽ không hoạt động.", 
                                "Mất kết nối", MessageBoxButton.OK, MessageBoxImage.Warning);
                allFoods = fallbackService.LoadFoods();
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
                currentCategory = btn.Content.ToString();
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
    }
}
