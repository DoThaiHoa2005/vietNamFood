using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service quản lý Favorites bằng SQLite (thay thế favorites.json)
    /// Offline-First: Luôn lưu vào SQLite, sync với API khi có mạng
    /// </summary>
    public class SQLiteFavoritesService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public SQLiteFavoritesService()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(appDir, "Data");
            
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            _dbPath = Path.Combine(dataDir, "favorites.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";

            System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Database path: {_dbPath}");

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                if (!File.Exists(_dbPath))
                {
                    System.Diagnostics.Debug.WriteLine("[SQLiteFavoritesService] Creating new database...");
                    SQLiteConnection.CreateFile(_dbPath);
                    CreateTables();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SQLiteFavoritesService] Database already exists");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Error initializing database: {ex.Message}");
            }
        }

        private void CreateTables()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS Favorites (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        FoodId INTEGER NOT NULL,
                        FoodName TEXT NOT NULL,
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        SyncedToServer INTEGER DEFAULT 0,
                        UNIQUE(UserId, FoodId)
                    )";

                using (var cmd = new SQLiteCommand(createTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                System.Diagnostics.Debug.WriteLine("[SQLiteFavoritesService] Table 'Favorites' created successfully");
            }
        }

        /// <summary>
        /// Thêm favorite (offline-first)
        /// </summary>
        public bool AddFavorite(int userId, int foodId, string foodName)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string insertSql = @"
                        INSERT OR IGNORE INTO Favorites (UserId, FoodId, FoodName, CreatedDate, SyncedToServer)
                        VALUES (@UserId, @FoodId, @FoodName, datetime('now'), 0)";

                    using (var cmd = new SQLiteCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@FoodId", foodId);
                        cmd.Parameters.AddWithValue("@FoodName", foodName);

                        int rows = cmd.ExecuteNonQuery();
                        
                        if (rows > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Added favorite: User {userId}, Food {foodId}");
                            return true;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Favorite already exists");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Error adding favorite: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Xóa favorite (offline-first)
        /// </summary>
        public bool RemoveFavorite(int userId, int foodId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string deleteSql = "DELETE FROM Favorites WHERE UserId = @UserId AND FoodId = @FoodId";

                    using (var cmd = new SQLiteCommand(deleteSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@FoodId", foodId);

                        int rows = cmd.ExecuteNonQuery();
                        
                        System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Removed favorite: User {userId}, Food {foodId}");
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Error removing favorite: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra có phải favorite không
        /// </summary>
        public bool IsFavorite(int userId, int foodId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string selectSql = "SELECT COUNT(*) FROM Favorites WHERE UserId = @UserId AND FoodId = @FoodId";

                    using (var cmd = new SQLiteCommand(selectSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@FoodId", foodId);

                        long count = (long)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Error checking favorite: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy tất cả favorites của user
        /// </summary>
        public List<FoodItem> GetUserFavorites(int userId)
        {
            var favorites = new List<FoodItem>();

            try
            {
                // Lấy danh sách FoodId từ favorites.db
                var foodIds = new List<int>();
                
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string selectSql = @"
                        SELECT FoodId 
                        FROM Favorites 
                        WHERE UserId = @UserId
                        ORDER BY CreatedDate DESC";

                    using (var cmd = new SQLiteCommand(selectSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                foodIds.Add(Convert.ToInt32(reader["FoodId"]));
                            }
                        }
                    }
                }

                // Load thông tin đầy đủ từ foods.db
                if (foodIds.Count > 0)
                {
                    var foodService = new SQLiteFoodService();
                    var allFoods = foodService.LoadFoods();
                    
                    // Filter chỉ lấy foods có trong favorites
                    foreach (var foodId in foodIds)
                    {
                        var food = allFoods.Find(f => f.Id == foodId);
                        if (food != null)
                        {
                            favorites.Add(food);
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Loaded {favorites.Count} favorites for user {userId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Error loading favorites: {ex.Message}");
            }

            return favorites;
        }

        /// <summary>
        /// Lấy danh sách favorites chưa sync lên server
        /// </summary>
        public List<(int UserId, int FoodId)> GetPendingSync()
        {
            var pending = new List<(int, int)>();

            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string selectSql = "SELECT UserId, FoodId FROM Favorites WHERE SyncedToServer = 0";

                    using (var cmd = new SQLiteCommand(selectSql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pending.Add((
                                Convert.ToInt32(reader["UserId"]),
                                Convert.ToInt32(reader["FoodId"])
                            ));
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Found {pending.Count} favorites pending sync");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Error getting pending sync: {ex.Message}");
            }

            return pending;
        }

        /// <summary>
        /// Đánh dấu favorite đã sync
        /// </summary>
        public void MarkAsSynced(int userId, int foodId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string updateSql = "UPDATE Favorites SET SyncedToServer = 1 WHERE UserId = @UserId AND FoodId = @FoodId";

                    using (var cmd = new SQLiteCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@FoodId", foodId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Error marking as synced: {ex.Message}");
            }
        }

        /// <summary>
        /// Sync favorites từ server về SQLite
        /// </summary>
        public void SyncFromServer(int userId, List<FoodItem> serverFavorites)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    // Xóa favorites cũ của user này
                    using (var cmd = new SQLiteCommand("DELETE FROM Favorites WHERE UserId = @UserId", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }

                    // Thêm favorites mới từ server
                    foreach (var food in serverFavorites)
                    {
                        string insertSql = @"
                            INSERT INTO Favorites (UserId, FoodId, FoodName, CreatedDate, SyncedToServer)
                            VALUES (@UserId, @FoodId, @FoodName, datetime('now'), 1)";

                        using (var cmd = new SQLiteCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@FoodId", food.Id);
                            cmd.Parameters.AddWithValue("@FoodName", food.Name);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Synced {serverFavorites.Count} favorites from server for user {userId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Error syncing from server: {ex.Message}");
            }
        }
    }
}
