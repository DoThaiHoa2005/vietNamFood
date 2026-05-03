using System;
using System.Windows;
using VietnamFoodGuide.Services;

namespace VietnamFoodGuide.Views
{
    public partial class OfflineMapDownloadWindow : Window
    {
        private readonly OfflineMapService _offlineMapService;
        private bool _isDownloading = false;

        public OfflineMapDownloadWindow()
        {
            InitializeComponent();
            _offlineMapService = new OfflineMapService();
            
            // Load current status
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            try
            {
                double sizeMB = _offlineMapService.GetOfflineTilesSize();
                TxtCurrentSize.Text = $"Dung lượng đã tải: {sizeMB:F2} MB";
                
                // Count tiles (rough estimate)
                int tilesCount = (int)(sizeMB * 50); // ~20KB per tile
                TxtTilesCount.Text = $"Số tiles: ~{tilesCount}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OfflineMapDownload] Error updating status: {ex.Message}");
            }
        }

        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (_isDownloading)
            {
                MessageDialog.ShowInformation("Đang tải bản đồ. Vui lòng đợi...", "Thông báo");
                return;
            }

            // Confirm
            var result = MessageBox.Show(
                "Bạn có muốn tải bản đồ offline cho khu vực Vĩnh Khánh?\n\n" +
                "Kích thước: ~50-100 MB\n" +
                "Thời gian: ~5-10 phút\n\n" +
                "Lưu ý: Cần kết nối internet để tải.",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Check internet
            bool isOnline = await NetworkService.IsInternetAvailableAsync();
            if (!isOnline)
            {
                MessageDialog.ShowError("Không có kết nối internet. Vui lòng kiểm tra và thử lại.", "Lỗi");
                return;
            }

            // Start download
            _isDownloading = true;
            BtnDownload.IsEnabled = false;
            BtnClear.IsEnabled = false;
            BtnClose.IsEnabled = false;
            ProgressPanel.Visibility = Visibility.Visible;

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    _offlineMapService.DownloadVinhKhanhArea((progress, message) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ProgressBar.Value = progress;
                            TxtProgress.Text = $"{progress}%";
                            TxtStatus.Text = message;
                        });
                    });
                });

                MessageDialog.ShowInformation("Tải bản đồ offline thành công!\n\nBây giờ bạn có thể sử dụng bản đồ khi không có mạng.", "Thành công");
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageDialog.ShowError($"Lỗi khi tải bản đồ:\n{ex.Message}", "Lỗi");
                System.Diagnostics.Debug.WriteLine($"[OfflineMapDownload] Download error: {ex}");
            }
            finally
            {
                _isDownloading = false;
                BtnDownload.IsEnabled = true;
                BtnClear.IsEnabled = true;
                BtnClose.IsEnabled = true;
                ProgressPanel.Visibility = Visibility.Collapsed;
                ProgressBar.Value = 0;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (_isDownloading)
            {
                MessageDialog.ShowInformation("Đang tải bản đồ. Không thể xóa lúc này.", "Thông báo");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa tất cả bản đồ offline đã tải?\n\n" +
                "Bạn sẽ cần tải lại nếu muốn sử dụng offline.",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _offlineMapService.ClearOfflineTiles();
                MessageDialog.ShowInformation("Đã xóa tất cả bản đồ offline.", "Thành công");
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageDialog.ShowError($"Lỗi khi xóa bản đồ:\n{ex.Message}", "Lỗi");
                System.Diagnostics.Debug.WriteLine($"[OfflineMapDownload] Clear error: {ex}");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_isDownloading)
            {
                var result = MessageBox.Show(
                    "Đang tải bản đồ. Bạn có chắc muốn đóng?\n\n" +
                    "Quá trình tải sẽ bị hủy.",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            this.Close();
        }
    }
}
