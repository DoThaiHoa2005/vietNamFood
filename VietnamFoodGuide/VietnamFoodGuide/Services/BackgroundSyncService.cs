using System;
using System.Threading;
using System.Threading.Tasks;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service tự động sync data khi có mạng (background)
    /// </summary>
    public class BackgroundSyncService
    {
        private static BackgroundSyncService _instance;
        private Timer _syncTimer;
        private bool _isRunning = false;
        private readonly int _syncIntervalSeconds = 60; // Sync mỗi 60 giây

        public static BackgroundSyncService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BackgroundSyncService();
                }
                return _instance;
            }
        }

        private BackgroundSyncService()
        {
            // Private constructor for singleton
        }

        /// <summary>
        /// Bắt đầu background sync
        /// </summary>
        public void Start()
        {
            if (_isRunning)
            {
                System.Diagnostics.Debug.WriteLine("[BackgroundSync] Already running");
                return;
            }

            _isRunning = true;
            System.Diagnostics.Debug.WriteLine("[BackgroundSync] Starting...");

            // Sync ngay lập tức
            _ = SyncNowAsync();

            // Sau đó sync định kỳ
            _syncTimer = new Timer(
                async (state) => await SyncNowAsync(),
                null,
                TimeSpan.FromSeconds(_syncIntervalSeconds),
                TimeSpan.FromSeconds(_syncIntervalSeconds)
            );

            System.Diagnostics.Debug.WriteLine($"[BackgroundSync] Started (interval: {_syncIntervalSeconds}s)");
        }

        /// <summary>
        /// Dừng background sync
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _syncTimer?.Dispose();
            _syncTimer = null;

            System.Diagnostics.Debug.WriteLine("[BackgroundSync] Stopped");
        }

        /// <summary>
        /// Sync ngay lập tức
        /// </summary>
        public async Task SyncNowAsync()
        {
            try
            {
                // Kiểm tra kết nối mạng
                bool isOnline = await NetworkService.IsInternetAvailableAsync();
                if (!isOnline)
                {
                    System.Diagnostics.Debug.WriteLine("[BackgroundSync] Offline - Skip sync");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("[BackgroundSync] Online - Starting sync...");

                // 1. Sync QR scans
                var qrScanService = new QRScanService();
                int qrSyncedCount = await qrScanService.SyncToServerAsync();
                
                if (qrSyncedCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ [BackgroundSync] Synced {qrSyncedCount} QR scans");
                }

                // 2. Sync favorites (nếu có)
                if (App.CurrentApiUser != null)
                {
                    var sqliteFavorites = new SQLiteFavoritesService();
                    var pendingFavorites = sqliteFavorites.GetPendingSync();
                    var favApi = new FavoritesApiService();
                    
                    int favSyncedCount = 0;
                    foreach (var pending in pendingFavorites)
                    {
                        var result = await favApi.AddFavoriteAsync(pending.UserId, pending.FoodId);
                        if (result.success || result.message == "Đã yêu thích rồi")
                        {
                            sqliteFavorites.MarkAsSynced(pending.UserId, pending.FoodId);
                            favSyncedCount++;
                        }
                    }
                    
                    if (favSyncedCount > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ [BackgroundSync] Synced {favSyncedCount} favorites");
                    }
                }

                // 3. Sync user data (nếu có)
                // TODO: Implement user data sync

                System.Diagnostics.Debug.WriteLine("[BackgroundSync] Sync complete");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [BackgroundSync] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin sync status
        /// </summary>
        public SyncStatus GetSyncStatus()
        {
            var qrScanService = new QRScanService();
            
            return new SyncStatus
            {
                IsRunning = _isRunning,
                QRScansTotal = qrScanService.GetTotalScans(),
                QRScansUnsynced = qrScanService.GetUnsyncedCount(),
                LastSyncTime = DateTime.Now // TODO: Store actual last sync time
            };
        }
    }

    /// <summary>
    /// Model cho sync status
    /// </summary>
    public class SyncStatus
    {
        public bool IsRunning { get; set; }
        public int QRScansTotal { get; set; }
        public int QRScansUnsynced { get; set; }
        public DateTime LastSyncTime { get; set; }
    }
}
