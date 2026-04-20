using System;

namespace VietnamFoodGuide.Services.Interfaces
{
    /// <summary>
    /// Contract for speech/audio services
    /// </summary>
    public interface ISpeechService : IDisposable
    {
        /// <summary>
        /// Event fired when playback starts
        /// </summary>
        event Action OnPlaybackStarted;

        /// <summary>
        /// Event fired when playback completes
        /// </summary>
        event Action OnPlaybackCompleted;

        /// <summary>
        /// Event fired on error
        /// </summary>
        event Action<string> OnError;

        /// <summary>
        /// Speak text using text-to-speech
        /// </summary>
        void Speak(string text, string cultureCode);

        /// <summary>
        /// Stop current speech playback
        /// </summary>
        void Stop();

        /// <summary>
        /// Check if currently playing
        /// </summary>
        bool IsPlaying { get; }
    }
}
