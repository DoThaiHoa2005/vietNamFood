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
            TxtUsername.Text = username;

            // Apply dark mode
            ThemeService.Instance.ApplyTheme();
            bool isDark = ThemeService.Instance.IsDarkMode;
            if (isDark)
            {
                DialogBorder.Background  = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x1E, 0x1E, 0x2E));
                DialogBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x33, 0x33, 0x4E));
            }

            // Subscribe to language changes
            _lang.LanguageChanged += (s, e) => UpdateLanguage();
        }

        private void UpdateLanguage()
        {
            TxtTitle.Text = _lang["account"];
            TxtUsernameLabel.Text = _lang.CurrentLanguage == "vi" ? "Tên đăng nhập" : 
                                    _lang.CurrentLanguage == "en" ? "Username" : "用户名";
            TxtStatusLabel.Text = _lang.CurrentLanguage == "vi" ? "Trạng thái" : 
                                  _lang.CurrentLanguage == "en" ? "Status" : "状态";
            TxtStatus.Text = _lang.CurrentLanguage == "vi" ? "Đã đăng nhập" : 
                            _lang.CurrentLanguage == "en" ? "Logged in" : "已登录";
            TxtQuestion.Text = _lang["logout_question"];
            BtnCancel.Content = _lang.CurrentLanguage == "vi" ? "❌ Không" : 
                               _lang.CurrentLanguage == "en" ? "❌ No" : "❌ 不";
            BtnLogout.Content = _lang.CurrentLanguage == "vi" ? "🚪 Đăng xuất" : 
                               _lang.CurrentLanguage == "en" ? "🚪 Logout" : "🚪 登出";
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
