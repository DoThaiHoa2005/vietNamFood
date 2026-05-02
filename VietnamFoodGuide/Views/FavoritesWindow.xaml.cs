using System;
using System.Windows;
using VietnamFoodGuide.Models;
using VietnamFoodGuide.Services;

namespace VietnamFoodGuide.Views
{
    public partial class FavoritesWindow : Window
    {
        private readonly StorageService storageService = new StorageService();
        private readonly FavoritesApiService _favoritesApiService = new FavoritesApiService();
        private readonly LanguageService lang = LanguageService.Instance;

        public FavoritesWindow()
        {
            InitializeComponent();
            InitializeLanguage();
            LoadFavorites();
            
            // Subscribe to language changes
            lang.LanguageChanged += (s, e) => UpdateUILanguage();
        }

        private void InitializeLanguage()
        {
            UpdateUILanguage();
        }

        private void UpdateUILanguage()
        {
            // Update window title
            Title = lang["my_favorites"];
            
            // Update UI elements
            if (BtnBack != null)
            {
                BtnBack.Content = lang["back"];
            }
            if (TxtTitle != null)
            {
                TxtTitle.Text = lang["my_favorites"];
            }
            if (TxtMyFavorites != null)
            {
                TxtMyFavorites.Text = lang["my_favorites"];
            }
            if (TxtNoFavorites != null)
            {
                TxtNoFavorites.Text = lang["no_favorites"];
            }
            if (TxtAddSomeFavorites != null)
            {
                TxtAddSomeFavorites.Text = lang["add_some_favorites"];
            }
            if (BtnBackToHome != null)
            {
                BtnBackToHome.Content = $"🏠 {lang["home"]}";
            }
        }

        private async void LoadFavorites()
        {
            try
            {
                var favorites = new System.Collections.Generic.List<FoodItem>();

                if (App.CurrentApiUser != null)
                {
                    // Load từ API
                    favorites = await _favoritesApiService.GetUserFavoritesAsync(App.CurrentApiUser.Id);
                }
                else
                {
                    // Fallback to local storage
                    favorites = storageService.GetFavorites();
                }

                // Set ViewDetailsText for all items
                foreach (var food in favorites)
                {
                    food.ViewDetailsText = lang["view_details"];
                }

                FavoritesList.ItemsSource = favorites;
                
                // Update count text based on language
                string countText = "";
                if (lang.CurrentLanguage == "vi")
                {
                    countText = $"{favorites.Count} món ăn";
                }
                else if (lang.CurrentLanguage == "en")
                {
                    countText = $"{favorites.Count} item{(favorites.Count != 1 ? "s" : "")}";
                }
                else // zh
                {
                    countText = $"{favorites.Count} 项";
                }
                CountTxt.Text = countText;
                
                EmptyState.Visibility = favorites.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading favorites: {ex.Message}");
                // Fallback to local storage
                var favorites = storageService.GetFavorites();
                
                // Set ViewDetailsText for all items
                foreach (var food in favorites)
                {
                    food.ViewDetailsText = lang["view_details"];
                }
                
                FavoritesList.ItemsSource = favorites;
                
                // Update count text based on language
                string countText = "";
                if (lang.CurrentLanguage == "vi")
                {
                    countText = $"{favorites.Count} món ăn";
                }
                else if (lang.CurrentLanguage == "en")
                {
                    countText = $"{favorites.Count} item{(favorites.Count != 1 ? "s" : "")}";
                }
                else // zh
                {
                    countText = $"{favorites.Count} 项";
                }
                CountTxt.Text = countText;
                
                EmptyState.Visibility = favorites.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ViewDetails(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is FoodItem food)
            {
                FoodDetailWindow detailWindow = new FoodDetailWindow(food);
                detailWindow.Show();
                this.Close();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            CountTxt.Text = lang.CurrentLanguage == "vi" ? "Đang tải..." : 
                           lang.CurrentLanguage == "en" ? "Loading..." : "加载中...";
            LoadFavorites();
        }
    }
}
