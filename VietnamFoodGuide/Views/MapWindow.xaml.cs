using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VietnamFoodGuide.Models;
using VietnamFoodGuide.Services;
using System.Device.Location;

namespace VietnamFoodGuide.Views
{
    public partial class MapWindow : Window
    {
        private readonly FoodItem _food;
        private List<FoodItem> _allFoods;
        private readonly SpeechService _speechService = new SpeechService();
        private readonly DispatcherTimer _locationTimer = new DispatcherTimer();
        private FoodItem _lastNarratedFood = null;
        private GeoCoordinateWatcher _geoWatcher;

        private double _userLat = 10.7769; // Quận 1 - Bến Thành (xa hơn ~5km)
        private double _userLng = 106.7009;
        private DateTime _lastServerReport = DateTime.MinValue;
        private bool _isNavigating = false;
        private string _destinationName = "";
        private double _destinationLat = 0;
        private double _destinationLng = 0;

        public MapWindow(FoodItem food)
        {
            InitializeComponent();
            _food = food ?? throw new ArgumentNullException(nameof(food));

            TxtName.Text = _food.Name;
            TxtCity.Text = _food.City;
            SetRating(_food.Rating);

            _allFoods = LoadAllFoods();
            
            // Initialize language
            UpdateUILanguage();
            LanguageService.Instance.LanguageChanged += (s, e) => UpdateUILanguage();

            this.Opacity = 0;
            this.Loaded += (s, e) => {
                var anim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                this.BeginAnimation(Window.OpacityProperty, anim);
                InitializeWebView();
                StartLocationTracking();
            };

            _speechService.OnPlaybackCompleted += () => Dispatcher.Invoke(() => NarrationBanner.Visibility = Visibility.Collapsed);
            _speechService.OnError += (err) => Dispatcher.Invoke(() => NarrationBanner.Visibility = Visibility.Collapsed);

            _geoWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            _geoWatcher.MovementThreshold = 0; // Cập nhật ngay khi có sự thay đổi nhỏ nhất
            _geoWatcher.PositionChanged += GeoWatcher_PositionChanged;
            _geoWatcher.StatusChanged += GeoWatcher_StatusChanged;
            _geoWatcher.Start();
        }

        private void GeoWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                string statusMsg = "";
                switch (e.Status)
                {
                    case GeoPositionStatus.Disabled:
                        statusMsg = "disabled";
                        System.Diagnostics.Debug.WriteLine("❌ [C# GPS] Quyền truy cập vị trí bị TẮT");
                        break;
                    case GeoPositionStatus.NoData:
                        statusMsg = "nodata";
                        System.Diagnostics.Debug.WriteLine("⚠️ [C# GPS] Không có dữ liệu vị trí");
                        break;
                    case GeoPositionStatus.Ready:
                        statusMsg = "ready";
                        System.Diagnostics.Debug.WriteLine("✅ [C# GPS] Sẵn sàng");
                        break;
                }

                if (!string.IsNullOrEmpty(statusMsg) && MapBrowser?.CoreWebView2 != null)
                {
                    var msg = JsonSerializer.Serialize(new { type = "gpsStatus", status = statusMsg });
                    MapBrowser.CoreWebView2.PostWebMessageAsString(msg);
                }
            });
        }

        private void GeoWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (e.Position.Location.IsUnknown) return;
            
            _userLat = e.Position.Location.Latitude;
            _userLng = e.Position.Location.Longitude;
            
            System.Diagnostics.Debug.WriteLine($"📍 [C# GPS] Vị trí chuẩn: {_userLat}, {_userLng}");
            
            // Gửi dữ liệu GPS sang JavaScript
            PushUserPositionToJS(_userLat, _userLng, e.Position.Location.HorizontalAccuracy);

            // Báo cáo lên server (Admin Dashboard) mỗi 5 giây
            ReportPositionToServer(_userLat, _userLng);
        }

        private void PushUserPositionToJS(double lat, double lng, double accuracy = 0)
        {
            if (MapBrowser?.CoreWebView2 != null)
            {
                var msg = JsonSerializer.Serialize(new
                {
                    type = "updateUserPosition",
                    lat = lat,
                    lng = lng,
                    source = "native",
                    accuracy = accuracy
                });
                MapBrowser.CoreWebView2.PostWebMessageAsString(msg);
            }
        }

        private async void ReportPositionToServer(double lat, double lng, bool forceImmediate = false)
        {
            // Throttle: chỉ báo cáo mỗi 3 giây (trừ khi forceImmediate = true)
            if (!forceImmediate && (DateTime.Now - _lastServerReport).TotalSeconds < 3) return;
            
            var user = App.CurrentApiUser;
            if (user == null) return;

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var data = new
                    {
                        userId = user.Id,
                        currentLat = lat,
                        currentLng = lng,
                        destinationLat = _destinationLat,
                        destinationLng = _destinationLng,
                        destinationName = _destinationName,
                        isNavigating = _isNavigating,
                        isActive = true
                    };

                    var json = JsonSerializer.Serialize(data);
                    System.Diagnostics.Debug.WriteLine($"📤 [Tracking] JSON gửi đi: {json}");
                    var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{AppConfig.ApiBaseUrl}?action=updateTracking", content);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"📥 [Tracking] API response: {responseBody}");
                    
                    _lastServerReport = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine($"📡 [Tracking] Đã gửi vị trí User {user.Username} ({lat:F5}, {lng:F5}) lên server - Navigate: {_isNavigating} (Force: {forceImmediate})");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [Tracking] Lỗi gửi vị trí lên server: {ex.Message}");
            }
        }

        private List<FoodItem> LoadAllFoods()
        {
            try
            {
                // Load từ SQLite (offline) hoặc API (online)
                var sqliteService = new SQLiteFoodService();
                var foods = sqliteService.LoadFoods();
                
                if (foods != null && foods.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[MapWindow] Loaded {foods.Count} foods from SQLite");
                    return foods;
                }
                
                System.Diagnostics.Debug.WriteLine("[MapWindow] No foods found in SQLite");
                return new List<FoodItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MapWindow] Error loading foods: {ex.Message}");
                return new List<FoodItem>();
            }
        }

        private async void InitializeWebView()
        {
            try
            {
                var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, System.IO.Path.GetTempPath(), new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions());
                await MapBrowser.EnsureCoreWebView2Async(env);
                
                MapBrowser.CoreWebView2.Settings.IsWebMessageEnabled = true;
                MapBrowser.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                MapBrowser.CoreWebView2.Settings.IsStatusBarEnabled = false;
                MapBrowser.CoreWebView2.Settings.AreDevToolsEnabled = true; // Enable F12 Developer Tools
                
                // MAPPING FOR OFFLINE LEAFLET ASSETS
                MapBrowser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app.local", 
                    AppContext.BaseDirectory, 
                    Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
                
                // Enable GPS permission - CRITICAL for geolocation
                MapBrowser.CoreWebView2.PermissionRequested += (sender, args) =>
                {
                    if (args.PermissionKind == Microsoft.Web.WebView2.Core.CoreWebView2PermissionKind.Geolocation)
                    {
                        args.State = Microsoft.Web.WebView2.Core.CoreWebView2PermissionState.Allow;
                        System.Diagnostics.Debug.WriteLine("? GPS permission granted to WebView2");
                    }
                };
                
                // Enable location services in WebView2
                MapBrowser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                
                // Register WebMessage event handler
                MapBrowser.CoreWebView2.WebMessageReceived += OnWebMessage;
                System.Diagnostics.Debug.WriteLine("? [C#] WebMessageReceived event handler registered");
                
                // MAPPING FOR MAP TILES CACHING
                MapBrowser.CoreWebView2.AddWebResourceRequestedFilter("https://mt1.google.com/*", Microsoft.Web.WebView2.Core.CoreWebView2WebResourceContext.Image);
                MapBrowser.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
                
                LoadMap();
            }
            catch (Exception ex)
            {
                MessageDialog.ShowError($"Lỗi: {ex.Message}\n\nCài WebView2 Runtime tại:\nhttps://go.microsoft.com/fwlink/p/?LinkId=2124703", "Lỗi Bản đồ");
            }
        }

        private async void CoreWebView2_WebResourceRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();
            try
            {
                string uri = e.Request.Uri;
                if (uri.Contains("mt1.google.com"))
                {
                    // URL is usually like: https://mt1.google.com/vt/lyrs=m&x=1&y=2&z=3
                    string fileName = uri.Replace("https://mt1.google.com/", "")
                                         .Replace("/", "_")
                                         .Replace("?", "_")
                                         .Replace("&", "_")
                                         .Replace("=", "_") + ".png";
                    
                    string tilesDir = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Tiles");
                    if (!System.IO.Directory.Exists(tilesDir)) System.IO.Directory.CreateDirectory(tilesDir);
                    
                    string filePath = System.IO.Path.Combine(tilesDir, fileName);
                    
                    bool isOnline = await NetworkService.IsInternetAvailableAsync();
                    
                    if (System.IO.File.Exists(filePath))
                    {
                        // Offline or Online, if cached, serve from cache to save bandwidth and speed up
                        // BUT maybe we should download if online? Google map tiles don't change often. Let's just serve cache.
                        var stream = System.IO.File.OpenRead(filePath);
                        e.Response = MapBrowser.CoreWebView2.Environment.CreateWebResourceResponse(
                            stream, 200, "OK", "Content-Type: image/png\nCache-Control: public, max-age=31536000"
                        );
                    }
                    else if (isOnline)
                    {
                        // Online, download and cache
                        using (var client = new System.Net.Http.HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                            var bytes = await client.GetByteArrayAsync(uri);
                            System.IO.File.WriteAllBytes(filePath, bytes);
                            
                            var stream = new System.IO.MemoryStream(bytes);
                            e.Response = MapBrowser.CoreWebView2.Environment.CreateWebResourceResponse(
                                stream, 200, "OK", "Content-Type: image/png\nCache-Control: public, max-age=31536000"
                            );
                        }
                    }
                    else
                    {
                        // Offline and no cache
                        e.Response = MapBrowser.CoreWebView2.Environment.CreateWebResourceResponse(
                            new System.IO.MemoryStream(), 404, "Not Found", ""
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TileCache] Error: {ex.Message}");
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void LoadMap()
        {
            if (_allFoods == null || _allFoods.Count == 0)
            {
                MapBrowser.NavigateToString("<html><body style='margin:0;padding:20px;font-family:Arial;background:#f0f0f0;text-align:center'><h2>Kh�ng c� d? li?u qu�n an</h2></body></html>");
                return;
            }

            var markersJson = new StringBuilder("[");
            foreach (var f in _allFoods)
            {
                string desc = (f.DescriptionVI ?? "").Replace("\"", "&quot;").Replace("\n", " ").Replace("'", "&#39;");
                if (desc.Length > 100) desc = desc.Substring(0, 100) + "...";
                string name = f.Name.Replace("\"", "&quot;").Replace("'", "&#39;");
                string category = (f.Category ?? "").Replace("\"", "&quot;").Replace("'", "&#39;");
                markersJson.Append($"{{name:'{name}',lat:{f.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},lng:{f.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},rating:{f.Rating.ToString(System.Globalization.CultureInfo.InvariantCulture)},desc:'{desc}',category:'{category}'}},");
            }
            if (_allFoods.Count > 0) markersJson.Length--;
            markersJson.Append("]");

            string html = BuildMapHtml(markersJson.ToString());
            MapBrowser.NavigateToString(html);
        }

        private string BuildMapHtml(string markersJson)
        {
            string focusLat = _food.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string focusLng = _food.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string userLat = _userLat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string userLng = _userLng.ToString(System.Globalization.CultureInfo.InvariantCulture);

            string html = "<!DOCTYPE html><html><head><meta charset='utf-8'/><meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>";
            
            // OFFLINE SUPPORT: Dùng file nội bộ thay vì CDN
            html += "<link rel='stylesheet' href='https://app.local/Assets/leaflet.css'/>";
            html += "<script src='https://app.local/Assets/leaflet.js'></script>";
            html += "<script src='https://app.local/Assets/leaflet-rotate.min.js'></script>";
            
            html += "<style>";
            html += "html,body{margin:0;padding:0;height:100%;width:100%;}";
            html += "#map{height:100%;width:100%;}";
            html += ".btn-location{position:absolute;top:calc(70px + env(safe-area-inset-top));right:15px;width:52px;height:52px;background:white;border:2px solid #ddd;border-radius:50%;cursor:pointer;font-size:24px;z-index:1000;box-shadow:0 2px 8px rgba(0,0,0,0.3);display:flex;align-items:center;justify-content:center;transition:all 0.2s;}";
            html += ".btn-location:hover{background:#f0f0f0;transform:scale(1.1);box-shadow:0 4px 12px rgba(0,0,0,0.4);}";
            html += ".btn-location:active{transform:scale(0.95);}";
            html += ".btn-compass{position:absolute;top:calc(135px + env(safe-area-inset-top));right:15px;width:52px;height:52px;background:white;border:2px solid #ddd;border-radius:50%;cursor:pointer;z-index:1000;box-shadow:0 2px 8px rgba(0,0,0,0.3);display:flex;align-items:center;justify-content:center;transition:background 0.2s,box-shadow 0.2s;}";
            html += ".btn-compass:hover{background:#f8f8f8;box-shadow:0 4px 12px rgba(0,0,0,0.4);}";
            html += ".btn-compass:active{transform:scale(0.95);}";
            html += ".compass-arrow{width:0;height:0;border-left:8px solid transparent;border-right:8px solid transparent;border-bottom:24px solid #EA4335;position:relative;transition:transform 0.3s ease-out;}";
            html += ".compass-arrow::after{content:'N';position:absolute;top:26px;left:-6px;font-size:10px;font-weight:bold;color:#5f6368;}";
            html += ".btn-compass.active{background:#e8f0fe;border-color:#1a73e8;}";
            html += ".top-controls{position:absolute;top:15px;right:15px;z-index:1000;display:flex;flex-direction:column;gap:8px;}";
            html += ".btn-test-mode{background:#ff9800;color:white;padding:8px 12px;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.2);border:none;cursor:pointer;font-size:12px;font-weight:600;}";
            html += ".bottom-panel{position:fixed;bottom:0;left:0;right:0;background:white;border-radius:16px 16px 0 0;box-shadow:0 -2px 12px rgba(0,0,0,0.15);z-index:1000;transform:translateY(100%);transition:transform 0.3s ease;padding:8px 12px;padding-bottom:calc(8px + env(safe-area-inset-bottom));height:auto;max-height:120px;overflow:visible;}";
            html += ".bottom-panel.show{transform:translateY(0);}";
            html += ".bottom-panel.auto-show{transform:translateY(0);}";
            html += ".panel-header{display:flex;justify-content:space-between;align-items:center;margin-bottom:4px;}";
            html += ".close-btn{background:none;border:none;font-size:18px;cursor:pointer;color:#666;padding:2px;}";
            html += ".route-title{font-size:14px;font-weight:600;color:#202124;margin:0;line-height:1.1;}";
            html += ".route-subtitle{font-size:11px;color:#5f6368;margin:0 0 2px;line-height:1.1;}";
            html += ".action-buttons{display:flex;gap:6px;margin-top:4px;}";
            html += ".btn-start-small{flex:1;padding:8px;background:#34a853;color:white;border:none;border-radius:6px;cursor:pointer;font-size:12px;font-weight:600;min-height:40px;}";
            html += ".btn-change-small{flex:1;padding:8px;background:#1a73e8;color:white;border:none;border-radius:6px;cursor:pointer;font-size:12px;font-weight:600;min-height:40px;}";
            html += ".suggestion-text{font-size:11px;color:#5f6368;text-align:center;margin-top:4px;font-style:italic;display:none;}";
            html += ".navigation-info{background:#f8f9fa;padding:4px 6px;border-radius:6px;margin:3px 0;font-size:10px;color:#202124;display:none;max-height:45px;overflow-y:auto;}";
            html += ".nav-instruction{display:flex;align-items:center;gap:6px;padding:3px 5px;background:white;border-radius:6px;margin:2px 0;}";
            html += ".nav-icon{font-size:22px;min-width:28px;text-align:center;line-height:1;}";
            html += ".nav-text{flex:1;}";
            html += ".nav-distance{font-size:18px;font-weight:700;color:#202124;line-height:1;}";
            html += ".nav-direction{font-size:10px;font-weight:500;color:#5f6368;margin-top:1px;line-height:1;}";
            html += ".nav-street{font-size:12px;font-weight:600;color:#202124;margin-top:1px;line-height:1;}";
            html += ".search-panel{position:absolute;top:15px;left:15px;right:15px;background:white;padding:15px;border-radius:12px;box-shadow:0 4px 20px rgba(0,0,0,0.15);z-index:1000;display:none;}";
            html += ".search-input{width:100%;padding:12px;border:2px solid #e0e0e0;border-radius:8px;font-size:14px;outline:none;}";
            html += ".search-input:focus{border-color:#1a73e8;}";
            html += ".search-buttons{display:flex;gap:10px;margin-top:10px;}";
            html += ".search-results{max-height:200px;overflow-y:auto;margin-top:10px;border-top:1px solid #e0e0e0;padding-top:10px;display:none;}";
            html += ".result-item{padding:10px;cursor:pointer;border-radius:6px;margin-bottom:5px;border:1px solid #e0e0e0;}";
            html += ".result-item:hover{background:#f5f5f5;}";
            html += ".result-name{font-weight:600;color:#202124;}";
            html += ".result-address{font-size:12px;color:#5f6368;margin-top:2px;}";
            html += ".error-panel{position:fixed;top:50%;left:50%;transform:translate(-50%,-50%);background:white;padding:20px;border-radius:12px;box-shadow:0 8px 32px rgba(0,0,0,0.3);z-index:2000;display:none;min-width:280px;text-align:center;}";
            html += ".error-title{font-size:16px;font-weight:600;color:#ea4335;margin-bottom:8px;}";
            html += ".error-message{font-size:14px;color:#5f6368;margin-bottom:15px;}";
            html += ".error-button{background:#1a73e8;color:white;border:none;padding:8px 16px;border-radius:6px;cursor:pointer;font-size:14px;}";
            html += ".start-location-panel{position:fixed;top:50%;left:50%;transform:translate(-50%,-50%);background:white;padding:25px;border-radius:16px;box-shadow:0 8px 32px rgba(0,0,0,0.3);z-index:2000;min-width:320px;text-align:center;display:none;}";
            html += ".start-location-title{font-size:18px;font-weight:600;color:#202124;margin-bottom:10px;}";
            html += ".start-location-subtitle{font-size:14px;color:#5f6368;margin-bottom:20px;}";
            html += ".start-location-buttons{display:flex;flex-direction:column;gap:10px;}";
            html += ".btn-gps{background:#34a853;color:white;border:none;padding:12px;border-radius:8px;cursor:pointer;font-size:14px;font-weight:600;}";
            html += ".btn-search-location{background:#1a73e8;color:white;border:none;padding:12px;border-radius:8px;cursor:pointer;font-size:14px;font-weight:600;}";
            html += ".btn-use-default{background:#5f6368;color:white;border:none;padding:10px;border-radius:8px;cursor:pointer;font-size:13px;}";
            html += ".gps-help{font-size:11px;color:#ea4335;margin-top:10px;padding:8px;background:#fef7e0;border-radius:6px;text-align:left;line-height:1.4;}";

            // DARK MODE: Inject extra CSS if dark mode is active
            if (ThemeService.Instance.IsDarkMode)
            {
                html += "body{background:#12121A;color:#E8E8F0;}";
                html += ".bottom-panel{background:#1E1E2E;box-shadow:0 -2px 12px rgba(0,0,0,0.5);}";
                html += ".route-title{color:#E8E8F0;}.route-subtitle{color:#A0A0B8;}";
                html += ".nav-instruction{background:#2A2A3E;}.nav-distance{color:#E8E8F0;}.nav-direction{color:#A0A0B8;}.nav-street{color:#E8E8F0;}";
                html += ".navigation-info{background:#1A1A2E;color:#E8E8F0;}";
                html += ".btn-location{background:#2A2A3E;border-color:#44445A;color:#E8E8F0;}";
                html += ".btn-compass{background:#2A2A3E;border-color:#44445A;}";
                html += ".search-panel{background:#1E1E2E;}.search-input{background:#2A2A3E;border-color:#44445A;color:#E8E8F0;}";
                html += ".result-item{background:#2A2A3E;border-color:#44445A;}.result-item:hover{background:#333350;}";
                html += ".result-name{color:#E8E8F0;}.result-address{color:#A0A0B8;}";
                html += ".error-panel{background:#1E1E2E;}.error-message{color:#A0A0B8;}";
                html += ".start-location-panel{background:#1E1E2E;}.start-location-title{color:#E8E8F0;}.start-location-subtitle{color:#A0A0B8;}";
                html += ".suggestion-text{color:#A0A0B8;}";
                // Dark map tiles via Leaflet CSS filter
                html += ".leaflet-tile{filter:invert(100%) hue-rotate(180deg) brightness(0.9) saturate(0.85);}";
                html += ".leaflet-container{background:#12121A;}";
            }

            html += "</style></head><body>";
            html += "<div id='map'></div>";
            html += "<div class='top-controls'>";
            html += "<button class='btn-test-mode' onclick='toggleTestMode()' id='btnTestMode'>🧪 Test Mode</button>";
            html += "</div>";
            html += "<button class='btn-location' onclick='recenterToUser()' title='Về vị trí hiện tại'>📍</button>";
            html += "<button class='btn-compass' onclick='toggleCompass()' id='btnCompass' title='Bật/Tắt la bàn'><div class='compass-arrow' id='compassArrow'></div></button>";
            html += "<div class='search-panel' id='searchPanel'>";
            html += "<input type='text' class='search-input' id='searchInput' placeholder='Nhập tên địa điểm (VD: Bến Thành, Bitexco, Nguyễn Huệ...)' onkeypress='handleSearchKeyPress(event)'>";
            html += "<div class='search-buttons'>";
            html += "<button class='btn-primary' id='btnSearch' onclick='searchLocation()'></button>";
            html += "<button class='btn-danger' id='btnCancelSearch' onclick='closeSearch()'></button>";
            html += "</div>";
            html += "<div class='search-results' id='searchResults'></div>";
            html += "</div>";
            html += "<div class='bottom-panel auto-show' id='bottomPanel'>";
            html += "<div class='panel-header'>";
            html += "<div>";
            html += "<h3 class='route-title' id='routeTitle'>" + _food.Name + "</h3>";
            html += "<div class='route-subtitle' id='routeSubtitle'>🔄 Đang tính toán đường đi...</div>";
            html += "</div>";
            html += "</div>";
            html += "<div class='navigation-info' id='navigationInfo'></div>";
            html += "<div class='action-buttons'>";
            html += "<button class='btn-start-small' id='btnStart' onclick='startNavigation()'></button>";
            html += "<button class='btn-change-small' id='btnChangeStart' onclick='changeStartPoint()'></button>";
            html += "</div>";
            html += "<div class='suggestion-text' id='suggestionText'></div>";
            html += "</div>";
            html += "<div class='error-panel' id='errorPanel'>";
            html += "<div class='error-title' id='errorTitle'>❌ Lỗi GPS</div>";
            html += "<div class='error-message' id='errorMessage'>Không thể xác định vị trí</div>";
            html += "<button class='error-button' onclick='closeError()'>Đóng</button>";
            html += "</div>";
            html += "<div class='start-location-panel' id='startLocationPanel'>";
            html += "<div class='start-location-title' id='startLocationTitle'></div>";
            html += "<div class='start-location-subtitle' id='startLocationSubtitle'></div>";
            html += "<div class='start-location-buttons'>";
            html += "<button class='btn-gps' id='btnUseGPS' onclick='useGPSLocation()'></button>";
            html += "<button class='btn-search-location' id='btnSearchLocation' onclick='showSearchForStart()'></button>";
            html += "<button class='btn-use-default' id='btnUseDefault' onclick='useDefaultLocation()'></button>";
            html += "</div>";
            html += "<div style='font-size:11px;color:#5f6368;margin-top:12px;text-align:left;line-height:1.5;'>";
            html += "📍 <b>Lưu ý:</b> Nếu máy không có GPS, app sẽ tự động dùng IP để xác định vị trí gần đúng.";
            html += "</div>";
            html += "</div>";
            html += "<script>";
            html += "window.onerror = function(msg, url, lineNo, columnNo, error) {";
            html += "  var message = [";
            html += "    'Message: ' + msg,";
            html += "    'URL: ' + url,";
            html += "    'Line: ' + lineNo,";
            html += "    'Column: ' + columnNo,";
            html += "    'Error object: ' + JSON.stringify(error)";
            html += "  ].join(' - ');";
            html += "  console.error('🛑 [JS ERROR]:', message);";
            html += "  var diag = document.getElementById('diagnosticArea');";
            html += "  if(diag) { diag.style.display = 'block'; diag.innerHTML += '<div>' + message + '</div>'; }";
            html += "  return false;";
            html += "};";
            html += "window.addEventListener('unhandledrejection', function(event) {";
            html += "  console.error('🛑 [JS PROMISE REJECTION]:', event.reason);";
            html += "  var diag = document.getElementById('diagnosticArea');";
            html += "  if(diag) { diag.style.display = 'block'; diag.innerHTML += '<div>Promise Reject: ' + event.reason + '</div>'; }";
            html += "});";
            
            // Get current language from LanguageService
            var currentLang = LanguageService.Instance.CurrentLanguage;
            html += $"var currentLanguage='{currentLang}';";
            html += "var routeCache={};";
            html += "var translations={";
            html += "vi:{";
            html += "selectRestaurant:'Chọn một quán ăn để bắt đầu',";
            html += "calculating:'Đang tính toán đường đi...',";
            html += "startNavigation:'🚀 Bắt đầu',";
            html += "changeStart:'🔄 Đổi xuất phát',";
            html += "changeDestination:'🎯 Đổi điểm đến',";
            html += "navigating:'Đang dẫn đường...',";
            html += "stopNavigation:'⏹ Dừng',";
            html += "ready:'Sẵn sàng bắt đầu',";
            html += "searchPlaceholder:'Nhập tên địa điểm (VD: Bến Thành, Bitexco, Nguyễn Huệ...)',";
            html += "searching:'🔍 Đang tìm kiếm...',";
            html += "noResults:'❌ Không tìm thấy địa điểm nào',";
            html += "gpsError:'Lỗi GPS',";
            html += "gettingLocation:'Đang lấy vị trí GPS...',";
            html += "arrived:'🎯 Đã đến nơi!',";
            html += "suggestion:'💡 Gợi ý: Kéo điểm xanh để thay đổi vị trí xuất phát',";
            html += "selectDestination:'Chọn điểm đến mới',";
            html += "defaultDestination:'Mặc định đến quán này',";
            html += "btnSearch:'🔍 Tìm kiếm',";
            html += "btnCancel:'Hủy',";
            html += "startLocationTitle:'🎯 Chọn điểm xuất phát',";
            html += "startLocationSubtitle:'Bạn muốn xuất phát từ đâu?',";
            html += "btnUseGPS:'📍 Dùng vị trí hiện tại (Tự động)',";
            html += "btnSearchLocation:'🔍 Tìm kiếm địa điểm',";
            html += "btnUseDefault:'🏢 Dùng vị trí mặc định (Bến Thành)'";
            html += "},";
            html += "en:{";
            html += "selectRestaurant:'Select a restaurant to start',";
            html += "calculating:'Calculating route...',";
            html += "startNavigation:'🚀 Start',";
            html += "changeStart:'🔄 Change Start',";
            html += "changeDestination:'🎯 Change Destination',";
            html += "navigating:'Navigating...',";
            html += "stopNavigation:'⏹ Stop',";
            html += "ready:'Ready to start',";
            html += "searchPlaceholder:'Enter place name (e.g. Ben Thanh, Bitexco, Nguyen Hue...)',";
            html += "searching:'🔍 Searching...',";
            html += "noResults:'❌ No places found',";
            html += "gpsError:'GPS Error',";
            html += "gettingLocation:'Getting GPS location...',";
            html += "arrived:'🎯 Arrived!',";
            html += "suggestion:'💡 Tip: Drag the blue marker to change start location',";
            html += "selectDestination:'Select new destination',";
            html += "defaultDestination:'Default to this restaurant',";
            html += "btnSearch:'🔍 Search',";
            html += "btnCancel:'Cancel',";
            html += "startLocationTitle:'🎯 Choose Starting Point',";
            html += "startLocationSubtitle:'Where do you want to start from?',";
            html += "btnUseGPS:'📍 Use Current Location (Auto)',";
            html += "btnSearchLocation:'🔍 Search Location',";
            html += "btnUseDefault:'🏢 Use Default Location (Ben Thanh)'";
            html += "},";
            html += "zh:{";
            html += "selectRestaurant:'选择餐厅开始',";
            html += "calculating:'正在计算路线...',";
            html += "startNavigation:'🚀 开始',";
            html += "changeStart:'🔄 更改起点',";
            html += "changeDestination:'🎯 更改终点',";
            html += "navigating:'正在导航...',";
            html += "stopNavigation:'⏹ 停止',";
            html += "ready:'准备开始',";
            html += "searchPlaceholder:'输入地点名称（例如：边城、Bitexco、阮惠...）',";
            html += "searching:'🔍 搜索中...',";
            html += "noResults:'❌ 未找到地点',";
            html += "gpsError:'GPS错误',";
            html += "gettingLocation:'正在获取GPS位置...',";
            html += "arrived:'🎯 已到达！',";
            html += "suggestion:'💡 提示：拖动蓝色标记更改起点',";
            html += "selectDestination:'选择新目的地',";
            html += "defaultDestination:'默认到此餐厅',";
            html += "btnSearch:'🔍 搜索',";
            html += "btnCancel:'取消',";
            html += "startLocationTitle:'🎯 选择起点',";
            html += "startLocationSubtitle:'您想从哪里出发？',";
            html += "btnUseGPS:'📍 使用当前位置（自动）',";
            html += "btnSearchLocation:'🔍 搜索地点',";
            html += "btnUseDefault:'🏢 使用默认位置（边城市场）'";
            html += "}";
            html += "};";
            html += "var webviewReady=false;";
            html += "window.addEventListener('load',function(){setTimeout(function(){webviewReady=true;console.log('✅ [INIT] WebView ready');updateButtonTexts();},500);});";
            html += "function updateButtonTexts(){";
            html += "var t=translations[currentLanguage];";
            html += "if(!t)t=translations['vi'];";
            html += "document.getElementById('btnStart').innerHTML=t.startNavigation;";
            html += "document.getElementById('btnChangeStart').innerHTML=t.changeStart;";
            html += "document.getElementById('suggestionText').innerHTML=t.suggestion;";
            html += "document.getElementById('searchInput').placeholder=t.searchPlaceholder;";
            html += "document.getElementById('btnSearch').innerHTML=t.btnSearch;";
            html += "document.getElementById('btnCancelSearch').innerHTML=t.btnCancel;";
            html += "document.getElementById('startLocationTitle').innerHTML=t.startLocationTitle;";
            html += "document.getElementById('startLocationSubtitle').innerHTML=t.startLocationSubtitle;";
            html += "document.getElementById('btnUseGPS').innerHTML=t.btnUseGPS;";
            html += "document.getElementById('btnSearchLocation').innerHTML=t.btnSearchLocation;";
            html += "document.getElementById('btnUseDefault').innerHTML=t.btnUseDefault;";
            html += "console.log('🌐 [LANG] Updated button texts to:',currentLanguage);";
            html += "}";
            html += "var foodRaw =" + markersJson + ";";
            html += "var foods = Array.isArray(foodRaw) ? foodRaw : [];";
            html += "var userPos={lat:" + userLat + ",lng:" + userLng + "};";
            html += "var map = null;";
            html += "try {";
            html += "  map = L.map('map', {rotate:true, bearing:0, touchRotate:true}).setView([" + focusLat + "," + focusLng + "], 14);";
            html += "  console.log('✅ [INIT] Map initialized with rotation support');";
            html += "} catch (e) {";
            html += "  console.warn('⚠️ [INIT] Rotation plugin failed, falling back to standard map:', e.message);";
            html += "  map = L.map('map').setView([" + focusLat + "," + focusLng + "], 14);";
            html += "}";
            html += "var routeLine=null,arrowLine=null,userMarker=null,selectingStart=false,currentDest=null,routeSteps=[],currentStepIndex=0,isNavigating=false,navigationInterval=null,fullRouteCoords=[],repeatVoiceTimer=null,lastUserPosition=null,userInteractedWithMap=false,isCalculatingRoute=false;";
            html += "var compassEnabled=false,mapBearing=0;";
            
            // Dùng Google Maps tiles - hoạt động tốt ở Việt Nam, miễn phí cho sử dụng cơ bản
            // Offline fallback: Bản đồ sẽ hiện nền xám nhưng các điểm đánh dấu (marker) vẫn hiện để xem được vị trí tương đối
            html += "L.tileLayer('https://mt1.google.com/vt/lyrs=m&x={x}&y={y}&z={z}',{";
            html += "attribution:'Google Maps',maxZoom:20,subdomains:['mt0','mt1','mt2','mt3']";
            html += "}).addTo(map);";
            
            html += "var userIcon=L.divIcon({html:'<div style=\"width:20px;height:20px;background:#4285f4;border:4px solid white;border-radius:50%;box-shadow:0 2px 8px rgba(0,0,0,0.4),0 0 0 3px rgba(66,133,244,0.3);cursor:move\"></div>',iconSize:[28,28],iconAnchor:[14,14],className:''});";
            html += "userMarker=L.marker([userPos.lat,userPos.lng],{icon:userIcon,zIndexOffset:1000,draggable:true}).addTo(map);";
            html += "userMarker.bindPopup('<div style=\"text-align:center;font-weight:600;color:#1a73e8\">📍 Vị trí xuất phát<br><small style=\"color:#5f6368\">Kéo để di chuyển</small></div>');";
            html += "userMarker.on('dragstart',function(){if(routeLine){map.removeLayer(routeLine);routeLine=null;}if(arrowLine){map.removeLayer(arrowLine);arrowLine=null;}});";
            html += "userMarker.on('dragend',function(){var pos=userMarker.getLatLng();userPos.lat=pos.lat;userPos.lng=pos.lng;window.chrome.webview.postMessage(JSON.stringify({type:'updateUserPosition',lat:pos.lat,lng:pos.lng}));openUserPopupSafely();if(currentDest){var t=translations[currentLanguage];if(currentLanguage==='vi'){document.getElementById('routeSubtitle').innerHTML='🔄 Đang tính lại đường đi từ vị trí mới...';}else if(currentLanguage==='en'){document.getElementById('routeSubtitle').innerHTML='🔄 Recalculating route from new position...';}else{document.getElementById('routeSubtitle').innerHTML='🔄 重新计算路线...';}setTimeout(function(){calculateRoute(currentDest.lat,currentDest.lng,currentDest.name);},100);}});";
            
            html += "function getCategoryIcon(category){";
            html += "var iconMap={";
            html += "'Hải sản':'🦐',";
            html += "'Ốc':'🐚',";
            html += "'Bún':'🍜',";
            html += "'Nướng':'🍢',";
            html += "'Lẩu & Nướng':'🍲',";
            html += "'Bánh Mì':'🥖',";
            html += "'Cơm':'🍚',";
            html += "'Phở':'🍲',";
            html += "'Bánh Khác':'🧁',";
            html += "'Thức uống':'☕',";
            html += "'Cà phê':'☕',";
            html += "'Coffee':'☕',";
            html += "'Trà sữa':'🧋',";
            html += "'Chè':'🍧',";
            html += "'Gỏi cuốn':'🌯',";
            html += "'Nem':'🥟',";
            html += "'Bánh xèo':'🥞',";
            html += "'Lẩu':'🍲',";
            html += "'Hủ tiếu':'🍜',";
            html += "'Mì':'🍜',";
            html += "'Cháo':'🥣',";
            html += "'Xôi':'🍚',";
            html += "'Bánh bao':'🥟',";
            html += "'Bánh cuốn':'🌯',";
            html += "'Bánh tráng':'🍪',";
            html += "'Kem':'🍦',";
            html += "'Sinh tố':'🥤',";
            html += "'Nước ép':'🧃',";
            html += "'Ăn vặt':'🍿',";
            html += "'Đồ nướng':'🍢',";
            html += "'Thịt nướng':'🥩',";
            html += "'Gà rán':'🍗',";
            html += "'Pizza':'🍕',";
            html += "'Burger':'🍔',";
            html += "'Sandwich':'🥪',";
            html += "'Sushi':'🍣',";
            html += "'Ramen':'🍜',";
            html += "'Dimsum':'🥟',";
            html += "'Tráng miệng':'🍰',";
            html += "'Bánh ngọt':'🧁'";
            html += "};";
            html += "return iconMap[category]||'🍽️';";
            html += "}";
            
            html += "var foodIcon=L.divIcon({html:'<div style=\"background:#ea4335;color:white;border-radius:50%;width:24px;height:24px;display:flex;align-items:center;justify-content:center;font-size:12px;box-shadow:0 2px 4px rgba(0,0,0,0.25);border:1.5px solid white\">&#127836;</div>',iconSize:[24,24],iconAnchor:[12,12],popupAnchor:[0,-14],className:''});";
            
            html += "foods.forEach(function(f){";
            html += "var icon=getCategoryIcon(f.category);";
            html += "var customIcon=L.divIcon({html:'<div style=\"background:#ea4335;color:white;border-radius:50%;width:24px;height:24px;display:flex;align-items:center;justify-content:center;font-size:12px;box-shadow:0 2px 4px rgba(0,0,0,0.25);border:1.5px solid white\">'+icon+'</div>',iconSize:[24,24],iconAnchor:[12,12],popupAnchor:[0,-14],className:''});";
            html += "var marker=L.marker([f.lat,f.lng],{icon:customIcon}).addTo(map);";
            html += "var popupContent='<div style=\"min-width:240px;text-align:center\"><h3 style=\"color:#202124;font-size:18px;font-weight:600;margin:0 0 8px\">'+icon+' '+f.name+'</h3><div style=\"color:#f9ab00;font-size:15px;margin:6px 0;font-weight:500\">⭐ '+f.rating+'/5.0</div><div style=\"color:#5f6368;font-size:14px;margin:10px 0;text-align:left\">'+f.desc+'</div></div>';";
            html += "marker.bindPopup(popupContent,{maxWidth:320});";
            html += "marker.on('click',function(){selectRestaurant(f.lat,f.lng,f.name,f.rating);});";
            html += "});";
            
            html += "map.on('click',function(e){if(selectingStart){userMarker.setLatLng(e.latlng);userPos.lat=e.latlng.lat;userPos.lng=e.latlng.lng;selectingStart=false;map.getContainer().style.cursor='';openUserPopupSafely();if(currentDest){var t=translations[currentLanguage];if(currentLanguage==='vi'){document.getElementById('routeSubtitle').innerHTML='🔄 Đang tính lại đường đi từ vị trí mới...';}else if(currentLanguage==='en'){document.getElementById('routeSubtitle').innerHTML='🔄 Recalculating route from new position...';}else{document.getElementById('routeSubtitle').innerHTML='🔄 重新计算路线...';}setTimeout(function(){calculateRoute(currentDest.lat,currentDest.lng,currentDest.name);},300);}}});";
            html += "map.on('dragstart',function(){userInteractedWithMap=true;});";
            html += "map.on('zoomstart',function(){userInteractedWithMap=true;});";
            html += "function showBottomPanelHelper(){";
            html += "document.getElementById('bottomPanel').classList.add('show');";
            html += "document.getElementById('bottomPanel').classList.add('auto-show');";
            html += "setTimeout(updateMapPadding,100);";
            html += "}";
            html += "function hideBottomPanelHelper(){";
            html += "document.getElementById('bottomPanel').classList.remove('show');";
            html += "document.getElementById('bottomPanel').classList.remove('auto-show');";
            html += "setTimeout(updateMapPadding,100);";
            html += "}";
            html += "function updateMapPadding(){";
            html += "if(map&&userMarker&&isNavigating&&!userInteractedWithMap){";
            html += "var userLatLng=userMarker.getLatLng();";
            html += "var offsetLat = getCenterOffset();";
            html += "map.setView([userLatLng.lat + offsetLat, userLatLng.lng], map.getZoom(), {animate:true});";
            html += "}";
            html += "}";
            

            
            html += "function startNavigation(){";
            html += "console.log('🚀 [START] Bắt đầu navigation');";
            html += "if(!currentDest){";
            html += "var errorMsg='Chưa có điểm đến. Vui lòng chọn quán ăn trước.';";
            html += "if(currentLanguage==='en')errorMsg='No destination. Please select a restaurant first.';";
            html += "else if(currentLanguage==='zh')errorMsg='没有目的地。请先选择餐厅。';";
            html += "alert(errorMsg);";
            html += "return;";
            html += "}";
            
            html += "var userLatLng = userMarker.getLatLng();";
            html += "if(fullRouteCoords && fullRouteCoords.length > 0){";
            html += "  var startPoint = L.latLng(fullRouteCoords[0][0], fullRouteCoords[0][1]);";
            html += "  var distFromStart = map.distance(userLatLng, startPoint);";
            html += "  if(distFromStart > 150 && !isCalculatingRoute){";
            html += "     if (navigator.onLine) {";
            html += "         console.log('⚠️ [START] Lộ trình cũ không khớp vị trí hiện tại ('+distFromStart.toFixed(0)+'m). Đang tính lại...');";
            html += "         if(currentLanguage==='vi') document.getElementById('routeSubtitle').innerHTML='🔄 Đang cập nhật lộ trình mới nhất...';";
            html += "         calculateRoute(currentDest.lat, currentDest.lng, currentDest.name);";
            html += "         setTimeout(startNavigation, 1500);"; // Thử lại sau 1.5s
            html += "         return;";
            html += "     } else {";
            html += "         console.warn('⚠️ [START] Lộ trình cũ không khớp nhưng đang offline, bỏ qua việc tính lại.');";
            html += "     }";
            html += "  }";
            html += "}";

            html += "if(!routeSteps||routeSteps.length===0){";
            html += "var errorMsg='Chưa có đường đi. Vui lòng đợi tính toán route hoặc thử lại.';";
            html += "if(currentLanguage==='en')errorMsg='No route available. Please wait for route calculation or try again.';";
            html += "else if(currentLanguage==='zh')errorMsg='没有可用路线。请等待路线计算或重试。';";
            html += "alert(errorMsg);";
            html += "return;";
            html += "}";
            html += "console.log('✅ [START] routeSteps có',routeSteps.length,'bước');";
            html += "isNavigating=true;";
            html += "userInteractedWithMap=false;";
            html += "currentStepIndex=0;";
            html += "lastSpokenStepIndex=-1;";
            html += "lastSpokenInstruction='';";
            html += "lastSpokenTime=0;";
            html += "startRealGPSTracking();";
            html += "var t=translations[currentLanguage];";
            html += "document.getElementById('btnStart').innerHTML='⏹ '+t.stopNavigation;";
            html += "document.getElementById('btnStart').onclick=stopNavigation;";
            html += "document.getElementById('btnStart').style.background='#EA4335';";
            html += "document.getElementById('btnChangeStart').disabled=true;";
            html += "document.getElementById('btnChangeStart').style.opacity='0.5';";
            html += "if(currentLanguage==='vi'){";
            html += "document.getElementById('suggestionText').innerHTML='🧭 Đang chỉ đường - Hãy làm theo hướng dẫn';";
            html += "}else if(currentLanguage==='en'){";
            html += "document.getElementById('suggestionText').innerHTML='🧭 Navigating - Follow the instructions';";
            html += "}else{";
            html += "document.getElementById('suggestionText').innerHTML='🧭 正在导航 - 请按照指示';";
            html += "}";
            html += "document.getElementById('navigationInfo').style.display='block';";
            html += "var userLatLng=userMarker.getLatLng();";
            html += "var offsetLat = getCenterOffset();";
            html += "map.setView([userLatLng.lat + offsetLat, userLatLng.lng], 18, {animate:true});";
            html += "console.log('🔄 [START] Gọi updateNavigation lần đầu');";
            html += "updateNavigation();";
            html += "if(navigationInterval)clearInterval(navigationInterval);";
            html += "navigationInterval=setInterval(updateNavigation,3000);";
            html += "console.log('✅ [START] Navigation đã bắt đầu thành công');";
            html += "console.log('📡 [START] GỬI navStateChanged message SAU KHI set isNavigating=true');";
            html += "if(window.chrome && window.chrome.webview){";
            html += "  window.chrome.webview.postMessage(JSON.stringify({type:'navStateChanged',navigating:true,destinationName:currentDest.name,destinationLat:currentDest.lat,destinationLng:currentDest.lng}));";
            html += "  console.log('✅ [START] Message navStateChanged (true) đã gửi');";
            html += "}else{";
            html += "  console.error('❌ [START] window.chrome.webview KHÔNG tồn tại!');";
            html += "}";
            html += "console.log('📡 [START] Gửi updateUserPosition để trigger ReportPositionToServer');";
            html += "var currentPos = userMarker.getLatLng();";
            html += "if(window.chrome && window.chrome.webview){";
            html += "  window.chrome.webview.postMessage(JSON.stringify({type:'updateUserPosition',lat:currentPos.lat,lng:currentPos.lng}));";
            html += "  console.log('✅ [START] Message updateUserPosition đã gửi');";
            html += "}else{";
            html += "  console.error('❌ [START] window.chrome.webview KHÔNG tồn tại!');";
            html += "}";
            html += "}";
            html += "function stopNavigation(){";
            html += "isNavigating=false;";
            html += "window.chrome.webview.postMessage(JSON.stringify({type:'navStateChanged',navigating:false,destinationName:'',destinationLat:0,destinationLng:0}));";
            html += "stopRealGPSTracking();";
            html += "if(navigationInterval){clearInterval(navigationInterval);navigationInterval=null;}";
            html += "if(repeatVoiceTimer){clearTimeout(repeatVoiceTimer);repeatVoiceTimer=null;}";
            html += "var t=translations[currentLanguage];";
            html += "document.getElementById('navigationInfo').style.display='none';";
            html += "document.getElementById('btnStart').innerHTML='🚀 '+t.startNavigation;";
            html += "document.getElementById('btnStart').onclick=startNavigation;";
            html += "document.getElementById('btnStart').style.background='#34A853';";
            html += "document.getElementById('btnChangeStart').disabled=false;";
            html += "document.getElementById('btnChangeStart').style.opacity='1';";
            html += "if(currentLanguage==='vi'){";
            html += "document.getElementById('suggestionText').innerHTML='💡 Gợi ý: Kéo điểm xanh để thay đổi vị trí xuất phát';";
            html += "}else if(currentLanguage==='en'){";
            html += "document.getElementById('suggestionText').innerHTML='💡 Tip: Drag the blue marker to change start location';";
            html += "}else{";
            html += "document.getElementById('suggestionText').innerHTML='💡 提示: 拖动蓝色标记更改起始位置';";
            html += "}";
            html += "if(currentDest){";
            html += "  map.setView([currentDest.lat,currentDest.lng],14);";
            html += "}";
            html += "}";
            html += "function closePanel(){";
            html += "hideBottomPanelHelper();";
            html += "setTimeout(updateMapPadding,100);";
            html += "if(routeLine){map.removeLayer(routeLine);routeLine=null;}";
            html += "if(arrowLine){map.removeLayer(arrowLine);arrowLine=null;}";
            html += "currentDest=null;routeSteps=[];isNavigating=false;";
            html += "map.setView([" + focusLat + "," + focusLng + "],14);";
            html += "}";
            html += "function changeStartPoint(){";
            html += "document.getElementById('searchPanel').style.display='block';";
            html += "document.getElementById('searchInput').focus();";
            html += "}";
            html += "function handleSearchKeyPress(event){";
            html += "if(event.key==='Enter'){searchLocation();}";
            html += "}";
            html += "function searchLocation(){";
            html += "var query=document.getElementById('searchInput').value.trim();";
            html += "var t=translations[currentLanguage];";
            html += "if(!query){";
            html += "if(currentLanguage==='vi')alert('Vui lòng nhập tên địa điểm');";
            html += "else if(currentLanguage==='en')alert('Please enter place name');";
            html += "else alert('请输入地点名称');";
            html += "return;";
            html += "}";
            html += "document.getElementById('searchResults').innerHTML='<div style=\"padding:10px;text-align:center;color:#5f6368\">'+t.searching+'</div>';";
            html += "document.getElementById('searchResults').style.display='block';";
            
            html += "var queries = [";
            html += "  query + ', Ho Chi Minh City, Vietnam',";
            html += "  query + ', Saigon, Vietnam',";
            html += "  query + ', Vietnam',";
            html += "  query";
            html += "];";
            html += "var allResults = [];";
            html += "var completedQueries = 0;";
            
            html += "queries.forEach(function(q){";
            html += "  var url = 'https://nominatim.openstreetmap.org/search?format=json&q=' + encodeURIComponent(q) + '&limit=5&addressdetails=1';";
            html += "  fetch(url).then(function(res){return res.json();}).then(function(data){";
            html += "    if(data && data.length > 0){";
            html += "      data.forEach(function(item){";
            html += "        var exists = allResults.find(function(r){return Math.abs(r.lat-item.lat)<0.0001 && Math.abs(r.lon-item.lon)<0.0001;});";
            html += "        if(!exists) allResults.push(item);";
            html += "      });";
            html += "    }";
            html += "    completedQueries++;";
            html += "    if(completedQueries === queries.length) displaySearchResults(allResults, t);";
            html += "  }).catch(function(err){";
            html += "    completedQueries++;";
            html += "    if(completedQueries === queries.length) displaySearchResults(allResults, t);";
            html += "  });";
            html += "});";
            html += "}";
            html += "function displaySearchResults(results,t){";
            html += "  var resultsHtml='';";
            html += "  if(results && results.length > 0){";
            html += "    results.slice(0,10).forEach(function(item){";
            html += "      var name=item.display_name.split(',')[0];";
            html += "      var address=item.display_name;";
            html += "      resultsHtml+='<div class=\"result-item\" onclick=\"selectSearchResult('+item.lat+','+item.lon+',\\''+name.replace(/'/g,\"&apos;\")+'\\')\"><div class=\"result-name\">'+name+'</div><div class=\"result-address\">'+address+'</div></div>';";
            html += "    });";
            html += "  }else{";
            html += "    resultsHtml='<div style=\"padding:10px;text-align:center;color:#ea4335\">'+t.noResults+'</div>';";
            html += "    resultsHtml+='<div style=\"padding:10px;font-size:12px;color:#5f6368;text-align:left\">💡 <b>Gợi ý:</b><br>- Thử tìm tên đường hoặc quận (VD: \"Nguyễn Huệ\", \"Quận 1\")<br>- Tìm địa danh nổi tiếng (VD: \"Bến Thành\", \"Bitexco\")<br>- Địa chỉ chi tiết có thể không có trong bản đồ</div>';";
            html += "  }";
            html += "  document.getElementById('searchResults').innerHTML=resultsHtml;";
            html += "}";
            html += "function selectSearchResult(lat,lng,name){";
            html += "userMarker.setLatLng([lat,lng]);";
            html += "userPos.lat=lat;userPos.lng=lng;";
            html += "map.setView([lat,lng],16);";
            html += "openUserPopupSafely();";
            html += "closeSearch();";
            html += "if(currentDest){";
            html += "var t=translations[currentLanguage];";
            html += "if(currentLanguage==='vi'){";
            html += "document.getElementById('routeSubtitle').innerHTML='🔄 Đang tính lại đường đi từ vị trí mới...';";
            html += "}else if(currentLanguage==='en'){";
            html += "document.getElementById('routeSubtitle').innerHTML='🔄 Recalculating route from new position...';";
            html += "}else{";
            html += "document.getElementById('routeSubtitle').innerHTML='?? ????????...';";
            html += "}";
            html += "setTimeout(function(){calculateRoute(currentDest.lat,currentDest.lng,currentDest.name);},100);";
            html += "}";
            html += "}";
            html += "function closeSearch(){";
            html += "document.getElementById('searchPanel').style.display='none';";
            html += "document.getElementById('searchInput').value='';";
            html += "document.getElementById('searchResults').style.display='none';";
            html += "}";
            html += "function getCurrentLocation(){";            
            html += "console.log('📍 [GPS] bắt đầu yêu cầu GPS...');";
            html += "var t=translations[currentLanguage];";
            html += "var loadingMsg=currentLanguage==='vi'?'Đang lấy vị trí GPS...':(currentLanguage==='en'?'Getting GPS location...':'正在获取GPS位置...');";
            html += "showError('🌐 GPS',loadingMsg);";
            html += "var gpsSuccess=false;";
            html += "var ipSuccess=false;";
            html += "function handleSuccess(lat,lng,source){";
            html += "if(gpsSuccess)return;";
            html += "if(source==='gps')gpsSuccess=true;";
            html += "else ipSuccess=true;";
            html += "console.log('✅ ['+source+'] thành công! Lat:',lat,'Lng:',lng);";
            html += "userMarker.setLatLng([lat,lng]);";
            html += "userPos.lat=lat;userPos.lng=lng;";
            html += "map.setView([lat,lng],source==='gps'?17:15);";
            html += "openUserPopupSafely();";
            html += "closeError();";
            html += "if(currentDest){";
            html += "var recalcMsg=currentLanguage==='vi'?'🔄 Đang tính lại đường đi...':(currentLanguage==='en'?'🔄 Recalculating route...':'🔄 重新计算路线...');";
            html += "document.getElementById('routeSubtitle').innerHTML=recalcMsg;";
            html += "setTimeout(function(){calculateRoute(currentDest.lat,currentDest.lng,currentDest.name);},300);";
            html += "}";
            html += "}";
            html += "if(navigator.geolocation){";
            html += "  navigator.geolocation.getCurrentPosition(function(pos){";
            html += "    handleSuccess(pos.coords.latitude,pos.coords.longitude,'gps');";
            html += "  },function(err){";
            html += "    console.warn('⚠️ [GPS] Lỗi/Từ chối:',err.message);";
            html += "    tryIPFallback();";
            html += "  },{enableHighAccuracy:true,timeout:7000,maximumAge:0});";
            html += "} else { tryIPFallback(); }";
            
            html += "function tryIPFallback(){";
            html += "  if(gpsSuccess) return;";
            html += "  console.log('🏢 [GPS] Đang thử định vị bằng IP (Fallback)...');";
            html += "  fetch('https://get.geojs.io/v1/ip/geo.json',{signal:AbortSignal.timeout(5000)})";
            html += "  .then(function(res){return res.json();})";
            html += "  .then(function(data){";
            html += "    if(data.latitude&&data.longitude) handleSuccess(parseFloat(data.latitude),parseFloat(data.longitude),'ip');";
            html += "    else throw new Error('No IP data from geojs');";
            html += "  })";
            html += "  .catch(function(){";
            html += "    fetch('https://ipapi.co/json/',{signal:AbortSignal.timeout(5000)})";
            html += "    .then(function(res){return res.json();})";
            html += "    .then(function(data){";
            html += "      if(data.latitude&&data.longitude) handleSuccess(data.latitude,data.longitude,'ipapi');";
            html += "      else throw new Error('Final failure');";
            html += "    })";
            html += "    .catch(function(){";
            html += "      if(!gpsSuccess){ useDefaultLocation(); }";
            html += "    });";
            html += "  });";
            html += "}";
            html += "}";
            html += "function showError(title,message){";
            html += "document.getElementById('errorTitle').innerHTML=title;";
            html += "document.getElementById('errorMessage').innerHTML=message;";
            html += "document.getElementById('errorPanel').style.display='block';";
            html += "}";
            html += "function closeError(){";
            html += "document.getElementById('errorPanel').style.display='none';";
            html += "}";
            html += "function showStartLocationPanel(){";
            html += "document.getElementById('startLocationPanel').style.display='block';";
            html += "}";
            html += "function hideStartLocationPanel(){";
            html += "document.getElementById('startLocationPanel').style.display='none';";
            html += "}";
            html += "function useGPSLocation(){";
            html += "hideStartLocationPanel();";
            html += "currentDest={lat:" + _food.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",lng:" + _food.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",name:'" + _food.Name.Replace("\"", "&quot;").Replace("'", "\\'") + "',rating:5.0};";
            html += "updateDestinationTitle(currentDest.name);";
            html += "showBottomPanelHelper();";
            html += "var t=translations[currentLanguage];";
            html += "if(currentLanguage==='vi'){";
            html += "document.getElementById('routeSubtitle').innerHTML='📍 Đang lấy vị trí GPS...';";
            html += "}else if(currentLanguage==='en'){";
            html += "document.getElementById('routeSubtitle').innerHTML='📍 Getting GPS location...';";
            html += "}else{";
            html += "document.getElementById('routeSubtitle').innerHTML='📍 正在获取GPS位置...';";
            html += "}";
            html += "getCurrentLocation();";
            html += "}";
            
            html += "function getCenterOffset(){";
            html += "  var topBarHeight = 100;";
            html += "  var bottomPanel = document.getElementById('bottomPanel');";
            html += "  var isBottomVisible = bottomPanel && (bottomPanel.classList.contains('show') || bottomPanel.classList.contains('auto-show'));";
            html += "  var bottomBarHeight = isBottomVisible ? 120 : 0;";
            html += "  var mapHeight = map.getSize().y;";
            html += "  var visibleHeight = mapHeight - topBarHeight - bottomBarHeight;";
            html += "  var centerY = topBarHeight + (visibleHeight / 2);";
            html += "  var offsetPixels = (mapHeight / 2) - centerY;";
            html += "  var zoom = map.getZoom();";
            html += "  var metersPerPixel = 156543.03392 * Math.cos(userPos.lat * Math.PI / 180) / Math.pow(2, zoom);";
            html += "  var offsetLat = offsetPixels * metersPerPixel / 111320;";
            html += "  console.log('📐 [Offset] Map:'+mapHeight+'px Top:'+topBarHeight+'px Bottom:'+bottomBarHeight+'px → Offset:'+offsetLat.toFixed(6)+'° ('+offsetPixels.toFixed(0)+'px)');";
            html += "  return offsetLat;";
            html += "}";
            html += "function getFitBoundsPadding(){";
            html += "  var topBarHeight = 100;";
            html += "  var bottomPanel = document.getElementById('bottomPanel');";
            html += "  var isBottomVisible = bottomPanel && (bottomPanel.classList.contains('show') || bottomPanel.classList.contains('auto-show'));";
            html += "  var bottomBarHeight = isBottomVisible ? 130 : 20;";
            html += "  var leftPadding = 20;";
            html += "  var rightPadding = 20;";
            html += "  console.log('📐 [FitBounds] Padding - Top:'+topBarHeight+'px Bottom:'+bottomBarHeight+'px Left:'+leftPadding+'px Right:'+rightPadding+'px');";
            html += "  return {paddingTopLeft:[leftPadding,topBarHeight],paddingBottomRight:[rightPadding,bottomBarHeight],maxZoom:17};";
            html += "}";
            html += "function recenterToUser(){";
            html += "  if(userPos.lat && userPos.lng){";
            html += "    var offsetLat = getCenterOffset();"; // ✅ Luôn luôn dùng offset
            html += "    map.setView([userPos.lat + offsetLat, userPos.lng], 18, {animate:true});";
            html += "    if(userMarker) openUserPopupSafely();";
            html += "  } else {";
            html += "    getCurrentLocation();";
            html += "  }";
            html += "}";
            html += "function openUserPopupSafely(){";
            html += "  if(!userMarker) return;";
            html += "  userMarker.openPopup();";
            html += "  setTimeout(function(){";
            html += "    var popup=userMarker.getPopup();";
            html += "    if(!popup||!popup.isOpen()) return;";
            html += "    var popupEl=popup._container;";
            html += "    if(!popupEl) return;";
            html += "    var rect=popupEl.getBoundingClientRect();";
            html += "    var mapHeight=map.getSize().y;";
            html += "    var bottomPanel=document.getElementById('bottomPanel');";
            html += "    var isBottomVisible=bottomPanel&&(bottomPanel.classList.contains('show')||bottomPanel.classList.contains('auto-show'));";
            html += "    var taskbarHeight=isBottomVisible?130:20;";
            html += "    var popupBottom=rect.bottom;";
            html += "    var taskbarTop=mapHeight-taskbarHeight;";
            html += "    if(popupBottom>taskbarTop){";
            html += "      var overlapPixels=popupBottom-taskbarTop+20;";
            html += "      var currentCenter=map.getCenter();";
            html += "      var zoom=map.getZoom();";
            html += "      var metersPerPixel=156543.03392*Math.cos(currentCenter.lat*Math.PI/180)/Math.pow(2,zoom);";
            html += "      var offsetLat=overlapPixels*metersPerPixel/111320;";
            html += "      map.panBy([0,-overlapPixels],{animate:true,duration:0.3});";
            html += "      console.log('📍 [Popup] Tự động pan map lên',overlapPixels.toFixed(0)+'px để không che popup');";
            html += "    }";
            html += "  },100);";
            html += "}";
            html += "function showSearchForStart(){";
            html += "hideStartLocationPanel();";
            html += "currentDest={lat:" + _food.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",lng:" + _food.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",name:'" + _food.Name.Replace("\"", "&quot;").Replace("'", "\\'") + "',rating:5.0};";
            html += "updateDestinationTitle(currentDest.name);";
            html += "showBottomPanelHelper();";
            html += "var t=translations[currentLanguage];";
            html += "if(currentLanguage==='vi'){";
            html += "document.getElementById('routeSubtitle').innerHTML='🔍 Tìm kiếm điểm xuất phát...';";
            html += "}else if(currentLanguage==='en'){";
            html += "document.getElementById('routeSubtitle').innerHTML='🔍 Search for start location...';";
            html += "}else{";
            html += "document.getElementById('routeSubtitle').innerHTML='🔍 搜索起点...';";
            html += "}";
            html += "changeStartPoint();";
            html += "}";
            html += "function useDefaultLocation(){";
            html += "hideStartLocationPanel();";
            html += "autoSelectDefaultRestaurant();";
            html += "}";
            
            html += "function updateDestinationTitle(restaurantName){";
            html += "var suffix='';";
            html += "if(currentLanguage==='vi'){";
            html += "suffix=' (đã chọn)';";
            html += "}else if(currentLanguage==='en'){";
            html += "suffix=' (Selected)';";
            html += "}else{";
            html += "suffix=' (已选择)';";
            html += "}";
            html += "document.getElementById('routeTitle').innerHTML=restaurantName+suffix;";
            html += "}";
            html += "function updateUILanguage(){";
            html += "var t=translations[currentLanguage];";
            html += "document.getElementById('searchInput').placeholder=t.searchPlaceholder;";
            html += "document.getElementById('suggestionText').innerHTML=t.suggestion;";
            html += "document.getElementById('btnStart').innerHTML=isNavigating?t.stopNavigation:t.startNavigation;";
            html += "document.getElementById('btnChangeStart').innerHTML=t.changeStart;";
            html += "if(currentDest){";
            html += "updateDestinationTitle(currentDest.name);";
            html += "if(!isNavigating){";
            html += "if(currentLanguage==='vi'){";
            html += "document.getElementById('routeSubtitle').innerHTML='✅ Sẵn sàng bắt đầu chỉ đường!';";
            html += "}else if(currentLanguage==='en'){";
            html += "document.getElementById('routeSubtitle').innerHTML='✅ Ready to start navigation!';";
            html += "}else{";
            html += "document.getElementById('routeSubtitle').innerHTML='✅ 准备开始导航!';";
            html += "}";
            html += "}";
            html += "}";
            html += "}";
            html += "function selectRestaurant(lat,lng,name,rating){";
            html += "currentDest={lat:lat,lng:lng,name:name,rating:rating};";
            html += "map.closePopup();";
            html += "showBottomPanel(name);";
            html += "var food = foods.find(f => f.name === name);";
            html += "if(food) speakWeb(food.desc, currentLanguage, true);";
            html += "calculateRoute(lat,lng,name);";
            html += "}";
            html += "function autoSelectDefaultRestaurant(){";
            html += "currentDest={lat:" + _food.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",lng:" + _food.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",name:'" + _food.Name.Replace("\"", "&quot;").Replace("'", "\\'") + "',rating:5.0};";
            html += "updateDestinationTitle(currentDest.name);";
            html += "var t=translations[currentLanguage];";
            html += "if(currentLanguage==='vi'){";
            html += "document.getElementById('routeSubtitle').innerHTML='🔄 Đang tính toán đường đi tự động...';";
            html += "}else if(currentLanguage==='en'){";
            html += "document.getElementById('routeSubtitle').innerHTML='🔄 Calculating route automatically...';";
            html += "}else{";
            html += "document.getElementById('routeSubtitle').innerHTML='🔄 自动计算路线...';";
            html += "}";
            html += "showBottomPanelHelper();";
            html += "setTimeout(function(){";
            html += "calculateRoute(currentDest.lat,currentDest.lng,currentDest.name);";
            html += "map.setView([currentDest.lat,currentDest.lng],15);";
            html += "},500);";
            html += "}";
            html += "function showBottomPanel(name){";
            html += "var t=translations[currentLanguage];";
            html += "document.getElementById('routeTitle').innerHTML=name;";
            html += "document.getElementById('routeSubtitle').innerHTML=t.calculating;";
            html += "showBottomPanelHelper();";
            html += "}";
            html += "function changeDestination(){";
            html += "if(isNavigating){";
            html += "alert('Đang chỉ đường. Vui lòng dừng navigation trước khi đổi điểm đến.');";
            html += "return;";
            html += "}";
            html += "var t=translations[currentLanguage];";
            html += "var restaurantList='';";
            html += "var promptTitle='';";
            html += "var promptFooter='';";
            html += "if(currentLanguage==='vi'){";
            html += "promptTitle='🍽️ CHỌN QUÁN ĂN MỚI\\n';";
            html += "promptFooter='\\n📝 Nhập số thứ tự (1-'+foods.length+') để chọn quán:';";
            html += "}else if(currentLanguage==='en'){";
            html += "promptTitle='🍽️ SELECT NEW RESTAURANT\\n';";
            html += "promptFooter='\\n📝 Enter number (1-'+foods.length+') to select:';";
            html += "}else{";
            html += "promptTitle='🍽️ 选择新餐厅\\n';";
            html += "promptFooter='\\n📝 输入数字 (1-'+foods.length+') 选择:';";
            html += "}";
            html += "foods.forEach(function(f,index){";
            html += "var isSelected=currentDest&&f.name===currentDest.name?' ✅':'';";
            html += "restaurantList+=(index+1)+'. '+f.name+' ⭐'+f.rating+'/5.0'+isSelected+'\\n';";
            html += "});";
            html += "var choice=prompt(promptTitle+'\\n'+restaurantList+promptFooter);";
            html += "if(choice&&!isNaN(choice)){";
            html += "var index=parseInt(choice)-1;";
            html += "if(index>=0&&index<foods.length){";
            html += "var selectedFood=foods[index];";
            html += "currentDest={lat:selectedFood.lat,lng:selectedFood.lng,name:selectedFood.name,rating:selectedFood.rating};";
            html += "updateDestinationTitle(selectedFood.name);";
            html += "document.getElementById('routeSubtitle').innerHTML='Đang tính toán đường đi mới...';";
            html += "calculateRoute(selectedFood.lat,selectedFood.lng,selectedFood.name);";
            html += "map.setView([selectedFood.lat,selectedFood.lng],15);";
            html += "}else{";
            html += "alert('Số không hợp lệ. Vui lòng chọn từ 1 đến '+foods.length);";
            html += "}";
            html += "}";
            html += "}";
            html += "function calculateRoute(lat,lng,name){";
            html += "if(isCalculatingRoute) return;";
            html += "isCalculatingRoute=true;";
            html += "console.log('🔄 [Route] Bắt đầu tính toán đến:', name);";
            html += "if(routeLine){map.removeLayer(routeLine);routeLine=null;}";
            html += "if(arrowLine){map.removeLayer(arrowLine);arrowLine=null;}";
            html += "routeSteps=[];";
            html += "var userLatLng=userMarker.getLatLng();";
            html += "var dist=(map.distance(userLatLng,[lat,lng])/1000).toFixed(1);";
            html += "var cacheKey=userLatLng.lat.toFixed(4)+','+userLatLng.lng.toFixed(4)+'->'+lat.toFixed(4)+','+lng.toFixed(4);";
            html += "if(routeCache[cacheKey]){";
            html += "console.log('📋 [Cache] Dùng route đã lưu');";
            html += "var cached=routeCache[cacheKey];";
            html += "routeLine=L.polyline(cached.coords,{color:'#1A73E8',weight:6,opacity:1,lineJoin:'round',lineCap:'round',smoothFactor:1}).addTo(map);";
            html += "fullRouteCoords=cached.coords;";
            html += "routeSteps=cached.steps;";
            html += "console.log('✅ [Cache] Route có',routeSteps.length,'steps');";
            html += "var bounds=L.latLngBounds([userLatLng,[lat,lng]]);";
            html += "map.fitBounds(bounds,getFitBoundsPadding());";
            html += "var t=translations[currentLanguage];";
            html += "if(currentLanguage==='vi'){";
            html += "document.getElementById('routeSubtitle').innerHTML='🚗 '+cached.distance+' km • '+cached.duration+' phút';";
            html += "}else if(currentLanguage==='en'){";
            html += "document.getElementById('routeSubtitle').innerHTML='🚗 '+cached.distance+' km • '+cached.duration+' min';";
            html += "}else{";
            html += "document.getElementById('routeSubtitle').innerHTML='🚗 '+cached.distance+' km • '+cached.duration+' 分钟';";
            html += "}";
            html += "isCalculatingRoute=false;";
            html += "return;";
            html += "}";
            html += "console.log('🗺️ [Route] Tính toán đường đi thực tế từ',userLatLng,'đến',[lat,lng]);";
            html += "var startTime=Date.now();";
            
            // Try OSRM first (best for Vietnam)
            html += "var osrmUrl='https://router.project-osrm.org/route/v1/driving/'+userLatLng.lng+','+userLatLng.lat+';'+lng+','+lat+'?overview=full&geometries=geojson&steps=true&alternatives=false';";
            html += "console.log('🌐 [Route] Thử OSRM API...');";
            html += "  var osrmUrl='https://router.project-osrm.org/route/v1/driving/'+userLatLng.lng+','+userLatLng.lat+';'+lng+','+lat+'?overview=full&geometries=geojson&steps=true&alternatives=false';";
            html += "  console.log('🌐 [Route] Thử OSRM API...');";
            html += "  fetch(osrmUrl,{signal:AbortSignal.timeout(15000)})";
            html += ".then(function(res){";
            html += "  if(!res.ok){";
            html += "    console.warn('⚠️ [Route] OSRM failed with status:',res.status);";
            html += "    throw new Error('OSRM failed');";
            html += "  }";
            html += "  return res.json();";
            html += "})";
            html += ".then(function(data){";
            html += "  var elapsed=Date.now()-startTime;";
            html += "  console.log('✅ [Route] OSRM thành công trong',elapsed,'ms');";
            html += "  if(data.code==='Ok'&&data.routes&&data.routes[0]){";
            html += "    var coords=data.routes[0].geometry.coordinates.map(function(c){return[c[1],c[0]];});";
            html += "    fullRouteCoords=coords;";
            html += "    var distance=(data.routes[0].distance/1000).toFixed(1);";
            html += "    var duration=Math.round(data.routes[0].duration/60);";
            html += "    var steps=data.routes[0].legs&&data.routes[0].legs[0].steps?data.routes[0].legs[0].steps:[];";
            html += "    routeCache[cacheKey]={coords:coords,distance:distance,duration:duration,steps:steps};";
            html += "    routeLine=L.polyline(coords,{color:'#1A73E8',weight:6,opacity:1,lineJoin:'round',lineCap:'round',smoothFactor:1}).addTo(map);";
            html += "    console.log('🗺️ [Route] Đã vẽ đường đi thực tế với',coords.length,'điểm');";
            html += "    var bounds=L.latLngBounds([userLatLng,[lat,lng]]);";
            html += "    map.fitBounds(bounds,getFitBoundsPadding());";
            html += "    var t=translations[currentLanguage];";
            html += "    if(currentLanguage==='vi'){";
            html += "      document.getElementById('routeSubtitle').innerHTML='🚗 '+distance+' km • '+duration+' phút';";
            html += "    }else if(currentLanguage==='en'){";
            html += "      document.getElementById('routeSubtitle').innerHTML='🚗 '+distance+' km • '+duration+' min';";
            html += "    }else{";
            html += "      document.getElementById('routeSubtitle').innerHTML='🚗 '+distance+' km • '+duration+' 分钟';";
            html += "    }";
            html += "    if(steps.length>0){routeSteps=steps;currentStepIndex=0;}";
            html += "    console.log('✅ Đường đi thực tế đã được tính toán. Steps:',routeSteps.length);";
            html += "    console.log('📋 [DEBUG] Route steps:', JSON.stringify(steps.slice(0,5).map(function(s){return {type:s.maneuver.type,modifier:s.maneuver.modifier,name:s.name,distance:Math.round(s.distance)+'m'};})));";
            html += "    try { var routeData = {coords:coords, distance:distance, duration:duration, steps:steps, start:[userLatLng.lat, userLatLng.lng], end:[lat, lng]}; localStorage.setItem('route_' + name, JSON.stringify(routeData)); console.log('💾 [Offline] Đã lưu cache lộ trình cho', name); } catch(e) { console.warn('Lỗi lưu cache:', e); }";
            html += "    isCalculatingRoute=false;";
            html += "  }else{";
            html += "    console.warn('⚠️ [Route] OSRM response invalid');";
            html += "    throw new Error('Invalid OSRM response');";
            html += "  }";
            html += "})";
            html += ".catch(function(err){";
            html += "  console.warn('⚠️ [Route] OSRM failed:',err.message,'- Thử OpenRouteService...');";
            html += "  var orsUrl='https://api.openrouteservice.org/v2/directions/driving-car?start='+userLatLng.lng+','+userLatLng.lat+'&end='+lng+','+lat;";
            html += "  fetch(orsUrl,{";
            html += "    headers:{'Accept':'application/json, application/geo+json, application/gpx+xml, img/png; charset=utf-8'},";
            html += "    signal:AbortSignal.timeout(15000)";
            html += "  })";
            html += "  .then(function(res){";
            html += "    if(!res.ok){";
            html += "      console.warn('⚠️ [Route] OpenRouteService failed with status:',res.status);";
            html += "      throw new Error('ORS failed');";
            html += "    }";
            html += "    return res.json();";
            html += "  })";
            html += "  .then(function(data){";
            html += "    var elapsed=Date.now()-startTime;";
            html += "    console.log('✅ [Route] OpenRouteService thành công trong',elapsed,'ms');";
            html += "    if(data.features&&data.features[0]&&data.features[0].geometry){";
            html += "      var coords=data.features[0].geometry.coordinates.map(function(c){return[c[1],c[0]];});";
            html += "      fullRouteCoords=coords;";
            html += "      var distance=(data.features[0].properties.segments[0].distance/1000).toFixed(1);";
            html += "      var duration=Math.round(data.features[0].properties.segments[0].duration/60);";
            html += "      var steps=data.features[0].properties.segments[0].steps||[];";
            html += "      routeSteps=steps.map(function(s){";
            html += "        return {";
            html += "          maneuver:{";
            html += "            type:s.type===0?'depart':(s.type===10?'arrive':'turn'),";
            html += "            modifier:s.type===1?'left':(s.type===2?'right':'straight'),";
            html += "            location:[s.way_points[0][0],s.way_points[0][1]]";
            html += "          },";
            html += "          name:s.name||'',";
            html += "          distance:s.distance";
            html += "        };";
            html += "      });";
            html += "      routeCache[cacheKey]={coords:coords,distance:distance,duration:duration,steps:routeSteps};";
            html += "      routeLine=L.polyline(coords,{color:'#1A73E8',weight:6,opacity:1,lineJoin:'round',lineCap:'round',smoothFactor:1}).addTo(map);";
            html += "      console.log('🗺️ [Route] Đã vẽ đường đi thực tế (ORS) với',coords.length,'điểm');";
            html += "      var bounds=L.latLngBounds([userLatLng,[lat,lng]]);";
            html += "      map.fitBounds(bounds,getFitBoundsPadding());";
            html += "      var t=translations[currentLanguage];";
            html += "      if(currentLanguage==='vi'){";
            html += "        document.getElementById('routeSubtitle').innerHTML='🚗 '+distance+' km • '+duration+' phút';";
            html += "      }else if(currentLanguage==='en'){";
            html += "        document.getElementById('routeSubtitle').innerHTML='🚗 '+distance+' km • '+duration+' min';";
            html += "      }else{";
            html += "        document.getElementById('routeSubtitle').innerHTML='🚗 '+distance+' km • '+duration+' 分钟';";
            html += "      }";
            html += "      currentStepIndex=0;";
            html += "      console.log('✅ Đường đi thực tế (ORS) đã được tính toán. Steps:',routeSteps.length);";
            html += "      try { var routeData = {coords:coords, distance:distance, duration:duration, steps:routeSteps, start:[userLatLng.lat, userLatLng.lng], end:[lat, lng]}; localStorage.setItem('route_' + name, JSON.stringify(routeData)); console.log('💾 [Offline] Đã lưu cache lộ trình (ORS) cho', name); } catch(e) { console.warn('Lỗi lưu cache:', e); }";
            html += "      isCalculatingRoute=false;";
            html += "    }else{";
            html += "      console.warn('⚠️ [Route] ORS response invalid');";
            html += "      throw new Error('Invalid ORS response');";
            html += "    }";
            html += "  })";
            html += "  .catch(function(err2){";
            html += "    console.error('❌ [Route] Cả 2 API đều thất bại. OSRM:',err.message,'ORS:',err2.message);";
            html += "    console.error('❌ [Route] Fallback về đường chim bay (không khuyến nghị)');";
            html += "    isCalculatingRoute=false;";
            html += "    drawOfflineRoute(userLatLng,lat,lng,name,dist);";
            html += "  });";
            html += "});";
            html += "}";
            
            html += "function drawOfflineRoute(userLatLng,lat,lng,name,dist){";
            html += "var t=translations[currentLanguage];";
            html += "try {";
            html += "  var cached = localStorage.getItem('route_' + name);";
            html += "  if(cached) {";
            html += "    var data = JSON.parse(cached);";
            html += "    var startPt = L.latLng(data.start[0], data.start[1]);";
            html += "    if(map.distance(userLatLng, startPt) < 2000) {";
            html += "      console.log('✅ [Offline] Tìm thấy lộ trình lưu sẵn cho', name);";
            html += "      fullRouteCoords = data.coords;";
            html += "      routeSteps = data.steps;";
            html += "      currentStepIndex = 0;";
            html += "      routeLine = L.polyline(data.coords, {color:'#1A73E8',weight:6,opacity:1,lineJoin:'round',lineCap:'round',smoothFactor:1}).addTo(map);";
            html += "      var bnds = L.latLngBounds([userLatLng, [lat, lng]]);";
            html += "      map.fitBounds(bnds, getFitBoundsPadding());";
            html += "      if(currentLanguage==='vi') document.getElementById('routeSubtitle').innerHTML='🚗 '+data.distance+' km • '+data.duration+' phút (Ngoại tuyến)';";
            html += "      else if(currentLanguage==='en') document.getElementById('routeSubtitle').innerHTML='🚗 '+data.distance+' km • '+data.duration+' min (Offline)';";
            html += "      else document.getElementById('routeSubtitle').innerHTML='🚗 '+data.distance+' km • '+data.duration+' 分钟 (离线)';";
            html += "      return;";
            html += "    }";
            html += "  }";
            html += "} catch(e) { console.error('Lỗi đọc cache offline:', e); }";
            
            html += "console.warn('⚠️ [Route] Sử dụng đường chim bay (không chính xác) - API routing thất bại');";
            html += "var coords=[[userLatLng.lat,userLatLng.lng],[lat,lng]];";
            html += "fullRouteCoords=coords;";
            html += "routeLine=L.polyline(coords,{color:'#EA4335',weight:6,opacity:0.6,dashArray:'10, 10',lineJoin:'round',lineCap:'round'}).addTo(map);";
            html += "var bounds=L.latLngBounds([userLatLng,[lat,lng]]);";
            html += "map.fitBounds(bounds,getFitBoundsPadding());";
            html += "var time=Math.round((parseFloat(dist)/30)*60);";
            
            html += "if(currentLanguage==='vi'){";
            html += "document.getElementById('routeSubtitle').innerHTML='⚠️ ~'+dist+' km • ~'+time+' phút (đường chim bay - không chính xác)';";
            html += "}else if(currentLanguage==='en'){";
            html += "document.getElementById('routeSubtitle').innerHTML='⚠️ ~'+dist+' km • ~'+time+' min (straight line - not accurate)';";
            html += "}else{";
            html += "document.getElementById('routeSubtitle').innerHTML='⚠️ ~'+dist+' km • ~'+time+' 分钟 (直线 - 不准确)';";
            html += "}";
            html += "routeSteps=[{";
            html += "maneuver:{type:'depart',location:[userLatLng.lng,userLatLng.lat]},";
            html += "name:'Đường đi trực tiếp (không theo đường phố)',";
            html += "distance:parseFloat(dist)*1000";
            html += "},{";
            html += "maneuver:{type:'arrive',location:[lng,lat]},";
            html += "name:'Điểm đến',";
            html += "distance:0";
            html += "}];";
            html += "currentStepIndex=0;";
            html += "isNavigating=false;";
            html += "console.log('⚠️ Đường chim bay đã được hiển thị (fallback)');";
            html += "}";

            html += "function updateNavigation(){";
            html += "if(!routeSteps||routeSteps.length===0||!isNavigating) return;";
            html += "var userLatLng=userMarker.getLatLng();";
            
            html += "var step = routeSteps[currentStepIndex];";
            html += "var stepLocation = step.maneuver.location;";
            html += "var distToStep = map.distance(userLatLng, [stepLocation[1], stepLocation[0]]);";
            
            html += "if(distToStep < 20 && currentStepIndex < routeSteps.length - 1){";
            html += "    console.log('⏭️ [NAV] Chuyển chặng:', currentStepIndex, '->', currentStepIndex+1);";
            html += "    currentStepIndex++;";
            html += "    step = routeSteps[currentStepIndex];";
            html += "    stepLocation = step.maneuver.location;";
            html += "    distToStep = map.distance(userLatLng, [stepLocation[1], stepLocation[0]]);";
            html += "    window.spokenPhases = {step: -1, phase: '', lastDist: 0};"; // Reset voice for new step
            html += "}";

            html += "var totalDistance=0;";
            html += "for(var i=currentStepIndex;i<routeSteps.length;i++){ totalDistance+=routeSteps[i].distance; }";
            html += "var distRemaining=totalDistance<1000?Math.round(totalDistance)+' m':(totalDistance/1000).toFixed(1)+' km';";
            html += "var timeMin=Math.round(totalDistance/500);";
            
            html += "if(currentStepIndex>=routeSteps.length-1 && distToStep < 15){";
            html += "    var arrivedMsg = currentLanguage==='vi'?'Bạn đã đến nơi. Chúc bạn ngon miệng!':'You have arrived!';";
            html += "    if(currentLanguage==='vi'){";
            html += "        document.getElementById('navigationInfo').innerHTML='<div style=\"font-size:18px;font-weight:600;color:#34A853\">🎯 Đã đến nơi!</div>';";
            html += "        document.getElementById('routeSubtitle').innerHTML='✅ Hoàn thành chuyến đi';";
            html += "    }";
            html += "    sendVoiceNavigation(arrivedMsg, true);";
            html += "    isNavigating = false;";
            html += "    if(navigationInterval){clearInterval(navigationInterval);navigationInterval=null;}";
            html += "    return;";
            html += "}";

            html += "var instruction = getInstruction(step);";
            html += "var type = step.maneuver.type;";
            html += "console.log('🧭 [NAV] Current step:', currentStepIndex, 'Type:', type, 'Name:', step.name, 'Distance:', Math.round(distToStep)+'m');";

            html += "window.spokenPhases = window.spokenPhases || {step:-1, phase:'', lastDist: 0};";
            html += "var phase = '';";
            html += "if(distToStep <= 40) phase = 'action';";
            html += "else if(distToStep <= 180) phase = 'prep';";
            html += "else if(distToStep > 450) phase = 'long';";
            
            html += "var distDiff = Math.abs(window.spokenPhases.lastDist - distToStep);";
            html += "var shouldSpeak = (window.spokenPhases.step !== currentStepIndex || window.spokenPhases.phase !== phase);";
            html += "if(phase === 'long' && distDiff < 350) shouldSpeak = false;";
            
            html += "if(phase && shouldSpeak){";
            html += "    var speechText = '';";
            html += "    var streetName = (step.name && step.name !== '') ? step.name : (currentLanguage==='vi'?'đường này':'this road');";
            html += "    var distText = Math.round(distToStep) + ' mét';";
            
            html += "    if(currentLanguage === 'vi'){";
            html += "        if(phase === 'action') speechText = 'Bây giờ, hãy ' + instruction;";
            html += "        else if(phase === 'prep') {";
            html += "            if(type === 'arrive') speechText = 'Bạn sắp đến nơi, ' + instruction;";
            html += "            else speechText = 'Sau ' + distText + ' nữa, hãy ' + instruction;";
            html += "        }";
            html += "        else if(phase === 'long') speechText = 'Tiếp tục đi thẳng trên ' + streetName + ' khoảng ' + (totalDistance > 1000 ? (totalDistance/1000).toFixed(1) + ' ki-lô-mét' : Math.round(totalDistance) + ' mét') + '.';";
            html += "    } else {";
            html += "        if(phase === 'action') speechText = 'Now, ' + instruction;";
            html += "        else if(phase === 'prep') speechText = 'In ' + Math.round(distToStep) + ' meters, ' + instruction;";
            html += "        else speechText = instruction;";
            html += "    }";
            
            html += "    if(speechText) {";
            html += "        sendVoiceNavigation(speechText, true);";
            html += "        window.spokenPhases.step = currentStepIndex;";
            html += "        window.spokenPhases.phase = phase;";
            html += "        window.spokenPhases.lastDist = distToStep;";
            html += "    }";
            html += "}";

            html += "var distance=Math.round(distToStep);";
            html += "var distNum=distance<1000?distance:(distance/1000).toFixed(1);";
            html += "var distUnit=distance<1000?' m':' km';";
            html += "var icon='🚀';";
            html += "var modifier=step.maneuver.modifier||'';";
            html += "if(type==='turn'&&modifier==='left')icon='↰';";
            html += "else if(type==='turn'&&modifier==='right')icon='↱';";
            html += "else if(type==='arrive')icon='🎯';";
            
            html += "console.log('🎯 [UI] Step',currentStepIndex,'Type:',type,'Name:',step.name,'Dist:',Math.round(distToStep)+'m');";
            
            html += "if(currentLanguage==='vi'){";
            html += "    var dirText='';";
            html += "    var streetName=(step.name&&step.name!=='')?step.name:'đường này';";
            html += "    if(type==='turn'&&modifier==='left')dirText='Rẽ trái';";
            html += "    else if(type==='turn'&&modifier==='right')dirText='Rẽ phải';";
            html += "    else if(type==='arrive')dirText='Đến nơi';";
            html += "    else if(type==='depart')dirText='Bắt đầu';";
            html += "    else dirText='Tiếp tục';";
            html += "    console.log('📺 [UI] Hiển thị:',distNum+distUnit,dirText,streetName);";
            html += "    document.getElementById('navigationInfo').innerHTML='<div class=\"nav-instruction\"><div class=\"nav-icon\">'+icon+'</div><div class=\"nav-text\"><div class=\"nav-distance\">'+distNum+'<span style=\"font-size:14px;font-weight:400\">'+distUnit+'</span></div><div class=\"nav-direction\">'+dirText+'</div><div class=\"nav-street\">'+streetName+'</div></div></div>';";
            html += "    document.getElementById('routeSubtitle').innerHTML='🧭 '+distRemaining+' • '+timeMin+' phút còn lại';";
            html += "} else {";
            html += "    var streetName=(step.name&&step.name!=='')?step.name:'this road';";
            html += "    document.getElementById('navigationInfo').innerHTML='<div class=\"nav-instruction\"><div class=\"nav-icon\">'+icon+'</div><div class=\"nav-text\"><div class=\"nav-distance\">'+distNum+'<span style=\"font-size:14px;font-weight:400\">'+distUnit+'</span></div><div class=\"nav-direction\">'+(step.maneuver.type)+'</div><div class=\"nav-street\">'+streetName+'</div></div></div>';";
            html += "}";
            html += "updateRouteProgress();";
            html += "}";
            
            html += "function updateRouteProgress(){";
            html += "if(!isNavigating||!fullRouteCoords||fullRouteCoords.length===0){";
            html += "    console.log('⏸️ [Progress] Skip - isNavigating:',isNavigating,'fullRouteCoords:',fullRouteCoords?fullRouteCoords.length:0);";
            html += "    return;";
            html += "}";
            html += "var userPos=userMarker.getLatLng();";
            html += "var closestIndex=0;";
            html += "var minDist=Infinity;";
            html += "for(var i=0;i<fullRouteCoords.length;i++){";
            html += "var dist=map.distance(userPos,fullRouteCoords[i]);";
            html += "if(dist<minDist){minDist=dist;closestIndex=i;}";
            html += "}";
            html += "var remainingCoords=fullRouteCoords.slice(closestIndex);";
            html += "if(remainingCoords.length<2){";
            html += "    console.log('⏸️ [Progress] Skip - remainingCoords too short:',remainingCoords.length);";
            html += "    return;";
            html += "}";
            html += "if(routeLine){map.removeLayer(routeLine);routeLine=null;}";
            html += "if(arrowLine){map.removeLayer(arrowLine);arrowLine=null;}";
            html += "routeLine=L.polyline(remainingCoords,{color:'#1A73E8',weight:6,opacity:1,lineJoin:'round',lineCap:'round',smoothFactor:1}).addTo(map);";
            html += "console.log('🔄 [Progress] Cập nhật route line. Remaining:',remainingCoords.length,'điểm');";
            html += "}";
            
            html += "window._navAudio = null;";
            html += "var lastSpokenInstruction='';";
            html += "var lastSpokenTime=0;";
            html += "var lastSpokenStepIndex=-1;";
             html += "function speakWeb(text, lang, forceSpeak){";
            html += "  if(!text) return;";
            html += "  var now = Date.now();";
            html += "  if(!forceSpeak && text === lastSpokenInstruction && (now - lastSpokenTime) < 5000) return;";
            html += "  lastSpokenInstruction = text; lastSpokenTime = now;";
            html += "  console.log('🗣️ [SPEECH] Speaking:', text);";
            html += "  try {";
            html += "    window.speechSynthesis.cancel();"; 
            html += "    var msg = new SpeechSynthesisUtterance(text);";
            html += "    msg.lang = lang === 'vi' ? 'vi-VN' : (lang === 'en' ? 'en-US' : 'zh-CN');";
            html += "    msg.rate = 1.0; msg.pitch = 1.0; msg.volume = 1.0;";
            html += "    var voices = window.speechSynthesis.getVoices();";
            html += "    var voice = voices.find(function(v){ return v.lang.startsWith(lang); });";
            html += "    if(voice) msg.voice = voice;";
            html += "    window.speechSynthesis.speak(msg);";
            html += "  } catch(e) { console.error('❌ [SPEECH] Error:', e.message); }";
            html += "}";
            html += "function sendVoiceNavigation(instruction, forceSpeak){";
            html += "  speakWeb(instruction, currentLanguage, forceSpeak);";
            html += "}";
            
            // ✅ Suppress Speech Recognition errors
            html += "window.addEventListener('error', function(e){";
            html += "  if(e.message && (e.message.includes('Speech Recognition') || e.message.includes('SpeechRecognition'))){";
            html += "    e.preventDefault();";
            html += "    e.stopPropagation();";
            html += "    console.log('⚠️ [SPEECH] Speech Recognition error suppressed');";
            html += "    return false;";
            html += "  }";
            html += "}, true);";
            
            html += "function getInstruction(step){";
            html += "var type=step.maneuver.type;";
            html += "var modifier=step.maneuver.modifier||'';";
            html += "var name=(step.name&&step.name!=='')?step.name:(currentLanguage==='vi'?'đường này':(currentLanguage==='en'?'this road':'这条路'));";
            html += "var modVi=modifier==='left'?'Rẽ trái':(modifier==='right'?'Rẽ phải':(modifier==='slight left'?'Chếch sang trái':(modifier==='slight right'?'Chếch sang phải':(modifier==='sharp left'?'Ngoặt trái':(modifier==='sharp right'?'Ngoặt phải':(modifier==='uturn'?'Quay đầu lại':'Đi thẳng'))))));";
            html += "var modEn=modifier==='left'?'Turn left':(modifier==='right'?'Turn right':(modifier==='slight left'?'Slight left':(modifier==='slight right'?'Slight right':(modifier==='sharp left'?'Sharp left':(modifier==='sharp right'?'Sharp right':(modifier==='uturn'?'Make a U-turn':'Go straight'))))));";
            html += "var modZh=modifier==='left'?'左转':(modifier==='right'?'右转':(modifier==='slight left'?'向左微转':(modifier==='slight right'?'向右微转':(modifier==='sharp left'?'向左急转':(modifier==='sharp right'?'向右急转':(modifier==='uturn'?'掉头':'直行'))))));";
            html += "if(currentLanguage==='vi'){";
            html += "if(type==='depart')return 'Bắt đầu di chuyển trên '+name;";
            html += "if(type==='arrive')return 'Bạn đã đến nơi. Chúc bạn ngon miệng!';";
            html += "if(type==='turn')return modVi+' vào '+name;";
            html += "if(type==='continue'||type==='new name')return 'Tiếp tục đi thẳng trên '+name;";
            html += "if(type==='roundabout')return 'Đi vào vòng xuyến rồi rẽ vào '+name;";
            html += "if(type==='merge'||type==='on ramp')return 'Nhập làn vào '+name;";
            html += "if(type==='fork')return 'Đi nhánh '+modVi+' vào '+name;";
            html += "return modVi+' vào '+name;";
            html += "}else if(currentLanguage==='en'){";
            html += "if(type==='depart')return 'Start on '+name;";
            html += "if(type==='arrive')return 'Arrive at destination';";
            html += "if(type==='turn')return modEn+' onto '+name;";
            html += "if(type==='continue'||type==='new name')return 'Continue on '+name;";
            html += "if(type==='roundabout')return 'Enter roundabout and go to '+name;";
            html += "if(type==='merge'||type==='on ramp')return 'Merge onto '+name;";
            html += "if(type==='fork')return 'Take the '+modEn+' fork onto '+name;";
            html += "return modEn+' onto '+name;";
            html += "}else{";
            html += "if(type==='depart')return '开始在 '+name;";
            html += "if(type==='arrive')return '到达';";
            html += "if(type==='turn'||type==='fork')return modZh+'进入 '+name;";
            html += "if(type==='continue'||type==='new name')return '继续在 '+name;";
            html += "if(type==='roundabout')return '进入环岛前往 '+name;";
            html += "if(type==='merge'||type==='on ramp')return '并入 '+name;";
            html += "return modZh+'进入 '+name;";
            html += "}";
            html += "}";
            
            html += "function stopRoute(){closePanel();}";
            
            html += "var realGPSWatchId=null;";
            html += "function startRealGPSTracking(){";
            html += "console.log('📍 [GPS] Bắt đầu theo dõi GPS thực tế');";
            html += "if(navigator.geolocation){";
            html += "realGPSWatchId=navigator.geolocation.watchPosition(function(position){";
            html += "if(!isNavigating)return;";
            html += "var newLat=position.coords.latitude;";
            html += "var newLng=position.coords.longitude;";
            html += "var speed=position.coords.speed||0;";
            html += "var heading=position.coords.heading;";
            html += "console.log('📍 [GPS] Vị trí mới:',newLat,newLng,'Speed:',speed,'Heading:',heading);";
            html += "var snappedLat=newLat, snappedLng=newLng;";
            html += "if(fullRouteCoords&&fullRouteCoords.length>0){";
            html += "var minDist=Infinity;";
            html += "var userP=L.latLng(newLat,newLng);";
            html += "for(var i=0;i<fullRouteCoords.length;i++){";
            html += "var routeP=L.latLng(fullRouteCoords[i][0],fullRouteCoords[i][1]);";
            html += "var dist=map.distance(userP,routeP);";
            html += "if(dist<minDist){minDist=dist;snappedLat=routeP.lat;snappedLng=routeP.lng;}";
            html += "}";
            html += "if(minDist<50){ newLat=snappedLat; newLng=snappedLng; }";
            html += "}";
            html += "userMarker.setLatLng([newLat,newLng]);";
            html += "userPos.lat=newLat;userPos.lng=newLng;";
            html += "if(!userInteractedWithMap){";
            html += "var offsetLat = getCenterOffset();";
            html += "var speedKmh=speed*3.6;";
            html += "var targetZoom=19;";
            html += "if(speedKmh>60) targetZoom=16;";
            html += "else if(speedKmh>40) targetZoom=17;";
            html += "else if(speedKmh>20) targetZoom=18;";
            html += "else if(speedKmh>1) targetZoom=19;";
            html += "var currentStep = routeSteps[currentStepIndex];";
            html += "if(currentStep && map.distance(userP, L.latLng(currentStep.maneuver.location[1], currentStep.maneuver.location[0])) < 50) targetZoom = 20;";
            html += "map.setView([newLat + offsetLat, newLng], targetZoom, {animate:true, duration:0.5});";
            html += "}";
            html += "updateNavigation();";
            html += "checkNearbyRestaurants(newLat,newLng);";
            html += "},function(error){";
            html += "console.error('❌ [GPS] Lỗi GPS:',error.message);";
            html += "},{enableHighAccuracy:true,timeout:10000,maximumAge:5000});";
            html += "}else{";
            html += "console.log('⚠️ [GPS] Geolocation không được hỗ trợ');";
            html += "}";
            html += "}";
            html += "function stopRealGPSTracking(){";
            html += "if(realGPSWatchId){";
            html += "navigator.geolocation.clearWatch(realGPSWatchId);";
            html += "realGPSWatchId=null;";
            html += "console.log('⏹️ [GPS] Đã dừng theo dõi GPS');";
            html += "}";
            html += "}";
            html += "function getDistanceFromLatLonInKm(lat1,lon1,lat2,lon2){";
            html += "var R=6371;";
            html += "var dLat=deg2rad(lat2-lat1);";
            html += "var dLon=deg2rad(lon2-lon1);";
            html += "var a=Math.sin(dLat/2)*Math.sin(dLat/2)+Math.cos(deg2rad(lat1))*Math.cos(deg2rad(lat2))*Math.sin(dLon/2)*Math.sin(dLon/2);";
            html += "var c=2*Math.atan2(Math.sqrt(a),Math.sqrt(1-a));";
            html += "var d=R*c;";
            html += "return d;";
            html += "}";
            html += "function deg2rad(deg){return deg*(Math.PI/180);}";
            html += "var testModeActive=false;";
            html += "var testModeInterval=null;";
            html += "var testRouteIndex=0;";
            html += "var lastTestLat=null;";
            html += "var lastTestLng=null;";
            
            html += "function toggleTestMode(){";
            html += "  testModeActive = !testModeActive;";
            html += "  var btn = document.getElementById('btnTestMode');";
            html += "  if(testModeActive){";
            html += "    if(!currentDest){";
            html += "      alert('Vui lòng chọn điểm đến trước!');";
            html += "      testModeActive = false;";
            html += "      return;";
            html += "    }";
            html += "    if(!isNavigating){";
            html += "      console.log('🧪 Test Mode: Tự động bật navigation');";
            html += "      startNavigation();";
            html += "      setTimeout(function(){ if(!isNavigating || !fullRouteCoords || fullRouteCoords.length === 0){ alert('Không thể tính route. Vui lòng thử lại!'); testModeActive = false; return; } }, 1000);";
            html += "    }";
            html += "    if(!fullRouteCoords || fullRouteCoords.length === 0){";
            html += "      alert('Đang tính route, vui lòng đợi...');";
            html += "      testModeActive = false;";
            html += "      return;";
            html += "    }";
            html += "    btn.innerHTML = '⏹ Stop Test';";
            html += "    btn.style.background = '#f44336';";
            html += "    console.log('🧪 Test Mode: bắt đầu giả lập di chuyển theo route');";
            html += "    testRouteIndex = 0;";
            html += "    userInteractedWithMap = false;";
            html += "    testModeInterval = setInterval(function(){";
            html += "      if(!currentDest || !isNavigating || !fullRouteCoords || fullRouteCoords.length === 0){";
            html += "        stopTestMode(); return;";
            html += "      }";
            html += "      if(testRouteIndex >= fullRouteCoords.length){";
            html += "        console.log('🏁 Test Mode: đã hoàn thành route!');";
            html += "        stopTestMode(); return;";
            html += "      }";
            html += "      var nextPoint = fullRouteCoords[testRouteIndex];";
            html += "      var newLat = nextPoint[0];";
            html += "      var newLng = nextPoint[1];";
            html += "      userMarker.setLatLng([newLat, newLng]);";
            html += "      userPos.lat = newLat; userPos.lng = newLng;";
            html += "      if(!userInteractedWithMap){";
            html += "        var currentStep = routeSteps[currentStepIndex];";
            html += "        var stepDist = currentStep ? currentStep.distance : 1000;";
            html += "        var zoomLevel = (stepDist < 30) ? 20 : (stepDist < 80) ? 19 : (stepDist < 150) ? 18 : (stepDist < 300) ? 17 : 16;";
            html += "        var offsetLat = getCenterOffset();";
            html += "        map.setView([newLat + offsetLat, newLng], zoomLevel, {animate:true, duration:0.5});";
            html += "      }";
            html += "      window.chrome.webview.postMessage(JSON.stringify({type:'updateUserPosition', lat:newLat, lng:newLng}));";
            html += "      checkNearbyRestaurants(newLat, newLng);";
            html += "      lastTestLat = newLat; lastTestLng = newLng;";
            html += "      testRouteIndex += 1;";
            html += "      updateNavigation();";
            html += "    }, 1200);";
            html += "  } else {";
            html += "    stopTestMode();";
            html += "  }";
            html += "}";
            
            html += "function stopTestMode(){";
            html += "  testModeActive = false;";
            html += "  var btn = document.getElementById('btnTestMode');";
            html += "  if(btn){";
            html += "    btn.innerHTML = '🧪 Test Mode';";
            html += "    btn.style.background = '#ff9800';";
            html += "  }";
            html += "  if(testModeInterval){";
            html += "    clearInterval(testModeInterval);";
            html += "    testModeInterval = null;";
            html += "  }";
            html += "  console.log('⏹️ Test Mode: Dừng giả lập');";
            html += "}";
            html += "function toggleCompass(){";
            html += "compassEnabled=!compassEnabled;";
            html += "var btn=document.getElementById('btnCompass');";
            html += "if(compassEnabled){";
            html += "btn.classList.add('active');";
            html += "btn.title='La bàn BẬT - Bản đồ xoay theo hướng di chuyển';";
            html += "console.log('🧭 La bàn: BẬT');";
            html += "}else{";
            html += "btn.classList.remove('active');";
            html += "btn.title='La bàn TẮT - Bản đồ hướng Bắc';";
            html += "map.setBearing(0);";
            html += "var arrow=document.getElementById('compassArrow');";
            html += "arrow.style.transform='rotate(0deg)';";
            html += "mapBearing=0;";
            html += "console.log('🧭 La bàn: TẮT - Reset về hướng Bắc');";
            html += "}";
            html += "}";
            html += "function checkNearbyRestaurants(lat,lng){";
            html += "  foods.forEach(function(food){";
            html += "    var dist=map.distance([lat,lng],[food.lat,food.lng]);";
            html += "    var narrateDistance=20;";
            html += "    if(food.rating>=4.7)narrateDistance=40;";
            html += "    else if(food.rating>=4.5)narrateDistance=35;";
            html += "    else if(food.rating>=4.3)narrateDistance=30;";
            html += "    else if(food.rating>=4.0)narrateDistance=25;";
            html += "    if(dist<=narrateDistance){";
            html += "      console.log('🍽️ [NAV] Gần quán:',food.name,'- '+dist.toFixed(0)+'m');";
            html += "      if(!window._spokenFoods) window._spokenFoods = {};";
            html += "      if(!window._spokenFoods[food.name]){";
            html += "          window._spokenFoods[food.name] = true;";
            html += "          speakWeb(food.desc, currentLanguage, true);";
            html += "      }";
            html += "    }";
            html += "  });";
            html += "}";
            html += "function checkMapOrientation(){";
            html += "  if(!isNavigating || userInteractedWithMap) return;";
            html += "}";

            
            html += "setInterval(function(){if(routeSteps.length>0&&isNavigating){updateNavigation();}},2000);";
            
            html += "window.chrome.webview.addEventListener('message',function(e){";
            html += "  var data=JSON.parse(e.data);";
            html += "  if(data.type==='languageChanged'){";
            html += "    console.log('🌐 [JS] Language changed to:', data.language);";
            html += "    currentLanguage = data.language;";
            html += "    updateButtonTexts();";
            html += "    return;";
            html += "  }";
            html += "  if(data.type==='gpsStatus'){";
            html += "    if(data.status==='disabled'){";
            html += "      var msg = currentLanguage==='vi'?'⚠️ Quyền truy cập vị trí đang bị TẮT trong Windows. Hãy bật lại trong Cài đặt Quyền riêng tư.':(currentLanguage==='en'?'⚠️ Location permission is DISABLED in Windows Settings.':'⚠️ 位置权限已禁用');";
            html += "      showError('🌐 GPS', msg);";
            html += "    }";
            html += "    return;";
            html += "  }";
            html += "  if((data.type==='updateUser' || data.type==='updateUserPosition') && userMarker){";
            html += "    var rawLat = data.lat;";
            html += "    var rawLng = data.lng;";
            html += "    var finalLat = rawLat;";
            html += "    var finalLng = rawLng;";
            
            // Road Snapping Logic
            html += "    if(isNavigating && fullRouteCoords && fullRouteCoords.length > 0){";
            html += "      var minDist = Infinity;";
            html += "      var snapped = null;";
            html += "      for(var i=0; i<fullRouteCoords.length; i++){";
            html += "        var p = fullRouteCoords[i];";
            html += "        var d = map.distance([rawLat, rawLng], p);";
            html += "        if(d < minDist){ minDist = d; snapped = p; }";
            html += "      }";
            // Nếu cách đường < 30m thì "hít" vào đường (nguồn native tin cậy hơn)
            html += "      if(minDist < 30){ finalLat = snapped[0]; finalLng = snapped[1]; }";
            html += "    }";
            
            html += "    userPos.lat = finalLat; userPos.lng = finalLng;";
            html += "    userMarker.setLatLng([finalLat, finalLng]);";
            
            html += "    if(!userInteractedWithMap){";
            html += "      var currentZoom = map.getZoom();";
            html += "      var offsetLat = getCenterOffset();"; // ✅ Luôn luôn dùng offset
            html += "      var targetZoom = isNavigating ? Math.max(currentZoom, 18) : currentZoom;";
            html += "      map.setView([finalLat + offsetLat, finalLng], targetZoom, {animate:true, duration:0.5});";
            html += "    }";
            html += "    if(isNavigating) updateNavigation();";
            html += "    if(!isNavigating && currentDest && !isCalculatingRoute){";
            html += "      var shouldRecalculate = false;";
            html += "      if(routeSteps.length === 0){";
            html += "        shouldRecalculate = true;";
            html += "      } else if(fullRouteCoords && fullRouteCoords.length > 0){";
            html += "        var startPoint = L.latLng(fullRouteCoords[0][0], fullRouteCoords[0][1]);";
            html += "        var distFromStart = map.distance([finalLat, finalLng], startPoint);";
            html += "        if(distFromStart > 100){";
            html += "          console.log('🔄 [GPS] Vị trí thay đổi đáng kể (' + distFromStart.toFixed(0) + 'm), tính lại lộ trình...');";
            html += "          shouldRecalculate = true;";
            html += "        }";
            html += "      }";
            html += "      if(shouldRecalculate){";
            html += "        calculateRoute(currentDest.lat, currentDest.lng, currentDest.name);";
            html += "      }";
            html += "    }";
            html += "  }";
            html += "});";
            
            html += "useGPSLocation();";
            html += "window.chrome.webview.postMessage(JSON.stringify({type:'map_ready'}));";
            html += "</script></body></html>";
            
            return html;
        }

        private void OnWebMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // e.WebMessageAsJson: nếu JS gọi postMessage(chuỗi) thì nó bọc thêm dấu ngoặc
                // VD: JS post JSON.stringify({type:'x'}) → WebMessageAsJson = "\"{ type: 'x' }\""
                // Cần giải mã JSON một lần để lấy ra chuỗi thực, rồi mới parse tiếp
                string rawMessage = e.WebMessageAsJson;
                System.Diagnostics.Debug.WriteLine($"📨 [C#] RAW MESSAGE: {rawMessage}");
                
                // Nếu đang bị double-encoded (wrapped in quotes), giải mã lần 1
                if (rawMessage != null && rawMessage.StartsWith("\""))
                    rawMessage = System.Text.Json.JsonSerializer.Deserialize<string>(rawMessage);
                
                using (JsonDocument doc = JsonDocument.Parse(rawMessage))
                {
                    var root = doc.RootElement;
                    
                    if (!root.TryGetProperty("type", out var typeElement))
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [C#] Không có key 'type' trong message");
                        return;
                    }
                    
                    string type = typeElement.GetString();
                    System.Diagnostics.Debug.WriteLine($"📋 [C#] Message type: {type}");
                    
                    if (type == "map_ready")
                    {
                        System.Diagnostics.Debug.WriteLine("🗺️ [C#] Bản đồ đã sẵn sàng. Gửi vị trí ban đầu.");
                        PushUserPositionToJS(_userLat, _userLng);
                    }
                    else if (type == "selectFood")
                    {
                        if (root.TryGetProperty("name", out var nameElement))
                        {
                            string name = nameElement.GetString();
                            var food = _allFoods.FirstOrDefault(f => f.Name == name);
                            if (food != null) AutoNarrate(food);
                        }
                    }
                    else if (type == "voiceNavigation")
                    {
                        System.Diagnostics.Debug.WriteLine($"🔊 [C#] Nhận được voiceNavigation message");
                        if (root.TryGetProperty("text", out var textElement) && root.TryGetProperty("language", out var langElement))
                        {
                            string text = textElement.GetString();
                            string language = langElement.GetString();
                            System.Diagnostics.Debug.WriteLine($"📝 [C#] Text: {text}, Language: {language}");
                            SpeakNavigation(text, language);
                        }
                    }
                    else if (type == "updateUserPosition")
                    {
                        System.Diagnostics.Debug.WriteLine($"📍 [C#] Nhận updateUserPosition");
                        if (root.TryGetProperty("lat", out var latElement) && root.TryGetProperty("lng", out var lngElement))
                        {
                            double lat = 0, lng = 0;
                            
                            // Try as number first, then as string
                            if (latElement.ValueKind == JsonValueKind.Number)
                            {
                                lat = latElement.GetDouble();
                            }
                            else if (latElement.ValueKind == JsonValueKind.String)
                            {
                                string latStr = latElement.GetString();
                                double.TryParse(latStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lat);
                            }
                            
                            if (lngElement.ValueKind == JsonValueKind.Number)
                            {
                                lng = lngElement.GetDouble();
                            }
                            else if (lngElement.ValueKind == JsonValueKind.String)
                            {
                                string lngStr = lngElement.GetString();
                                double.TryParse(lngStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lng);
                            }
                            
                            _userLat = lat;
                            _userLng = lng;
                            System.Diagnostics.Debug.WriteLine($"📍 [C#] User position updated: {lat}, {lng}");
                            // Gửi lên server để admin map cập nhật theo (throttle 3s)
                            ReportPositionToServer(lat, lng, forceImmediate: false);
                        }
                    }
                    else if (type == "nearFood")
                    {
                        if (root.TryGetProperty("name", out var nameElement) && root.TryGetProperty("desc", out var descElement))
                        {
                            string desc = descElement.GetString();
                            Dispatcher.Invoke(() => {
                                string script = $"speakWeb('{desc.Replace("'", "\\'")}', currentLanguage, true);";
                                MapBrowser.CoreWebView2.ExecuteScriptAsync(script);
                            });
                        }
                    }
                    else if (type == "navStateChanged")
                    {
                        System.Diagnostics.Debug.WriteLine($"🧭 [C#] Nhận navStateChanged message!");
                        
                        if (root.TryGetProperty("navigating", out var navElement))
                        {
                            bool nav = navElement.GetBoolean();
                            _isNavigating = nav;
                            System.Diagnostics.Debug.WriteLine($"🧭 [C#] Parsed navigating: {nav}");
                            
                            if (root.TryGetProperty("destinationName", out var destElement))
                            {
                                _destinationName = destElement.GetString();
                                System.Diagnostics.Debug.WriteLine($"🧭 [C#] destinationName: {_destinationName}");
                            }
                            
                            if (root.TryGetProperty("destinationLat", out var latElement))
                            {
                                // Try as number first, then as string
                                if (latElement.ValueKind == JsonValueKind.Number)
                                {
                                    _destinationLat = latElement.GetDouble();
                                }
                                else if (latElement.ValueKind == JsonValueKind.String)
                                {
                                    string latStr = latElement.GetString();
                                    if (double.TryParse(latStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat))
                                    {
                                        _destinationLat = lat;
                                    }
                                }
                                System.Diagnostics.Debug.WriteLine($"🧭 [C#] destinationLat: {_destinationLat}");
                            }
                            
                            if (root.TryGetProperty("destinationLng", out var lngElement))
                            {
                                // Try as number first, then as string
                                if (lngElement.ValueKind == JsonValueKind.Number)
                                {
                                    _destinationLng = lngElement.GetDouble();
                                }
                                else if (lngElement.ValueKind == JsonValueKind.String)
                                {
                                    string lngStr = lngElement.GetString();
                                    if (double.TryParse(lngStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lng))
                                    {
                                        _destinationLng = lng;
                                    }
                                }
                                System.Diagnostics.Debug.WriteLine($"🧭 [C#] destinationLng: {_destinationLng}");
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"🧭 [C#] Navigation state changed: {nav}, Goal: {_destinationName} ({_destinationLat:F5}, {_destinationLng:F5})");
                            System.Diagnostics.Debug.WriteLine($"🚀 [C#] GỬI lên server sau 200ms delay (isNavigating={nav})");
                            
                            // Capture snapshot values to avoid closure issues
                            bool navSnapshot = nav;
                            double latSnap = _userLat, lngSnap = _userLng;
                            double destLatSnap = _destinationLat, destLngSnap = _destinationLng;
                            string destNameSnap = _destinationName;
                            
                            // Wait 200ms so that the updateUserPosition message sent just before
                            // navStateChanged has time to update _userLat/_userLng
                            System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
                            {
                                // Re-read latest values after delay
                                Dispatcher.Invoke(() =>
                                {
                                    System.Diagnostics.Debug.WriteLine($"🔍 [C#] DELAYED CHECK: _isNavigating={_isNavigating}, lat={_userLat:F5}, lng={_userLng:F5}");
                                    ReportPositionToServer(_userLat, _userLng, forceImmediate: true);
                                });
                            });
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ [C#] Không tìm thấy key 'navigating' trong message");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❓ [C#] Unknown message type: {type}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [C#] EXCEPTION trong OnWebMessage: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ [C#] Stack trace: {ex.StackTrace}");
            }
        }

        private void SpeakNavigation(string text, string language)
        {
            System.Diagnostics.Debug.WriteLine($"🔊 [C#] SpeakNavigation được gọi. Text: {text}, Language: {language}");
            
            Dispatcher.Invoke(() =>
            {
                TxtNarrationName.Text = "🧭 " + (language == "vi" ? "Chỉ đường" : (language == "en" ? "Navigation" : "导航"));
                NarrationBanner.Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine($"📱 [C#] Banner hiển thị: {TxtNarrationName.Text}");
            });
            
            // Map language code to speech locale
            string locale;
            if (language == "vi")
            {
                locale = "vi-VN";
            }
            else if (language == "en")
            {
                locale = "en-US";
            }
            else if (language == "zh")
            {
                locale = "zh-CN";
            }
            else
            {
                locale = "vi-VN"; // Default
            }
            
            System.Diagnostics.Debug.WriteLine($"📞 [C#] Gọi _speechService.Speak với locale: {locale}");
            
            try
            {
                _speechService.Speak(text, locale);
                System.Diagnostics.Debug.WriteLine($"✅ [C#] _speechService.Speak đã được gọi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [C#] Exception khi gọi Speak: {ex.Message}");
                Dispatcher.Invoke(() =>
                {
                    MessageDialog.ShowError($"Lỗi giọng nói: {ex.Message}\n\nStack: {ex.StackTrace}", "Lỗi");
                });
            }
        }

        private void StartLocationTracking()
        {
            _locationTimer.Interval = TimeSpan.FromSeconds(2);
            _locationTimer.Tick += OnLocationTick;
            _locationTimer.Start();
        }

        private void OnLocationTick(object sender, EventArgs e)
        {
            // Chỉ check nearby foods, không tự động di chuyển
            CheckNearbyFoods();
        }

        private void SimulateMovement()
        {
            // Không tự động di chuyển - chỉ di chuyển khi người dùng thực sự di chuyển
            // Có thể thêm GPS tracking thực tế ở đây nếu cần
        }

        private void CheckNearbyFoods()
        {
            if (_allFoods == null) return;
            
            System.Diagnostics.Debug.WriteLine($"🔍 [CheckNearby] Checking from position: {_userLat}, {_userLng}");
            
            foreach (var food in _allFoods)
            {
                double dist = GetDistance(_userLat, _userLng, food.Latitude, food.Longitude);
                
                double narrateDistance = 20;
                if (food.Rating >= 4.7) narrateDistance = 40;
                else if (food.Rating >= 4.5) narrateDistance = 35;
                else if (food.Rating >= 4.3) narrateDistance = 30;
                else if (food.Rating >= 4.0) narrateDistance = 25;
                
                System.Diagnostics.Debug.WriteLine($"  📍 {food.Name}: {dist:F1}m (threshold: {narrateDistance}m, rating: {food.Rating})");
                
                if (dist <= narrateDistance)
                {
                    if (_lastNarratedFood?.Name != food.Name && !_speechService.IsPlaying)
                    {
                        System.Diagnostics.Debug.WriteLine($"  🔊 [Narrate] Thuyết minh: {food.Name}");
                        _lastNarratedFood = food;
                        AutoNarrate(food);
                        if (MapBrowser.CoreWebView2 != null)
                        {
                            string msg = JsonSerializer.Serialize(new { type = "nearFood", lat = food.Latitude, lng = food.Longitude });
                            MapBrowser.CoreWebView2.PostWebMessageAsString(msg);
                        }
                        break;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  ⏸️ [Skip] Already narrated or speech playing");
                    }
                }
            }
        }

        private void AutoNarrate(FoodItem food)
        {
            Dispatcher.Invoke(() =>
            {
                TxtNarrationName.Text = food.Name;
                NarrationBanner.Visibility = Visibility.Visible;
            });
            string text = food.DescriptionVI ?? food.Name;
            _speechService.Speak(text, "vi-VN");
        }

        private void StopNarration_Click(object sender, RoutedEventArgs e)
        {
            _speechService.Stop();
            NarrationBanner.Visibility = Visibility.Collapsed;
        }

        private double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371000;
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLng = (lng2 - lng1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        public void SetRating(double rating)
        {
            double floorRating = Math.Floor(rating);
            int fullStars = (int)(floorRating < 0 ? 0 : (floorRating > 5 ? 5 : floorRating));
            var stars = new List<string>();
            for (int i = 0; i < fullStars; i++) stars.Add("⭐");
            StarRating.ItemsSource = stars;
            TxtRatingCount.Text = $"({rating}/5.0)";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _locationTimer.Stop();
            if (_geoWatcher != null) _geoWatcher.Stop();
            _speechService.Stop();
            _speechService.Dispose();
            
            // Mở lại FoodDetailWindow với food hiện tại
            if (_food != null)
            {
                FoodDetailWindow detailWindow = new FoodDetailWindow(_food);
                detailWindow.Show();
            }
            
            this.Close();
        }
        
        private void UpdateUILanguage()
        {
            var lang = LanguageService.Instance;
            
            // Update window title
            this.Title = $"Vietnam Food Guide - {lang["map_title"]}";
            
            // Update status bar
            if (BtnBack != null)
            {
                BtnBack.Content = lang["back"];
            }
            if (TxtMapTitle != null)
            {
                TxtMapTitle.Text = lang["map_title"];
            }
            
            // Update loading overlay
            if (TxtLoadingMap != null)
            {
                TxtLoadingMap.Text = lang["loading_map"];
            }
            
            // Update narration banner
            if (TxtNarrating != null)
            {
                TxtNarrating.Text = lang["narrating"];
            }
            if (BtnStopNarration != null)
            {
                BtnStopNarration.Content = lang["stop"];
            }
            
            // Notify JavaScript about language change
            if (MapBrowser?.CoreWebView2 != null)
            {
                var msg = JsonSerializer.Serialize(new { type = "languageChanged", language = lang.CurrentLanguage });
                MapBrowser.CoreWebView2.PostWebMessageAsString(msg);
                System.Diagnostics.Debug.WriteLine($"🌐 [C#] Sent language change to JavaScript: {lang.CurrentLanguage}");
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // F12 d? m? Developer Tools
            if (e.Key == System.Windows.Input.Key.F12)
            {
                if (MapBrowser?.CoreWebView2 != null)
                {
                    MapBrowser.CoreWebView2.OpenDevToolsWindow();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _locationTimer.Stop();
            if (_geoWatcher != null)
            {
                _geoWatcher.Stop();
                _geoWatcher.Dispose();
            }
            _speechService.Dispose();
            base.OnClosed(e);
        }
    }
}

