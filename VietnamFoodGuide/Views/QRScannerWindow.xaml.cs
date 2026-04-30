using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using VietnamFoodGuide.Models.Entities;
using VietnamFoodGuide.Services;

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
                // Check local storage first
                var storageService = new StorageService();
                var hasScannedLocal = storageService.HasScannedQR();

                if (hasScannedLocal)
                {
                    System.Diagnostics.Debug.WriteLine("✅ [QR] Already scanned (local storage)");
                    await ShowSuccessAndProceed();
                    return;
                }

                // Check API
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
                            System.Diagnostics.Debug.WriteLine("✅ [QR] Already scanned (API)");
                            storageService.SaveQRScanned(); // Save to local storage
                            await ShowSuccessAndProceed();
                            return;
                        }
                    }
                }

                // Not scanned yet, initialize QR scanner
                await InitializeQRScanner();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [QR] Check error: {ex.Message}");
                // If error, allow scanning
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

                // Load QR scanner HTML
                QRWebView.NavigateToString(GetQRScannerHTML());

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

                if (data.TryGetProperty("type", out var type) && type.GetString() == "qrScanned")
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
            await Dispatcher.InvokeAsync(async () =>
            {
                SuccessOverlay.Visibility = Visibility.Visible;
                
                // Wait 2 seconds then close window
                await Task.Delay(2000);
                
                // Close this window (MainWindow will refresh automatically)
                this.DialogResult = true;
                this.Close();
            });
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
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
                    System.Diagnostics.Debug.WriteLine($"📁 [QR] Selected image: {imagePath}");
                    
                    // Send image to JavaScript for QR decoding
                    var script = $"decodeQRFromImage('{imagePath.Replace("\\", "\\\\")}');";
                    QRWebView.CoreWebView2?.ExecuteScriptAsync(script);
                    
                    ShowStatus(_lang.CurrentLanguage == "vi" ? "🔍 Đang quét ảnh..." : "🔍 Scanning image...", false);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"❌ {ex.Message}", true);
            }
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                _lang.CurrentLanguage == "vi" ? 
                    "Bạn có chắc muốn bỏ qua quét QR?\nBạn có thể quét sau trong trang chủ." :
                    "Are you sure you want to skip QR scan?\nYou can scan later from home page.",
                _lang.CurrentLanguage == "vi" ? "Xác nhận" : "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.DialogResult = false;
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
    <script src='https://unpkg.com/html5-qrcode@2.3.8/html5-qrcode.min.js'></script>
    <style>
        body { margin: 0; padding: 0; background: #f8f9fa; font-family: Arial, sans-serif; }
        #reader { width: 100%; height: 350px; }
        #reader video { width: 100% !important; height: 100% !important; object-fit: cover; border-radius: 12px; }
        .status { text-align: center; padding: 12px; font-size: 14px; color: #5f6368; }
    </style>
</head>
<body>
    <div id='reader'></div>
    <div class='status' id='status'>📷 Camera ready - Point at QR code</div>
    
    <script>
        let html5QrCode;
        let isScanning = false;

        function onScanSuccess(decodedText, decodedResult) {
            if (isScanning) return;
            isScanning = true;
            
            console.log('✅ QR Code scanned:', decodedText);
            document.getElementById('status').innerHTML = '✅ Scanned successfully!';
            
            // Send to C#
            window.chrome.webview.postMessage(JSON.stringify({
                type: 'qrScanned',
                code: decodedText
            }));
            
            // Stop scanner
            if (html5QrCode) {
                html5QrCode.stop().then(() => {
                    console.log('Scanner stopped');
                }).catch(err => {
                    console.error('Stop error:', err);
                });
            }
        }

        function onScanError(errorMessage) {
            // Ignore scan errors (too noisy)
        }

        // Start scanner
        html5QrCode = new Html5Qrcode('reader');
        html5QrCode.start(
            { facingMode: 'environment' },
            { fps: 10, qrbox: { width: 250, height: 250 } },
            onScanSuccess,
            onScanError
        ).catch(err => {
            console.error('Camera error:', err);
            document.getElementById('status').innerHTML = '❌ Camera not available. Please use Select Image button.';
        });

        // Function to decode QR from image file
        function decodeQRFromImage(imagePath) {
            // This would need additional implementation
            // For now, we'll use the file input method
            console.log('Decode from image:', imagePath);
        }
    </script>
</body>
</html>";
        }
    }
}
