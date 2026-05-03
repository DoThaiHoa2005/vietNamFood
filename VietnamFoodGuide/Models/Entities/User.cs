using System;
using System.Collections.Generic;

namespace VietnamFoodGuide.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
