using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using VietnamFoodGuide.Data;
using VietnamFoodGuide.Services;

namespace VietnamFoodGuide.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ApiAuthService _apiAuthService;
        private readonly SQLiteUserService _sqliteUserService; // ✅ Offline registration
        private bool _isPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        public string RegisteredUsername { get; private set; }

        public RegisterWindow(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _apiAuthService = new ApiAuthService();
            _sqliteUserService = new SQLiteUserService(); // ✅ SQLite (offline)
            InitializeComponent();
            
            // Thêm event handler cho phím Enter
            this.KeyDown += RegisterWindow_KeyDown;
            UsernameTextBox.KeyDown += TextBox_KeyDown;
            EmailTextBox.KeyDown += TextBox_KeyDown;
            PasswordBox.KeyDown += PasswordBox_KeyDown;
            PasswordTextBox.KeyDown += TextBox_KeyDown;
            ConfirmPasswordBox.KeyDown += PasswordBox_KeyDown;
            ConfirmPasswordTextBox.KeyDown += TextBox_KeyDown;
        }

        private void RegisterWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RegisterButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RegisterButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RegisterButton_Click(sender, new RoutedEventArgs());
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text?.Trim();
            string email = EmailTextBox.Text?.Trim();
            string password = PasswordBox.Visibility == Visibility.Visible ? PasswordBox.Password : PasswordTextBox.Text;
            string confirmPassword = ConfirmPasswordBox.Visibility == Visibility.Visible ? ConfirmPasswordBox.Password : ConfirmPasswordTextBox.Text;

            // Validation
            if (string.IsNullOrEmpty(username))
            {
                ShowError("Vui lòng nhập tài khoản");
                return;
            }

            if (username.Length < 3)
            {
                ShowError("Tài khoản phải có ít nhất 3 ký tự");
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Vui lòng nhập email");
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Email không hợp lệ");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Vui lòng nhập mật khẩu");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Mật khẩu phải có ít nhất 6 ký tự");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Mật khẩu xác nhận không khớp");
                return;
            }

            // Disable button và hiển thị loading
            RegisterButton.IsEnabled = false;
            RegisterButton.Content = "Đang đăng ký...";
            if (MessageBorder != null) MessageBorder.Visibility = Visibility.Collapsed;

            try
            {
                // ✅ OFFLINE-FIRST: Đăng ký vào SQLite trước
                System.Diagnostics.Debug.WriteLine("[Register] Trying offline registration with SQLite...");
                bool offlineSuccess = _sqliteUserService.Register(username, password);

                if (offlineSuccess)
                {
                    // ✅ Đăng ký offline thành công
                    System.Diagnostics.Debug.WriteLine($"[Register] Offline registration successful for user: {username}");
                    ShowSuccess("Đăng ký thành công! (Offline)");
                    RegisteredUsername = username;
                    
                    // Thử đồng bộ lên server (không bắt buộc)
                    try
                    {
                        var (apiSuccess, apiMessage) = await _apiAuthService.RegisterAsync(username, email, password);
                        if (apiSuccess)
                        {
                            System.Diagnostics.Debug.WriteLine("[Register] Synced to online server successfully");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[Register] Failed to sync to server: {apiMessage}");
                        }
                    }
                    catch (Exception syncEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Register] Server sync error (ignored): {syncEx.Message}");
                    }
                    
                    // Đợi 1.5 giây để user thấy thông báo
                    await System.Threading.Tasks.Task.Delay(1500);
                    
                    // Đóng window và trả về true
                    DialogResult = true;
                    this.Close();
                }
                else
                {
                    // ❌ Offline registration failed (username đã tồn tại)
                    ShowError("Tài khoản đã tồn tại");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Register error: {ex}");
            }
            finally
            {
                RegisterButton.IsEnabled = true;
                RegisterButton.Content = "Đăng Ký";
            }
        }

        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordTextBox.Focus();
                ShowPasswordButton.Content = "🙈";
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Focus();
                ShowPasswordButton.Content = "👁";
            }
        }

        private void ShowConfirmPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;

            if (_isConfirmPasswordVisible)
            {
                ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
                ConfirmPasswordBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordTextBox.Visibility = Visibility.Visible;
                ConfirmPasswordTextBox.Focus();
                ShowConfirmPasswordButton.Content = "🙈";
            }
            else
            {
                ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
                ConfirmPasswordBox.Visibility = Visibility.Visible;
                ConfirmPasswordTextBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordBox.Focus();
                ShowConfirmPasswordButton.Content = "👁";
            }
        }

        private void BackToLoginLink_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void ShowError(string message)
        {
            MessageTextBlock.Text = "❌ " + message;
            MessageTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 53, 69));
            if (MessageBorder != null) { MessageBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 240, 240)); MessageBorder.Visibility = Visibility.Visible; }
        }

        private void ShowSuccess(string message)
        {
            MessageTextBlock.Text = "✅ " + message;
            MessageTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 167, 69));
            if (MessageBorder != null) { MessageBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240, 255, 244)); MessageBorder.Visibility = Visibility.Visible; }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}
