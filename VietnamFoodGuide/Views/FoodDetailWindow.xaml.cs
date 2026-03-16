using System;
using System.Diagnostics; // Dùng cho Process
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VietnamFoodGuide.Models;
using VietnamFoodGuide.Services;

namespace VietnamFoodGuide.Views
{
    public partial class FoodDetailWindow : Window
    {
        private readonly FoodItem _food;
        private readonly SpeechService _speechService = new SpeechService();

        public FoodDetailWindow(FoodItem selectedFood)
        {
            InitializeComponent();

            _food = selectedFood ?? throw new ArgumentNullException(nameof(selectedFood));

            FoodName.Text = _food.Name;
            FoodCity.Text = _food.City;
            FoodRating.Text = $"⭐ {_food.Rating}";
            FoodDescription.Text = _food.DescriptionVI;

            if (!string.IsNullOrEmpty(_food.Image))
            {
                try { FoodImage.Source = new BitmapImage(new Uri(_food.Image, UriKind.RelativeOrAbsolute)); }
                catch { }
            }
        }

        // HÀM MỞ BẢN ĐỒ (Đã gộp thành 1 hàm duy nhất)
        // SỬA LẠI HÀM OpenMap
        private void OpenMap(object sender, RoutedEventArgs e)
        {
            if (_food != null)
            {
                MapWindow mapWin = new MapWindow(_food);
                mapWin.Show();

                // Dừng các dịch vụ đang chạy trước khi đóng (như âm thanh)
                _speechService.Stop();

                // Tự động đóng cửa sổ chi tiết
                this.Close();
            }
            else
            {
                MessageBox.Show("Không tìm thấy thông tin vị trí!");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _speechService.Stop();
            this.Close();
        }

        private void SpeakVI(object sender, RoutedEventArgs e)
        {
            string textToSpeak = GetDescription();
            string cultureCode = GetCultureCode();

            FoodDescription.Text = textToSpeak;
            _speechService.Speak(textToSpeak, cultureCode);
        }

        private string GetDescription()
        {
            if (LangEN.IsChecked == true) return _food.DescriptionEN ?? "No description.";
            if (LangCN.IsChecked == true) return _food.DescriptionCN ?? "抱歉，没有描述。";
            return _food.DescriptionVI ?? "Chưa có mô tả.";
        }

        private string GetCultureCode()
        {
            if (LangEN.IsChecked == true) return "en-US";
            if (LangCN.IsChecked == true) return "zh-CN";
            return "vi-VN";
        }
    }
}