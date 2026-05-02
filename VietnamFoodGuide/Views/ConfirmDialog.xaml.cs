using System.Windows;

namespace VietnamFoodGuide.Views
{
    public partial class ConfirmDialog : Window
    {
        public bool Result { get; private set; } = false;

        public ConfirmDialog(string title, string message, string yesText = "Có", string noText = "Không")
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtMessage.Text = message;
            BtnYes.Content = yesText;
            BtnNo.Content = noText;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            this.Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.Close();
        }

        // Helper method để dùng như MessageBox
        public static bool Show(string message, string title = "Xác nhận", string yesText = "Có", string noText = "Không")
        {
            var dialog = new ConfirmDialog(title, message, yesText, noText);
            dialog.ShowDialog();
            return dialog.Result;
        }
    }
}
