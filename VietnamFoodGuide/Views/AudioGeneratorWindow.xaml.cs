using System;
using System.Windows;
using VietnamFoodGuide.Services;

namespace VietnamFoodGuide.Views
{
    public partial class AudioGeneratorWindow : Window
    {
        private readonly GoogleTTSAudioGenerator _audioService;
        private bool _isGenerating = false;

        public AudioGeneratorWindow()
        {
            InitializeComponent();
            _audioService = new GoogleTTSAudioGenerator();
            
            // Update info text
            TxtInfo.Text = "📁 Thư mục: Assets/Audio/\n🌐 Sử dụng: Google Translate TTS (không cần cài giọng đọc!)";
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (_isGenerating) return;

            try
            {
                _isGenerating = true;
                BtnGenerate.IsEnabled = false;
                BtnClose.IsEnabled = false;

                TxtProgress.Text = "Đang bắt đầu...";
                TxtStatus.Text = "";
                ProgressBar.Value = 0;

                // Tạo audio cho tất cả quán ăn
                var result = await _audioService.GenerateAllAudioAsync((current, total, message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        double progress = total > 0 ? (double)current / total * 100 : 0;
                        ProgressBar.Value = progress;
                        TxtProgress.Text = $"Đang xử lý: {current}/{total} ({progress:F1}%)";
                        TxtStatus.Text += $"\n{message}";
                        
                        // Auto scroll to bottom
                        if (TxtStatus.Text.Length > 2000)
                        {
                            TxtStatus.Text = TxtStatus.Text.Substring(TxtStatus.Text.Length - 2000);
                        }
                    });
                });

                // Hiển thị kết quả
                if (result.IsSuccess)
                {
                    MessageBox.Show(result.Message, "Hoàn Thành", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isGenerating = false;
                BtnGenerate.IsEnabled = true;
                BtnClose.IsEnabled = true;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_isGenerating)
            {
                MessageBox.Show("Đang tạo audio, vui lòng đợi...", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.Close();
        }
    }
}
