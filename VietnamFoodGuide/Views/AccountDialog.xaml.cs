using System.Windows;
using VietnamFoodGuide.Services;

namespace VietnamFoodGuide.Views
{
    public partial class AccountDialog : Window
    {
        private readonly LanguageService _lang = LanguageService.Instance;

        public bool ShouldLogout { get; private set; } = false;

        public AccountDialog(string username)
        {
            InitializeComponent();
            UpdateLanguage();
            TxtGreeting.Text = $"{_lang["hello"]}, {username}!";

            // Subscribe to language changes
            _lang.LanguageChanged += (s, e) => UpdateLanguage();
        }

        private void UpdateLanguage()
        {
            TxtTitle.Text = _lang["account"];
            TxtQuestion.Text = _lang["logout_question"];
            BtnCancel.Content = _lang["no"];
            BtnLogout.Content = _lang["logout"];
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ShouldLogout = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ShouldLogout = false;
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ShouldLogout = false;
            DialogResult = false;
            Close();
        }
    }
}
