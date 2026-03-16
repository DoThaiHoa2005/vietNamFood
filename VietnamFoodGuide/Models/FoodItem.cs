namespace VietnamFoodGuide.Models
{
    public class FoodItem
    {
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
    }
}