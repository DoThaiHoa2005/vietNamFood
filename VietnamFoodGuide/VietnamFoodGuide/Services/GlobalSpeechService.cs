using System;
using System.Threading.Tasks;
using System.Windows;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Global singleton service cho Web Speech API
    /// Khởi tạo MỘT LẦN khi app start, tái sử dụng cho tất cả windows
    /// </summary>
    public class GlobalSpeechService
    {
        private static GlobalSpeechService _instance;
        private static readonly object _lock = new object();
        
        private Microsoft.Web.WebView2.Wpf.WebView2 _webView;
        private bool _isReady = false;
        private TaskCompletionSource<bool> _readyTask = new TaskCompletionSource<bool>();
        
        public event Action OnSpeechStarted;
        public event Action OnSpeechCompleted;
        public event Action<string> OnSpeechError;
        
        public static GlobalSpeechService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new GlobalSpeechService();
                        }
                    }
                }
                return _instance;
            }
        }
        
        private GlobalSpeechService()
        {
            // Private constructor - singleton pattern
        }
        
        /// <summary>
        /// Khởi tạo WebView2 - Gọi MỘT LẦN khi app start
        /// PHẢI gọi từ UI thread (Dispatcher)
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isReady) return;
            
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 [GlobalSpeech] Bắt đầu khởi tạo WebView2...");
                
                // Tạo một Window ẩn để chứa WebView2
                var hiddenWindow = new Window
                {
                    Width = 1,
                    Height = 1,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    Visibility = Visibility.Hidden,
                    Left = -10000,
                    Top = -10000,
                    Title = "GlobalSpeechService Hidden Window"
                };
                
                var grid = new System.Windows.Controls.Grid();
                hiddenWindow.Content = grid;
                
                _webView = new Microsoft.Web.WebView2.Wpf.WebView2();
                _webView.Visibility = Visibility.Collapsed;
                grid.Children.Add(_webView);
                
                // Show window (cần thiết để WebView2 hoạt động)
                hiddenWindow.Show();
                
                System.Diagnostics.Debug.WriteLine("✅ [GlobalSpeech] Hidden window created");
                
                // Khởi tạo WebView2 environment
                var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(
                    null,
                    System.IO.Path.GetTempPath(),
                    new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions()
                );
                
                System.Diagnostics.Debug.WriteLine("✅ [GlobalSpeech] WebView2 environment created");
                
                // Ensure CoreWebView2
                await _webView.EnsureCoreWebView2Async(env);
                
                System.Diagnostics.Debug.WriteLine("✅ [GlobalSpeech] WebView2 CoreWebView2 ready");
                
                // Load HTML với Web Speech API
                string html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'/>
    <script>
        var currentUtterance = null;
        var isReady = false;
        
        function speak(text, lang) {
            console.log('🗣️ [Global Speech] Speaking:', text.substring(0, 50) + '...', 'Lang:', lang);
            
            if (currentUtterance) {
                window.speechSynthesis.cancel();
            }
            
            try {
                currentUtterance = new SpeechSynthesisUtterance(text);
                currentUtterance.lang = lang;
                currentUtterance.rate = 0.9;
                currentUtterance.pitch = 1.0;
                currentUtterance.volume = 1.0;
                
                var voices = window.speechSynthesis.getVoices();
                var voice = voices.find(function(v) { 
                    return v.lang.startsWith(lang.split('-')[0]); 
                });
                if (voice) {
                    currentUtterance.voice = voice;
                    console.log('🎙️ [Global Speech] Selected voice:', voice.name);
                }
                
                currentUtterance.onstart = function() {
                    console.log('✅ [Global Speech] Started');
                    window.chrome.webview.postMessage(JSON.stringify({type: 'speechStarted'}));
                };
                
                currentUtterance.onend = function() {
                    console.log('✅ [Global Speech] Completed');
                    window.chrome.webview.postMessage(JSON.stringify({type: 'speechCompleted'}));
                    currentUtterance = null;
                };
                
                currentUtterance.onerror = function(e) {
                    console.error('❌ [Global Speech] Error:', e.error);
                    window.chrome.webview.postMessage(JSON.stringify({type: 'speechError', error: e.error}));
                    currentUtterance = null;
                };
                
                window.speechSynthesis.speak(currentUtterance);
            } catch (e) {
                console.error('❌ [Global Speech] Exception:', e.message);
                window.chrome.webview.postMessage(JSON.stringify({type: 'speechError', error: e.message}));
            }
        }
        
        function stop() {
            console.log('⏹️ [Global Speech] Stopping');
            window.speechSynthesis.cancel();
            currentUtterance = null;
            window.chrome.webview.postMessage(JSON.stringify({type: 'speechCompleted'}));
        }
        
        function checkReady() {
            var voices = window.speechSynthesis.getVoices();
            if (voices.length > 0 && !isReady) {
                isReady = true;
                console.log('✅ [Global Speech] Ready! Available voices:', voices.length);
                window.chrome.webview.postMessage(JSON.stringify({type: 'ready'}));
            }
        }
        
        checkReady();
        window.speechSynthesis.onvoiceschanged = checkReady;
        
        setTimeout(function() {
            if (!isReady) {
                console.log('⚠️ [Global Speech] Timeout - forcing ready');
                isReady = true;
                window.chrome.webview.postMessage(JSON.stringify({type: 'ready'}));
            }
        }, 2000);
    </script>
</head>
<body>
    <h3>Global Web Speech API</h3>
</body>
</html>";
                    
                _webView.CoreWebView2.NavigateToString(html);
                _webView.CoreWebView2.WebMessageReceived += OnWebMessage;
                
                System.Diagnostics.Debug.WriteLine("✅ [GlobalSpeech] HTML loaded, waiting for ready...");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [GlobalSpeech] Init error: {ex.Message}");
                _readyTask.TrySetException(ex);
            }
        }
        
        private void OnWebMessage(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson;
                System.Diagnostics.Debug.WriteLine($"📨 [GlobalSpeech] Raw JSON: {json}");
                
                // Parse JSON manually để tránh lỗi deserialize
                using (var doc = System.Text.Json.JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("type", out var typeElement))
                    {
                        var type = typeElement.GetString();
                        System.Diagnostics.Debug.WriteLine($"📨 [GlobalSpeech] WebMessage type: {type}");
                        
                        if (type == "ready")
                        {
                            _isReady = true;
                            _readyTask.TrySetResult(true);
                            System.Diagnostics.Debug.WriteLine("✅ [GlobalSpeech] Web Speech API ready!");
                        }
                        else if (type == "speechStarted")
                        {
                            OnSpeechStarted?.Invoke();
                        }
                        else if (type == "speechCompleted")
                        {
                            OnSpeechCompleted?.Invoke();
                        }
                        else if (type == "speechError")
                        {
                            if (root.TryGetProperty("error", out var errorElement))
                            {
                                var error = errorElement.GetString();
                                OnSpeechError?.Invoke(error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [GlobalSpeech] WebMessage error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Đợi cho đến khi Web Speech API sẵn sàng
        /// </summary>
        public async Task WaitForReadyAsync()
        {
            if (_isReady) return;
            await _readyTask.Task;
        }
        
        /// <summary>
        /// Kiểm tra xem đã sẵn sàng chưa (không đợi)
        /// </summary>
        public bool IsReady => _isReady;
        
        /// <summary>
        /// Phát giọng nói
        /// </summary>
        public void Speak(string text, string languageCode)
        {
            if (!_isReady)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ [GlobalSpeech] Not ready yet!");
                OnSpeechError?.Invoke("Web Speech API chưa sẵn sàng");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(text))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ [GlobalSpeech] Empty text");
                return;
            }
            
            try
            {
                var script = $"speak({System.Text.Json.JsonSerializer.Serialize(text)}, '{languageCode}');";
                _webView.CoreWebView2.ExecuteScriptAsync(script);
                System.Diagnostics.Debug.WriteLine($"🗣️ [GlobalSpeech] Calling speak: {languageCode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [GlobalSpeech] Speak error: {ex.Message}");
                OnSpeechError?.Invoke(ex.Message);
            }
        }
        
        /// <summary>
        /// Dừng giọng nói
        /// </summary>
        public void Stop()
        {
            if (!_isReady) return;
            
            try
            {
                _webView.CoreWebView2.ExecuteScriptAsync("stop();");
                System.Diagnostics.Debug.WriteLine("⏹️ [GlobalSpeech] Stop called");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [GlobalSpeech] Stop error: {ex.Message}");
            }
        }
    }
}
