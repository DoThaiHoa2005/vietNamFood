using System;
using System.Data.SQLite;
using System.IO;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service quản lý Users bằng SQLite (offline login)
    /// </summary>
    public class SQLiteUserService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public SQLiteUserService()
        {
            // Đường dẫn database trong thư mục Data
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(appDir, "Data");
            
            // Tạo thư mục Data nếu chưa có
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            _dbPath = Path.Combine(dataDir, "users.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";

            System.Diagnostics.Debug.WriteLine($"[SQLiteUserService] Database path: {_dbPath}");

            // Khởi tạo database nếu chưa có
            InitializeDatabase();
        }

        /// <summary>
        /// Khởi tạo database và bảng Users
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                // Nếu database chưa tồn tại, tạo mới
                if (!File.Exists(_dbPath))
                {
                    System.Diagnostics.Debug.WriteLine("[SQLiteUserService] Creating new database...");
                    SQLiteConnection.CreateFile(_dbPath);
                    CreateTables();
                    InsertDefaultUsers();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SQLiteUserService] Database already exists");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteUserService] Error initializing database: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo bảng Users
        /// </summary>
        private void CreateTables()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        Role TEXT NOT NULL DEFAULT 'User',
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        LastLoginDate TEXT
                    )";

                using (var cmd = new SQLiteCommand(createTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                System.Diagnostics.Debug.WriteLine("[SQLiteUserService] Table 'Users' created successfully");
            }
        }

        /// <summary>
        /// Thêm users mặc định (admin và user123)
        /// </summary>
        private void InsertDefaultUsers()
        {
            System.Diagnostics.Debug.WriteLine("[SQLiteUserService] Inserting default users...");

            // Hash passwords với BCrypt
            var adminHash = BCrypt.Net.BCrypt.HashPassword("admin123");
            var userHash = BCrypt.Net.BCrypt.HashPassword("user123");

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                // Admin user
                string insertSql = @"
                    INSERT INTO Users (Username, PasswordHash, Role, CreatedDate)
                    VALUES (@Username, @PasswordHash, @Role, @CreatedDate)";

                using (var cmd = new SQLiteCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", "admin");
                    cmd.Parameters.AddWithValue("@PasswordHash", adminHash);
                    cmd.Parameters.AddWithValue("@Role", "Admin");
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }

                // Regular user
                using (var cmd = new SQLiteCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", "user123");
                    cmd.Parameters.AddWithValue("@PasswordHash", userHash);
                    cmd.Parameters.AddWithValue("@Role", "User");
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }

            System.Diagnostics.Debug.WriteLine("[SQLiteUserService] Inserted 2 default users (admin, user123)");
        }

        /// <summary>
        /// Đăng nhập offline (kiểm tra username và password)
        /// </summary>
        public UserInfo Login(string username, string password)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string selectSql = "SELECT * FROM Users WHERE Username = @Username";

                    using (var cmd = new SQLiteCommand(selectSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var storedHash = reader["PasswordHash"].ToString();

                                // Verify password với BCrypt
                                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                                {
                                    // Update last login date
                                    UpdateLastLoginDate(Convert.ToInt32(reader["Id"]));

                                    // Return user info
                                    return new UserInfo
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        Username = reader["Username"].ToString(),
                                        Role = reader["Role"].ToString()
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteUserService] Login error: {ex.Message}");
            }

            return null; // Login failed
        }

        /// <summary>
        /// Cập nhật thời gian đăng nhập cuối
        /// </summary>
        private void UpdateLastLoginDate(int userId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string updateSql = "UPDATE Users SET LastLoginDate = @LastLoginDate WHERE Id = @Id";

                    using (var cmd = new SQLiteCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LastLoginDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@Id", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteUserService] Update last login error: {ex.Message}");
            }
        }

        /// <summary>
        /// Đăng ký user mới
        /// </summary>
        public bool Register(string username, string password)
        {
            try
            {
                // Check if username already exists
                if (UserExists(username))
                {
                    return false;
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string insertSql = @"
                        INSERT INTO Users (Username, PasswordHash, Role, CreatedDate)
                        VALUES (@Username, @PasswordHash, @Role, @CreatedDate)";

                    using (var cmd = new SQLiteCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@Role", "User");
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[SQLiteUserService] User '{username}' registered successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteUserService] Register error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra username đã tồn tại chưa
        /// </summary>
        private bool UserExists(string username)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string selectSql = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

                    using (var cmd = new SQLiteCommand(selectSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        var count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteUserService] UserExists error: {ex.Message}");
                return false;
            }
        }
    }
}
