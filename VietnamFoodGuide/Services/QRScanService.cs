using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service quản lý QR scan với offline-first và auto-sync
    /// </summary>
    public class QRScanService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public QRScanService()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(appDir, "Data");
            
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            _dbPath = Path.Combine(dataDir, "qr_scans.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";
            
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS qr_scans (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            device_id TEXT NOT NULL,
                            qr_code TEXT NOT NULL,
                            device_name TEXT,
                            os_version TEXT,
                            scanned_at TEXT NOT NULL,
                            synced INTEGER DEFAULT 0,
                            synced_at TEXT,
                            created_at TEXT DEFAULT CURRENT_TIMESTAMP
                        );
                        
                        CREATE INDEX IF NOT EXISTS idx_device_id ON qr_scans(device_id);
                        CREATE INDEX IF NOT EXISTS idx_synced ON qr_scans(synced);
                    ";

                    using (var command = new SQLiteCommand(createTableSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[QRScanService] Database initialized: {_dbPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QRScanService] Error initializing database: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu QR scan vào local database (offline-first)
        /// </summary>
        public bool SaveQRScanLocal(string deviceId, string qrCode, string deviceName, string osVersion)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string insertSql = @"
                        INSERT INTO qr_scans (device_id, qr_code, device_name, os_version, scanned_at, synced)
                        VALUES (@deviceId, @qrCode, @deviceName, @osVersion, @scannedAt, 0)
                    ";

                    using (var command = new SQLiteCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("@deviceId", deviceId);
                        command.Parameters.AddWithValue("@qrCode", qrCode);
                        command.Parameters.AddWithValue("@deviceName", deviceName ?? "");
                        command.Parameters.AddWithValue("@osVersion", osVersion ?? "");
                        command.Parameters.AddWithValue("@scannedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        command.ExecuteNonQuery();
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ [QRScanService] Saved to local DB: {qrCode}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QRScanService] Error saving to local DB: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra device đã quét QR chưa (check local first)
        /// </summary>
        public bool HasScanned(string deviceId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string selectSql = "SELECT COUNT(*) FROM qr_scans WHERE device_id = @deviceId";

                    using (var command = new SQLiteCommand(selectSql, connection))
                    {
                        command.Parameters.AddWithValue("@deviceId", deviceId);
                        var count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QRScanService] Error checking scan: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách QR scans chưa sync
        /// </summary>
        public List<QRScanRecord> GetUnsyncedScans()
        {
            var scans = new List<QRScanRecord>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string selectSql = "SELECT * FROM qr_scans WHERE synced = 0 ORDER BY scanned_at ASC";

                    using (var command = new SQLiteCommand(selectSql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            scans.Add(new QRScanRecord
                            {
                                Id = reader.GetInt32(0),
                                DeviceId = reader.GetString(1),
                                QRCode = reader.GetString(2),
                                DeviceName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                OSVersion = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                ScannedAt = reader.GetString(5),
                                Synced = reader.GetInt32(6) == 1,
                                SyncedAt = reader.IsDBNull(7) ? null : reader.GetString(7)
                            });
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[QRScanService] Found {scans.Count} unsynced scans");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QRScanService] Error getting unsynced scans: {ex.Message}");
            }

            return scans;
        }

        /// <summary>
        /// Đánh dấu scan đã sync
        /// </summary>
        public bool MarkAsSynced(int scanId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string updateSql = @"
                        UPDATE qr_scans 
                        SET synced = 1, synced_at = @syncedAt 
                        WHERE id = @id
                    ";

                    using (var command = new SQLiteCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@syncedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@id", scanId);
                        command.ExecuteNonQuery();
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ [QRScanService] Marked scan {scanId} as synced");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QRScanService] Error marking as synced: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sync tất cả QR scans chưa sync lên server
        /// </summary>
        public async Task<int> SyncToServerAsync()
        {
            int syncedCount = 0;

            try
            {
                // Kiểm tra kết nối mạng
                bool isOnline = await NetworkService.IsInternetAvailableAsync();
                if (!isOnline)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ [QRScanService] No internet, skip sync");
                    return 0;
                }

                // Lấy danh sách chưa sync
                var unsyncedScans = GetUnsyncedScans();
                if (unsyncedScans.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[QRScanService] No scans to sync");
                    return 0;
                }

                System.Diagnostics.Debug.WriteLine($"[QRScanService] Syncing {unsyncedScans.Count} scans to server...");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    foreach (var scan in unsyncedScans)
                    {
                        try
                        {
                            var scanData = new
                            {
                                deviceId = scan.DeviceId,
                                qrCode = scan.QRCode,
                                deviceName = scan.DeviceName,
                                osVersion = scan.OSVersion,
                                scannedAt = scan.ScannedAt
                            };

                            var json = JsonSerializer.Serialize(scanData);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var response = await client.PostAsync($"{AppConfig.ApiBaseUrl}?action=saveQRScan", content);

                            if (response.IsSuccessStatusCode)
                            {
                                MarkAsSynced(scan.Id);
                                syncedCount++;
                                System.Diagnostics.Debug.WriteLine($"✅ [QRScanService] Synced scan {scan.Id}: {scan.QRCode}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"⚠️ [QRScanService] Failed to sync scan {scan.Id}: {response.StatusCode}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ [QRScanService] Error syncing scan {scan.Id}: {ex.Message}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ [QRScanService] Sync complete: {syncedCount}/{unsyncedScans.Count} scans synced");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QRScanService] Error in sync process: {ex.Message}");
            }

            return syncedCount;
        }

        /// <summary>
        /// Lấy tổng số QR scans
        /// </summary>
        public int GetTotalScans()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string selectSql = "SELECT COUNT(*) FROM qr_scans";

                    using (var command = new SQLiteCommand(selectSql, connection))
                    {
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QRScanService] Error getting total scans: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Lấy số QR scans chưa sync
        /// </summary>
        public int GetUnsyncedCount()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string selectSql = "SELECT COUNT(*) FROM qr_scans WHERE synced = 0";

                    using (var command = new SQLiteCommand(selectSql, connection))
                    {
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QRScanService] Error getting unsynced count: {ex.Message}");
                return 0;
            }
        }
    }

    /// <summary>
    /// Model cho QR scan record
    /// </summary>
    public class QRScanRecord
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string QRCode { get; set; }
        public string DeviceName { get; set; }
        public string OSVersion { get; set; }
        public string ScannedAt { get; set; }
        public bool Synced { get; set; }
        public string SyncedAt { get; set; }
    }
}
