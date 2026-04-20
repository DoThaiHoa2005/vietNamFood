using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VietnamFoodGuide.Models
{
    public class FoodItem : INotifyPropertyChanged
    {
        private bool _isFavorite;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}