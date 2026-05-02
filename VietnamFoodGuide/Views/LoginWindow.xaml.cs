using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VietnamFoodGuide.Data;
using VietnamFoodGuide.Models.Entities;
using VietnamFoodGuide.Services;

namespace VietnamFoodGuide.Views
{
    public partial class LoginWindow : Window
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ApiAuthService _apiAuthService;
        private readonly SQLiteUserService _sqliteUserService; // ✅ Offline login
        private bool _isPasswordVisible = false;
        private readonly string _credentialsFile;
        private static DispatcherTimer _activityTimer;

        public LoginWindow(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _apiAuthService = new ApiAuthService(); // API XAMPP (online)
            _sqliteUserService = new SQLiteUserService(); // ✅ SQLite (offline)
            
            // File lưu thông tin đăng nhập
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VietnamFoodGuide");
            Directory.CreateDirectory(appDataPath);
            _credentialsFile = Path.Combine(appDataPath, "credentials.dat");
            
            InitializeComponent();
            
            // Load thông tin đã lưu
            LoadSavedCredentials();
            
            // Thêm event handler cho phím Enter
            this.KeyDown += LoginWindow_KeyDown;
            UsernameTextBox.KeyDown += TextBox_KeyDown;
            PasswordBox.KeyDown += PasswordBox_KeyDown;
            PasswordTextBox.KeyDown += TextBox_KeyDown;
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, new RoutedEventArgs());
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text?.Trim();

            // Get password from whichever control is visible
            string password = "";
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                password = PasswordBox.Password;
            }
            else
            {
                password = PasswordTextBox.Text;
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Vui lòng nhập tài khoản và mật khẩu");
                return;
            }

            // Disable button và hiển thị loading
            LoginButton.IsEnabled = false;
            LoginButton.Content = "Đang đăng nhập...";
            if (ErrorBorder != null) ErrorBorder.Visibility = Visibility.Collapsed;

            try
            {
                // ✅ OFFLINE-FIRST: Thử đăng nhập bằng SQLite trước
                Debug.WriteLine("[Login] Trying offline login with SQLite...");
                var offlineUser = _sqliteUserService.Login(username, password);

                if (offlineUser != null)
                {
                    // ✅ Đăng nhập offline thành công
                    Debug.WriteLine($"[Login] Offline login successful for user: {offlineUser.Username}");
                    ShowSuccess($"Chào mừng {offlineUser.Username}! (Offline)");
                    
                    // Lưu thông tin nếu chọn "Ghi nhớ"
                    if (RememberMeCheckBox.IsChecked == true)
                    {
                        SaveCredentials(username, password);
                    }
                    else
                    {
                        ClearSavedCredentials();
                    }
                    
                    // Lưu thông tin user (offline mode)
                    App.CurrentApiUser = offlineUser;
                    App.SessionToken = null; // Không có token khi offline
                    App.SessionExpiresAt = null;
                    App.DbContext = _dbContext;

                    // Đợi 1 giây để user thấy thông báo
                    await System.Threading.Tasks.Task.Delay(1000);

                    // Kiểm tra role
                    if (offlineUser.IsAdmin)
                    {
                        // Admin → Mở web admin dashboard
                        OpenAdminDashboard();
                    }
                    else
                    {
                        // User → Mở MainWindow
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    return;
                }

                // ❌ Offline login failed, thử online login
                Debug.WriteLine("[Login] Offline login failed, trying online login...");
                
                try
                {
                    var (success, apiUser, errorMessage) = await _apiAuthService.LoginAsync(username, password);

                    if (success && apiUser != null)
                    {
                        // ✅ Đăng nhập online thành công
                        Debug.WriteLine($"[Login] Online login successful for user: {apiUser.Username}");
                        ShowSuccess($"Chào mừng {apiUser.Username}! (Online)");
                        
                        // Lưu thông tin nếu chọn "Ghi nhớ"
                        if (RememberMeCheckBox.IsChecked == true)
                        {
                            SaveCredentials(username, password);
                        }
                        else
                        {
                            ClearSavedCredentials();
                        }
                        
                        // Lưu thông tin user và session token
                        App.CurrentApiUser = apiUser;
                        App.SessionToken = apiUser.Token;
                        App.SessionExpiresAt = apiUser.ExpiresAt;
                        App.DbContext = _dbContext;
                        
                        // Bắt đầu cập nhật activity mỗi 30 giây
                        StartActivityTimer();

                        // Đợi 1 giây để user thấy thông báo
                        await System.Threading.Tasks.Task.Delay(1000);

                        // Kiểm tra role
                        if (apiUser.IsAdmin)
                        {
                            // Admin → Mở web admin dashboard
                            OpenAdminDashboard();
                        }
                        else
                        {
                            // User → Mở MainWindow
                            MainWindow mainWindow = new MainWindow();
                            mainWindow.Show();
                            this.Close();
                        }
                    }
                    else
                    {
                        // ❌ Online login cũng thất bại
                        ShowError(errorMessage ?? "Tài khoản hoặc mật khẩu sai");
                        if (PasswordBox.Visibility == Visibility.Visible)
                        {
                            PasswordBox.Clear();
                        }
                        else
                        {
                            PasswordTextBox.Clear();
                        }
                    }
                }
                catch (Exception onlineEx)
                {
                    // ❌ Không thể kết nối online
                    Debug.WriteLine($"[Login] Online login error: {onlineEx.Message}");
                    ShowError("Không thể đăng nhập. Vui lòng kiểm tra tài khoản và mật khẩu.");
                    if (PasswordBox.Visibility == Visibility.Visible)
                    {
                        PasswordBox.Clear();
                    }
                    else
                    {
                        PasswordTextBox.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login] Unexpected error: {ex.Message}");
                ShowError($"Lỗi: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Đăng Nhập";
            }
        }

        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // Show password - copy from PasswordBox to TextBox
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordTextBox.Focus();
                ShowPasswordButton.Content = "🙈";
            }
            else
            {
                // Hide password - copy from TextBox to PasswordBox
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Focus();
                ShowPasswordButton.Content = "👁";
            }
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            // Mở RegisterWindow
            RegisterWindow registerWindow = new RegisterWindow(_dbContext);
            registerWindow.Owner = this;
            bool? result = registerWindow.ShowDialog();
            
            if (result == true)
            {
                // Đăng ký thành công, tự động điền username
                UsernameTextBox.Text = registerWindow.RegisteredUsername;
                ShowSuccess("Đăng ký thành công! Vui lòng đăng nhập.");
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 53, 69));
            if (ErrorBorder != null) ErrorBorder.Visibility = Visibility.Visible;
        }

        private void ShowSuccess(string message)
        {
            ErrorMessage.Text = "✅ " + message;
            ErrorMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 167, 69));
            if (ErrorBorder != null) ErrorBorder.Visibility = Visibility.Visible;
        }

        private void OpenAdminDashboard()
        {
            try
            {
                // Mở web admin dashboard qua host
                string adminUrl = AppConfig.AdminDashboardUrl;
                
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = adminUrl,
                    UseShellExecute = true
                };
                Process.Start(psi);

                // Đóng app sau khi mở browser
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageDialog.ShowInformation($"Không thể mở Admin Dashboard: {ex.Message}\n\nVui lòng mở thủ công: http://localhost/admin_dashboard.html",
                                "Thông báo");
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Lưu thông tin đăng nhập (mã hóa đơn giản)
        /// </summary>
        private void SaveCredentials(string username, string password)
        {
            try
            {
                // Mã hóa đơn giản bằng Base64 (không an toàn tuyệt đối nhưng đủ dùng)
                string data = $"{username}|{password}";
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                string encoded = Convert.ToBase64String(ProtectData(bytes));
                File.WriteAllText(_credentialsFile, encoded);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving credentials: {ex.Message}");
            }
        }

        /// <summary>
        /// Load thông tin đăng nhập đã lưu
        /// </summary>
        private void LoadSavedCredentials()
        {
            try
            {
                if (File.Exists(_credentialsFile))
                {
                    string encoded = File.ReadAllText(_credentialsFile);
                    byte[] bytes = Convert.FromBase64String(encoded);
                    byte[] decrypted = UnprotectData(bytes);
                    string data = Encoding.UTF8.GetString(decrypted);
                    
                    string[] parts = data.Split('|');
                    if (parts.Length == 2)
                    {
                        UsernameTextBox.Text = parts[0];
                        PasswordBox.Password = parts[1];
                        PasswordTextBox.Text = parts[1];
                        RememberMeCheckBox.IsChecked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading credentials: {ex.Message}");
                // Nếu lỗi, xóa file
                ClearSavedCredentials();
            }
        }

        /// <summary>
        /// Xóa thông tin đăng nhập đã lưu
        /// </summary>
        private void ClearSavedCredentials()
        {
            try
            {
                if (File.Exists(_credentialsFile))
                {
                    File.Delete(_credentialsFile);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing credentials: {ex.Message}");
            }
        }

        /// <summary>
        /// Mã hóa dữ liệu bằng XOR đơn giản (đủ cho mục đích lưu local)
        /// </summary>
        private byte[] ProtectData(byte[] data)
        {
            // XOR với key đơn giản
            byte[] key = Encoding.UTF8.GetBytes("VFG2024SecretKey");
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return result;
        }

        /// <summary>
        /// Giải mã dữ liệu (XOR là reversible)
        /// </summary>
        private byte[] UnprotectData(byte[] data)
        {
            // XOR với cùng key để giải mã
            return ProtectData(data);
        }

        /// <summary>
        /// Bắt đầu timer cập nhật activity mỗi 30 giây
        /// </summary>
        private static void StartActivityTimer()
        {
            if (_activityTimer != null)
            {
                _activityTimer.Stop();
            }

            _activityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };

            _activityTimer.Tick += async (s, e) =>
            {
                if (string.IsNullOrEmpty(App.SessionToken))
                {
                    _activityTimer?.Stop();
                    return;
                }

                try
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(5);
                        
                        var data = new { token = App.SessionToken };
                        var json = System.Text.Json.JsonSerializer.Serialize(data);
                        var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
                        
                        var response = await client.PostAsync($"{AppConfig.ApiBaseUrl}?action=updateActivity", content);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            System.Diagnostics.Debug.WriteLine("[Activity] Updated LastActiveTime");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Activity] Error: {ex.Message}");
                }
            };

            _activityTimer.Start();
            System.Diagnostics.Debug.WriteLine("[Activity] Timer started - will update every 30 seconds");
        }
    }
}
