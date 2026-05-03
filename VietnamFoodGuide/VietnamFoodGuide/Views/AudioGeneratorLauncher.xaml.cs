using System.Windows;

namespace VietnamFoodGuide.Views
{
    public partial class AudioGeneratorLauncher : Window
    {
        public AudioGeneratorLauncher()
        {
            InitializeComponent();
        }

        private void StartGeneration_Click(object sender, RoutedEventArgs e)
        {
            // Mở AudioGeneratorWindow
            var audioWindow = new AudioGeneratorWindow();
            audioWindow.ShowDialog();
            
            // Đóng launcher sau khi hoàn thành
            this.Close();
        }
    }
}
