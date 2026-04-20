using System;

namespace VietnamFoodGuide.Models.Entities
{
    public class Favorite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FoodId { get; set; }
        public DateTime AddedDate { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Food Food { get; set; }
    }
}
