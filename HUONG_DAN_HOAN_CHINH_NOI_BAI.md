# 🎯 HƯỚNG DẪN HOÀN CHỈNH - VIETNAM FOOD GUIDE APP

## 📋 TỔNG QUAN DỰ ÁN

**Vietnam Food Guide** là ứng dụng WPF C# hỗ trợ người dùng khám phá ẩm thực Việt Nam với các tính năng:

### ✨ TÍNH NĂNG CHÍNH
- 🗺️ **Bản đồ tương tác** với Google Maps tiles
- 🧭 **Chỉ đường thông minh** với giọng nói đa ngôn ngữ (Việt, Anh, Trung)
- 🔊 **Thuyết minh tự động** khi đến gần quán ăn
- 📍 **GPS tracking thực tế** và Test Mode mô phỏng
- 🧭 **La bàn xoay bản đồ** theo hướng di chuyển
- ❤️ **Yêu thích quán ăn** với database MySQL
- 👤 **Đăng ký/Đăng nhập** người dùng
- 🎯 **Admin Dashboard** quản lý người dùng và tracking

### 🛠️ CÔNG NGHỆ SỬ DỤNG
- **Frontend**: WPF C# (.NET Framework 4.8)
- **Backend**: PHP API với XAMPP
- **Database**: MySQL
- **Maps**: Leaflet.js + Google Maps tiles
- **Speech**: Windows Speech Platform
- **Navigation**: OSRM routing service

---

## 🚀 HƯỚNG DẪN CÀI ĐẶT

### 1️⃣ CÀI ĐẶT XAMPP VÀ DATABASE

1. **Tải và cài XAMPP**: https://www.apachefriends.org/
2. **Khởi động Apache và MySQL** trong XAMPP Control Panel
3. **Tạo database**:
   ```sql
   -- Mở http://localhost/phpmyadmin
   -- Tạo database mới tên: vietnam_food_guide
   -- Import file: xampp_api/setup_complete.sql
   ```

### 2️⃣ CÀI ĐẶT API

1. **Copy thư mục API**:
   ```bash
   # Copy thư mục xampp_api vào C:/xampp/htdocs/
   # Đường dẫn cuối: C:/xampp/htdocs/xampp_api/
   ```

2. **Kiểm tra API**:
   ```
   http://localhost/xampp_api/api.php
   # Phải trả về: {"status":"API is working"}
   ```

### 3️⃣ CHẠY ỨNG DỤNG

1. **Mở Visual Studio** và load project `VietnamFoodGuide.sln`
2. **Build và Run** (F5)
3. **Đăng ký tài khoản** hoặc dùng tài khoản có sẵn:
   - Username: `admin` / Password: `123456` (Admin)
   - Username: `khach` / Password: `123456` (User)

---

## 🎮 HƯỚNG DẪN SỬ DỤNG

### 📱 ỨNG DỤNG CHÍNH

#### 🔐 Đăng nhập
- Nhập username/password và nhấn Enter hoặc click "Đăng nhập"
- Có thể đăng ký tài khoản mới

#### 🗺️ Sử dụng bản đồ
1. **Chọn quán ăn**: Click vào marker đỏ trên bản đồ
2. **Chọn điểm xuất phát**:
   - 📍 GPS tự động (khuyến nghị)
   - 🔍 Tìm kiếm địa điểm
   - 🏢 Vị trí mặc định (Bến Thành)
3. **Bắt đầu chỉ đường**: Nhấn nút "🚀 Bắt đầu"

#### 🧭 Tính năng chỉ đường
- **Giọng nói**: Tự động phát hướng dẫn mỗi bước
- **La bàn**: Nhấn nút 🧭 để bật/tắt xoay bản đồ
- **Zoom thông minh**: Tự động zoom chi tiết khi gần điểm rẽ
- **Thuyết minh**: Tự động giới thiệu quán khi đến gần (20-40m)

#### 🧪 Test Mode
- Nhấn "🧪 Test Mode" để mô phỏng di chuyển
- Chỉ hoạt động khi đã bắt đầu navigation
- Tự động di chuyển theo route đã tính

#### ❤️ Yêu thích
- Nhấn ❤️ trong chi tiết quán để thêm/bỏ yêu thích
- Xem danh sách yêu thích từ menu chính

### 🖥️ ADMIN DASHBOARD

Truy cập: `http://localhost/xampp_api/../admin_dashboard.html`

#### 📊 Thống kê
- Tổng người dùng, quán ăn, yêu thích
- QR Scanned, App Installed, Currently Online
- Người đang chỉ đường

#### 👥 Quản lý người dùng
- Xem danh sách, thêm/sửa/xóa user
- Phân quyền Admin/User
- Theo dõi hoạt động

#### 🗺️ User Tracking
- Bản đồ real-time vị trí người dùng
- Danh sách người đang navigation
- Thống kê di chuyển

#### ❤️ Quản lý yêu thích
- Xem tất cả favorites
- Thống kê quán được yêu thích nhiều nhất
- Xóa favorites

---

## 🔧 TÍNH NĂNG KỸ THUẬT

### 🎯 Chỉ đường thông minh
```javascript
// Zoom tự động theo khoảng cách
if(distToStep < 50) zoomLevel = 19;      // Rất gần điểm rẽ
else if(distToStep < 150) zoomLevel = 18; // Gần điểm rẽ  
else if(distToStep < 300) zoomLevel = 17; // Trung bình
else zoomLevel = 16;                      // Xa
```

### 🔊 Giọng nói đa ngôn ngữ
```csharp
// Hỗ trợ 3 ngôn ngữ
vi-VN: Tiếng Việt
en-US: English  
zh-CN: 中文
```

### 🧭 La bàn và xoay bản đồ
```javascript
// Tính bearing từ hướng di chuyển
var bearing = Math.atan2(newLng-lastLng, newLat-lastLat) * (180/Math.PI);
map.setBearing(bearing); // Xoay bản đồ
```

### 📍 GPS Tracking thực tế
```javascript
// Theo dõi vị trí real-time
navigator.geolocation.watchPosition(callback, {
    enableHighAccuracy: true,
    timeout: 10000,
    maximumAge: 5000
});
```

---

## 🐛 XỬ LÝ LỖI THƯỜNG GẶP

### ❌ Lỗi Database
```
Lỗi: "Connection failed"
Giải pháp: 
1. Kiểm tra XAMPP MySQL đã chạy
2. Kiểm tra database vietnam_food_guide đã tạo
3. Import lại file setup_complete.sql
```

### ❌ Lỗi API
```
Lỗi: "API not found"
Giải pháp:
1. Kiểm tra thư mục xampp_api trong htdocs
2. Kiểm tra Apache đã chạy
3. Test: http://localhost/xampp_api/api.php
```

### ❌ Lỗi giọng nói
```
Lỗi: "Speech engine not found"
Giải pháp:
1. Cài đặt Windows Speech Platform
2. Tải voice pack tiếng Việt
3. Restart ứng dụng
```

### ❌ Lỗi GPS
```
Lỗi: "GPS not available"
Giải pháp:
1. Cho phép location access trong browser
2. Dùng IP Geolocation backup
3. Chọn vị trí mặc định
```

---

## 📁 CẤU TRÚC PROJECT

```
VietnamFoodGuide/
├── 📁 VietnamFoodGuide/           # Main WPF Application
│   ├── 📁 Views/                 # UI Windows
│   │   ├── LoginWindow.xaml      # Đăng nhập
│   │   ├── RegisterWindow.xaml   # Đăng ký
│   │   ├── MainWindow.xaml       # Menu chính
│   │   ├── MapWindow.xaml        # Bản đồ chính
│   │   ├── FoodDetailWindow.xaml # Chi tiết quán
│   │   └── FavoritesWindow.xaml  # Danh sách yêu thích
│   ├── 📁 Models/                # Data Models
│   │   ├── Entities/             # Database entities
│   │   └── FoodItem.cs           # Food model
│   ├── 📁 Services/              # Business Logic
│   │   ├── ApiService.cs         # API calls
│   │   ├── FoodService.cs        # Food data
│   │   ├── SpeechService.cs      # Text-to-Speech
│   │   └── FavoritesApiService.cs # Favorites API
│   └── 📁 Data/                  # Local data
│       └── foods.json            # Offline food data
├── 📁 xampp_api/                 # PHP Backend API
│   ├── api.php                   # Main API endpoints
│   └── setup_complete.sql        # Database schema
├── 📁 docs/                      # Documentation
│   ├── API.md                    # API documentation
│   ├── ARCHITECTURE.md           # System architecture
│   └── DEPLOYMENT.md             # Deployment guide
├── admin_dashboard.html          # Admin web interface
└── README.md                     # Project overview
```

---

## 🎯 ĐIỂM NỔI BẬT CỦA DỰ ÁN

### 🏆 Tính năng độc đáo
1. **Giọng nói chỉ đường thực tế** - Giống Google Maps
2. **Thuyết minh tự động** - Giới thiệu quán khi đến gần
3. **La bàn xoay bản đồ** - Trải nghiệm immersive
4. **Test Mode mô phỏng** - Dễ dàng demo và test
5. **Admin tracking real-time** - Quản lý người dùng

### 🔧 Kỹ thuật cao cấp
1. **Hybrid architecture** - WPF + Web technologies
2. **Real-time GPS tracking** - Vị trí chính xác
3. **Smart caching** - Route caching tối ưu performance
4. **Multi-language TTS** - Hỗ trợ 3 ngôn ngữ
5. **Responsive design** - UI thích ứng nhiều kích thước

### 📊 Quản lý dữ liệu
1. **MySQL database** - Lưu trữ an toàn
2. **RESTful API** - Chuẩn công nghiệp
3. **Data validation** - Kiểm tra dữ liệu đầu vào
4. **Error handling** - Xử lý lỗi toàn diện
5. **Backup & restore** - Sao lưu dữ liệu

---

## 🎓 KẾT LUẬN

**Vietnam Food Guide** là một ứng dụng hoàn chỉnh, tích hợp nhiều công nghệ hiện đại để tạo ra trải nghiệm người dùng tuyệt vời. Dự án thể hiện:

- ✅ **Kỹ năng lập trình đa nền tảng** (C#, PHP, JavaScript)
- ✅ **Thiết kế UI/UX chuyên nghiệp**
- ✅ **Tích hợp API và database**
- ✅ **Xử lý real-time data**
- ✅ **Tối ưu performance**
- ✅ **Quản lý dự án hoàn chỉnh**

Ứng dụng sẵn sàng triển khai thực tế và có thể mở rộng thêm nhiều tính năng khác.

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề khi cài đặt hoặc sử dụng:

1. **Kiểm tra log**: Nhấn F12 trong app để mở Developer Tools
2. **Kiểm tra API**: Test các endpoint trong Postman
3. **Kiểm tra database**: Xem dữ liệu trong phpMyAdmin
4. **Restart services**: Khởi động lại XAMPP và ứng dụng

**Chúc bạn thành công! 🎉**