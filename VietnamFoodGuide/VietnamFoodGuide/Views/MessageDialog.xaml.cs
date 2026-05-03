using System.Windows;
using System.Windows.Media;
using VietnamFoodGuide.Services;

namespace VietnamFoodGuide.Views
{
    public enum MessageType
    {
        Information,
        Warning,
        Error,
        Confirm
    }

    public partial class MessageDialog : Window
    {
        public bool IsConfirmed { get; private set; } = false;

        public MessageDialog(string title, string message, MessageType type = MessageType.Information)
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtMessage.Text = message;

            // Apply dark mode to dialog
            bool isDark = ThemeService.Instance.IsDarkMode;
            if (isDark)
            {
                DialogBorder.Background  = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));
                DialogBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x4E));
                TxtMessage.Foreground    = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xDD));
            }
            else
            {
                DialogBorder.Background  = new SolidColorBrush(Colors.White);
                DialogBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                TxtMessage.Foreground    = new SolidColorBrush(Color.FromRgb(0x2C, 0x3E, 0x50));
            }

            // Set icon, header color and button visibility
            switch (type)
            {
                case MessageType.Information:
                    TxtIcon.Text = "ℹ️";
                    HeaderBorder.Background = new SolidColorBrush(Color.FromRgb(0x34, 0xA8, 0x53));
                    BtnOk.Content = "OK";
                    BtnCancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageType.Warning:
                    TxtIcon.Text = "⚠️";
                    HeaderBorder.Background = new SolidColorBrush(Color.FromRgb(0xF3, 0x9C, 0x12));
                    BtnOk.Content = "OK";
                    BtnCancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageType.Error:
                    TxtIcon.Text = "❌";
                    HeaderBorder.Background = new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));
                    BtnOk.Content = "OK";
                    BtnCancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageType.Confirm:
                    TxtIcon.Text = "❓";
                    HeaderBorder.Background = new SolidColorBrush(Color.FromRgb(0x34, 0xA8, 0x53));
                    BtnOk.Content = "Có";
                    BtnCancel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public static void Show(string message, string title = "Thông báo", MessageType type = MessageType.Information)
        {
            var dialog = new MessageDialog(title, message, type);
            dialog.ShowDialog();
        }

        public static void ShowInformation(string message, string title = "Thông báo")
            => Show(message, title, MessageType.Information);

        public static void ShowWarning(string message, string title = "Cảnh báo")
            => Show(message, title, MessageType.Warning);

        public static void ShowError(string message, string title = "Lỗi")
            => Show(message, title, MessageType.Error);

        public static bool ShowConfirm(string message, string title = "Xác nhận")
        {
            var dialog = new MessageDialog(title, message, MessageType.Confirm);
            dialog.ShowDialog();
            return dialog.IsConfirmed;
        }
    }
}
