# 🗄️ HƯỚNG DẪN SETUP DATABASE TỪ ĐẦU

## 🎯 MỤC TIÊU

Setup database `VietnamFoodGuide` hoàn chỉnh từ đầu với **CHỈ MỘT FILE DUY NHẤT**.

---

## ⚡ SETUP NHANH (CHỈ 3 BƯỚC)

### BƯỚC 1: Mở phpMyAdmin
```
http://localhost/phpmyadmin
```

### BƯỚC 2: Chọn Database
```
1. Click vào "VietnamFoodGuide" ở menu bên trái
2. Nếu chưa có database này, tạo mới:
   - Click "New" ở menu trái
   - Tên: VietnamFoodGuide
   - Collation: utf8mb4_unicode_ci
   - Click "Create"
```

### BƯỚC 3: Chạy File SQL DUY NHẤT
```
1. Click tab "SQL"
2. Copy TOÀN BỘ nội dung file: xampp_api/setup_complete.sql
3. Paste vào
4. Click "Go"
5. ✅ XONG! Không cần làm gì thêm!
```

---

## ✅ KẾT QUẢ

File `setup_complete.sql` đã tạo SẴN:
- ✅ 6 bảng: Users, Foods, Favorites, Sessions, UserTracking, qr_scans
- ✅ 8 users mẫu (admin/admin123, user123/user123)
- ✅ 11 quán ăn với **ĐẦY ĐỦ** các trường Geofence (Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes)
- ✅ 15 favorites mẫu
- ✅ 2 tracking records mẫu

**KHÔNG CẦN CHẠY FILE `update_database_complete.sql`** - File đó chỉ dùng để cập nhật database CŨ!

---

## ❌ LƯU Ý QUAN TRỌNG

**File `update_database_complete.sql` CHỈ dùng khi:**
- Bạn đã có database CŨ từ trước
- Bạn muốn THÊM các cột mới vào database hiện tại
- **KHÔNG dùng cho setup từ đầu!**

**Cho setup mới:** Chỉ cần `setup_complete.sql` - File này đã có SẴN tất cả!

---

## 🧪 BƯỚC 5: Kiểm Tra

### Kiểm tra database:
```sql
SHOW DATABASES LIKE 'VietnamFoodGuide';
```
**Kết quả:** 1 row

### Kiểm tra bảng:
```sql
SHOW TABLES FROM VietnamFoodGuide;
```
**Kết quả:** 6 tables (Users, Foods, Favorites, Sessions, UserTracking, qr_scans)

### Kiểm tra cột Foods:
```sql
DESCRIBE Foods;
```
**Kết quả:** Phải có các cột:
- Id, Name, City, Category
- Description_VI, Description_EN, Description_CN
- Latitude, Longitude, Rating, ImagePath
- **Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes** ← Mới

### Kiểm tra dữ liệu:
```sql
SELECT COUNT(*) FROM Users;    -- Kết quả: 8
SELECT COUNT(*) FROM Foods;    -- Kết quả: 11
SELECT COUNT(*) FROM Favorites; -- Kết quả: 15
```

---

## ✅ HOÀN TẤT

Nếu tất cả kiểm tra đều OK:
- ✅ Database đã setup xong
- ✅ Có đầy đủ bảng và dữ liệu
- ✅ Có 5 cột mới cho Geofence
- ✅ Sẵn sàng chạy app!

---

## 🔧 TROUBLESHOOTING

### Lỗi: "Database already exists"
```
Không sao! Bỏ qua bước 1, chuyển sang bước 2
```

### Lỗi: "Table already exists"
```sql
-- Xóa database và tạo lại
DROP DATABASE VietnamFoodGuide;
-- Sau đó chạy lại từ bước 1
```

### Lỗi: "Column already exists"
```
Không sao! Bỏ qua, cột đã tồn tại rồi
```

### Lỗi: "Access denied"
```
1. Kiểm tra user MySQL có quyền CREATE DATABASE
2. Thử đăng nhập lại phpMyAdmin
3. Restart MySQL trong XAMPP
```

---

## 📊 TỔNG QUAN DATABASE

```
VietnamFoodGuide/
├── Users (8 users)
│   ├── admin (Admin)
│   ├── user123 (User)
│   └── ... (6 users khác)
├── Foods (11 quán ăn)
│   ├── Bánh Mì Trần Văn Hành
│   ├── Bún Chả Hàng Cót
│   ├── Phở Đặc Biệt
│   └── ... (8 quán khác)
├── Favorites (15 favorites)
├── Sessions (authentication)
├── UserTracking (2 tracking records)
└── qr_scans (QR scan history)
```

---

## 🎯 BƯỚC TIẾP THEO

Sau khi setup database xong:

1. **Test API:**
```
http://localhost/vfg-api/api.php?action=foods
```

2. **Chạy app:**
```
F5 trong Visual Studio
```

3. **Đăng nhập:**
```
Username: admin
Password: admin123
```

4. **✅ Enjoy!**

---

**Thời gian:** ~5 phút  
**Độ khó:** ⭐⭐☆☆☆ (Dễ)  
**Yêu cầu:** XAMPP đang chạy
