# Vietnam Food Guide - XAMPP API

Backend API cho admin dashboard, chạy trên XAMPP (Apache + MySQL + PHP).

## 📁 Cấu trúc thư mục

```
xampp_api/
├── api.php              # REST API endpoints
├── import_foods.sql     # Script tạo database + import dữ liệu
├── SETUP_XAMPP.md       # Hướng dẫn setup chi tiết
├── README.md            # File này
└── uploads/             # Thư mục chứa ảnh upload (tự tạo)
```

## 🚀 Quick Start

### 1. Copy vào XAMPP
```bash
copy xampp_api C:\xampp\htdocs\vfg-api
```

### 2. Start XAMPP
- Mở XAMPP Control Panel
- Start Apache + MySQL

### 3. Import Database
- Vào http://localhost/phpmyadmin
- Tab SQL → paste nội dung `import_foods.sql`
- Click Go

### 4. Test API
```
http://localhost/vfg-api/api.php?action=foods
```

## 📡 API Endpoints

### GET /api.php?action=foods
Lấy danh sách tất cả quán ăn
```json
[
  {
    "Id": 1,
    "Name": "Bánh Mì Trần Văn Hành",
    "City": "TP.HCM - Vĩnh Khánh",
    "Category": "Bánh Mì",
    "Description_VI": "...",
    "Description_EN": "...",
    "Description_CN": "...",
    "Latitude": 10.78024,
    "Longitude": 106.70532,
    "Rating": 4.7,
    "ImagePath": "/Assets/Images/vn_banh_mi.png"
  }
]
```

### POST /api.php?action=foods
Thêm quán ăn mới
```json
{
  "Name": "Tên quán",
  "City": "TP.HCM",
  "Category": "Phở",
  "Description_VI": "Mô tả tiếng Việt",
  "Description_EN": "English description",
  "Description_CN": "中文描述",
  "Latitude": 10.78,
  "Longitude": 106.70,
  "Rating": 4.5,
  "ImagePath": "/path/to/image.jpg"
}
```

### PUT /api.php?action=food&id=1
Cập nhật quán ăn (body giống POST)

### DELETE /api.php?action=food&id=1
Xóa quán ăn

### GET /api.php?action=stats
Lấy thống kê
```json
{
  "totalFoods": 11,
  "totalCategories": 6,
  "avgRating": 4.6,
  "totalUsers": 2
}
```

### POST /api.php?action=upload
Upload ảnh (multipart/form-data)
```
FormData: { image: File }
```
Response:
```json
{
  "url": "http://localhost/vfg-api/uploads/food_xxxxx.jpg",
  "filename": "food_xxxxx.jpg"
}
```

### GET /api.php?action=users
Lấy danh sách users

## 🗄️ Database Schema

### Foods
```sql
Id INT PRIMARY KEY AUTO_INCREMENT
Name VARCHAR(255)
City VARCHAR(255)
Category VARCHAR(100)
Description_VI TEXT
Description_EN TEXT
Description_CN TEXT
Latitude DOUBLE
Longitude DOUBLE
Rating DOUBLE
ImagePath VARCHAR(500)
```

### Users
```sql
Id INT PRIMARY KEY AUTO_INCREMENT
Username VARCHAR(100) UNIQUE
PasswordHash VARCHAR(255)
Role VARCHAR(20)
CreatedDate DATETIME
```

### Favorites
```sql
Id INT PRIMARY KEY AUTO_INCREMENT
UserId INT FOREIGN KEY → Users(Id)
FoodId INT FOREIGN KEY → Foods(Id)
CreatedDate DATETIME
```

### Sessions
```sql
Id INT PRIMARY KEY AUTO_INCREMENT
UserId INT FOREIGN KEY → Users(Id)
Token VARCHAR(255)
ExpiresAt DATETIME
CreatedDate DATETIME
```

## 🔧 Cấu hình

### Database Connection (api.php)
```php
$host = 'localhost';
$db   = 'VietnamFoodGuide';
$user = 'root';
$pass = '';  // Đổi nếu có password
```

### CORS Headers
API đã enable CORS cho phép admin dashboard gọi từ file:// protocol:
```php
header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS");
```

### Upload Settings
- Thư mục: `xampp_api/uploads/`
- Định dạng: JPG, JPEG, PNG, GIF, WebP
- Tên file: `food_{uniqid}.{ext}`
- URL: `http://localhost/vfg-api/uploads/{filename}`

## 🧪 Testing

### Test với Browser
```
http://localhost/vfg-api/api.php?action=foods
http://localhost/vfg-api/api.php?action=stats
```

### Test với cURL
```bash
# GET foods
curl http://localhost/vfg-api/api.php?action=foods

# POST new food
curl -X POST http://localhost/vfg-api/api.php?action=foods \
  -H "Content-Type: application/json" \
  -d '{"Name":"Test","City":"HCM","Category":"Phở","Rating":4.5,...}'

# PUT update
curl -X PUT http://localhost/vfg-api/api.php?action=food&id=1 \
  -H "Content-Type: application/json" \
  -d '{"Name":"Updated Name",...}'

# DELETE
curl -X DELETE http://localhost/vfg-api/api.php?action=food&id=1

# Upload image
curl -X POST http://localhost/vfg-api/api.php?action=upload \
  -F "image=@/path/to/image.jpg"
```

### Test với Postman
1. Import collection từ endpoints trên
2. Set base URL: `http://localhost/vfg-api/api.php`
3. Test từng endpoint

## 📝 Default Data

Sau khi import `import_foods.sql`:
- **11 quán ăn** tại khu vực Vĩnh Khánh, TP.HCM
- **6 danh mục**: Phở, Bún, Cơm, Bánh Mì, Bánh Khác, Thức uống
- **2 users**: admin/admin123, user123/user123
- **Mô tả 3 ngôn ngữ** cho mỗi quán: VI, EN, CN

## 🔒 Security Notes

⚠️ **Đây là development setup, KHÔNG dùng cho production!**

Cần thêm cho production:
- [ ] Authentication/Authorization
- [ ] Input validation & sanitization
- [ ] SQL injection protection (đã có prepared statements)
- [ ] File upload validation nâng cao
- [ ] Rate limiting
- [ ] HTTPS
- [ ] Environment variables cho credentials

## 🐛 Troubleshooting

### "DB connection failed"
→ MySQL chưa start hoặc sai password

### "Action không hợp lệ"
→ Kiểm tra URL parameter `?action=...`

### Upload ảnh lỗi
→ Tạo folder `uploads/` và set quyền ghi

### CORS error
→ Kiểm tra headers trong api.php

### 404 Not Found
→ Kiểm tra đường dẫn: `C:\xampp\htdocs\vfg-api\api.php`

## 📚 Xem thêm

- [SETUP_XAMPP.md](SETUP_XAMPP.md) - Hướng dẫn setup chi tiết
- [../admin_dashboard.html](../admin_dashboard.html) - Frontend admin
- [../VietnamFoodGuide/](../VietnamFoodGuide/) - WPF Desktop App
