using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VietnamFoodGuide.Models
{
    public class FoodItem : INotifyPropertyChanged
    {
        private bool _isFavorite;
        private string _viewDetailsText;

        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Category { get; set; } // Phải có trường này để nút bấm hoạt động
        public string Image { get; set; }
        public string DescriptionVI { get; set; }
        public string DescriptionEN { get; set; }
        public string DescriptionCN { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Rating { get; set; }

        // ===== GIAI ĐOẠN 1: Thêm các trường mới cho Geofence & Narration =====
        /// <summary>
        /// Bán kính kích hoạt geofence (đơn vị: mét)
        /// Ví dụ: 50 = kích hoạt khi người dùng trong vòng 50m
        /// </summary>
        public double Radius { get; set; } = 30.0; // Mặc định 30m

        /// <summary>
        /// Mức ưu tiên phát thuyết minh (1-10, cao hơn = ưu tiên hơn)
        /// Khi nhiều POI cùng trong vùng, phát POI có Priority cao nhất trước
        /// </summary>
        public int Priority { get; set; } = 5; // Mặc định mức trung bình

        /// <summary>
        /// URL file audio thuyết minh tiếng Việt (.mp3)
        /// Nếu có, ưu tiên phát file audio thay vì TTS
        /// </summary>
        public string AudioUrl_VI { get; set; }

        /// <summary>
        /// URL file audio thuyết minh tiếng Anh (.mp3)
        /// </summary>
        public string AudioUrl_EN { get; set; }

        /// <summary>
        /// URL file audio thuyết minh tiếng Trung (.mp3)
        /// </summary>
        public string AudioUrl_CN { get; set; }

        /// <summary>
        /// Thời gian chờ trước khi phát lại (đơn vị: phút)
        /// Ví dụ: 5 = sau khi phát xong, phải đợi 5 phút mới phát lại
        /// </summary>
        public int CooldownMinutes { get; set; } = 5; // Mặc định 5 phút

        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ViewDetailsText
        {
            get => _viewDetailsText;
            set
            {
                if (_viewDetailsText != value)
                {
                    _viewDetailsText = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}