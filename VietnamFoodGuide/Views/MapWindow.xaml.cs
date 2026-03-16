using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Views
{
    public partial class MapWindow : Window
    {
        private readonly FoodItem _food;

        // CHỈ GIỮ LẠI MỘT CONSTRUCTOR DUY NHẤT
        public MapWindow(FoodItem food)
        {
            InitializeComponent();
            _food = food ?? throw new ArgumentNullException(nameof(food));

            // 1. Gán dữ liệu cơ bản
            TxtName.Text = _food.Name;
            TxtCity.Text = _food.City;

            // 2. Gọi hàm gán đánh giá sao
            SetRating(_food.Rating);

            // 3. Hiệu ứng Fade-in
            this.Opacity = 0;
            this.Loaded += (s, e) => {
                var anim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                this.BeginAnimation(Window.OpacityProperty, anim);
            };

            // 4. Gọi hàm khởi tạo WebView
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                await MapBrowser.EnsureCoreWebView2Async(null);
                MapBrowser.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";

                // URL Google Maps nhúng
                string url = $"https://www.google.com/maps/search/?api=1&query={_food.Latitude},{_food.Longitude}";
                MapBrowser.Source = new Uri(url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khởi tạo bản đồ: {ex.Message}");
            }
        }

        public void SetRating(double rating)
        {
            // Logic tính sao không dùng Math.Clamp để tránh lỗi version cũ
            double floorRating = Math.Floor(rating);
            int fullStars = (int)(floorRating < 0 ? 0 : (floorRating > 5 ? 5 : floorRating));

            var stars = new List<string>();
            for (int i = 0; i < fullStars; i++) stars.Add("★");

            StarRating.ItemsSource = stars;
            TxtRatingCount.Text = $"({rating}/5.0)";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}