using System;
using System.Collections.Generic;
using System.IO;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Storage Service - Đã loại bỏ JSON, chuyển sang SQLite hoàn toàn
    /// Favorites: SQLiteFavoritesService
    /// Foods: SQLiteFoodService
    /// QR Scans: QRScanService (SQLite)
    /// </summary>
    public class StorageService
    {
        private readonly string storageDirectory;
        private readonly string qrScanPath;
        private readonly SQLiteFavoritesService _favoritesService;

        public StorageService()
        {
            storageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VietnamFoodGuide");
            qrScanPath = Path.Combine(storageDirectory, "qr_scanned.txt");

            if (!Directory.Exists(storageDirectory))
                Directory.CreateDirectory(storageDirectory);

            _favoritesService = new SQLiteFavoritesService();
        }

        // ============================================================
        // FAVORITES - Chuyển sang SQLite (không dùng JSON nữa)
        // ============================================================
        
        public void AddFavorite(FoodItem item)
        {
            if (App.CurrentApiUser != null)
            {
                _favoritesService.AddFavorite(App.CurrentApiUser.Id, item.Id, item.Name);
            }
        }

        public void RemoveFavorite(string foodName)
        {
            // Tìm food theo tên để lấy ID
            if (App.CurrentApiUser != null)
            {
                var foods = new SQLiteFoodService().LoadFoods();
                var food = foods.Find(f => f.Name == foodName);
                if (food != null)
                {
                    _favoritesService.RemoveFavorite(App.CurrentApiUser.Id, food.Id);
                }
            }
        }

        public List<FoodItem> GetFavorites()
        {
            if (App.CurrentApiUser != null)
            {
                return _favoritesService.GetUserFavorites(App.CurrentApiUser.Id);
            }
            return new List<FoodItem>();
        }

        public bool IsFavorite(string foodName)
        {
            if (App.CurrentApiUser != null)
            {
                var foods = new SQLiteFoodService().LoadFoods();
                var food = foods.Find(f => f.Name == foodName);
                if (food != null)
                {
                    return _favoritesService.IsFavorite(App.CurrentApiUser.Id, food.Id);
                }
            }
            return false;
        }

        // ============================================================
        // QR SCAN - Giữ lại file txt đơn giản (không cần SQLite cho cái này)
        // ============================================================
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

        public void ClearQRScanned()
        {
            try { File.Delete(qrScanPath); } catch { }
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
