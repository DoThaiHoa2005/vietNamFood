using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    public class FoodService
    {
        public List<FoodItem> LoadFoods()
        {
            // 1. Xác định đường dẫn file: AppContext.BaseDirectory giúp tìm đúng file khi chạy ứng dụng
            string path = Path.Combine(AppContext.BaseDirectory, "Data", "foods.json");

            try
            {
                if (!File.Exists(path))
                {
                    // Nếu không tìm thấy file, tạo một danh sách trống thay vì làm sập ứng dụng
                    return new List<FoodItem>();
                }

                string json = File.ReadAllText(path);

                // 2. Cấu hình JsonSerializer: Cho phép không phân biệt chữ hoa chữ thường giữa JSON và Model
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<List<FoodItem>>(json, options) ?? new List<FoodItem>();
            }
            catch (Exception ex)
            {
                // In lỗi ra cửa sổ Output để bạn dễ dàng debug nếu file JSON bị sai cú pháp
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc file JSON: {ex.Message}");
                return new List<FoodItem>();
            }
        }
    }
}