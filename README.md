# 🍜 Vietnam Food Guide

Ứng dụng WPF C# giúp khám phá ẩm thực Việt Nam với bản đồ tương tác, chỉ đường thông minh, và thuyết minh tự động.

## ⚡ Cài đặt nhanh (5 phút)

### 1. Setup Database
```sql
1. Cài XAMPP: https://www.apachefriends.org/
2. Start Apache + MySQL
3. Mở phpMyAdmin: http://localhost/phpmyadmin
4. Tạo database "VietnamFoodGuide" (utf8mb4_unicode_ci)
5. Chạy file: xampp_api/setup_complete.sql
   ⚠️ CHỈ CHẠY FILE NÀY - Đã có sẵn tất cả!
```

**Lưu ý:** 
- ✅ `setup_complete.sql` - Dùng cho setup MỚI (đã có đầy đủ tất cả cột)
- ⚠️ `update_database_complete.sql` - CHỈ dùng để cập nhật database CŨ

### 2. Setup API
```bash
# Copy thư mục xampp_api vào:
C:/xampp/htdocs/vfg-api/
```

### 3. Chạy App
```
1. Mở Visual Studio
2. Build > Rebuild Solution
3. F5
4. Đăng nhập: admin / admin123
```

## ✨ Tính năng

- 🗺️ **Bản đồ tương tác** - Google Maps tiles
- 🧭 **Chỉ đường thông minh** - OSRM routing
- 🔊 **Thuyết minh tự động** - Sắp có (GIAI ĐOẠN 2)
- ❤️ **Yêu thích quán ăn** - Lưu vào MySQL
- 📱 **QR Scanner** - Tracking người dùng
- 🗄️ **SQLite offline** - Hoạt động không cần Internet
- 🌐 **Đa ngôn ngữ** - Việt, Anh, Trung

## 🛠️ Công nghệ

- **Frontend:** WPF C# (.NET Framework 4.8)
- **Backend:** PHP + MySQL (XAMPP)
- **Database:** MySQL (online) + SQLite (offline)
- **Maps:** Leaflet.js + Google Maps
- **Routing:** OSRM API

## 📁 Cấu trúc

```
VietnamFoodGuide/
├── VietnamFoodGuide/          # WPF App
│   ├── Services/              # Business logic
│   ├── Views/                 # UI Windows
│   ├── Models/                # Data models
│   └── Data/
│       └── foods.db           # SQLite (tự động tạo)
├── xampp_api/                 # PHP Backend
│   ├── api.php                # API endpoints
│   ├── setup_complete.sql     # Database setup (CHỈ CHẠY FILE NÀY!)
│   ├── update_database_complete.sql  # Cập nhật DB cũ (không dùng cho setup mới)
│   ├── utilities.php          # Tools (check DB, test API)
│   └── admin_dashboard.html   # Admin panel
└── docs/                      # Documentation
```

## 🧪 Kiểm tra

### Test API:
```bash
http://localhost/vfg-api/api.php?action=foods
```

### Test Database:
```bash
cd xampp_api
php utilities.php check-db
```

### Test Offline:
```
1. Tắt XAMPP
2. Chạy app
3. ✅ Vẫn hiển thị dữ liệu từ SQLite
```

## 📖 Tài liệu

### 🚀 Bắt đầu nhanh:
- **DATABASE_SETUP_SUMMARY.md** - 📚 Tổng quan setup database (ĐỌC ĐẦU TIÊN!)
- **SETUP_DATABASE_1_FILE_DUY_NHAT.md** - ⚡ Setup database 2 phút (CHỈ 1 FILE!)
- **HUONG_DAN_SETUP_DATABASE_DON_GIAN.md** - Setup database đơn giản

### 📘 Hướng dẫn chi tiết:
- **HUONG_DAN_SU_DUNG.md** - Hướng dẫn sử dụng đầy đủ
- **HUONG_DAN_HOAN_CHINH_NOI_BAI.md** - Hướng dẫn hoàn chỉnh nội bài
- **HUONG_DAN_DEPLOY_ONLINE.md** - Hướng dẫn deploy online

### 🔧 Xử lý lỗi:
- **FIX_DATABASE_ERROR.md** - Fix lỗi "Table doesn't exist"
- **SETUP_DATABASE_TU_DAU.md** - Setup database từ đầu chi tiết
- **SO_SANH_2_FILE_SQL.md** - So sánh 2 file SQL

## 🐛 Xử lý lỗi

### API không hoạt động:
```
1. Kiểm tra XAMPP đang chạy
2. Kiểm tra đường dẫn: C:/xampp/htdocs/vfg-api/
3. Test: http://localhost/vfg-api/api.php
```

### Database lỗi "Table doesn't exist":
```sql
-- Bạn đang chạy SAI FILE!
-- Chạy file này cho setup mới:
xampp_api/setup_complete.sql

-- File update_database_complete.sql CHỈ dùng để cập nhật DB cũ!
```

### App không có dữ liệu:
```
1. Kiểm tra Output Window (View > Output)
2. Xem log để debug
3. Kiểm tra XAMPP và database
```

## 🎯 Roadmap

### ✅ Đã hoàn thành:
- Bản đồ và chỉ đường
- Yêu thích quán ăn
- QR Scanner
- SQLite offline
- Đa ngôn ngữ

### 🚧 Đang phát triển (GIAI ĐOẠN 2):
- GeofenceEngine - Tự động phát thuyết minh
- NarrationEngine - Quản lý priority
- AudioService - Phát audio
- BackgroundLocationService - Tracking liên tục

## 📊 Dữ liệu

- **11 quán ăn** ở Vĩnh Khánh, TP.HCM
- **8 users** mẫu
- **Favorites** và tracking data
- **5 trường mới** cho Geofence: Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes

## 👥 Đăng nhập mẫu

```
Admin:
- Username: admin
- Password: admin123

User:
- Username: user123
- Password: user123
```

## 📞 Hỗ trợ

- **Issues:** Tạo issue trên GitHub
- **Documentation:** Đọc HUONG_DAN_SU_DUNG.md
- **Tools:** Dùng utilities.php để debug

## 📄 License

MIT License - Tự do sử dụng cho mục đích học tập

---

**Phiên bản:** 2.0  
**Ngày cập nhật:** 30/04/2026  
**Trạng thái:** ✅ Production Ready

🎉 **Chúc bạn sử dụng thành công!**
