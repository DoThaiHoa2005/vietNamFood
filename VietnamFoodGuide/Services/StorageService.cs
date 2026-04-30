using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    public class StorageService
    {
        private readonly string storageDirectory;
        private readonly string favoritesPath;
        private readonly string cacheDirectory;
        private readonly string qrScanPath;

        public StorageService()
        {
            storageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VietnamFoodGuide");
            cacheDirectory = Path.Combine(storageDirectory, "Cache");
            favoritesPath = Path.Combine(storageDirectory, "favorites.json");
            qrScanPath = Path.Combine(storageDirectory, "qr_scanned.txt");

            if (!Directory.Exists(storageDirectory))
                Directory.CreateDirectory(storageDirectory);

            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);
        }

        // Favorites Management
        public void AddFavorite(FoodItem item)
        {
            var favorites = GetFavorites();
            if (!favorites.Exists(f => f.Name == item.Name))
            {
                favorites.Add(item);
                SaveFavorites(favorites);
            }
        }

        public void RemoveFavorite(string foodName)
        {
            var favorites = GetFavorites();
            favorites.RemoveAll(f => f.Name == foodName);
            SaveFavorites(favorites);
        }

        public List<FoodItem> GetFavorites()
        {
            try
            {
                if (File.Exists(favoritesPath))
                {
                    string json = File.ReadAllText(favoritesPath);
                    return JsonSerializer.Deserialize<List<FoodItem>>(json) ?? new List<FoodItem>();
                }
            }
            catch { }
            return new List<FoodItem>();
        }

        public bool IsFavorite(string foodName)
        {
            return GetFavorites().Exists(f => f.Name == foodName);
        }

        private void SaveFavorites(List<FoodItem> favorites)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(favorites, options);
                File.WriteAllText(favoritesPath, json);
            }
            catch { }
        }

        // Cache Management
        public void CacheData(string key, string data)
        {
            try
            {
                string cachePath = Path.Combine(cacheDirectory, $"{key}.json");
                File.WriteAllText(cachePath, data);
            }
            catch { }
        }

        public string GetCachedData(string key)
        {
            try
            {
                string cachePath = Path.Combine(cacheDirectory, $"{key}.json");
                if (File.Exists(cachePath))
                {
                    return File.ReadAllText(cachePath);
                }
            }
            catch { }
            return null;
        }

        // QR Scan Management
        public void SaveQRScanned()
        {
            try
            {
                File.WriteAllText(qrScanPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch { }
        }

        public bool HasScannedQR()
        {
            return File.Exists(qrScanPath);
        }

        public DateTime? GetQRScanDate()
        {
            try
            {
                if (File.Exists(qrScanPath))
                {
                    string dateStr = File.ReadAllText(qrScanPath);
                    if (DateTime.TryParse(dateStr, out var date))
                    {
                        return date;
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
