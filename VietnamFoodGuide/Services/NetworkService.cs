using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service kiểm tra kết nối mạng
    /// </summary>
    public class NetworkService
    {
        /// <summary>
        /// Kiểm tra có kết nối internet không
        /// </summary>
        public static bool IsInternetAvailable()
        {
            try
            {
                // Kiểm tra network interface
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    System.Diagnostics.Debug.WriteLine("[Network] ❌ No network interface available");
                    return false;
                }

                // Ping Google DNS để kiểm tra internet
                using (var ping = new Ping())
                {
                    var reply = ping.Send("8.8.8.8", 3000); // Timeout 3 seconds
                    bool isOnline = reply.Status == IPStatus.Success;
                    System.Diagnostics.Debug.WriteLine($"[Network] {(isOnline ? "✅" : "❌")} Internet {(isOnline ? "available" : "unavailable")}");
                    return isOnline;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Network] ❌ Error checking internet: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra có kết nối internet không (async)
        /// </summary>
        public static async Task<bool> IsInternetAvailableAsync()
        {
            return await Task.Run(() => IsInternetAvailable());
        }
    }
}
