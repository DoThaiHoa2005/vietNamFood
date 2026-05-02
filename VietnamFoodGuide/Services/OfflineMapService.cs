using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Offline map support - caches map locations for offline use
    /// </summary>
    public class OfflineMapService
    {
        private readonly string _cacheDirectory;
        private readonly string _mapDataFile;

        public OfflineMapService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _cacheDirectory = Path.Combine(appData, "VietnamFoodGuide", "MapCache");
            _mapDataFile = Path.Combine(_cacheDirectory, "map_locations.json");

            if (!Directory.Exists(_cacheDirectory))
                Directory.CreateDirectory(_cacheDirectory);
        }

        /// <summary>
        /// Cache location data for offline access
        /// </summary>
        public void CacheLocationData(List<Models.FoodItem> foods)
        {
            try
            {
                var mapData = foods.Select(f => new OfflineMapData
                {
                    Name = f.Name,
                    City = f.City,
                    Latitude = f.Latitude,
                    Longitude = f.Longitude,
                    CachedTime = DateTime.Now
                }).ToList();

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(mapData, options);
                File.WriteAllText(_mapDataFile, json);

                System.Diagnostics.Debug.WriteLine($"✅ Cached {mapData.Count} locations for offline maps");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error caching map data: {ex.Message}");
            }
        }

        /// <summary>
        /// Get offline map HTML (works without internet)
        /// </summary>
        public string GenerateOfflineMapHtml(double latitude, double longitude, string locationName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>Bản đồ: {locationName}</title>
    <style>
        body {{ margin: 0; padding: 10px; font-family: Arial, sans-serif; }}
        #map {{ width: 100%; height: 400px; background: #f0f0f0; border: 1px solid #ccc; position: relative; }}
        .info {{ padding: 10px; background: #f9f9f9; margin-top: 10px; border-radius: 5px; }}
        .marker {{ position: absolute; width: 30px; height: 40px; background: #E63946; border-radius: 50% 50% 50% 0;
                   transform: rotate(-45deg); left: 50%; top: 50%; margin-left: -15px; margin-top: -40px; }}
        .marker::after {{ content: ''; position: absolute; width: 8px; height: 8px; background: white;
                          border-radius: 50%; top: 5px; left: 11px; }}
        .offline-badge {{ background: #FFC107; color: black; padding: 5px 10px; border-radius: 3px;
                         font-weight: bold; display: inline-block; margin-bottom: 10px; }}
    </style>
</head>
<body>
    <div class='offline-badge'>📍 Bản đồ Offline</div>
    <h2>{locationName}</h2>
    <div class='info'>
        <p><strong>Tọa độ:</strong> {latitude:F4}, {longitude:F4}</p>
        <p><strong>Ghi chú:</strong> Bản đồ này là bản cached khi còn kết nối internet.</p>
        <p><strong>Hướng dẫn:</strong> Sử dụng ứng dụng Maps hoặc Google Maps trên điện thoại để dẫn đường tới địa chỉ này.</p>
    </div>
    <div id='map'>
        <div class='marker'></div>
        <p style='position: absolute; bottom: 10px; left: 10px; margin: 0; background: white; padding: 5px; border-radius: 3px;'>
            Vị trí: {latitude:F4}, {longitude:F4}
        </p>
    </div>
    <div class='info' style='margin-top: 10px;'>
        <h3>📱 Mở trong Maps Apps:</h3>
        <p>
            <a href='https://maps.google.com/?q={latitude},{longitude}' target='_blank'>
                🔗 Google Maps
            </a> |
            <a href='https://www.openstreetmap.org/?mlat={latitude}&mlon={longitude}' target='_blank'>
                🔗 OpenStreetMap
            </a>
        </p>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Get cached locations for offline display
        /// </summary>
        public List<OfflineMapData> GetCachedLocations()
        {
            try
            {
                if (!File.Exists(_mapDataFile))
                    return new List<OfflineMapData>();

                string json = File.ReadAllText(_mapDataFile);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<OfflineMapData>>(json, options)
                    ?? new List<OfflineMapData>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error loading cached map data: {ex.Message}");
                return new List<OfflineMapData>();
            }
        }

        /// <summary>
        /// Find nearby location (offline search)
        /// </summary>
        public OfflineMapData FindNearbyLocation(double latitude, double longitude, double radiusKm = 1.0)
        {
            var cached = GetCachedLocations();

            return cached.OrderBy(l =>
                CalculateDistance(latitude, longitude, l.Latitude, l.Longitude)
            ).FirstOrDefault(l =>
                CalculateDistance(latitude, longitude, l.Latitude, l.Longitude) <= radiusKm
            );
        }

        /// <summary>
        /// Calculate distance between two coordinates (Haversine formula)
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in km
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Check if map data is cached and fresh
        /// </summary>
        public bool IsCacheFresh(int maxAgeHours = 24)
        {
            if (!File.Exists(_mapDataFile))
                return false;

            var fileInfo = new FileInfo(_mapDataFile);
            return DateTime.Now - fileInfo.LastWriteTime < TimeSpan.FromHours(maxAgeHours);
        }

        /// <summary>
        /// Clear cache if needed
        /// </summary>
        public void ClearCache()
        {
            try
            {
                if (Directory.Exists(_cacheDirectory))
                    Directory.Delete(_cacheDirectory, true);
                Directory.CreateDirectory(_cacheDirectory);
                System.Diagnostics.Debug.WriteLine("✅ Map cache cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error clearing cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Get size of offline tiles in MB
        /// </summary>
        public double GetOfflineTilesSize()
        {
            try
            {
                string tilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "MapTiles");
                if (!Directory.Exists(tilesDir))
                    return 0;

                var dirInfo = new DirectoryInfo(tilesDir);
                long totalBytes = dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
                return totalBytes / (1024.0 * 1024.0); // Convert to MB
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error getting tiles size: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Download map tiles for Vinh Khanh area
        /// </summary>
        public void DownloadVinhKhanhArea(Action<int, string> progressCallback)
        {
            try
            {
                progressCallback?.Invoke(0, "Bắt đầu tải bản đồ...");

                // Vinh Khanh coordinates
                double centerLat = 10.78024;
                double centerLon = 106.70532;
                int[] zoomLevels = { 13, 14, 15, 16, 17 };

                string tilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "MapTiles");
                if (!Directory.Exists(tilesDir))
                    Directory.CreateDirectory(tilesDir);

                int totalTiles = 0;
                int downloadedTiles = 0;

                // Calculate total tiles
                foreach (int zoom in zoomLevels)
                {
                    var tiles = GetTilesForArea(centerLat, centerLon, zoom, 0.02);
                    totalTiles += tiles.Count;
                }

                progressCallback?.Invoke(0, $"Tổng số tiles: {totalTiles}");

                // Download tiles
                using (var client = new System.Net.WebClient())
                {
                    foreach (int zoom in zoomLevels)
                    {
                        var tiles = GetTilesForArea(centerLat, centerLon, zoom, 0.02);

                        foreach (var tile in tiles)
                        {
                            string tileDir = Path.Combine(tilesDir, zoom.ToString(), tile.X.ToString());
                            if (!Directory.Exists(tileDir))
                                Directory.CreateDirectory(tileDir);

                            string tilePath = Path.Combine(tileDir, $"{tile.Y}.png");

                            if (!File.Exists(tilePath))
                            {
                                try
                                {
                                    string url = $"https://tile.openstreetmap.org/{zoom}/{tile.X}/{tile.Y}.png";
                                    client.Headers.Add("User-Agent", "VietnamFoodGuide/1.0");
                                    client.DownloadFile(url, tilePath);
                                    System.Threading.Thread.Sleep(100); // Rate limiting
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"❌ Error downloading tile {zoom}/{tile.X}/{tile.Y}: {ex.Message}");
                                }
                            }

                            downloadedTiles++;
                            int progress = (int)((downloadedTiles / (double)totalTiles) * 100);
                            progressCallback?.Invoke(progress, $"Đã tải: {downloadedTiles}/{totalTiles} tiles");
                        }
                    }
                }

                progressCallback?.Invoke(100, "Hoàn thành!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error downloading Vinh Khanh area: {ex.Message}");
                progressCallback?.Invoke(0, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear offline tiles
        /// </summary>
        public void ClearOfflineTiles()
        {
            try
            {
                string tilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "MapTiles");
                if (Directory.Exists(tilesDir))
                {
                    Directory.Delete(tilesDir, true);
                    System.Diagnostics.Debug.WriteLine("✅ Offline tiles cleared");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error clearing tiles: {ex.Message}");
            }
        }

        /// <summary>
        /// Get tiles for a specific area
        /// </summary>
        private List<TileCoordinate> GetTilesForArea(double lat, double lon, int zoom, double radiusDegrees)
        {
            var tiles = new List<TileCoordinate>();

            double minLat = lat - radiusDegrees;
            double maxLat = lat + radiusDegrees;
            double minLon = lon - radiusDegrees;
            double maxLon = lon + radiusDegrees;

            var minTile = LatLonToTile(minLat, minLon, zoom);
            var maxTile = LatLonToTile(maxLat, maxLon, zoom);

            for (int x = Math.Min(minTile.X, maxTile.X); x <= Math.Max(minTile.X, maxTile.X); x++)
            {
                for (int y = Math.Min(minTile.Y, maxTile.Y); y <= Math.Max(minTile.Y, maxTile.Y); y++)
                {
                    tiles.Add(new TileCoordinate { X = x, Y = y });
                }
            }

            return tiles;
        }

        /// <summary>
        /// Convert lat/lon to tile coordinates
        /// </summary>
        private TileCoordinate LatLonToTile(double lat, double lon, int zoom)
        {
            int n = 1 << zoom;
            int x = (int)((lon + 180.0) / 360.0 * n);
            int y = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * n);
            return new TileCoordinate { X = x, Y = y };
        }

        private class TileCoordinate
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }

    /// <summary>
    /// Offline map data structure
    /// </summary>
    public class OfflineMapData
    {
        public string Name { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CachedTime { get; set; }
    }
}
