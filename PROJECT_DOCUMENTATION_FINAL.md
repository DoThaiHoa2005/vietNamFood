# 📚 VIETNAM FOOD GUIDE - TÀI LIỆU DỰ ÁN HOÀN CHỈNH

## 📋 MỤC LỤC

1. [Tổng quan dự án](#tổng-quan-dự-án)
2. [Cấu trúc dự án](#cấu-trúc-dự-án)
3. [Cài đặt và triển khai](#cài-đặt-và-triển-khai)
4. [Tính năng chính](#tính-năng-chính)
5. [API Documentation](#api-documentation)
6. [Database Schema](#database-schema)
7. [Hướng dẫn sử dụng](#hướng-dẫn-sử-dụng)
8. [Troubleshooting](#troubleshooting)

---

## 🎯 TỔNG QUAN DỰ ÁN

**Vietnam Food Guide** là ứng dụng WPF hướng dẫn ẩm thực Việt Nam với các tính năng:

### Tính năng chính:
- ✅ **QR Scanner**: Quét QR code để truy cập app
- ✅ **Đa ngôn ngữ**: Hỗ trợ Tiếng Việt, English, 中文
- ✅ **Bản đồ tương tác**: Leaflet.js với navigation
- ✅ **Thuyết minh tự động**: Google Translate TTS
- ✅ **Yêu thích**: Lưu món ăn yêu thích
- ✅ **Admin Dashboard**: Theo dõi người dùng real-time
- ✅ **User Tracking**: GPS tracking và navigation

### Công nghệ sử dụng:
- **Frontend**: WPF (C# .NET Framework 4.8)
- **Backend**: PHP 7.4+ (XAMPP)
- **Database**: MySQL 5.7+
- **Map**: Leaflet.js + Google Maps Tiles
- **Speech**: Google Translate TTS API
- **QR Scanner**: html5-qrcode library

---

## 📁 CẤU TRÚC DỰ ÁN

```
VietnamFoodGuide/
├── VietnamFoodGuide/              # Main WPF Application
│   ├── Data/                      # Database Context
│   │   ├── ApplicationDbContext.cs
│   │   └── foods.json
│   ├── Models/                    # Data Models
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Food.cs
│   │   │   ├── Favorite.cs
│   │   │   ├── Session.cs
│   │   │   └── QRScan.cs
│   │   └── FoodItem.cs
│   ├── Services/                  # Business Logic
│   │   ├── LanguageService.cs     # Multi-language support
│   │   ├── StorageService.cs      # Local storage
│   │   ├── ApiFoodService.cs      # API calls
│   │   ├── FavoritesApiService.cs
│   │   ├── GoogleTranslateSpeechService.cs
│   │   └── AppConfig.cs
│   ├── Views/                     # UI Windows
│   │   ├── LoginWindow.xaml       # Login/Register
│   │   ├── MainWindow.xaml        # Home screen
│   │   ├── FoodDetailWindow.xaml  # Food details
│   │   ├── MapWindow.xaml         # Interactive map
│   │   ├── FavoritesWindow.xaml   # Favorites list
│   │   ├── AccountDialog.xaml     # Account settings
│   │   └── QRScannerWindow.xaml   # QR Scanner
│   └── Assets/                    # Images & Resources
│
├── xampp_api/                     # PHP Backend API
│   ├── api.php                    # Main API endpoint
│   ├── setup_complete.sql         # Database setup script
│   ├── create_qr_scans_table.sql  # QR scans table
│   └── uploads/                   # Uploaded images
│
├── docs/                          # Documentation
│   ├── API.md
│   ├── ARCHITECTURE.md
│   └── DEPLOYMENT.md
│
├── admin_dashboard.html           # Admin Dashboard
├── setup_api.ps1                  # API setup script
├── deploy.ps1                     # Deployment script
└── README.md                      # Project README
```

---

## 🔧 CÀI ĐẶT VÀ TRIỂN KHAI

### Bước 1: Cài đặt XAMPP

1. Download XAMPP: https://www.apachefriends.org/
2. Cài đặt và khởi động Apache + MySQL
3. Mở phpMyAdmin: http://localhost/phpmyadmin

### Bước 2: Tạo Database

```sql
-- Tạo database
CREATE DATABASE VietnamFoodGuide CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Chọn database
USE VietnamFoodGuide;

-- Import file setup_complete.sql
-- Hoặc copy nội dung file và paste vào SQL tab
```

### Bước 3: Cấu hình API

1. Copy folder `xampp_api` vào `C:\xampp\htdocs\vfg-api\`
2. Kiểm tra API: http://localhost/vfg-api/api.php?action=stats

### Bước 4: Build Application

```bash
# Clone repository
git clone <repository-url>

# Build project
dotnet build VietnamFoodGuide/VietnamFoodGuide.csproj --configuration Release

# Run application
.\VietnamFoodGuide\bin\Release\net48\VietnamFoodGuide.exe
```

### Bước 5: Tạo QR Code (Optional)

1. Truy cập: https://www.qr-code-generator.com/
2. Nội dung: `VIETNAM_FOOD_GUIDE_ACCESS_2024`
3. Download và lưu QR code

---

## 🎮 TÍNH NĂNG CHÍNH

### 1. QR Scanner System

**Mô tả**: Quét QR code để truy cập app lần đầu

**Tính năng**:
- Quét QR từ camera
- Chọn ảnh QR từ thư viện
- Lưu trạng thái đã quét (local + database)
- Kiểm tra đã quét → Bỏ qua lần sau
- Hỗ trợ đa ngôn ngữ

**Luồng hoạt động**:
```
App Start → QRScannerWindow
    ↓
Check if scanned (Local + API)
    ↓
    ├─ YES → MainWindow
    └─ NO → Show QR Scanner
        ↓
        ├─ Scan with Camera
        ├─ Select Image
        └─ Skip
        ↓
Save to Local + API → MainWindow
```

### 2. Multi-Language System

**Ngôn ngữ hỗ trợ**:
- 🇻🇳 Tiếng Việt (vi)
- 🇺🇸 English (en)
- 🇨🇳 中文 (zh)

**Tính năng**:
- 160+ translation keys
- Real-time language switching
- Tất cả windows tự động cập nhật
- JavaScript buttons cũng cập nhật

**Cách sử dụng**:
```csharp
// Get translation
var lang = LanguageService.Instance;
string text = lang["key_name"];

// Change language
lang.CurrentLanguage = "en"; // vi, en, zh

// Subscribe to changes
lang.LanguageChanged += (s, e) => UpdateUI();
```

### 3. Interactive Map & Navigation

**Tính năng**:
- Leaflet.js map với Google Maps tiles
- GPS tracking real-time
- Turn-by-turn navigation
- Voice guidance (Google TTS)
- Route calculation (OSRM)
- Compass mode
- Test mode (simulate movement)

**Các nút điều khiển**:
- 🚀 Start Navigation
- ⏹ Stop Navigation
- 🔄 Change Start Point
- 🎯 Change Destination
- 📍 Recenter to User
- 🧭 Toggle Compass

### 4. Speech & Narration

**Google Translate TTS**:
- Tự động phát âm chuẩn
- Hỗ trợ 3 ngôn ngữ
- Không cần API key
- Tự động thuyết minh khi gần quán ăn

**Cách sử dụng**:
```csharp
var speech = GoogleTranslateSpeechService.Instance;
speech.Speak("Xin chào", "vi-VN");
speech.Stop();
```

### 5. Favorites System

**Tính năng**:
- Lưu món ăn yêu thích
- Sync với API server
- Offline support (local storage)
- Real-time update

### 6. Admin Dashboard

**URL**: http://localhost/vfg-api/admin_dashboard.html

**Tính năng**:
- 📊 Statistics (Users, Foods, Favorites, QR Scans)
- 🗺️ Real-time User Tracking Map
- 👥 User Management
- 🍜 Food Management (CRUD)
- ❤️ Favorites Analytics
- 📱 QR Scan Statistics

---

## 📡 API DOCUMENTATION

### Base URL
```
http://localhost/vfg-api/api.php
```

### Authentication Endpoints

#### 1. Login
```http
POST /api.php?action=login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}

Response:
{
  "success": true,
  "user": {
    "id": 1,
    "username": "admin",
    "role": "Admin"
  }
}
```

#### 2. Register
```http
POST /api.php?action=register
Content-Type: application/json

{
  "username": "newuser",
  "password": "password123",
  "email": "user@example.com"
}
```

### Food Endpoints

#### 3. Get All Foods
```http
GET /api.php?action=foods

Response:
[
  {
    "Id": 1,
    "Name": "Bánh Mì Trần Văn Hành",
    "City": "TP.HCM - Vĩnh Khánh",
    "Category": "Bánh Mì",
    "Description_VI": "...",
    "Latitude": 10.78024,
    "Longitude": 106.70532,
    "Rating": 4.7,
    "ImagePath": "/Assets/Images/vn_banh_mi.png"
  }
]
```

#### 4. Add Food
```http
POST /api.php?action=foods
Content-Type: application/json

{
  "Name": "Phở Bò",
  "City": "Hà Nội",
  "Category": "Phở",
  "Description_VI": "...",
  "Latitude": 21.0285,
  "Longitude": 105.8542,
  "Rating": 4.5,
  "ImagePath": "/path/to/image.jpg"
}
```

### Favorites Endpoints

#### 5. Add Favorite
```http
POST /api.php?action=addFavorite
Content-Type: application/json

{
  "userId": 1,
  "foodId": 5
}
```

#### 6. Remove Favorite
```http
DELETE /api.php?action=removeFavorite?userId=1&foodId=5
```

#### 7. Get User Favorites
```http
GET /api.php?action=getUserFavorites?userId=1
```

### Tracking Endpoints

#### 8. Update Tracking
```http
POST /api.php?action=updateTracking
Content-Type: application/json

{
  "userId": 1,
  "currentLat": 10.7769,
  "currentLng": 106.7009,
  "destinationName": "Phở Đặc Biệt",
  "isNavigating": true,
  "isActive": true
}
```

#### 9. Get Tracking
```http
GET /api.php?action=getTracking
```

### QR Scanner Endpoints

#### 10. Save QR Scan
```http
POST /api.php?action=saveQRScan
Content-Type: application/json

{
  "deviceId": "xxx-xxx-xxx",
  "qrCode": "VIETNAM_FOOD_GUIDE_ACCESS_2024",
  "deviceName": "DESKTOP-ABC",
  "osVersion": "Windows 10"
}
```

#### 11. Check QR Scan
```http
GET /api.php?action=checkQRScan&deviceId=xxx-xxx-xxx

Response:
{
  "hasScanned": true,
  "scanDate": "2024-01-01 12:00:00"
}
```

#### 12. Get QR Stats
```http
GET /api.php?action=getQRStats

Response:
{
  "success": true,
  "data": {
    "totalScans": 100,
    "todayScans": 10,
    "weekScans": 50
  }
}
```

---

## 🗄️ DATABASE SCHEMA

### Tables Overview

```sql
-- 1. Users (Người dùng)
CREATE TABLE Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) DEFAULT 'User',
    QRScanned BOOLEAN DEFAULT FALSE,
    AppInstalled BOOLEAN DEFAULT FALSE,
    LastActiveTime DATETIME,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 2. Foods (Món ăn)
CREATE TABLE Foods (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL,
    City VARCHAR(255),
    Category VARCHAR(100),
    Description_VI TEXT,
    Description_EN TEXT,
    Description_CN TEXT,
    Latitude DOUBLE,
    Longitude DOUBLE,
    Rating DOUBLE,
    ImagePath VARCHAR(500)
);

-- 3. Favorites (Yêu thích)
CREATE TABLE Favorites (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    FoodId INT NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (FoodId) REFERENCES Foods(Id),
    UNIQUE KEY (UserId, FoodId)
);

-- 4. Sessions (Phiên đăng nhập)
CREATE TABLE Sessions (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    Token VARCHAR(255) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 5. UserTracking (Theo dõi vị trí)
CREATE TABLE UserTracking (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    CurrentLat DOUBLE,
    CurrentLng DOUBLE,
    DestinationLat DOUBLE,
    DestinationLng DOUBLE,
    DestinationName VARCHAR(255),
    IsNavigating BOOLEAN DEFAULT FALSE,
    IsActive BOOLEAN DEFAULT TRUE,
    LastUpdate DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 6. qr_scans (Quét QR)
CREATE TABLE qr_scans (
    id INT PRIMARY KEY AUTO_INCREMENT,
    device_id VARCHAR(255) UNIQUE NOT NULL,
    qr_code VARCHAR(500) NOT NULL,
    scan_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    device_name VARCHAR(255),
    os_version VARCHAR(100)
);
```

---

## 📖 HƯỚNG DẪN SỬ DỤNG

### Cho Người Dùng

#### 1. Lần đầu sử dụng
1. Mở app → QR Scanner xuất hiện
2. Quét QR code (hoặc chọn ảnh QR)
3. Sau khi quét thành công → Vào trang chủ
4. Đăng ký/Đăng nhập tài khoản

#### 2. Tìm kiếm món ăn
1. Nhập tên món ăn vào ô tìm kiếm
2. Hoặc chọn danh mục (Phở, Bún, Cơm, etc.)
3. Click "Xem chi tiết" để xem thông tin

#### 3. Xem bản đồ và navigation
1. Trong trang chi tiết → Click "📍 Bản đồ"
2. Click "🚀 Bắt đầu" để bắt đầu navigation
3. Nghe hướng dẫn giọng nói
4. Click "⏹ Dừng" để dừng navigation

#### 4. Thêm yêu thích
1. Trong trang chi tiết → Click "☆ Thêm yêu thích"
2. Xem danh sách yêu thích: Click icon ❤️ ở bottom bar

#### 5. Thay đổi ngôn ngữ
1. Chọn ngôn ngữ ở góc trên cùng
2. Tất cả giao diện tự động cập nhật

### Cho Admin

#### 1. Truy cập Admin Dashboard
- URL: http://localhost/vfg-api/admin_dashboard.html
- Không cần đăng nhập

#### 2. Xem thống kê
- Dashboard tab: Tổng quan
- Users tab: Quản lý người dùng
- Foods tab: Quản lý món ăn
- Tracking tab: Theo dõi real-time

#### 3. Thêm/Sửa/Xóa món ăn
1. Vào tab "Foods Management"
2. Click "Add New Food"
3. Điền thông tin và upload ảnh
4. Click "Save"

---

## 🔧 TROUBLESHOOTING

### Lỗi thường gặp

#### 1. "Cannot connect to Server"
**Nguyên nhân**: XAMPP chưa chạy hoặc API URL sai

**Giải pháp**:
```bash
# Kiểm tra XAMPP
- Mở XAMPP Control Panel
- Start Apache
- Start MySQL

# Kiểm tra API
- Truy cập: http://localhost/vfg-api/api.php?action=stats
- Nếu lỗi 404 → Kiểm tra đường dẫn folder
```

#### 2. "WebView2 Runtime not found"
**Nguyên nhân**: Chưa cài WebView2 Runtime

**Giải pháp**:
- Download: https://go.microsoft.com/fwlink/p/?LinkId=2124703
- Cài đặt và khởi động lại app

#### 3. "Camera not available"
**Nguyên nhân**: Quyền camera bị từ chối

**Giải pháp**:
- Windows Settings → Privacy → Camera
- Cho phép ứng dụng truy cập camera

#### 4. "Database connection failed"
**Nguyên nhân**: MySQL chưa chạy hoặc database chưa tạo

**Giải pháp**:
```sql
-- Kiểm tra MySQL đang chạy
-- Tạo database nếu chưa có
CREATE DATABASE VietnamFoodGuide;

-- Import setup_complete.sql
```

#### 5. "GPS not working"
**Nguyên nhân**: Quyền location bị tắt

**Giải pháp**:
- Windows Settings → Privacy → Location
- Bật "Location services"

---

## 📝 NOTES

### Default Accounts
```
Admin:
- Username: admin
- Password: admin123

User:
- Username: user123
- Password: user123
```

### API Base URL
```
Local: http://localhost/vfg-api/api.php
Ngrok: https://your-ngrok-url.ngrok.io/vfg-api/api.php
```

### QR Code Content
```
Recommended: VIETNAM_FOOD_GUIDE_ACCESS_2024
```

### Supported Languages
```
- vi: Tiếng Việt
- en: English
- zh: 中文
```

---

## 🚀 DEPLOYMENT

### Build Release
```bash
dotnet build --configuration Release
dotnet publish -c Release -r win-x64 --self-contained true
```

### Output
```
VietnamFoodGuide\bin\Release\net48\win-x64\publish\
```

### Deploy to Server
1. Upload `publish` folder to server
2. Upload `xampp_api` folder
3. Import `setup_complete.sql`
4. Update `AppConfig.cs` with server URL
5. Run `VietnamFoodGuide.exe`

---

## 📞 SUPPORT

- **Email**: support@vietnamfoodguide.com
- **GitHub**: https://github.com/your-repo
- **Documentation**: /docs folder

---

**Version**: 1.0.0  
**Last Updated**: 2026-04-29  
**Status**: ✅ Production Ready
