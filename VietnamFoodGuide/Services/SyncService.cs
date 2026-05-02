using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service đồng bộ dữ liệu giữa SQLite (local) và API (server)
    /// Offline-first: Luôn load từ SQLite, sync khi có mạng
    /// </summary>
    public class SyncService
    {
        private readonly SQLiteFoodService _sqliteService;
        private readonly string _apiBaseUrl;
        private DateTime _lastSyncTime;
        private bool _isSyncing = false;

        public event EventHandler<SyncEventArgs> SyncCompleted;
        public event EventHandler<SyncEventArgs> SyncFailed;

        public SyncService()
        {
            _sqliteService = new SQLiteFoodService();
            _apiBaseUrl = AppConfig.ApiBaseUrl;
            _lastSyncTime = DateTime.MinValue;
        }

        /// <summary>
        /// Load dữ liệu (Offline-first)
        /// 1. Load từ SQLite trước (nhanh, luôn có dữ liệu)
        /// 2. Sync với server trong background (nếu có mạng)
        /// </summary>
        public async Task<List<FoodItem>> LoadFoodsAsync(bool forceSync = false)
        {
            // 1. Load từ SQLite trước (offline-first)
            var localFoods = _sqliteService.LoadFoods();
            System.Diagnostics.Debug.WriteLine($"[SyncService] Loaded {localFoods.Count} foods from SQLite");

            // 2. Sync với server trong background (nếu có mạng)
            if (forceSync || ShouldSync())
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await SyncWithServerAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SyncService] Background sync failed: {ex.Message}");
                    }
                });
            }

            return localFoods;
        }

        /// <summary>
        /// Kiểm tra xem có nên sync không
        /// - Chưa sync lần nào
        /// - Đã quá 5 phút kể từ lần sync cuối
        /// </summary>
        private bool ShouldSync()
        {
            if (_lastSyncTime == DateTime.MinValue)
                return true;

            var timeSinceLastSync = DateTime.Now - _lastSyncTime;
            return timeSinceLastSync.TotalMinutes >= 5;
        }

        /// <summary>
        /// Đồng bộ với server (2 chiều)
        /// 1. Pull: Tải dữ liệu mới từ server về SQLite
        /// 2. Push: Đẩy thay đổi local lên server (nếu có)
        /// </summary>
        public async Task<SyncResult> SyncWithServerAsync()
        {
            if (_isSyncing)
            {
                System.Diagnostics.Debug.WriteLine("[SyncService] Sync already in progress");
                return new SyncResult { Success = false, Message = "Sync đang chạy" };
            }

            _isSyncing = true;
            var result = new SyncResult();

            try
            {
                System.Diagnostics.Debug.WriteLine("[SyncService] Starting sync with server...");

                // Kiểm tra kết nối mạng
                if (!await IsServerAvailableAsync())
                {
                    result.Success = false;
                    result.Message = "Không có kết nối mạng";
                    SyncFailed?.Invoke(this, new SyncEventArgs { Result = result });
                    return result;
                }

                // 1. PULL: Tải dữ liệu từ server
                var serverFoods = await FetchFoodsFromServerAsync();
                if (serverFoods != null && serverFoods.Count > 0)
                {
                    // So sánh và merge dữ liệu
                    var localFoods = _sqliteService.LoadFoods();
                    var mergedFoods = MergeFoods(localFoods, serverFoods);

                    // Cập nhật SQLite với dữ liệu merged
                    _sqliteService.SyncFromAPI(mergedFoods);

                    result.Success = true;
                    result.Message = $"Đồng bộ thành công {mergedFoods.Count} quán ăn";
                    result.ItemsUpdated = mergedFoods.Count;
                    _lastSyncTime = DateTime.Now;

                    System.Diagnostics.Debug.WriteLine($"[SyncService] Sync completed: {result.Message}");
                    SyncCompleted?.Invoke(this, new SyncEventArgs { Result = result });
                }
                else
                {
                    result.Success = false;
                    result.Message = "Không có dữ liệu từ server";
                }

                // 2. PUSH: Đẩy thay đổi local lên server (nếu cần)
                // TODO: Implement push logic nếu có thay đổi local

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Lỗi đồng bộ: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[SyncService] Sync error: {ex.Message}");
                SyncFailed?.Invoke(this, new SyncEventArgs { Result = result });
            }
            finally
            {
                _isSyncing = false;
            }

            return result;
        }

        /// <summary>
        /// Kiểm tra server có available không
        /// </summary>
        private async Task<bool> IsServerAvailableAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetAsync($"{_apiBaseUrl}?action=stats");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tải dữ liệu từ server
        /// </summary>
        private async Task<List<FoodItem>> FetchFoodsFromServerAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var response = await client.GetAsync($"{_apiBaseUrl}?action=foods");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var foods = JsonSerializer.Deserialize<List<FoodItem>>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        System.Diagnostics.Debug.WriteLine($"[SyncService] Fetched {foods?.Count ?? 0} foods from server");
                        return foods;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SyncService] Error fetching from server: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Merge dữ liệu local và server
        /// Strategy: Server wins (server data overrides local)
        /// </summary>
        private List<FoodItem> MergeFoods(List<FoodItem> localFoods, List<FoodItem> serverFoods)
        {
            // Strategy 1: Server wins - Dùng dữ liệu từ server
            // Phù hợp khi admin cập nhật dữ liệu trên server
            return serverFoods;

            // Strategy 2: Merge by ID (nếu cần logic phức tạp hơn)
            /*
            var merged = new List<FoodItem>();
            var serverDict = serverFoods.ToDictionary(f => f.Id);

            foreach (var localFood in localFoods)
            {
                if (serverDict.ContainsKey(localFood.Id))
                {
                    // Server có dữ liệu mới hơn
                    merged.Add(serverDict[localFood.Id]);
                    serverDict.Remove(localFood.Id);
                }
                else
                {
                    // Chỉ có local (có thể đã bị xóa trên server)
                    // Không thêm vào merged
                }
            }

            // Thêm các items mới từ server
            merged.AddRange(serverDict.Values);

            return merged;
            */
        }

        /// <summary>
        /// Force sync ngay lập tức
        /// </summary>
        public async Task<SyncResult> ForceSyncAsync()
        {
            _lastSyncTime = DateTime.MinValue; // Reset để force sync
            return await SyncWithServerAsync();
        }

        /// <summary>
        /// Lấy thời gian sync cuối cùng
        /// </summary>
        public DateTime GetLastSyncTime()
        {
            return _lastSyncTime;
        }

        /// <summary>
        /// Kiểm tra xem đang sync không
        /// </summary>
        public bool IsSyncing()
        {
            return _isSyncing;
        }
    }

    /// <summary>
    /// Kết quả đồng bộ
    /// </summary>
    public class SyncResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int ItemsUpdated { get; set; }
        public DateTime SyncTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Event args cho sync events
    /// </summary>
    public class SyncEventArgs : EventArgs
    {
        public SyncResult Result { get; set; }
    }
}
