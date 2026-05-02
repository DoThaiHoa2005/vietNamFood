# 📖 HƯỚNG DẪN SỬ DỤNG - VIETNAM FOOD GUIDE

## 🎯 TỔNG QUAN

**Vietnam Food Guide** là ứng dụng WPF C# giúp khám phá ẩm thực Việt Nam với:
- 🗺️ Bản đồ tương tác
- 🧭 Chỉ đường thông minh
- 🔊 Thuyết minh tự động (sắp có)
- ❤️ Yêu thích quán ăn
- 📱 QR Scanner
- 🗄️ SQLite database (offline)

---

## 🚀 CÀI ĐẶT NHANH

### 1. Cài đặt XAMPP và Database
```sql
1. Tải XAMPP: https://www.apachefriends.org/
2. Khởi động Apache và MySQL
3. Mở phpMyAdmin: http://localhost/phpmyadmin
4. Tạo database "VietnamFoodGuide" (utf8mb4_unicode_ci)
5. Chạy file: xampp_api/setup_complete.sql
   ⚠️ CHỈ CHẠY FILE NÀY - Đã có sẵn tất cả!
```

**Lưu ý quan trọng:**
- ✅ `setup_complete.sql` - Dùng cho setup MỚI (đã có đầy đủ tất cả cột)
- ⚠️ `update_database_complete.sql` - CHỈ dùng để cập nhật database CŨ
- 📖 Xem thêm: `HUONG_DAN_SETUP_DATABASE_DON_GIAN.md` hoặc `SO_SANH_2_FILE_SQL.md`

### 2. Cài đặt API
```bash
# Copy thư mục xampp_api vào C:/xampp/htdocs/
# Đường dẫn: C:/xampp/htdocs/vfg-api/
```

### 3. Chạy ứng dụng
```
1. Mở Visual Studio
2. Build > Rebuild Solution
3. F5 để chạy
4. Đăng nhập: admin / admin123
```

---

## 📁 CẤU TRÚC PROJECT

### Backend (PHP + MySQL)
```
xampp_api/
├── api.php                          # API chính
├── setup_complete.sql               # Setup database MỚI (CHỈ CHẠY FILE NÀY!)
├── update_database_complete.sql     # Cập nhật database CŨ (không dùng cho setup mới)
├── utilities.php                    # Tiện ích (check DB, test API)
└── admin_dashboard.html             # Quản trị
```

### Frontend (WPF C#)
```
VietnamFoodGuide/
├── Services/
│   ├── SQLiteFoodService.cs        # Quản lý SQLite (offline)
│   ├── ApiFoodService.cs           # Gọi API (online)
│   └── FavoritesApiService.cs      # Yêu thích
├── Views/
│   ├── MainWindow.xaml             # Màn hình chính
│   ├── MapWindow.xaml              # Bản đồ
│   ├── FoodDetailWindow.xaml       # Chi tiết quán
│   └── QRScannerWindow.xaml        # Quét QR
└── Data/
    └── foods.db                     # SQLite database (tự động tạo)
```

---

## 🔧 TÍNH NĂNG CHÍNH

### 1. Hệ thống Offline với SQLite
**Cách hoạt động:**
```
API Online  → Load từ API → Sync vào SQLite → Hiển thị
API Offline → Load từ SQLite (dữ liệu cũ) → Hiển thị
```

**Lợi ích:**
- ✅ App hoạt động không cần Internet
- ✅ Dữ liệu tự động sync từ API
- ✅ Nhanh hơn JSON

### 2. Quản lý Yêu thích
```
1. Click ❤️ trong chi tiết quán
2. Dữ liệu lưu vào MySQL
3. Xem danh sách: Menu > Yêu thích
```

### 3. QR Scanner
```
1. Sau khi đăng nhập → Hiện banner "Quét QR"
2. Click banner hoặc nút "📱 Quét QR"
3. Quét QR code
4. Lưu vào database
5. Banner biến mất vĩnh viễn
```

### 4. Bản đồ và Chỉ đường
```
1. Click quán ăn → Mở MapWindow
2. Chọn điểm xuất phát
3. Click "🚀 Bắt đầu"
4. Theo dõi chỉ đường real-time
```

---

## 🗄️ DATABASE

### Bảng chính:
- **Foods** - Quán ăn (với 5 trường mới: Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes)
- **Users** - Người dùng
- **Favorites** - Yêu thích
- **QRScans** - Lịch sử quét QR
- **UserTracking** - Theo dõi vị trí

### Cập nhật database:
```sql
-- Chạy file này trong phpMyAdmin
xampp_api/update_database_complete.sql
```

### Kiểm tra database:
```bash
cd xampp_api
php utilities.php check-db
```

---

## 🧪 KIỂM TRA

### 1. Kiểm tra API
```bash
# Mở browser
http://localhost/vfg-api/api.php?action=foods

# Hoặc dùng utilities
cd xampp_api
php utilities.php test-api
```

### 2. Kiểm tra SQLite
```
1. Chạy app
2. Kiểm tra file: VietnamFoodGuide/bin/Debug/net48/Data/foods.db
3. Mở bằng DB Browser for SQLite
```

### 3. Test Offline Mode
```
1. Tắt XAMPP (stop Apache)
2. Chạy app
3. ✅ App vẫn hiển thị quán ăn từ SQLite
```

---

## 🛠️ TIỆN ÍCH

### utilities.php - Công cụ đa năng
```bash
cd xampp_api

# Kiểm tra database
php utilities.php check-db

# Test API
php utilities.php test-api

# Backup database
php utilities.php backup-db

# Chạy tất cả
php utilities.php all
```

---

## 🐛 XỬ LÝ LỖI

### Lỗi: "Column 'Radius' not found"
```sql
-- Chạy lại file SQL
xampp_api/update_database_complete.sql
```

### Lỗi: "API not found"
```
1. Kiểm tra XAMPP Apache đang chạy
2. Kiểm tra đường dẫn: C:/xampp/htdocs/vfg-api/
3. Test: http://localhost/vfg-api/api.php
```

### Lỗi: "Unable to load DLL 'SQLite.Interop.dll'"
```bash
dotnet build --force
```

### App không có dữ liệu
```
1. Kiểm tra XAMPP đang chạy
2. Kiểm tra database đã setup
3. Xem Output Window (View > Output) để debug
```

---

## 📊 THỐNG KÊ DỮ LIỆU

### Dữ liệu mẫu:
- 11 quán ăn ở Vĩnh Khánh
- 8 users mẫu
- Favorites và tracking data

### Trường mới (cho Geofence & Narration):
| Trường | Mô tả | Mặc định |
|--------|-------|----------|
| Radius | Bán kính kích hoạt (m) | 30.0 |
| Priority | Ưu tiên phát (1-10) | 5 |
| AudioUrl | File audio | null |
| NarrationScript | Script thuyết minh | null |
| CooldownMinutes | Thời gian chờ (phút) | 5 |

---

## 🎓 TÍNH NĂNG SẮP CÓ (GIAI ĐOẠN 2)

1. **GeofenceEngine** - Tự động phát thuyết minh khi đến gần quán
2. **NarrationEngine** - Quản lý priority và cooldown
3. **AudioService** - Phát audio với NAudio
4. **BackgroundLocationService** - Theo dõi vị trí liên tục

---

## 📞 HỖ TRỢ

### Kiểm tra log:
- Visual Studio: View > Output
- XAMPP: Apache Error Log
- Browser: F12 Console

### Files quan trọng:
- `QUICK_START_GUIDE.md` - Hướng dẫn cài đặt nhanh
- `SQLITE_QUICK_GUIDE.md` - Hướng dẫn SQLite
- `xampp_api/utilities.php` - Công cụ kiểm tra

---

## ✅ CHECKLIST

### Lần đầu cài đặt:
- [ ] Cài XAMPP
- [ ] Chạy update_database_complete.sql
- [ ] Copy xampp_api vào htdocs
- [ ] Build project
- [ ] Chạy app
- [ ] Kiểm tra foods.db được tạo

### Mỗi lần chạy:
- [ ] Start XAMPP (Apache + MySQL)
- [ ] Chạy app
- [ ] Đăng nhập
- [ ] ✅ Sử dụng!

---

**Phiên bản:** 2.0  
**Ngày cập nhật:** 30/04/2026  
**Trạng thái:** ✅ SẴN SÀNG SỬ DỤNG

🎉 **Chúc bạn sử dụng thành công!**
