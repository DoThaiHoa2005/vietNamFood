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
        private readonly FoodService service = new FoodService();
        private List<FoodItem> allFoods;
        private string currentCategory = "Tất cả";

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            allFoods = service.LoadFoods();
            // Nếu allFoods null, khởi tạo danh sách rỗng để tránh lỗi Crash
            if (allFoods == null) allFoods = new List<FoodItem>();

            FoodList.ItemsSource = allFoods;
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
                // 1. Kiểm tra từ khóa tìm kiếm (Tên hoặc Thành phố)
                bool matchesKeyword = string.IsNullOrEmpty(keyword) ||
                                     (f.Name != null && f.Name.ToLower().Contains(keyword)) ||
                                     (f.City != null && f.City.ToLower().Contains(keyword));

                // 2. Kiểm tra thể loại (Category)
                bool matchesCategory = true;
                if (currentCategory != "Tất cả")
                {
                    // SỬA LỖI Ở ĐÂY: Lấy toàn bộ chuỗi sau dấu cách đầu tiên (bỏ Emoji)
                    string categoryName = currentCategory;
                    int spaceIndex = currentCategory.IndexOf(' ');

                    if (spaceIndex >= 0)
                    {
                        // Ví dụ: "🥮 Bánh Khác" -> "Bánh Khác"
                        categoryName = currentCategory.Substring(spaceIndex + 1).Trim();
                    }

                    // So sánh không phân biệt hoa thường để an toàn nhất
                    matchesCategory = f.Category != null &&
                                      f.Category.ToLower().Contains(categoryName.ToLower());
                }

                return matchesKeyword && matchesCategory;
            }).ToList();

            FoodList.ItemsSource = result;
        }

        private void OpenDetail(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is FoodItem selectedFood)
            {
                // Mở cửa sổ chi tiết (Đảm bảo bạn đã có class FoodDetailWindow)
                FoodDetailWindow detailWindow = new FoodDetailWindow(selectedFood);
                detailWindow.Show();
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