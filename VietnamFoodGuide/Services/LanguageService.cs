using System;
using System.Collections.Generic;

namespace VietnamFoodGuide.Services
{
    public class LanguageService
    {
        public enum Language
        {
            Vietnamese,
            English,
            Chinese
        }

        private Language currentLanguage = Language.Vietnamese;

        public Language CurrentLanguage
        {
            get => currentLanguage;
            set => currentLanguage = value;
        }

        public string GetCultureCode(Language language)
        {
            if (language == Language.Vietnamese) return "vi-VN";
            if (language == Language.English) return "en-US";
            if (language == Language.Chinese) return "zh-CN";
            return "vi-VN";
        }

        public string GetLanguageName(Language language)
        {
            if (language == Language.Vietnamese) return "Tiếng Việt";
            if (language == Language.English) return "English";
            if (language == Language.Chinese) return "中文";
            return "Tiếng Việt";
        }

        public Dictionary<string, string> GetUIStrings(Language language)
        {
            if (language == Language.English) return GetEnglishStrings();
            if (language == Language.Chinese) return GetChineseStrings();
            return GetVietnameseStrings();
        }

        private Dictionary<string, string> GetVietnameseStrings()
        {
            return new Dictionary<string, string>
            {
                { "AppTitle", "Vietnam Food Guide" },
                { "SearchPlaceholder", "Tìm quán ăn..." },
                { "AllCategories", "Tất cả" },
                { "ViewDetails", "Xem chi tiết" },
                { "Back", "Quay lại" },
                { "Map", "Bản đồ vị trí" },
                { "Speak", "Nghe thuyết minh" },
                { "AddFavorite", "Thêm yêu thích" },
                { "SelectLanguage", "Chọn ngôn ngữ" },
                { "NoResults", "Không tìm thấy quán ăn" }
            };
        }

        private Dictionary<string, string> GetEnglishStrings()
        {
            return new Dictionary<string, string>
            {
                { "AppTitle", "Vietnam Food Guide" },
                { "SearchPlaceholder", "Find restaurants..." },
                { "AllCategories", "All" },
                { "ViewDetails", "View Details" },
                { "Back", "Back" },
                { "Map", "Map Location" },
                { "Speak", "Audio Guide" },
                { "AddFavorite", "Add to Favorites" },
                { "SelectLanguage", "Select Language" },
                { "NoResults", "No restaurants found" }
            };
        }

        private Dictionary<string, string> GetChineseStrings()
        {
            return new Dictionary<string, string>
            {
                { "AppTitle", "越南美食指南" },
                { "SearchPlaceholder", "搜索餐厅..." },
                { "AllCategories", "全部" },
                { "ViewDetails", "查看详情" },
                { "Back", "返回" },
                { "Map", "地图位置" },
                { "Speak", "语音导览" },
                { "AddFavorite", "添加收藏" },
                { "SelectLanguage", "选择语言" },
                { "NoResults", "未找到餐厅" }
            };
        }
    }
}
