using System;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// File cấu hình tập trung cho toàn bộ hệ thống.
    /// Bạn chỉ cần sửa tên miền ở đây, toàn bộ App và Admin sẽ tự động cập nhật theo.
    /// </summary>
    public static class AppConfig
    {
        // 1. TÊN MIỀN NGROK (Thay đổi mỗi khi bạn bật lại ngrok)
        // Ví dụ: https://abcd-1234.ngrok-free.app/vfg-api
        
        // OPTION 1: Dùng Ngrok (cho remote access)
        // public static readonly string Domain = "https://bridged-shindig-feminize.ngrok-free.dev/vfg-api";
        
        // OPTION 2: Dùng IP LAN (cho local network - các thiết bị trong mạng)
        public static readonly string Domain = "http://192.168.1.112/vfg-api";
        
        // OPTION 3: Dùng Localhost (chỉ máy local)
        // public static readonly string Domain = "http://localhost/vfg-api";

        // 2. ĐƯỜNG DẪN API (Tự động tạo dựa trên Domain)
        public static string ApiBaseUrl => $"{Domain}/api.php";

        // 3. ĐƯỜNG DẪN ADMIN DASHBOARD (Tự động tạo dựa trên Domain)
        public static string AdminDashboardUrl => $"{Domain}/admin_dashboard.html";

        // 4. ĐƯỜNG DẪN LANDING PAGE (Tự động tạo dựa trên Domain)
        public static string LandingPageUrl => $"{Domain}/index.html";
        
        // 5. DEBUG MODE (Bật/tắt log)
        public static bool IsDebugMode = true;
    }
}
