using System;
using System.Speech.Synthesis;
using System.Linq;
using System.Media;

namespace VietnamFoodGuide.Services
{
    public class SpeechService : IDisposable
    {
        private SpeechSynthesizer _synthesizer;
        private bool isPlaying = false;

        public event Action OnPlaybackStarted;
        public event Action OnPlaybackCompleted;
        public event Action<string> OnError;

        public SpeechService()
        {
            try
            {
                _synthesizer = new SpeechSynthesizer();
                
                // DIAGNOSTIC: List all voices
                System.Diagnostics.Debug.WriteLine("--- VOICES INSTALLED ON SYSTEM ---");
                foreach (var v in _synthesizer.GetInstalledVoices())
                {
                    System.Diagnostics.Debug.WriteLine($"Found Voice: {v.VoiceInfo.Name} ({v.VoiceInfo.Culture})");
                }
                System.Diagnostics.Debug.WriteLine("-----------------------------------");

                _synthesizer.SpeakStarted += (s, e) => { isPlaying = true; OnPlaybackStarted?.Invoke(); };
                _synthesizer.SpeakCompleted += (s, e) => { isPlaying = false; OnPlaybackCompleted?.Invoke(); };
                
                System.Diagnostics.Debug.WriteLine("✅ [SpeechService] Khởi tạo System.Speech thành công");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [SpeechService] Lỗi khởi tạo: {ex.Message}");
            }
        }

        public void Speak(string text, string cultureCode)
        {
            System.Diagnostics.Debug.WriteLine($"🔊 [SpeechService] Speak Request: {text} ({cultureCode})");
            
            if (string.IsNullOrWhiteSpace(text)) return;

            try
            {
                // BEEP TEST: Play a sound regardless of voice availability
                SystemSounds.Beep.Play();

                _synthesizer.SpeakAsyncCancelAll();

                // 1. Try exact match (e.g. vi-VN, en-US)
                var voice = _synthesizer.GetInstalledVoices()
                    .OrderByDescending(v => v.VoiceInfo.Culture.Name.Equals(cultureCode, StringComparison.OrdinalIgnoreCase))
                    .ThenByDescending(v => v.VoiceInfo.Culture.Name.StartsWith(cultureCode.Split('-')[0], StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(v => v.Enabled);

                if (voice != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🎙️ [SpeechService] Selected voice: {voice.VoiceInfo.Name}");
                    _synthesizer.SelectVoice(voice.VoiceInfo.Name);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ [SpeechService] No matching voice found! Using default system voice.");
                }

                _synthesizer.Volume = 100;
                _synthesizer.Rate = 0;
                _synthesizer.SpeakAsync(text);
            }
            catch (Exception ex)
            {
                isPlaying = false;
                System.Diagnostics.Debug.WriteLine($"❌ [SpeechService] Speak Error: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }

        public void Stop()
        {
            _synthesizer?.SpeakAsyncCancelAll();
            isPlaying = false;
        }

        public bool IsPlaying => isPlaying;

        public void Dispose()
        {
            _synthesizer?.Dispose();
        }
    }
}