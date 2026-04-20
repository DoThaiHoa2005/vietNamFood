using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VietnamFoodGuide.Models.Entities
{
    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Category { get; set; }

        [JsonPropertyName("DescriptionVI")]
        public string Description_VI { get; set; }

        [JsonPropertyName("DescriptionEN")]
        public string Description_EN { get; set; }

        [JsonPropertyName("DescriptionCN")]
        public string Description_CN { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Rating { get; set; }

        [JsonPropertyName("Image")]
        public string ImagePath { get; set; }

        // Navigation properties
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
