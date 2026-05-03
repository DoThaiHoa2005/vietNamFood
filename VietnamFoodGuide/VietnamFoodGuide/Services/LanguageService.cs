using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace VietnamFoodGuide.Services
{
    public class LanguageService
    {
        private static LanguageService _instance;
        private string _currentLanguage = "vi"; // Default: Tiếng Việt
        private Dictionary<string, Dictionary<string, string>> _translations;
        private readonly string _settingsFile;

        public static LanguageService Instance => _instance ?? (_instance = new LanguageService());

        public event EventHandler LanguageChanged;

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    SaveLanguagePreference();
                    LanguageChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private LanguageService()
        {
            _settingsFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VietnamFoodGuide",
                "language.json"
            );

            LoadTranslations();
            LoadLanguagePreference();
        }

        private void LoadTranslations()
        {
            _translations = new Dictionary<string, Dictionary<string, string>>
            {
                ["vi"] = new Dictionary<string, string>
                {
                    // Main Window
                    ["app_title"] = "Khám Phá Ẩm Thực Việt Nam",
                    ["app_subtitle"] = "Khám phá ẩm thực Việt Nam",
                    ["search_placeholder"] = "Tìm món ăn, quán ăn...",
                    ["all_categories"] = "Tất cả",
                    ["categories"] = "DANH MỤC",
                    ["home"] = "Trang chủ",
                    ["search"] = "Tìm kiếm",
                    ["favorites"] = "Yêu thích",
                    ["account"] = "Tài khoản",
                    ["view_details"] = "Xem chi tiết →",
                    ["no_results"] = "Không tìm thấy món ăn nào",
                    ["try_different_search"] = "Thử tìm kiếm với từ khóa khác",
                    ["guest"] = "Khách",
                    ["back"] = "← Quay lại",
                    
                    // Categories
                    ["cat_haisan"] = "🦐 Hải sản",
                    ["cat_oc"] = "🐚 Ốc",
                    ["cat_bun"] = "🍜 Bún",
                    ["cat_nuong"] = "🍢 Nướng",
                    ["cat_lau_nuong"] = "🍲 Lẩu & Nướng",
                    ["cat_com"] = "🍚 Cơm",
                    ["cat_banh_mi"] = "🥖 Bánh Mì",
                    ["cat_banh_khac"] = "🥮 Bánh Khác",
                    ["cat_thuc_uong"] = "☕ Thức uống",
                    
                    // Account Dialog
                    ["account"] = "Tài khoản",
                    ["hello"] = "Xin chào",
                    ["logout_question"] = "Bạn có muốn đăng xuất không?",
                    ["logout_success"] = "Đã đăng xuất thành công!",
                    ["yes"] = "Có",
                    ["no"] = "Không",
                    ["logout"] = "Đăng xuất",
                    ["cancel"] = "Hủy",
                    
                    // Food Details
                    ["food_detail"] = "Chi tiết món ăn",
                    ["description"] = "MÔ TẢ",
                    ["view_map"] = "📍 Bản đồ",
                    ["listen"] = "🔊 Nghe",
                    ["stop"] = "⏹ Dừng",
                    ["add_favorite"] = "☆ Thêm yêu thích",
                    ["remove_favorite"] = "⭐ Đã yêu thích",
                    ["rating"] = "Đánh giá",
                    ["processing"] = "Đang xử lý...",
                    ["need_login"] = "Cần đăng nhập!",
                    ["notification"] = "Thông báo",
                    ["error"] = "Lỗi",
                    ["playing_narration"] = "Đang phát thuyết minh...",
                    
                    // Map Window
                    ["start_navigation"] = "🚀 Bắt đầu",
                    ["stop_navigation"] = "⏹ Dừng",
                    ["change_start"] = "🔄 Đổi xuất phát",
                    ["change_destination"] = "🎯 Đổi điểm đến",
                    ["calculating_route"] = "Đang tính toán đường đi...",
                    ["navigating"] = "Đang dẫn đường...",
                    ["arrived"] = "Đã đến nơi!",
                    ["current_location"] = "Về vị trí hiện tại",
                    ["compass"] = "Bật/Tắt la bàn",
                    ["map_title"] = "📍 Bản đồ vị trí",
                    ["loading_map"] = "Đang tải bản đồ...",
                    ["narrating"] = "🔊 Đang thuyết minh tự động",
                    
                    // Language
                    ["language"] = "Ngôn ngữ",
                    ["vietnamese"] = "🇻🇳 Tiếng Việt",
                    ["english"] = "🇺🇸 English",
                    ["chinese"] = "🇨🇳 中文",
                    
                    // Connection
                    ["connection_error"] = "Không thể kết nối tới Server",
                    ["offline_mode"] = "App sẽ chạy ở chế độ Offline (Dữ liệu cục bộ).\nLưu ý: Tính năng Yêu thích sẽ không hoạt động.",
                    ["connection_lost"] = "Mất kết nối",
                    
                    // Favorites
                    ["favorites"] = "Yêu thích",
                    ["my_favorites"] = "Món ăn yêu thích",
                    ["no_favorites"] = "Chưa có món ăn yêu thích",
                    ["add_some_favorites"] = "Thêm món ăn yêu thích để xem ở đây"
                },
                
                ["en"] = new Dictionary<string, string>
                {
                    // Main Window
                    ["app_title"] = "Discover Vietnamese Cuisine",
                    ["app_subtitle"] = "Discover Vietnamese Cuisine",
                    ["search_placeholder"] = "Search for food, restaurants...",
                    ["all_categories"] = "All",
                    ["categories"] = "CATEGORIES",
                    ["home"] = "Home",
                    ["search"] = "Search",
                    ["favorites"] = "Favorites",
                    ["account"] = "Account",
                    ["view_details"] = "View Details →",
                    ["no_results"] = "No food found",
                    ["try_different_search"] = "Try searching with different keywords",
                    ["guest"] = "Guest",
                    ["back"] = "← Back",
                    
                    // Categories
                    ["cat_haisan"] = "🦐 Seafood",
                    ["cat_oc"] = "🐚 Snails",
                    ["cat_bun"] = "🍜 Bun",
                    ["cat_nuong"] = "🍢 Grilled",
                    ["cat_lau_nuong"] = "🍲 Hotpot & Grill",
                    ["cat_com"] = "🍚 Rice",
                    ["cat_banh_mi"] = "🥖 Banh Mi",
                    ["cat_banh_khac"] = "🥮 Other Cakes",
                    ["cat_thuc_uong"] = "☕ Drinks",
                    
                    // Account Dialog
                    ["account"] = "Account",
                    ["hello"] = "Hello",
                    ["logout_question"] = "Do you want to logout?",
                    ["logout_success"] = "Logged out successfully!",
                    ["yes"] = "Yes",
                    ["no"] = "No",
                    ["logout"] = "Logout",
                    ["cancel"] = "Cancel",
                    
                    // Food Details
                    ["food_detail"] = "Food Details",
                    ["description"] = "DESCRIPTION",
                    ["view_map"] = "📍 Map",
                    ["listen"] = "🔊 Listen",
                    ["stop"] = "⏹ Stop",
                    ["add_favorite"] = "☆ Add Favorite",
                    ["remove_favorite"] = "⭐ Favorited",
                    ["rating"] = "Rating",
                    ["processing"] = "Processing...",
                    ["need_login"] = "Login required!",
                    ["notification"] = "Notification",
                    ["error"] = "Error",
                    ["playing_narration"] = "Playing narration...",
                    
                    // Map Window
                    ["start_navigation"] = "🚀 Start",
                    ["stop_navigation"] = "⏹ Stop",
                    ["change_start"] = "🔄 Change Start",
                    ["change_destination"] = "🎯 Change Destination",
                    ["calculating_route"] = "Calculating route...",
                    ["navigating"] = "Navigating...",
                    ["arrived"] = "Arrived!",
                    ["current_location"] = "Current location",
                    ["compass"] = "Toggle compass",
                    ["map_title"] = "📍 Map Location",
                    ["loading_map"] = "Loading map...",
                    ["narrating"] = "🔊 Auto narration",
                    
                    // Language
                    ["language"] = "Language",
                    ["vietnamese"] = "🇻🇳 Tiếng Việt",
                    ["english"] = "🇺🇸 English",
                    ["chinese"] = "🇨🇳 中文",
                    
                    // Connection
                    ["connection_error"] = "Cannot connect to Server",
                    ["offline_mode"] = "App will run in Offline mode (Local data).\nNote: Favorite feature will not work.",
                    ["connection_lost"] = "Connection Lost",
                    
                    // Favorites
                    ["favorites"] = "Favorites",
                    ["my_favorites"] = "My Favorites",
                    ["no_favorites"] = "No favorites yet",
                    ["add_some_favorites"] = "Add some favorites to see them here"
                },
                
                ["zh"] = new Dictionary<string, string>
                {
                    // Main Window
                    ["app_title"] = "探索越南美食",
                    ["app_subtitle"] = "探索越南美食",
                    ["search_placeholder"] = "搜索食物、餐厅...",
                    ["all_categories"] = "全部",
                    ["categories"] = "类别",
                    ["home"] = "主页",
                    ["search"] = "搜索",
                    ["favorites"] = "收藏",
                    ["account"] = "账户",
                    ["view_details"] = "查看详情 →",
                    ["no_results"] = "未找到食物",
                    ["try_different_search"] = "尝试使用不同的关键词搜索",
                    ["guest"] = "访客",
                    ["back"] = "← 返回",
                    
                    // Categories
                    ["cat_haisan"] = "🦐 海鲜",
                    ["cat_oc"] = "🐚 蜗牛",
                    ["cat_bun"] = "🍜 米粉",
                    ["cat_nuong"] = "🍢 烧烤",
                    ["cat_lau_nuong"] = "🍲 火锅烧烤",
                    ["cat_com"] = "🍚 米饭",
                    ["cat_banh_mi"] = "🥖 越南法棍",
                    ["cat_banh_khac"] = "🥮 其他糕点",
                    ["cat_thuc_uong"] = "☕ 饮料",
                    
                    // Account Dialog
                    ["account"] = "账户",
                    ["hello"] = "你好",
                    ["logout_question"] = "您要退出登录吗？",
                    ["logout_success"] = "已成功退出登录！",
                    ["yes"] = "是",
                    ["no"] = "否",
                    ["logout"] = "退出登录",
                    ["cancel"] = "取消",
                    
                    // Food Details
                    ["food_detail"] = "食物详情",
                    ["description"] = "描述",
                    ["view_map"] = "📍 地图",
                    ["listen"] = "🔊 收听",
                    ["stop"] = "⏹ 停止",
                    ["add_favorite"] = "☆ 添加收藏",
                    ["remove_favorite"] = "⭐ 已收藏",
                    ["rating"] = "评分",
                    ["processing"] = "处理中...",
                    ["need_login"] = "需要登录！",
                    ["notification"] = "通知",
                    ["error"] = "错误",
                    ["playing_narration"] = "正在播放解说...",
                    
                    // Map Window
                    ["start_navigation"] = "🚀 开始",
                    ["stop_navigation"] = "⏹ 停止",
                    ["change_start"] = "🔄 更改起点",
                    ["change_destination"] = "🎯 更改终点",
                    ["calculating_route"] = "正在计算路线...",
                    ["navigating"] = "正在导航...",
                    ["arrived"] = "已到达！",
                    ["current_location"] = "当前位置",
                    ["compass"] = "切换指南针",
                    ["map_title"] = "📍 地图位置",
                    ["loading_map"] = "正在加载地图...",
                    ["narrating"] = "🔊 自动解说",
                    
                    // Language
                    ["language"] = "语言",
                    ["vietnamese"] = "🇻🇳 Tiếng Việt",
                    ["english"] = "🇺🇸 English",
                    ["chinese"] = "🇨🇳 中文",
                    
                    // Connection
                    ["connection_error"] = "无法连接到服务器",
                    ["offline_mode"] = "应用将以离线模式运行（本地数据）。\n注意：收藏功能将不可用。",
                    ["connection_lost"] = "连接丢失",
                    
                    // Favorites
                    ["favorites"] = "收藏",
                    ["my_favorites"] = "我的收藏",
                    ["no_favorites"] = "还没有收藏",
                    ["add_some_favorites"] = "添加一些收藏以在此处查看"
                }
            };
        }

        public string Translate(string key)
        {
            if (_translations.ContainsKey(_currentLanguage) && 
                _translations[_currentLanguage].ContainsKey(key))
            {
                return _translations[_currentLanguage][key];
            }
            
            // Fallback to Vietnamese
            if (_translations["vi"].ContainsKey(key))
            {
                return _translations["vi"][key];
            }
            
            return key; // Return key if translation not found
        }

        public string this[string key] => Translate(key);

        private void LoadLanguagePreference()
        {
            try
            {
                if (File.Exists(_settingsFile))
                {
                    var json = File.ReadAllText(_settingsFile);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (settings != null && settings.ContainsKey("language"))
                    {
                        _currentLanguage = settings["language"];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading language preference: {ex.Message}");
            }
        }

        private void SaveLanguagePreference()
        {
            try
            {
                var directory = Path.GetDirectoryName(_settingsFile);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var settings = new Dictionary<string, string>
                {
                    ["language"] = _currentLanguage
                };

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving language preference: {ex.Message}");
            }
        }
    }
}
