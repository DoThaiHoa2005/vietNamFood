using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service tạo file audio MP3 từ Google Translate TTS
    /// KHÔNG CẦN cài giọng đọc Windows!
    /// </summary>
    public class GoogleTTSAudioGenerator
    {
        private readonly string _audioFolder;
        private readonly ApiFoodService _apiService;
        private readonly HttpClient _httpClient;

        public GoogleTTSAudioGenerator()
        {
            // Thư mục lưu audio: Assets/Audio
            _audioFolder = Path.Combine(AppContext.BaseDirectory, "Assets", "Audio");
            
            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(_audioFolder))
            {
                Directory.CreateDirectory(_audioFolder);
            }

            _apiService = new ApiFoodService();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        /// <summary>
        /// Tạo file audio cho TẤT CẢ quán ăn (3 ngôn ngữ)
        /// </summary>
        public async Task<GenerateAudioResult> GenerateAllAudioAsync(Action<int, int, string> progressCallback = null)
        {
            var result = new GenerateAudioResult();
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"🚀 [GoogleTTS] Bắt đầu tạo audio cho tất cả quán ăn...");
                progressCallback?.Invoke(0, 0, "🔄 Đang tải danh sách quán ăn...");

                // Load tất cả foods từ database
                var foods = await _apiService.LoadFoodsAsync();
                
                if (foods == null || foods.Count == 0)
                {
                    result.ErrorMessage = "❌ Không có quán ăn nào trong database";
                    System.Diagnostics.Debug.WriteLine(result.ErrorMessage);
                    progressCallback?.Invoke(0, 0, result.ErrorMessage);
                    return result;
                }

                int total = foods.Count;
                int current = 0;

                System.Diagnostics.Debug.WriteLine($"📝 [GoogleTTS] Tìm thấy {total} quán ăn");
                progressCallback?.Invoke(0, total, $"📝 Tìm thấy {total} quán ăn");

                foreach (var food in foods)
                {
                    current++;
                    string message = $"[{current}/{total}] Đang tạo audio cho: {food.Name}";
                    System.Diagnostics.Debug.WriteLine($"🍽️ {message}");
                    progressCallback?.Invoke(current, total, message);

                    try
                    {
                        // Tạo audio cho 3 ngôn ngữ
                        var audioUrls = await GenerateAudioForFoodAsync(food);
                        
                        if (audioUrls != null && audioUrls.Count > 0)
                        {
                            // Cập nhật AudioUrl vào database qua API
                            await UpdateAudioUrlsAsync(food.Id, audioUrls);
                            result.SuccessCount++;
                            System.Diagnostics.Debug.WriteLine($"✅ [GoogleTTS] Thành công: {food.Name} ({audioUrls.Count}/3 file)");
                        }
                        else
                        {
                            result.FailedCount++;
                            System.Diagnostics.Debug.WriteLine($"❌ [GoogleTTS] Thất bại: {food.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [GoogleTTS] Lỗi tạo audio cho {food.Name}: {ex.Message}");
                        result.FailedCount++;
                    }

                    // Delay nhỏ để tránh bị Google block
                    await Task.Delay(500);
                }

                result.IsSuccess = true;
                result.Message = $"✅ Hoàn thành! Thành công: {result.SuccessCount}, Thất bại: {result.FailedCount}";
                System.Diagnostics.Debug.WriteLine($"🎉 {result.Message}");
                progressCallback?.Invoke(total, total, result.Message);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"❌ Lỗi: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ [GoogleTTS] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ [GoogleTTS] Stack trace: {ex.StackTrace}");
                progressCallback?.Invoke(0, 0, result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Tạo file audio cho 1 quán ăn (3 ngôn ngữ)
        /// </summary>
        private async Task<Dictionary<string, string>> GenerateAudioForFoodAsync(FoodItem food)
        {
            var audioUrls = new Dictionary<string, string>();

            try
            {
                System.Diagnostics.Debug.WriteLine($"🍽️ [GoogleTTS] Bắt đầu tạo audio cho: {food.Name} (ID: {food.Id})");

                // 1. Tiếng Việt
                if (!string.IsNullOrEmpty(food.DescriptionVI))
                {
                    System.Diagnostics.Debug.WriteLine($"🇻🇳 [GoogleTTS] Tạo VI cho {food.Name}...");
                    string viPath = await GenerateSingleAudioAsync(
                        food.Id, 
                        "VI", 
                        food.DescriptionVI, 
                        "vi",
                        food.Name
                    );
                    if (!string.IsNullOrEmpty(viPath))
                    {
                        audioUrls["VI"] = viPath;
                        System.Diagnostics.Debug.WriteLine($"✅ [GoogleTTS] VI: {viPath}");
                    }
                }

                // 2. Tiếng Anh
                if (!string.IsNullOrEmpty(food.DescriptionEN))
                {
                    System.Diagnostics.Debug.WriteLine($"🇺🇸 [GoogleTTS] Tạo EN cho {food.Name}...");
                    string enPath = await GenerateSingleAudioAsync(
                        food.Id, 
                        "EN", 
                        food.DescriptionEN, 
                        "en",
                        food.Name
                    );
                    if (!string.IsNullOrEmpty(enPath))
                    {
                        audioUrls["EN"] = enPath;
                        System.Diagnostics.Debug.WriteLine($"✅ [GoogleTTS] EN: {enPath}");
                    }
                }

                // 3. Tiếng Trung
                if (!string.IsNullOrEmpty(food.DescriptionCN))
                {
                    System.Diagnostics.Debug.WriteLine($"🇨🇳 [GoogleTTS] Tạo CN cho {food.Name}...");
                    string cnPath = await GenerateSingleAudioAsync(
                        food.Id, 
                        "CN", 
                        food.DescriptionCN, 
                        "zh-CN",
                        food.Name
                    );
                    if (!string.IsNullOrEmpty(cnPath))
                    {
                        audioUrls["CN"] = cnPath;
                        System.Diagnostics.Debug.WriteLine($"✅ [GoogleTTS] CN: {cnPath}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"🎉 [GoogleTTS] Hoàn thành {food.Name}: {audioUrls.Count}/3 file");
                return audioUrls.Count > 0 ? audioUrls : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [GoogleTTS] Lỗi GenerateAudioForFoodAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tạo 1 file audio MP3 từ Google Translate TTS
        /// </summary>
        private async Task<string> GenerateSingleAudioAsync(int foodId, string language, string text, string langCode, string foodName = "")
        {
            try
            {
                // Sanitize food name for filename
                string safeFoodName = "";
                if (!string.IsNullOrEmpty(foodName))
                {
                    safeFoodName = foodName
                        .Replace(" ", "_")
                        .Replace("/", "_")
                        .Replace("\\", "_")
                        .Replace(":", "_")
                        .Replace("*", "_")
                        .Replace("?", "_")
                        .Replace("\"", "_")
                        .Replace("<", "_")
                        .Replace(">", "_")
                        .Replace("|", "_");
                    safeFoodName = "_" + safeFoodName;
                }

                string mp3File = Path.Combine(_audioFolder, $"{foodId}{safeFoodName}_{language}.mp3");

                System.Diagnostics.Debug.WriteLine($"🎵 [GoogleTTS] Tạo file: {mp3File}");
                System.Diagnostics.Debug.WriteLine($"📝 [GoogleTTS] Text: {text.Substring(0, Math.Min(50, text.Length))}...");
                System.Diagnostics.Debug.WriteLine($"🌐 [GoogleTTS] Language: {langCode}");

                // Google Translate TTS API
                // Chia text thành các đoạn nhỏ (Google giới hạn 200 ký tự)
                var chunks = SplitTextIntoChunks(text, 200);
                var audioChunks = new List<byte[]>();

                foreach (var chunk in chunks)
                {
                    string encodedText = Uri.EscapeDataString(chunk);
                    string url = $"https://translate.google.com/translate_tts?ie=UTF-8&tl={langCode}&client=tw-ob&q={encodedText}";
                    
                    System.Diagnostics.Debug.WriteLine($"📥 [GoogleTTS] Downloading chunk: {chunk.Substring(0, Math.Min(30, chunk.Length))}...");
                    
                    var audioData = await _httpClient.GetByteArrayAsync(url);
                    audioChunks.Add(audioData);
                    
                    // Delay nhỏ giữa các request
                    await Task.Delay(300);
                }

                // Gộp các chunk lại
                using (var fileStream = new FileStream(mp3File, FileMode.Create, FileAccess.Write))
                {
                    foreach (var chunk in audioChunks)
                    {
                        await fileStream.WriteAsync(chunk, 0, chunk.Length);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ [GoogleTTS] Đã tạo MP3: {mp3File} ({new FileInfo(mp3File).Length / 1024} KB)");

                // Trả về relative path để lưu vào database
                return $"/Assets/Audio/{foodId}{safeFoodName}_{language}.mp3";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [GoogleTTS] Lỗi GenerateSingleAudioAsync ({language}): {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ [GoogleTTS] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Chia text thành các đoạn nhỏ (Google giới hạn 200 ký tự)
        /// </summary>
        private List<string> SplitTextIntoChunks(string text, int maxLength)
        {
            var chunks = new List<string>();
            
            if (text.Length <= maxLength)
            {
                chunks.Add(text);
                return chunks;
            }

            // Chia theo câu (dấu chấm, dấu phẩy, dấu chấm phẩy)
            var sentences = text.Split(new[] { ". ", ", ", "; ", "。", "，", "；" }, StringSplitOptions.RemoveEmptyEntries);
            
            var currentChunk = new StringBuilder();
            foreach (var sentence in sentences)
            {
                if (currentChunk.Length + sentence.Length + 2 <= maxLength)
                {
                    if (currentChunk.Length > 0)
                        currentChunk.Append(". ");
                    currentChunk.Append(sentence);
                }
                else
                {
                    if (currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString());
                        currentChunk.Clear();
                    }
                    
                    // Nếu câu quá dài, chia nhỏ hơn
                    if (sentence.Length > maxLength)
                    {
                        for (int i = 0; i < sentence.Length; i += maxLength)
                        {
                            int length = Math.Min(maxLength, sentence.Length - i);
                            chunks.Add(sentence.Substring(i, length));
                        }
                    }
                    else
                    {
                        currentChunk.Append(sentence);
                    }
                }
            }
            
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString());
            }

            return chunks;
        }

        /// <summary>
        /// Cập nhật AudioUrl vào database qua API
        /// </summary>
        private async Task UpdateAudioUrlsAsync(int foodId, Dictionary<string, string> audioUrls)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    var data = new
                    {
                        foodId = foodId,
                        audioUrl_VI = audioUrls.ContainsKey("VI") ? audioUrls["VI"] : null,
                        audioUrl_EN = audioUrls.ContainsKey("EN") ? audioUrls["EN"] : null,
                        audioUrl_CN = audioUrls.ContainsKey("CN") ? audioUrls["CN"] : null
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{AppConfig.ApiBaseUrl}?action=updateAudioUrls", content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"📤 [GoogleTTS] Cập nhật AudioUrl cho Food #{foodId}: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [GoogleTTS] Lỗi UpdateAudioUrlsAsync: {ex.Message}");
            }
        }
    }
}
