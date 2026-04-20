# 📖 VIETNAM FOOD GUIDE - TÀI LIỆU HOÀN CHỈNH

**Phiên bản**: 1.0 Final  
**Ngày cập nhật**: Tháng 4, 2026  
**Trạng thái**: ✅ HOÀN THÀNH 100%

---

## 📑 MỤC LỤC

1. [Giới thiệu dự án](#1-giới-thiệu-dự-án)
2. [Cài đặt nhanh](#2-cài-đặt-nhanh)
3. [Kiến trúc hệ thống](#3-kiến-trúc-hệ-thống)
4. [Tính năng chính](#4-tính-năng-chính)
5. [Hướng dẫn sử dụng](#5-hướng-dẫn-sử-dụng)
6. [API Documentation](#6-api-documentation)
7. [Troubleshooting](#7-troubleshooting)
8. [Changelog](#8-changelog)

---

## 1. GIỚI THIỆU DỰ ÁN

**Vietnam Food Guide** là ứng dụng desktop hướng dẫn ẩm thực Việt Nam với các tính năng:

### 🎯 Mục tiêu
- Giúp du khách khám phá ẩm thực địa phương
- Cung cấp chỉ đường thông minh với giọng nói
- Hỗ trợ đa ngôn ngữ (Tiếng Việt, English, 中文)
- Thuyết minh tự động dựa trên vị trí

### 👥 Đối tượng sử dụng
- **Du khách**: Tìm kiếm và khám phá món ăn
- **Người dùng thường**: Xem thông tin quán ăn, đánh giá
- **Quản trị viên**: Quản lý dữ liệu qua Web Dashboard

### 🏆 Điểm nổi bật
- ✅ 11 quán ăn nổi tiếng tại Vĩnh Khánh, TP.HCM
- ✅ Bản đồ tương tác với OpenStreetMap
- ✅ Chỉ đường thông minh với OSRM routing
- ✅ Giọng nói đa ngôn ngữ (Google TTS)
- ✅ Thuyết minh tự động theo rating quán
- ✅ GPS fallback 3 cấp độ
- ✅ Admin dashboard đầy đủ

---

## 2. CÀI ĐẶT NHANH

### 📋 Yêu cầu hệ thống
- Windows 10/11 (64-bit)
- .NET Framework 4.8
- WebView2 Runtime
- XAMPP (Apache + MySQL + PHP 7.4+)

### ⚡ Các bước cài đặt

#### Bước 1: Setup Database
```bash
# 1. Mở XAMPP Control Panel
# 2. Start Apache và MySQL
# 3. Mở phpMyAdmin: http://localhost/phpmyadmin
# 4. Import file: xampp_api/setup_complete.sql
```

#### Bước 2: Setup API
```bash
# Windows: Chạy file
setup_api.bat

# Hoặc PowerShell
.\setup_api.ps1

# Hoặc manual: Copy xampp_api vào C:\xampp\htdocs\vfg-api
```

#### Bước 3: Build Application
```bash
# Mở Visual Studio
# Mở file VietnamFoodGuide.sln
# Nhấn F5 để build và chạy

# Hoặc dùng command line
dotnet build VietnamFoodGuide/VietnamFoodGuide.csproj
```

#### Bước 4: Login
```
Admin: admin / admin123
User: user123 / admin123
```

### ✅ Kiểm tra cài đặt
- [ ] XAMPP Apache đang chạy (port 80)
- [ ] XAMPP MySQL đang chạy (port 3306)
- [ ] Database `VietnamFoodGuide` đã được tạo
- [ ] API hoạt động: http://localhost/vfg-api/api.php?action=foods
- [ ] App build thành công, không có lỗi

---

## 3. KIẾN TRÚC HỆ THỐNG

### 📐 Sơ đồ tổng quan

```
┌─────────────────────────────────────────────────────────┐
│                  WPF DESKTOP APP                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │ LoginWindow  │  │ MainWindow   │  │ FoodDetail   │ │
│  │ RegisterWin  │  │ (Food Grid)  │  │ (TTS Audio)  │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
│                          │                              │
│                          ▼                              │
│  ┌─────────────────────────────────────────────────┐  │
│  │           MapWindow (WebView2)                  │  │
│  │  • Leaflet.js + OpenStreetMap                   │  │
│  │  • OSRM Routing API                             │  │
│  │  • Google TTS                                   │  │
│  │  • GPS Fallback (HTML5 → IP → Default)         │  │
│  └─────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                          │ HTTP API
                          ▼
┌─────────────────────────────────────────────────────────┐
│                   XAMPP API (PHP)                       │
│  • Authentication (Login/Register)                      │
│  • CRUD Operations (Foods, Users)                       │
│  • Session Management                                   │
│  • BCrypt Password Hashing                              │
└─────────────────────────────────────────────────────────┘
                          │ MySQL
                          ▼
┌─────────────────────────────────────────────────────────┐
│                   MySQL DATABASE                        │
│  • Users (Id, Username, PasswordHash, Role)             │
│  • Foods (Id, Name, Category, Lat, Lng, Rating...)      │
│  • Favorites (Id, UserId, FoodId)                       │
│  • Sessions (Id, UserId, Token, ExpiresAt)              │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│            ADMIN DASHBOARD (HTML/JS)                    │
│  • Manage Foods (CRUD)                                  │
│  • Manage Users (Role, Delete)                          │
│  • Upload Images                                        │
│  • Analytics & Export                                   │
└─────────────────────────────────────────────────────────┘
```

### 🗂️ Cấu trúc thư mục

```
VietnamFoodGuide/
├── VietnamFoodGuide/           # WPF Desktop App
│   ├── Assets/                 # Hình ảnh, icons
│   ├── Data/                   # foods.json
│   ├── Models/                 # FoodItem, User
│   ├── Services/               # ApiAuthService, SpeechService
│   ├── Views/                  # XAML windows
│   │   ├── LoginWindow.xaml
│   │   ├── RegisterWindow.xaml
│   │   ├── MainWindow.xaml
│   │   ├── FoodDetailWindow.xaml
│   │   └── MapWindow.xaml
│   └── VietnamFoodGuide.csproj
├── xampp_api/                  # Backend API
│   ├── api.php                 # RESTful endpoints
│   └── setup_complete.sql      # Database setup
├── admin_dashboard.html        # Web admin
└── README.md
```

---

## 4. TÍNH NĂNG CHÍNH

### 🔐 Authentication
- **Login**: Username/Password với BCrypt
- **Register**: Tạo tài khoản mới (auto role=User)
- **Remember Me**: Ghi nhớ tài khoản (XOR encryption)
- **Role-based**: Admin → Web Dashboard, User → Main Window

### 🍜 Food Management
- **11 quán ăn** tại Vĩnh Khánh, TP.HCM
- **Đa ngôn ngữ**: Tiếng Việt, English, 中文
- **Categories**: Bánh Mì, Bún, Cơm, Phở, Bánh Khác, Thức uống
- **Rating**: 4.4 - 4.9 sao
- **Audio Narration**: TTS đọc mô tả món ăn

### 🗺️ Interactive Map
- **OpenStreetMap**: Google Maps tiles
- **11 Markers**: Emoji icons theo category
  - 🥖 Bánh Mì
  - 🍜 Bún
  - 🍚 Cơm
  - 🍲 Phở
  - 🥟 Bánh Khác
  - ☕ Thức uống
- **User Position**: Blue draggable marker
- **Popup**: Click marker để xem thông tin

### 🧭 Smart Navigation
- **OSRM Routing**: Đường đi ngắn nhất
- **Route Caching**: Instant on repeat (0ms)
- **Distance Display**: 
  - < 1000m: "500 m"
  - ≥ 1000m: "1.5 km"
- **Time Estimation**: Phút còn lại
- **Auto-zoom**: Theo khoảng cách (zoom 16-19)
- **Route Progress**: Xóa vệt đã đi qua

### 🔊 Voice Navigation
- **Google TTS**: Giọng nói tự nhiên
- **Multi-language**: Auto-follow selected language
  - vi → vi-VN
  - en → en-US
  - zh → zh-CN
- **Turn-by-turn**: Hướng dẫn từng bước
- **Voice Repeat**: Lặp lại sau 10s nếu chưa di chuyển

### 🎤 Auto Narration (NEW!)
- **Proximity-based**: Thuyết minh khi đến gần quán
- **Rating-based Distance**:
  - Rating ≥ 4.7: 40m
  - Rating ≥ 4.5: 35m
  - Rating ≥ 4.3: 30m
  - Rating ≥ 4.0: 25m
  - Default: 20m
- **No Duplicate**: Chỉ đọc 1 lần mỗi quán
- **Banner Display**: Hiển thị tên quán đang đọc

### 📍 GPS & Location
- **3-level Fallback**:
  1. HTML5 Geolocation (GPS hardware)
  2. IP Geolocation API (city-level)
  3. Default Location (Bến Thành Market)
- **Auto Fallback**: Không hiển thị lỗi, tự động chuyển
- **Search Location**: Nominatim API (toàn Việt Nam)
- **Drag to Move**: Kéo marker để đổi vị trí

### 👨‍💼 Admin Dashboard
- **Food CRUD**: Create, Read, Update, Delete
- **User Management**: Change role, Delete user
- **Image Upload**: Drag-and-drop
- **Multi-language Tabs**: VI/EN/CN
- **Analytics**: Statistics, Export CSV
- **No Login Required**: Direct access

---

## 5. HƯỚNG DẪN SỬ DỤNG

### 🚀 Khởi động ứng dụng

1. **Đăng nhập**
   - Nhập username và password
   - Check "Ghi nhớ tài khoản" nếu muốn
   - Click "Đăng nhập"

2. **Đăng ký** (nếu chưa có tài khoản)
   - Click "Đăng ký ngay"
   - Nhập username, email, password
   - Click "Đăng ký"

### 🍽️ Xem thông tin món ăn

1. **Màn hình chính**
   - Hiển thị 11 quán ăn dạng grid
   - Mỗi card có: Ảnh, Tên, Rating, Category

2. **Chi tiết món ăn**
   - Click vào card để xem chi tiết
   - Xem ảnh lớn, mô tả đầy đủ
   - Đổi ngôn ngữ: VI/EN/CN
   - Click "Phát âm thanh" để nghe TTS
   - Click "Xem bản đồ" để chỉ đường

### 🗺️ Sử dụng bản đồ

#### Chọn điểm xuất phát
Khi mở bản đồ, popup sẽ hỏi:
1. **🌐 Dùng vị trí hiện tại (Tự động)**
   - Thử GPS → IP → Default
   - Tự động không hiển thị lỗi
2. **🔍 Tìm kiếm địa điểm**
   - Nhập tên địa điểm (VD: Bến Thành, Bitexco)
   - Chọn từ 10 kết quả
3. **📍 Dùng vị trí mặc định**
   - Bến Thành Market

#### Bắt đầu chỉ đường
1. Click **"🚀 Bắt đầu"**
2. Nghe hướng dẫn đầu tiên
3. Hướng dẫn lặp lại sau 10s nếu chưa di chuyển
4. Khi đến gần quán khác (20-40m), tự động thuyết minh

#### Thay đổi điểm xuất phát
1. Click **"📍 Đổi xuất phát"**
2. Chọn cách mới: Search hoặc Drag marker

#### Đổi ngôn ngữ
- Click selector góc phải trên
- Chọn: 🇻🇳 Tiếng Việt / 🇺🇸 English / 🇨🇳 中文
- UI và giọng nói tự động cập nhật

### 👨‍💼 Quản trị (Admin)

1. **Đăng nhập với tài khoản Admin**
   - Username: `admin`
   - Password: `admin123`
   - Tự động mở browser với Admin Dashboard

2. **Quản lý quán ăn**
   - Xem danh sách: Table view
   - Thêm mới: Click "Add Food"
   - Sửa: Click "Edit" trên row
   - Xóa: Click "Delete" (có confirm)

3. **Quản lý người dùng**
   - Xem danh sách users
   - Đổi role: Admin/User
   - Xóa user (có confirm)

4. **Upload ảnh**
   - Drag-and-drop vào form
   - Preview trước khi save
   - Delete nếu không muốn

---

## 6. API DOCUMENTATION

### Base URL
```
http://localhost/vfg-api/api.php
```

### Endpoints

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
  "token": "abc123...",
  "role": "Admin",
  "userId": 1
}
```

#### 2. Register
```http
POST /api.php?action=register
Content-Type: application/json

{
  "username": "newuser",
  "email": "user@example.com",
  "password": "password123"
}

Response:
{
  "success": true,
  "userId": 3,
  "message": "Registration successful"
}
```

#### 3. Get Foods
```http
GET /api.php?action=foods

Response:
{
  "success": true,
  "data": [
    {
      "Id": 1,
      "Name": "Bánh Mì Trần Văn Hành",
      "Category": "Bánh Mì",
      "Latitude": 10.78024,
      "Longitude": 106.70532,
      "Rating": 4.7,
      ...
    }
  ]
}
```

#### 4. CRUD Food
```http
# Create
POST /api.php?action=food
Content-Type: application/json
{ "name": "...", "category": "...", ... }

# Update
PUT /api.php?action=food
Content-Type: application/json
{ "id": 1, "name": "...", ... }

# Delete
DELETE /api.php?action=food
Content-Type: application/json
{ "id": 1 }
```

#### 5. Get Users
```http
GET /api.php?action=users

Response:
{
  "success": true,
  "data": [
    {
      "Id": 1,
      "Username": "admin",
      "Role": "Admin",
      "CreatedDate": "2026-04-01 10:00:00"
    }
  ]
}
```

#### 6. Update User Role
```http
POST /api.php?action=update_user_role
Content-Type: application/json

{
  "userId": 2,
  "newRole": "Admin"
}

Response:
{
  "success": true,
  "message": "Role updated successfully"
}
```

#### 7. Statistics
```http
GET /api.php?action=stats

Response:
{
  "success": true,
  "data": {
    "totalUsers": 2,
    "totalFoods": 11,
    "totalFavorites": 0
  }
}
```

---

## 7. TROUBLESHOOTING

### ❌ Lỗi thường gặp

#### 1. Không kết nối được API
**Triệu chứng**: Login failed, "Cannot connect to API"

**Giải pháp**:
```bash
# 1. Kiểm tra XAMPP
- Mở XAMPP Control Panel
- Đảm bảo Apache đang chạy (port 80)
- Đảm bảo MySQL đang chạy (port 3306)

# 2. Kiểm tra API URL
- Mở browser: http://localhost/vfg-api/api.php?action=foods
- Phải thấy JSON response

# 3. Kiểm tra folder API
- Đảm bảo xampp_api đã copy vào C:\xampp\htdocs\vfg-api
- Chạy lại setup_api.bat nếu cần
```

#### 2. Database không tồn tại
**Triệu chứng**: "Database 'VietnamFoodGuide' doesn't exist"

**Giải pháp**:
```bash
# 1. Mở phpMyAdmin: http://localhost/phpmyadmin
# 2. Click tab "Import"
# 3. Choose file: xampp_api/setup_complete.sql
# 4. Click "Go"
# 5. Kiểm tra database đã được tạo
```

#### 3. Build failed
**Triệu chứng**: "Could not copy VietnamFoodGuide.exe"

**Giải pháp**:
```bash
# 1. Đóng app đang chạy
# 2. Đóng Visual Studio
# 3. Xóa folder bin và obj
# 4. Mở lại Visual Studio
# 5. Build lại
```

#### 4. WebView2 Runtime not found
**Triệu chứng**: "WebView2 Runtime is not installed"

**Giải pháp**:
```bash
# Download và cài đặt:
https://go.microsoft.com/fwlink/p/?LinkId=2124703
```

#### 5. GPS không hoạt động
**Triệu chứng**: "GPS bị từ chối"

**Giải pháp**:
```bash
# App đã có GPS fallback tự động:
# GPS → IP Geolocation → Default Location
# Không cần làm gì, app sẽ tự xử lý

# Nếu muốn bật GPS:
# 1. Windows Settings → Privacy → Location
# 2. Bật "Location services"
# 3. Cho phép app truy cập location
```

#### 6. Bản đồ không hiển thị
**Triệu chứng**: Màn hình trắng trong MapWindow

**Giải pháp**:
```bash
# 1. Kiểm tra internet connection
# 2. Nhấn F12 để mở Developer Tools
# 3. Xem Console tab để check lỗi
# 4. Thử refresh: Đóng và mở lại MapWindow
```

#### 7. Giọng nói không hoạt động
**Triệu chứng**: Không nghe thấy TTS

**Giải pháp**:
```bash
# 1. Kiểm tra internet (Google TTS cần mạng)
# 2. Kiểm tra volume máy tính
# 3. Nhấn F12 → Console → Xem log
# 4. Thử đổi ngôn ngữ và phát lại
```

---

## 8. CHANGELOG

### Version 1.0 (April 2026) - FINAL RELEASE ✅

#### ✨ Tính năng mới
- ✅ Desktop WPF application hoàn chỉnh
- ✅ Login/Register với Remember Me
- ✅ 11 quán ăn tại Vĩnh Khánh, TP.HCM
- ✅ Bản đồ tương tác với OpenStreetMap
- ✅ Chỉ đường thông minh với OSRM routing
- ✅ Giọng nói đa ngôn ngữ (VI/EN/CN)
- ✅ Thuyết minh tự động dựa trên rating
- ✅ GPS fallback 3 cấp độ
- ✅ Admin dashboard đầy đủ
- ✅ RESTful API với PHP/MySQL
- ✅ BCrypt password hashing
- ✅ Role-based access control

#### 🔧 Cải tiến
- ✅ Hiển thị khoảng cách bằng mét (< 1km)
- ✅ Route caching cho hiệu suất
- ✅ Auto-zoom theo khoảng cách
- ✅ Route progress update (xóa vệt đã đi)
- ✅ Voice repeat sau 10s
- ✅ Auto narration khi đến gần quán
- ✅ GPS fallback không hiển thị lỗi
- ✅ Search location toàn Việt Nam

#### 🐛 Bug fixes
- ✅ Sửa lỗi build C# 7.3 compatibility
- ✅ Sửa lỗi register API
- ✅ Sửa lỗi GPS permission
- ✅ Sửa lỗi route calculation timeout
- ✅ Sửa lỗi voice navigation language
- ✅ Sửa lỗi auto narration không hoạt động
- ✅ Sửa lỗi distance display (km → m)

#### 📚 Documentation
- ✅ PRD v1.0 hoàn chỉnh
- ✅ Project completion checklist
- ✅ API documentation
- ✅ Setup guides
- ✅ Troubleshooting guide
- ✅ User manual

---

## 📞 LIÊN HỆ & HỖ TRỢ

### 🐛 Báo lỗi
- GitHub Issues: https://github.com/yourusername/VietnamFoodGuide/issues
- Email: support@vietnamfoodguide.com

### 📖 Tài liệu
- PRD: `PRD_VietnamFoodGuide_v1.0.md`
- API Docs: Section 6 của file này
- Checklist: `PROJECT_COMPLETION_CHECKLIST.md`

### 👨‍💻 Đóng góp
- Fork repository
- Tạo branch mới
- Commit changes
- Push và tạo Pull Request

---

## 📄 LICENSE

MIT License - Copyright © 2026 Vietnam Food Guide

---

## 🎉 KẾT LUẬN

**Vietnam Food Guide v1.0** đã hoàn thành 100% theo PRD với:
- ✅ 35/35 tính năng
- ✅ 11 quán ăn với dữ liệu đầy đủ
- ✅ Bản đồ tương tác chuyên nghiệp
- ✅ Giọng nói đa ngôn ngữ
- ✅ GPS fallback thông minh
- ✅ Admin dashboard hoàn chỉnh

**Sẵn sàng cho**: Demo, Presentation, Deployment, Production

**Cảm ơn bạn đã sử dụng Vietnam Food Guide!** 🍜🗺️🎤

---

*Tài liệu này tổng hợp từ 54 file markdown thành 1 file duy nhất để dễ quản lý.*

*Last updated: April 2026*
