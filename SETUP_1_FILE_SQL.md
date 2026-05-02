# 🎯 SETUP DATABASE - CHỈ 1 FILE SQL

## ✅ File Duy Nhất

**`xampp_api/setup_complete.sql`**

Chạy file này **1 LẦN** là xong tất cả!

## 🚀 Cách Chạy (3 Bước)

### 1️⃣ Mở phpMyAdmin
```
http://localhost/phpmyadmin
```

### 2️⃣ Chạy SQL
- Click tab **"SQL"**
- Copy toàn bộ nội dung file `xampp_api/setup_complete.sql`
- Paste vào ô SQL
- Click **"Go"**

### 3️⃣ Xong!
```
✅ Setup hoàn tất - Bắt đầu từ 0!

📊 Kết quả:
👥 Users: 2 tài khoản (admin + user123)
📱 QR Scanned: 0 người dùng (bắt đầu từ 0)
📲 App Installed: 0 người dùng (bắt đầu từ 0)
🟢 Currently Online: 0 người dùng (bắt đầu từ 0)
🧭 Currently Navigating: 0 người dùng (bắt đầu từ 0)
🍜 Foods: 11 quán ăn
❤️ Favorites: 0 yêu thích (bắt đầu từ 0)
```

## 📝 File Này Bao Gồm

- ✅ Tạo database VietnamFoodGuide
- ✅ Tạo 6 bảng (Users, Foods, Favorites, Sessions, UserTracking, qr_scans)
- ✅ Thêm **2 users**: `admin` (Admin) và `user123` (User)
- ✅ Thêm 11 quán ăn ở Vĩnh Khánh
- ✅ **BẮT ĐẦU TỪ 0**: Chưa có tracking, favorites, QR scans
- ✅ Khi user thực hiện hành động (đăng nhập, quét QR, yêu thích), số liệu sẽ tăng lên

## 🔐 Tài Khoản Test

### Admin (Quản Trị)
```
Username: admin
Password: admin123
Role: Admin
→ Đăng nhập sẽ mở Admin Dashboard
```

### User (Người Dùng)
```
Username: user123
Password: user123
Role: User
→ Đăng nhập sẽ mở App chính
```

**Chỉ có 2 tài khoản này. Muốn thêm user mới → dùng chức năng Đăng ký trong app.**

## ⚠️ Lưu Ý

### Trên XAMPP (localhost)
✅ Chạy file SQL trực tiếp, không cần sửa gì

### Trên Hosting
❌ Phải XÓA 2 dòng này:
```sql
DROP DATABASE IF EXISTS VietnamFoodGuide;
CREATE DATABASE VietnamFoodGuide CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```
✅ Sau đó chọn database có sẵn từ hosting panel

---

**CHỈ CẦN 1 FILE - CHẠY 1 LẦN - XONG NGAY!**
