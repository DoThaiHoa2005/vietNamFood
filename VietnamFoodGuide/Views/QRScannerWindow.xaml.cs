using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using VietnamFoodGuide.Models.Entities;
using VietnamFoodGuide.Services;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace VietnamFoodGuide.Views
{
    public partial class QRScannerWindow : Window
    {
        private readonly LanguageService _lang = LanguageService.Instance;
        private string _deviceId;
        private bool _hasScanned = false;

        public QRScannerWindow()
        {
            InitializeComponent();
            _deviceId = GetDeviceId();
            
            // Initialize language
            UpdateUILanguage();
            _lang.LanguageChanged += (s, e) => UpdateUILanguage();

            // Fade in animation
            this.Opacity = 0;
            this.Loaded += async (s, e) =>
            {
                var anim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                this.BeginAnimation(Window.OpacityProperty, anim);
                
                // Check if already scanned
                await CheckIfAlreadyScanned();
            };
        }

        private void UpdateUILanguage()
        {
            if (_lang.CurrentLanguage == "vi")
            {
                TxtSubtitle.Text = "Quét mã QR để truy cập";
                TxtLoading.Text = "Đang khởi động camera...";
                TxtSuccess.Text = "Quét thành công!";
                TxtSuccessDetail.Text = "Đang chuyển đến trang chủ...";
                TxtInstructionTitle.Text = "📋 Hướng dẫn:";
                TxtInstruction1.Text = "1. Đưa mã QR vào khung hình";
                TxtInstruction2.Text = "2. Hoặc chọn ảnh QR từ thư viện";
                TxtInstruction3.Text = "3. Hệ thống sẽ tự động quét và lưu";
                BtnSelectImage.Content = "📁 Chọn ảnh QR";
                BtnSkip.Content = "⏭️ Bỏ qua";
                TxtInfo.Text = "💡 Chỉ cần quét 1 lần duy nhất";
            }
            else if (_lang.CurrentLanguage == "en")
            {
                TxtSubtitle.Text = "Scan QR Code to Access";
                TxtLoading.Text = "Starting camera...";
                TxtSuccess.Text = "Scan successful!";
                TxtSuccessDetail.Text = "Redirecting to home...";
                TxtInstructionTitle.Text = "📋 Instructions:";
                TxtInstruction1.Text = "1. Place QR code in frame";
                TxtInstruction2.Text = "2. Or select QR image from gallery";
                TxtInstruction3.Text = "3. System will auto scan and save";
                BtnSelectImage.Content = "📁 Select QR Image";
                BtnSkip.Content = "⏭️ Skip";
                TxtInfo.Text = "💡 Only need to scan once";
            }
            else // zh
            {
                TxtSubtitle.Text = "扫描二维码以访问";
                TxtLoading.Text = "正在启动相机...";
                TxtSuccess.Text = "扫描成功！";
                TxtSuccessDetail.Text = "正在跳转到主页...";
                TxtInstructionTitle.Text = "📋 说明：";
                TxtInstruction1.Text = "1. 将二维码放入框架";
                TxtInstruction2.Text = "2. 或从图库选择二维码图片";
                TxtInstruction3.Text = "3. 系统将自动扫描并保存";
                BtnSelectImage.Content = "📁 选择二维码图片";
                BtnSkip.Content = "⏭️ 跳过";
                TxtInfo.Text = "💡 只需扫描一次";
            }
        }

        private async Task CheckIfAlreadyScanned()
        {
            try
            {
                // LUÔN kiểm tra API trước - DB là nguồn sự thật duy nhất
                // File local chỉ là cache phụ, không được dùng để bypass API
                bool apiSaysScanned = false;
                
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(5);
                        var response = await client.GetAsync($"{AppConfig.ApiBaseUrl}?action=checkQRScan&deviceId={_deviceId}");
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var result = JsonSerializer.Deserialize<JsonElement>(json);
                            
                            if (result.TryGetProperty("hasScanned", out var hasScannedProp) && 
                                hasScannedProp.GetBoolean())
                            {
                                apiSaysScanned = true;
                                System.Diagnostics.Debug.WriteLine("✅ [QR] Already scanned (API confirmed)");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [QR] API check failed ({ex.Message}), falling back to local storage");
                    // Nếu API lỗi (offline), mới dùng file local làm fallback
                    var storageService = new StorageService();
                    apiSaysScanned = storageService.HasScannedQR();
                }

                if (apiSaysScanned)
                {
                    // Đồng bộ lại local file
                    var storageService = new StorageService();
                    storageService.SaveQRScanned();
                    await ShowSuccessAndProceed();
                    return;
                }

                // API xác nhận chưa quét → Xóa file local cũ (nếu có) để đồng bộ
                var storage = new StorageService();
                if (storage.HasScannedQR())
                {
                    storage.ClearQRScanned();
                    System.Diagnostics.Debug.WriteLine("🗑️ [QR] Local file cleared (out of sync with DB)");
                }

                // Khởi động màn hình quét
                await InitializeQRScanner();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QR] Check error: {ex.Message}");
                await InitializeQRScanner();
            }
        }

        private async Task InitializeQRScanner()
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                
                var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(
                    null, Path.GetTempPath(), new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions());
                await QRWebView.EnsureCoreWebView2Async(env);

                QRWebView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                QRWebView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                QRWebView.CoreWebView2.Settings.IsStatusBarEnabled = false;

                // Handle camera permission
                QRWebView.CoreWebView2.PermissionRequested += (sender, args) =>
                {
                    if (args.PermissionKind == Microsoft.Web.WebView2.Core.CoreWebView2PermissionKind.Camera)
                    {
                        args.State = Microsoft.Web.WebView2.Core.CoreWebView2PermissionState.Allow;
                    }
                };

                // Handle messages from JavaScript
                QRWebView.CoreWebView2.WebMessageReceived += OnQRScanned;

                // Write HTML to a temp file and serve over HTTPS using VirtualHost mapping
                string tempFolder = Path.Combine(Path.GetTempPath(), "vfg_qr_scanner");
                Directory.CreateDirectory(tempFolder);
                string htmlPath = Path.Combine(tempFolder, "index.html");
                File.WriteAllText(htmlPath, GetQRScannerHTML());

                string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
                QRWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "assets.local", 
                    assetsPath, 
                    Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

                QRWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "qr.local", 
                    tempFolder, 
                    Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

                QRWebView.CoreWebView2.Navigate("https://qr.local/index.html");

                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                ShowStatus($"❌ {(_lang.CurrentLanguage == "vi" ? "Lỗi khởi động camera" : "Camera error")}: {ex.Message}", true);
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnQRScanned(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = e.WebMessageAsJson;
                var data = JsonSerializer.Deserialize<JsonElement>(message);

                if (data.TryGetProperty("type", out var type))
                {
                    string typeStr = type.GetString();
                    if (typeStr == "qrScanned")
                    {
                        if (data.TryGetProperty("code", out var code))
                        {
                            var qrCode = code.GetString();
                            System.Diagnostics.Debug.WriteLine($"📱 [QR] Scanned: {qrCode}");
                            
                            if (!_hasScanned)
                            {
                                _hasScanned = true;
                                await SaveQRScan(qrCode);
                            }
                        }
                    }
                    else if (typeStr == "scanFailed")
                    {
                        ShowStatus(_lang.CurrentLanguage == "vi" ? "❌ Không tìm thấy mã QR trong ảnh. Vui lòng thử lại." : "❌ Could not find QR code in image. Please try again.", true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QR] Parse error: {ex.Message}");
            }
        }

        private async Task SaveQRScan(string qrCode)
        {
            try
            {
                // Save to local storage
                var storageService = new StorageService();
                storageService.SaveQRScanned();

                // Save to API
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    
                    var scanData = new
                    {
                        deviceId = _deviceId,
                        qrCode = qrCode,
                        deviceName = Environment.MachineName,
                        osVersion = Environment.OSVersion.ToString()
                    };

                    var json = JsonSerializer.Serialize(scanData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = await client.PostAsync($"{AppConfig.ApiBaseUrl}?action=saveQRScan", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ [QR] Saved to API successfully");
                    }
                }

                await ShowSuccessAndProceed();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QR] Save error: {ex.Message}");
                // Still proceed even if save fails
                await ShowSuccessAndProceed();
            }
        }

        private async Task ShowSuccessAndProceed()
        {
            // Hiện overlay thành công
            SuccessOverlay.Visibility = Visibility.Visible;
            StatusBorder.Visibility = Visibility.Collapsed;

            // Đợi 2 giây cho user thấy thông báo
            await Task.Delay(2000);

            // Mở MainWindow rồi đóng cửa sổ QR
            try
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QR] Open MainWindow error: {ex.Message}");
            }
            this.Close();
        }

        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = _lang.CurrentLanguage == "vi" ? "Chọn ảnh QR Code" : "Select QR Code Image",
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var imagePath = openFileDialog.FileName;
                    BtnSelectImage.IsEnabled = false;
                    ShowStatus(_lang.CurrentLanguage == "vi" ? "🔍 Đang quét ảnh..." : "🔍 Scanning...", false);

                    // Add 8-second timeout - ZXing can hang on bad images
                    var decodeTask = Task.Run(() => DecodeQRCodeFromFile(imagePath));
                    var timeoutTask = Task.Delay(8000);
                    var finished = await Task.WhenAny(decodeTask, timeoutTask);

                    if (finished == timeoutTask)
                    {
                        ShowStatus(
                            _lang.CurrentLanguage == "vi"
                                ? "❌ Quét quá lâu. Hãy thử ảnh khác rõ nét hơn."
                                : "❌ Timeout. Try a clearer image.",
                            true);
                    }
                    else
                    {
                        try
                        {
                            string qrCode = await decodeTask;
                            if (!string.IsNullOrEmpty(qrCode))
                            {
                                System.Diagnostics.Debug.WriteLine($"✅ [ZXing] OK: {qrCode}");
                                if (!_hasScanned)
                                {
                                    _hasScanned = true;
                                    await SaveQRScan(qrCode);
                                }
                            }
                            else
                            {
                                ShowStatus(
                                    _lang.CurrentLanguage == "vi"
                                        ? "❌ Không tìm thấy mã QR. Thử ảnh khác."
                                        : "❌ No QR code found. Try another image.",
                                    true);
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowStatus($"❌ {ex.Message}", true);
                        }
                    }
                    BtnSelectImage.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"❌ {ex.Message}", true);
                BtnSelectImage.IsEnabled = true;
            }
        }

        private string DecodeQRCodeFromFile(string imagePath)
        {
            using (var bitmap = new Bitmap(imagePath))
            {
                // Dùng tên đầy đủ để tránh xung đột
                var reader = new ZXing.Windows.Compatibility.BarcodeReader();
                reader.AutoRotate = true;
                reader.TryInverted = true;
                reader.Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE },
                    // Cho phép nhận diện mã xa, nghiêng, lệch
                    PureBarcode = false,
                };

                var result = reader.Decode(bitmap);
                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ [ZXing] Decoded: {result.Text}");
                    return result.Text;
                }

                // Thử lại với tất cả định dạng barcode
                reader.Options.PossibleFormats = null;
                result = reader.Decode(bitmap);
                System.Diagnostics.Debug.WriteLine(result != null
                    ? $"✅ [ZXing] Decoded (all): {result.Text}"
                    : "❌ [ZXing] No QR found");
                return result?.Text;
            }
        }

        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap wbm)
        {
            System.Drawing.Bitmap bmp;
            using (var ms = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(ms);
                ms.Position = 0;
                bmp = new System.Drawing.Bitmap(ms);
            }
            return bmp;
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            string message = _lang.CurrentLanguage == "vi" ? 
                "Bạn có chắc muốn bỏ qua quét QR?\nBạn có thể quét sau trong trang chủ." :
                "Are you sure you want to skip QR scan?\nYou can scan later from home page.";
            
            string title = _lang.CurrentLanguage == "vi" ? "Xác nhận" : "Confirm";
            string yesText = _lang.CurrentLanguage == "vi" ? "Có" : "Yes";
            string noText = _lang.CurrentLanguage == "vi" ? "Không" : "No";

            bool result = ConfirmDialog.Show(message, title, yesText, noText);

            if (result)
            {
                // Mở MainWindow và đóng QRScannerWindow
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        private void ShowStatus(string message, bool isError)
        {
            StatusBorder.Visibility = Visibility.Visible;
            StatusBorder.Background = isError ? 
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 224)) :
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
            TxtStatus.Text = message;
            TxtStatus.Foreground = isError ?
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 81, 0)) :
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 125, 50));
        }

        private string GetDeviceId()
        {
            try
            {
                // Try to get unique device ID from hardware
                var query = new SelectQuery("Win32_ComputerSystemProduct");
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        var uuid = mo["UUID"]?.ToString();
                        if (!string.IsNullOrEmpty(uuid))
                        {
                            return uuid;
                        }
                    }
                }
            }
            catch { }

            // Fallback to machine name + user name
            return $"{Environment.MachineName}_{Environment.UserName}";
        }

        private string GetQRScannerHTML()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'/>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <script src='https://assets.local/html5-qrcode.min.js'></script>
    <style>
        body { margin: 0; padding: 0; background: #000; overflow: hidden; }
        #reader { width: 100%; }
        #status { text-align: center; padding: 8px; font-size: 13px; color: #ccc; background: #111; }
    </style>
</head>
<body>
    <div id='reader'></div>
    <div id='status'>📷 Initializing...</div>
    <script>
        var scanned = false;
        function onOk(text) {
            if (scanned) return;
            scanned = true;
            document.getElementById('status').innerText = '✅ ' + text;
            window.chrome.webview.postMessage(JSON.stringify({ type: 'qrScanned', code: text }));
        }

        var scanner = new Html5QrcodeScanner('reader', {
            fps: 15,
            qrbox: { width: 250, height: 250 },
            rememberLastUsedCamera: true,
            showTorchButtonIfSupported: false,
            showZoomSliderIfSupported: false,
            defaultZoomValueIfSupported: 2
        }, false);

        scanner.render(onOk, function(err) {});
        document.getElementById('status').innerText = '📷 Hướng camera vào mã QR';
    </script>
</body>
</html>";
        }
    }
}
