using System;
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
        private readonly StorageService _storageService = new StorageService();
        private readonly FavoritesApiService _favoritesApiService = new FavoritesApiService();

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

            // Load favorite status từ API
            LoadFavoriteStatus();
            
            FavoritesBtn.Click += async (s, e) => await ToggleFavoriteAsync();
            _speechService.OnError += (error) => MessageBox.Show(error);
        }

        private async void LoadFavoriteStatus()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[LoadFavoriteStatus] App.CurrentApiUser: {App.CurrentApiUser?.Username ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"[LoadFavoriteStatus] Food ID: {_food.Id}, Food Name: {_food.Name}");

                if (App.CurrentApiUser != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoadFavoriteStatus] Calling API with UserId: {App.CurrentApiUser.Id}, FoodId: {_food.Id}");
                    _food.IsFavorite = await _favoritesApiService.IsFavoriteAsync(App.CurrentApiUser.Id, _food.Id);
                    System.Diagnostics.Debug.WriteLine($"[LoadFavoriteStatus] API result: {_food.IsFavorite}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LoadFavoriteStatus] Using local storage fallback (No user logged in)");
                    _food.IsFavorite = _storageService.IsFavorite(_food.Name);
                }
                UpdateFavoriteButton();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadFavoriteStatus] CRITICAL ERROR: {ex.Message}");
                // Nếu lỗi kết nối server, báo cho người dùng
                MessageBox.Show($"Không thể kết nối với máy chủ để kiểm tra trạng thái yêu thích.\nChi tiết: {ex.Message}", 
                                "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                _food.IsFavorite = _storageService.IsFavorite(_food.Name);
                UpdateFavoriteButton();
            }
        }

        private void UpdateFavoriteButton()
        {
            Dispatcher.Invoke(() => {
                if (_food.IsFavorite)
                {
                    FavoritesBtn.Content = "⭐ Đã yêu thích";
                    FavoritesBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD700")); // MÀU VÀNG CHIẾM ƯU THẾ
                    FavoritesBtn.Foreground = System.Windows.Media.Brushes.Black;
                }
                else
                {
                    FavoritesBtn.Content = "☆ Thêm yêu thích";
                    FavoritesBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200)); // MÀU XÁM KHI CHƯA THÍCH
                    FavoritesBtn.Foreground = System.Windows.Media.Brushes.White;
                }
            });
        }

        private async System.Threading.Tasks.Task ToggleFavoriteAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ToggleFavorite] Starting - Current state: {_food.IsFavorite}");
                System.Diagnostics.Debug.WriteLine($"[ToggleFavorite] User: {App.CurrentApiUser?.Username ?? "NULL"}, Food: {_food.Name}");

                FavoritesBtn.IsEnabled = false;
                FavoritesBtn.Content = "Đang xử lý...";

                if (App.CurrentApiUser != null)
                {
                    if (_food.Id <= 0)
                    {
                        MessageBox.Show("Món ăn này không có ID hợp lệ trong Database (có thể bạn đang chạy App ở chế độ Offline).\nVui lòng kiểm tra lại link ngrok và XAMPP!", 
                                        "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"[ToggleFavorite] Using API - UserId: {App.CurrentApiUser.Id}, FoodId: {_food.Id}");
                    
                    if (_food.IsFavorite)
                    {
                        var result = await _favoritesApiService.RemoveFavoriteAsync(App.CurrentApiUser.Id, _food.Id);
                        if (result.success)
                        {
                            _food.IsFavorite = false;
                            System.Diagnostics.Debug.WriteLine("[ToggleFavorite] Removed successfully");
                        }
                        else
                        {
                            MessageBox.Show($"Lỗi xóa yêu thích: {result.message}", "Lỗi Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        var result = await _favoritesApiService.AddFavoriteAsync(App.CurrentApiUser.Id, _food.Id);
                        if (result.success)
                        {
                            _food.IsFavorite = true;
                            System.Diagnostics.Debug.WriteLine("[ToggleFavorite] Added successfully");
                        }
                        else
                        {
                            MessageBox.Show($"Lỗi thêm yêu thích: {result.message}", "Lỗi Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Bạn cần đăng nhập để lưu yêu thích vào Database!", "Chưa đăng nhập", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Fallback local storage
                    if (_food.IsFavorite) { _storageService.RemoveFavorite(_food.Name); _food.IsFavorite = false; }
                    else { _storageService.AddFavorite(_food); _food.IsFavorite = true; }
                }
                
                System.Diagnostics.Debug.WriteLine($"[ToggleFavorite] Final state: {_food.IsFavorite}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ToggleFavorite] Exception: {ex.Message}");
                MessageBox.Show($"Lỗi: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                FavoritesBtn.IsEnabled = true;
                UpdateFavoriteButton();
            }
        }

        private void OpenMap(object sender, RoutedEventArgs e)
        {
            if (_food != null)
            {
                MapWindow mapWin = new MapWindow(_food);
                mapWin.Show();
                _speechService.Stop();
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
            _speechService.OnPlaybackStarted += () => Dispatcher.Invoke(() => {
                BtnStop.Visibility = System.Windows.Visibility.Visible;
                BtnSpeak.IsEnabled = false;
            });
            _speechService.OnPlaybackCompleted += () => Dispatcher.Invoke(() => {
                BtnStop.Visibility = System.Windows.Visibility.Collapsed;
                BtnSpeak.IsEnabled = true;
            });
            _speechService.Speak(textToSpeak, cultureCode);
        }

        private void StopSpeak(object sender, RoutedEventArgs e)
        {
            _speechService.Stop();
            BtnStop.Visibility = System.Windows.Visibility.Collapsed;
            BtnSpeak.IsEnabled = true;
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
