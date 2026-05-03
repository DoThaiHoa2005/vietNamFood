using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Lame;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service tự động tạo file audio MP3 từ Description cho 3 ngôn ngữ
    /// Sử dụng System.Speech.Synthesis + NAudio + LAME
    /// </summary>
    public class AudioGeneratorService
    {
        private readonly string _audioFolder;
        private readonly ApiFoodService _apiService;

        public AudioGeneratorService()
        {
            // Thư mục lưu audio: Assets/Audio
            _audioFolder = Path.Combine(AppContext.BaseDirectory, "Assets", "Audio");
            
            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(_audioFolder))
            {
                Directory.CreateDirectory(_audioFolder);
            }

            _apiService = new ApiFoodService();
        }

        /// <summary>
        /// Tạo file audio cho TẤT CẢ quán ăn (3 ngôn ngữ)
        /// </summary>
        public async Task<GenerateAudioResult> GenerateAllAudioAsync(Action<int, int, string> progressCallback = null)
        {
            var result = new GenerateAudioResult();
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"🚀 [Audio] Bắt đầu tạo audio cho tất cả quán ăn...");
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

                System.Diagnostics.Debug.WriteLine($"📝 [Audio] Tìm thấy {total} quán ăn");
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
                            System.Diagnostics.Debug.WriteLine($"✅ [Audio] Thành công: {food.Name} ({audioUrls.Count}/3 file)");
                        }
                        else
                        {
                            result.FailedCount++;
                            System.Diagnostics.Debug.WriteLine($"❌ [Audio] Thất bại: {food.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [Audio] Lỗi tạo audio cho {food.Name}: {ex.Message}");
                        result.FailedCount++;
                    }
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
                System.Diagnostics.Debug.WriteLine($"❌ [Audio] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ [Audio] Stack trace: {ex.StackTrace}");
                progressCallback?.Invoke(0, 0, result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Tạo file audio cho 1 quán ăn (3 ngôn ngữ)
        /// </summary>
        public async Task<Dictionary<string, string>> GenerateAudioForFoodAsync(FoodItem food)
        {
            var audioUrls = new Dictionary<string, string>();

            try
            {
                System.Diagnostics.Debug.WriteLine($"🍽️ [Audio] Bắt đầu tạo audio cho: {food.Name} (ID: {food.Id})");

                // 1. Tiếng Việt
                if (!string.IsNullOrEmpty(food.DescriptionVI))
                {
                    System.Diagnostics.Debug.WriteLine($"🇻🇳 [Audio] Tạo VI cho {food.Name}...");
                    string viPath = await GenerateSingleAudioAsync(
                        food.Id, 
                        "VI", 
                        food.DescriptionVI, 
                        "vi-VN",
                        food.Name
                    );
                    if (!string.IsNullOrEmpty(viPath))
                    {
                        audioUrls["VI"] = viPath;
                        System.Diagnostics.Debug.WriteLine($"✅ [Audio] VI: {viPath}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [Audio] Lỗi tạo VI");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [Audio] Không có DescriptionVI");
                }

                // 2. Tiếng Anh
                if (!string.IsNullOrEmpty(food.DescriptionEN))
                {
                    System.Diagnostics.Debug.WriteLine($"🇺🇸 [Audio] Tạo EN cho {food.Name}...");
                    string enPath = await GenerateSingleAudioAsync(
                        food.Id, 
                        "EN", 
                        food.DescriptionEN, 
                        "en-US",
                        food.Name
                    );
                    if (!string.IsNullOrEmpty(enPath))
                    {
                        audioUrls["EN"] = enPath;
                        System.Diagnostics.Debug.WriteLine($"✅ [Audio] EN: {enPath}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [Audio] Lỗi tạo EN");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [Audio] Không có DescriptionEN");
                }

                // 3. Tiếng Trung
                if (!string.IsNullOrEmpty(food.DescriptionCN))
                {
                    System.Diagnostics.Debug.WriteLine($"🇨🇳 [Audio] Tạo CN cho {food.Name}...");
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
                        System.Diagnostics.Debug.WriteLine($"✅ [Audio] CN: {cnPath}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [Audio] Lỗi tạo CN");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [Audio] Không có DescriptionCN");
                }

                System.Diagnostics.Debug.WriteLine($"🎉 [Audio] Hoàn thành {food.Name}: {audioUrls.Count}/3 file");
                return audioUrls.Count > 0 ? audioUrls : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [Audio] Lỗi GenerateAudioForFoodAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ [Audio] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Tạo 1 file audio MP3 từ text
        /// </summary>
        private async Task<string> GenerateSingleAudioAsync(int foodId, string language, string text, string locale, string foodName = "")
        {
            return await Task.Run(() =>
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

                    // File paths with food name
                    string wavFile = Path.Combine(_audioFolder, $"{foodId}{safeFoodName}_{language}.wav");
                    string mp3File = Path.Combine(_audioFolder, $"{foodId}{safeFoodName}_{language}.mp3");

                    System.Diagnostics.Debug.WriteLine($"🎵 [Audio] Tạo file: {mp3File}");
                    System.Diagnostics.Debug.WriteLine($"📝 [Audio] Text: {text.Substring(0, Math.Min(50, text.Length))}...");
                    System.Diagnostics.Debug.WriteLine($"🌐 [Audio] Locale: {locale}");

                    // Bước 1: Text → WAV (System.Speech)
                    using (var synth = new SpeechSynthesizer())
                    {
                        // Chọn giọng đọc theo locale
                        var voices = synth.GetInstalledVoices();
                        System.Diagnostics.Debug.WriteLine($"🔊 [Audio] Tìm thấy {voices.Count} giọng đọc:");
                        foreach (var v in voices)
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {v.VoiceInfo.Name} ({v.VoiceInfo.Culture.Name})");
                        }

                        var voice = voices.FirstOrDefault(v => v.VoiceInfo.Culture.Name == locale);
                        
                        if (voice != null)
                        {
                            synth.SelectVoice(voice.VoiceInfo.Name);
                            System.Diagnostics.Debug.WriteLine($"✅ [Audio] Chọn giọng: {voice.VoiceInfo.Name}");
                        }
                        else
                        {
                            // Fallback: dùng giọng mặc định
                            System.Diagnostics.Debug.WriteLine($"⚠️ [Audio] Không tìm thấy giọng {locale}, dùng giọng mặc định: {synth.Voice.Name}");
                        }

                        synth.SetOutputToWaveFile(wavFile);
                        synth.Speak(text);
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ [Audio] Đã tạo WAV: {wavFile}");

                    // Bước 2: WAV → MP3 (NAudio + LAME)
                    using (var reader = new AudioFileReader(wavFile))
                    using (var writer = new LameMP3FileWriter(mp3File, reader.WaveFormat, LAMEPreset.ABR_128))
                    {
                        reader.CopyTo(writer);
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ [Audio] Đã tạo MP3: {mp3File}");

                    // Xóa file WAV tạm
                    if (File.Exists(wavFile))
                    {
                        File.Delete(wavFile);
                        System.Diagnostics.Debug.WriteLine($"🗑️ [Audio] Đã xóa WAV tạm");
                    }

                    // Trả về relative path để lưu vào database
                    return $"/Assets/Audio/{foodId}{safeFoodName}_{language}.mp3";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [Audio] Lỗi GenerateSingleAudioAsync ({language}): {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"❌ [Audio] Stack trace: {ex.StackTrace}");
                    return null;
                }
            });
        }

        /// <summary>
        /// Cập nhật AudioUrl vào database qua API
        /// </summary>
        private async Task UpdateAudioUrlsAsync(int foodId, Dictionary<string, string> audioUrls)
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
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
                    var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{AppConfig.ApiBaseUrl}?action=updateAudioUrls", content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"📤 Cập nhật AudioUrl cho Food #{foodId}: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Lỗi UpdateAudioUrlsAsync: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra xem quán ăn đã có file audio chưa
        /// </summary>
        public bool HasAudioFiles(int foodId)
        {
            string viFile = Path.Combine(_audioFolder, $"{foodId}_VI.mp3");
            string enFile = Path.Combine(_audioFolder, $"{foodId}_EN.mp3");
            string cnFile = Path.Combine(_audioFolder, $"{foodId}_CN.mp3");

            return File.Exists(viFile) || File.Exists(enFile) || File.Exists(cnFile);
        }

        /// <summary>
        /// Lấy đường dẫn file audio theo ngôn ngữ
        /// </summary>
        public string GetAudioPath(int foodId, string language)
        {
            string fileName = $"{foodId}_{language}.mp3";
            string fullPath = Path.Combine(_audioFolder, fileName);

            return File.Exists(fullPath) ? fullPath : null;
        }
    }

    /// <summary>
    /// Kết quả generate audio
    /// </summary>
    public class GenerateAudioResult
    {
        public bool IsSuccess { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
    }
}
