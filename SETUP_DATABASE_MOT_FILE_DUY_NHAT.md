# 🎯 SETUP DATABASE - CHỈ MỘT FILE DUY NHẤT

## ✅ Chỉ Cần 1 File SQL

**File duy nhất:** `xampp_api/setup_complete.sql`

Chạy file này **MỘT LẦN** là xong tất cả:
- ✅ Tạo database VietnamFoodGuide
- ✅ Tạo tất cả bảng (Users, Foods, Favorites, Sessions, UserTracking, qr_scans)
- ✅ Thêm dữ liệu mẫu (11 quán ăn, 8 users, tracking data, favorites)
- ✅ Dữ liệu đã được cấu hình đúng cho test tracking stats

## 🚀 Cách Chạy

### **Bước 1: Mở phpMyAdmin**
```
http://localhost/phpmyadmin
```

### **Bước 2: Import File SQL**
1. Click tab **"SQL"** ở menu trên
2. Copy toàn bộ nội dung file `xampp_api/setup_complete.sql`
3. Paste vào ô SQL
4. Click nút **"Go"** (hoặc "Thực hiện")

### **Bước 3: Xem Kết Quả**
Sau khi chạy xong, bạn sẽ thấy:

```
✅ Setup hoàn tất với User Tracking & QR Scanner!

📊 THỐNG KÊ:
👥 Users: 8 tài khoản
📱 QR Scanned: 6 người dùng
📲 App Installed: 4 người dùng
🟢 Currently Online: 3 người dùng
🧭 Currently Navigating: 2 người dùng
🍜 Foods: 11 quán ăn
❤️ Favorites: 15 yêu thích
📍 Tracking Active: 3 phiên
📱 QR Scans: 0 lượt quét
```

## 📊 Dữ Liệu Mẫu Có Sẵn

### **Users (8 tài khoản)**
| Username | Password | Role | QR | App | Online |
|----------|----------|------|----|----|--------|
| admin | admin123 | Admin | ✅ | ✅ | 🟢 |
| user123 | user123 | User | ✅ | ✅ | 🟢 |
| testuser | user123 | User | ✅ | ❌ | ⚪ |
| khach001 | user123 | User | ✅ | ❌ | ⚪ |
| khach002 | user123 | User | ❌ | ❌ | ⚪ |
| khach003 | user123 | User | ❌ | ❌ | ⚪ |
| nguoiyeuthich | user123 | User | ✅ | ✅ | ⚪ |
| dukhach01 | user123 | User | ✅ | ✅ | 🟢 |

### **Foods (11 quán ăn)**
- Bánh Mì Trần Văn Hành
- Bún Chả Hàng Cót
- Cơm Tấm Quân Đội
- Bánh Canh Cua Nha Trang
- Phở Đặc Biệt Trần Hưng Đạo
- Cà Phê Truyền Thống Sài Gòn
- Bánh Mì Thục
- Mì Quảng Minh Châu
- Bún Bò Huế Xứ Quảng
- Bánh Chưng Mỳ Tươi Homeboy
- Tiramisu Café Vĩnh Khánh

### **UserTracking (3 phiên)**
- **user123**: Đang navigate đến "Phở Đặc Biệt Trần Hưng Đạo" (🟢 Online)
- **testuser**: Đang navigate đến "Cà Phê Truyền Thống Sài Gòn" (⚪ Offline)
- **dukhach01**: Đang tracking nhưng không navigate (🟢 Online)

### **Favorites (15 yêu thích)**
- Nhiều users đã yêu thích các quán ăn khác nhau

## 🔄 Nếu Muốn Reset Lại

### **Option 1: Xóa Database Cũ**
```sql
DROP DATABASE IF EXISTS VietnamFoodGuide;
```
Sau đó chạy lại `setup_complete.sql`

### **Option 2: Xóa Từng Bảng**
```sql
USE VietnamFoodGuide;
DROP TABLE IF EXISTS Favorites;
DROP TABLE IF EXISTS UserTracking;
DROP TABLE IF EXISTS Sessions;
DROP TABLE IF EXISTS qr_scans;
DROP TABLE IF EXISTS Foods;
DROP TABLE IF EXISTS Users;
```
Sau đó chạy lại `setup_complete.sql`

## ⚠️ Lưu Ý Quan Trọng

### **Trên XAMPP (localhost)**
- File SQL đã có sẵn lệnh tạo database
- Chỉ cần chạy file là xong

### **Trên Hosting (InfinityFree, etc.)**
- **KHÔNG ĐƯỢC** dùng lệnh `DROP DATABASE` hoặc `CREATE DATABASE`
- Phải **XÓA** 2 dòng này trong file SQL:
  ```sql
  -- XÓA 2 DÒNG NÀY KHI CHẠY TRÊN HOSTING:
  DROP DATABASE IF EXISTS VietnamFoodGuide;
  CREATE DATABASE VietnamFoodGuide CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
  ```
- Sau đó **CHỌN** database có sẵn từ hosting panel
- Rồi mới chạy phần còn lại của file SQL

## 📱 Test Sau Khi Setup

### **1. Test Login**
```
Username: admin
Password: admin123
```
Hoặc
```
Username: user123
Password: user123
```

### **2. Test Admin Dashboard**
- Đăng nhập với tài khoản `admin`
- Sẽ tự động mở Admin Dashboard
- Vào "User Tracking" section
- Xem stats:
  - 📱 Quét QR Code: 6
  - 📲 Đã Cài App: 4
  - 🟢 Đang Online: 3
  - 🧭 Đang Chỉ Đường: 1 (chỉ user123)

### **3. Test App**
- Đăng nhập với tài khoản `user123`
- Sẽ mở MainWindow
- Xem danh sách 11 quán ăn
- Click vào Map để xem bản đồ

## 🎯 Tóm Tắt

```
1️⃣ Mở phpMyAdmin
2️⃣ Click tab "SQL"
3️⃣ Copy/Paste nội dung file xampp_api/setup_complete.sql
4️⃣ Click "Go"
5️⃣ ✅ XONG!
```

**Chỉ cần 1 file SQL duy nhất, chạy 1 lần là xong tất cả!**

---

**File cần chạy:** `xampp_api/setup_complete.sql`
**Số lần chạy:** 1 lần duy nhất
**Thời gian:** ~2-3 giây
**Kết quả:** Database hoàn chỉnh với đầy đủ dữ liệu mẫu
