using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Text.RegularExpressions; // Thêm thư viện này để dọn dẹp văn bản

namespace VietnamFoodGuide.Services
{
    public class SpeechService : IDisposable
    {
        private IWavePlayer waveOut;
        private Mp3FileReader mp3Reader;
        private readonly HttpClient httpClient = new HttpClient();

        public SpeechService()
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        public async void Speak(string text, string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            Stop();

            // 1. Làm sạch văn bản: Loại bỏ ký tự xuống dòng, tab để tránh lỗi URL
            string cleanText = Regex.Replace(text, @"\s+", " ").Trim();

            // 2. Giới hạn độ dài (Google TTS chỉ nhận dưới ~200 ký tự mỗi request)
            if (cleanText.Length > 180) cleanText = cleanText.Substring(0, 180);

            string lang = cultureCode.Substring(0, 2);
            string encodedText = Uri.EscapeDataString(cleanText);
            string url = $"https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl={lang}&q={encodedText}";

            try
            {
                byte[] data = await httpClient.GetByteArrayAsync(url);
                var ms = new MemoryStream(data);

                // Dùng NAudio để phát MP3 từ MemoryStream
                mp3Reader = new Mp3FileReader(ms);
                waveOut = new WaveOutEvent();
                waveOut.Init(mp3Reader);
                waveOut.Play();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi thuyết minh: " + ex.Message);
            }
        }

        public void Stop()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            if (mp3Reader != null)
            {
                mp3Reader.Dispose();
                mp3Reader = null;
            }
        }

        public void Dispose()
        {
            Stop();
            httpClient.Dispose();
        }
    }
}