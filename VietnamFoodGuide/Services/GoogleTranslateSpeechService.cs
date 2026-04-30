using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service sử dụng Google Translate Text-to-Speech API
    /// Giọng nói CHUẨN, không cần WebView2, không cần Web Speech API
    /// </summary>
    public class GoogleTranslateSpeechService
    {
        private static GoogleTranslateSpeechService _instance;
        private static readonly object _lock = new object();
        private readonly HttpClient _httpClient;
        private System.Windows.Media.MediaPlayer _mediaPlayer;
        private bool _isPlaying = false;
        
        public event Action OnSpeechStarted;
        public event Action OnSpeechCompleted;
        public event Action<string> OnSpeechError;
        
        public static GoogleTranslateSpeechService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new GoogleTranslateSpeechService();
                        }
                    }
                }
                return _instance;
            }
        }
        
        private GoogleTranslateSpeechService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            // Khởi tạo MediaPlayer
            Application.Current.Dispatcher.Invoke(() =>
            {
                _mediaPlayer = new System.Windows.Media.MediaPlayer();
                _mediaPlayer.MediaEnded += OnMediaEnded;
                _mediaPlayer.MediaFailed += OnMediaFailed;
            });
            
            Debug.WriteLine("✅ [GoogleTTS] Service initialized");
        }
        
        /// <summary>
        /// Luôn sẵn sàng (không cần khởi tạo async)
        /// </summary>
        public bool IsReady => true;
        
        /// <summary>
        /// Phát giọng nói bằng Google Translate TTS
        /// </summary>
        public async void Speak(string text, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.WriteLine("⚠️ [GoogleTTS] Empty text");
                return;
            }
            
            try
            {
                // Dừng giọng nói hiện tại (nếu có)
                Stop();
                
                Debug.WriteLine($"🗣️ [GoogleTTS] Speaking: {text.Substring(0, Math.Min(50, text.Length))}... Lang: {languageCode}");
                
                // Chuyển language code (vi-VN → vi, en-US → en, zh-CN → zh)
                string lang = languageCode.Split('-')[0];
                
                // Tạo URL Google Translate TTS
                // Chia text thành các đoạn nhỏ (Google TTS giới hạn ~200 ký tự)
                var chunks = SplitText(text, 200);
                
                _isPlaying = true;
                OnSpeechStarted?.Invoke();
                
                // Phát từng đoạn
                foreach (var chunk in chunks)
                {
                    if (!_isPlaying) break; // User đã dừng
                    
                    string url = $"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl={lang}&q={Uri.EscapeDataString(chunk)}";
                    
                    Debug.WriteLine($"🌐 [GoogleTTS] Fetching audio: {url.Substring(0, Math.Min(100, url.Length))}...");
                    
                    // Tải audio từ Google
                    var audioData = await _httpClient.GetByteArrayAsync(url);
                    
                    // Lưu vào file tạm
                    string tempFile = Path.Combine(Path.GetTempPath(), $"tts_{Guid.NewGuid()}.mp3");
                    File.WriteAllBytes(tempFile, audioData);
                    
                    Debug.WriteLine($"💾 [GoogleTTS] Saved to: {tempFile}");
                    
                    // Phát audio
                    await PlayAudioAsync(tempFile);
                    
                    // Xóa file tạm
                    try { File.Delete(tempFile); } catch { }
                }
                
                if (_isPlaying)
                {
                    _isPlaying = false;
                    OnSpeechCompleted?.Invoke();
                    Debug.WriteLine("✅ [GoogleTTS] Completed");
                }
            }
            catch (Exception ex)
            {
                _isPlaying = false;
                Debug.WriteLine($"❌ [GoogleTTS] Error: {ex.Message}");
                OnSpeechError?.Invoke(ex.Message);
            }
        }
        
        /// <summary>
        /// Dừng giọng nói
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying) return;
            
            _isPlaying = false;
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _mediaPlayer.Stop();
                    Debug.WriteLine("⏹️ [GoogleTTS] Stopped");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ [GoogleTTS] Stop error: {ex.Message}");
                }
            });
            
            OnSpeechCompleted?.Invoke();
        }
        
        /// <summary>
        /// Chia text thành các đoạn nhỏ
        /// </summary>
        private string[] SplitText(string text, int maxLength)
        {
            if (text.Length <= maxLength)
            {
                return new[] { text };
            }
            
            var chunks = new System.Collections.Generic.List<string>();
            int start = 0;
            
            while (start < text.Length)
            {
                int length = Math.Min(maxLength, text.Length - start);
                
                // Tìm dấu câu gần nhất để cắt
                if (start + length < text.Length)
                {
                    int lastPeriod = text.LastIndexOfAny(new[] { '.', '!', '?', ',', ';', '\n' }, start + length, length);
                    if (lastPeriod > start)
                    {
                        length = lastPeriod - start + 1;
                    }
                }
                
                chunks.Add(text.Substring(start, length).Trim());
                start += length;
            }
            
            return chunks.ToArray();
        }
        
        /// <summary>
        /// Phát audio file và đợi cho đến khi phát xong
        /// </summary>
        private Task PlayAudioAsync(string filePath)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _mediaPlayer.Open(new Uri(filePath));
                    
                    // Đăng ký event tạm thời
                    EventHandler mediaEndedHandler = null;
                    EventHandler<System.Windows.Media.ExceptionEventArgs> mediaFailedHandler = null;
                    
                    mediaEndedHandler = (s, e) =>
                    {
                        _mediaPlayer.MediaEnded -= mediaEndedHandler;
                        _mediaPlayer.MediaFailed -= mediaFailedHandler;
                        tcs.TrySetResult(true);
                    };
                    
                    mediaFailedHandler = (s, e) =>
                    {
                        _mediaPlayer.MediaEnded -= mediaEndedHandler;
                        _mediaPlayer.MediaFailed -= mediaFailedHandler;
                        tcs.TrySetException(e.ErrorException);
                    };
                    
                    _mediaPlayer.MediaEnded += mediaEndedHandler;
                    _mediaPlayer.MediaFailed += mediaFailedHandler;
                    
                    _mediaPlayer.Play();
                    Debug.WriteLine($"▶️ [GoogleTTS] Playing: {filePath}");
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            
            return tcs.Task;
        }
        
        private void OnMediaEnded(object sender, EventArgs e)
        {
            Debug.WriteLine("✅ [GoogleTTS] Media ended");
        }
        
        private void OnMediaFailed(object sender, System.Windows.Media.ExceptionEventArgs e)
        {
            Debug.WriteLine($"❌ [GoogleTTS] Media failed: {e.ErrorException?.Message}");
        }
    }
}
