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

        public FavoritesWindow()
        {
            InitializeComponent();
            LoadFavorites();
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

                FavoritesList.ItemsSource = favorites;
                CountTxt.Text = $"{favorites.Count} món ăn";
                EmptyState.Visibility = favorites.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading favorites: {ex.Message}");
                // Fallback to local storage
                var favorites = storageService.GetFavorites();
                FavoritesList.ItemsSource = favorites;
                CountTxt.Text = $"{favorites.Count} món ăn";
                EmptyState.Visibility = favorites.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ViewDetails(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is FoodItem food)
            {
                FoodDetailWindow detailWindow = new FoodDetailWindow(food);
                detailWindow.Show();
            }
        }
    }
}
