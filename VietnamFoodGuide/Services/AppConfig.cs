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
        public static readonly string Domain = "https://bridged-shindig-feminize.ngrok-free.dev/vfg-api";

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
