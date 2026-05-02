using System.Windows;
using System.Windows.Media;

namespace VietnamFoodGuide.Views
{
    public enum MessageType
    {
        Information,
        Warning,
        Error
    }

    public partial class MessageDialog : Window
    {
        public MessageDialog(string title, string message, MessageType type = MessageType.Information)
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtMessage.Text = message;

            // Set icon and color based on type
            switch (type)
            {
                case MessageType.Information:
                    TxtIcon.Text = "ℹ️";
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C5CE7"));
                    break;
                case MessageType.Warning:
                    TxtIcon.Text = "⚠️";
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12"));
                    break;
                case MessageType.Error:
                    TxtIcon.Text = "❌";
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
                    break;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Helper methods to use like MessageBox
        public static void Show(string message, string title = "Thông báo", MessageType type = MessageType.Information)
        {
            var dialog = new MessageDialog(title, message, type);
            dialog.ShowDialog();
        }

        public static void ShowInformation(string message, string title = "Thông báo")
        {
            Show(message, title, MessageType.Information);
        }

        public static void ShowWarning(string message, string title = "Cảnh báo")
        {
            Show(message, title, MessageType.Warning);
        }

        public static void ShowError(string message, string title = "Lỗi")
        {
            Show(message, title, MessageType.Error);
        }
    }
}
