using System;

namespace VietnamFoodGuide.Models.Entities
{
    public class Session
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
