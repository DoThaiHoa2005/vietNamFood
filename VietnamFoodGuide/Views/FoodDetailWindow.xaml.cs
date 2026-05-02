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
        private readonly StorageService _storageService = new StorageService();
        private readonly FavoritesApiService _favoritesApiService = new FavoritesApiService();
        private readonly LanguageService _lang = LanguageService.Instance;

        public FoodDetailWindow(FoodItem selectedFood)
        {
            InitializeComponent();

            _food = selectedFood ?? throw new ArgumentNullException(nameof(selectedFood));

            FoodName.Text = _food.Name;
            FoodCity.Text = _food.City;
            FoodRating.Text = $"⭐ {_food.Rating}";
            
            // Set description based on current language
            UpdateDescription();

            if (!string.IsNullOrEmpty(_food.Image))
            {
                try { FoodImage.Source = new BitmapImage(new Uri(_food.Image, UriKind.RelativeOrAbsolute)); }
                catch { }
            }

            // Load favorite status
            LoadFavoriteStatus();
            
            FavoritesBtn.Click += async (s, e) => await ToggleFavoriteAsync();
            
            // ✅ Dùng GoogleTranslateSpeechService (giọng nói chuẩn từ Google Translate)
            // LUÔN LUÔN sẵn sàng ngay lập tức, không cần đợi
            GoogleTranslateSpeechService.Instance.OnSpeechStarted += OnSpeechStarted;
            GoogleTranslateSpeechService.Instance.OnSpeechCompleted += OnSpeechCompleted;
            GoogleTranslateSpeechService.Instance.OnSpeechError += OnSpeechError;
            
            BtnSpeak.IsEnabled = true; // Luôn sẵn sàng
            System.Diagnostics.Debug.WriteLine("✅ [FoodDetail] GoogleTranslateSpeechService sẵn sàng ngay lập tức");
            
            // Update UI with translations
            UpdateUILanguage();
            
            // Subscribe to language changes
            _lang.LanguageChanged += (s, e) => UpdateUILanguage();
        }
        
        private void UpdateDescription()
        {
            var currentLang = _lang.CurrentLanguage;
            if (currentLang == "en")
                FoodDescription.Text = _food.DescriptionEN ?? "No description.";
            else if (currentLang == "zh")
                FoodDescription.Text = _food.DescriptionCN ?? "抱歉，没有描述。";
            else
                FoodDescription.Text = _food.DescriptionVI ?? "Chưa có mô tả.";
        }
        
        private void UpdateUILanguage()
        {
            // Update description
            UpdateDescription();
            
            // Update window title
            this.Title = _lang["food_detail"];
            
            // Update status bar
            if (BtnBack != null)
            {
                BtnBack.Content = _lang["back"];
            }
            if (TxtTitle != null)
            {
                TxtTitle.Text = _lang["food_detail"];
            }
            
            // Update description label
            if (TxtDescriptionLabel != null)
            {
                TxtDescriptionLabel.Text = _lang["description"];
            }
            
            // Update buttons
            if (BtnMap != null)
            {
                BtnMap.Content = _lang["view_map"];
            }
            if (BtnSpeak != null)
            {
                BtnSpeak.Content = _lang["listen"];
            }
            if (BtnStop != null)
            {
                BtnStop.Content = _lang["stop"];
            }
            
            // Update favorite button
            UpdateFavoriteButton();
            
            // Update status bar text (if playing)
            if (SpeechStatusBar != null && SpeechStatusBar.Visibility == Visibility.Visible)
            {
                var statusText = SpeechStatusBar.FindName("StatusText") as TextBlock;
                if (statusText != null)
                {
                    statusText.Text = _lang["playing_narration"];
                }
            }
        }
        
        private void OnSpeechStarted()
        {
            Dispatcher.Invoke(() => {
                BtnStop.Visibility = Visibility.Visible;
                if (StopPlaceholder != null) StopPlaceholder.Visibility = Visibility.Collapsed;
                if (SpeechStatusBar != null)
                {
                    SpeechStatusBar.Visibility = Visibility.Visible;
                    var statusText = SpeechStatusBar.FindName("StatusText") as TextBlock;
                    if (statusText != null)
                    {
                        statusText.Text = _lang["playing_narration"];
                    }
                }
                BtnSpeak.IsEnabled = false;
            });
        }
        
        private void OnSpeechCompleted()
        {
            Dispatcher.Invoke(() => {
                BtnStop.Visibility = Visibility.Collapsed;
                if (StopPlaceholder != null) StopPlaceholder.Visibility = Visibility.Visible;
                if (SpeechStatusBar != null) SpeechStatusBar.Visibility = Visibility.Collapsed;
                BtnSpeak.IsEnabled = true;
            });
        }
        
        private void OnSpeechError(string error)
        {
            Dispatcher.Invoke(() => {
                BtnStop.Visibility = Visibility.Collapsed;
                if (StopPlaceholder != null) StopPlaceholder.Visibility = Visibility.Visible;
                if (SpeechStatusBar != null) SpeechStatusBar.Visibility = Visibility.Collapsed;
                BtnSpeak.IsEnabled = true;
                MessageDialog.ShowWarning($"Lỗi: {error}", "Lỗi");
            });
        }
        
        protected override void OnClosed(EventArgs e)
        {
            // Hủy đăng ký events
            GoogleTranslateSpeechService.Instance.OnSpeechStarted -= OnSpeechStarted;
            GoogleTranslateSpeechService.Instance.OnSpeechCompleted -= OnSpeechCompleted;
            GoogleTranslateSpeechService.Instance.OnSpeechError -= OnSpeechError;
            
            base.OnClosed(e);
        }

        private async void LoadFavoriteStatus()
        {
            try
            {
                if (App.CurrentApiUser != null)
                {
                    _food.IsFavorite = await _favoritesApiService.IsFavoriteAsync(App.CurrentApiUser.Id, _food.Id);
                }
                else
                {
                    _food.IsFavorite = _storageService.IsFavorite(_food.Name);
                }
                UpdateFavoriteButton();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadFavoriteStatus] Error: {ex.Message}");
                _food.IsFavorite = _storageService.IsFavorite(_food.Name);
                UpdateFavoriteButton();
            }
        }

        private void UpdateFavoriteButton()
        {
            Dispatcher.Invoke(() => {
                if (_food.IsFavorite)
                {
                    FavoritesBtn.Content = _lang["remove_favorite"];
                    FavoritesBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD700"));
                    FavoritesBtn.Foreground = System.Windows.Media.Brushes.Black;
                }
                else
                {
                    FavoritesBtn.Content = _lang["add_favorite"];
                    FavoritesBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                    FavoritesBtn.Foreground = System.Windows.Media.Brushes.White;
                }
            });
        }

        private async System.Threading.Tasks.Task ToggleFavoriteAsync()
        {
            try
            {
                FavoritesBtn.IsEnabled = false;
                FavoritesBtn.Content = _lang["processing"];

                if (App.CurrentApiUser != null)
                {
                    if (_food.Id <= 0)
                    {
                        MessageDialog.ShowError("Món ăn không có ID hợp lệ!", _lang["error"]);
                        return;
                    }
                    
                    if (_food.IsFavorite)
                    {
                        var result = await _favoritesApiService.RemoveFavoriteAsync(App.CurrentApiUser.Id, _food.Id);
                        if (result.success) _food.IsFavorite = false;
                        else MessageDialog.ShowError($"{_lang["error"]}: {result.message}", _lang["error"]);
                    }
                    else
                    {
                        var result = await _favoritesApiService.AddFavoriteAsync(App.CurrentApiUser.Id, _food.Id);
                        if (result.success) _food.IsFavorite = true;
                        else MessageDialog.ShowError($"{_lang["error"]}: {result.message}", _lang["error"]);
                    }
                }
                else
                {
                    MessageDialog.ShowInformation(_lang["need_login"], _lang["notification"]);
                    if (_food.IsFavorite) { _storageService.RemoveFavorite(_food.Name); _food.IsFavorite = false; }
                    else { _storageService.AddFavorite(_food); _food.IsFavorite = true; }
                }
            }
            catch (Exception ex)
            {
                MessageDialog.ShowError($"{_lang["error"]}: {ex.Message}", _lang["error"]);
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
                GoogleTranslateSpeechService.Instance.Stop();
                this.Close();
            }
            else
            {
                MessageDialog.ShowInformation("Không tìm thấy thông tin vị trí!");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            GoogleTranslateSpeechService.Instance.Stop();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void SpeakVI(object sender, RoutedEventArgs e)
        {
            string textToSpeak = GetDescription();
            string langCode = GetLanguageCode();

            FoodDescription.Text = textToSpeak;
            
            // Gọi GoogleTranslateSpeechService (giọng nói chuẩn từ Google Translate)
            GoogleTranslateSpeechService.Instance.Speak(textToSpeak, langCode);
        }

        private void StopSpeak(object sender, RoutedEventArgs e)
        {
            GoogleTranslateSpeechService.Instance.Stop();
        }

        private string GetDescription()
        {
            var currentLang = _lang.CurrentLanguage;
            if (currentLang == "en") return _food.DescriptionEN ?? "No description.";
            if (currentLang == "zh") return _food.DescriptionCN ?? "抱歉，没有描述。";
            return _food.DescriptionVI ?? "Chưa có mô tả.";
        }

        private string GetLanguageCode()
        {
            var currentLang = _lang.CurrentLanguage;
            if (currentLang == "en") return "en-US";
            if (currentLang == "zh") return "zh-CN";
            return "vi-VN";
        }
    }
}
