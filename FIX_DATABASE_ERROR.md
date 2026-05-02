# 🔧 FIX LỖI: Table 'vietnamfoodguide.foods' doesn't exist

## ❌ LỖI

```
#1146 - Table 'vietnamfoodguide.foods' doesn't exist
```

## 🔍 NGUYÊN NHÂN

Bạn đang chạy **SAI FILE**! 

- ❌ Bạn chạy `update_database_complete.sql` (file này chỉ dùng để UPDATE database CŨ)
- ✅ Bạn cần chạy `setup_complete.sql` (file này TẠO MỚI tất cả bảng)

**File `update_database_complete.sql` giả định bảng Foods ĐÃ TỒN TẠI, nên nó chỉ ALTER TABLE thêm cột mới!**

---

## ✅ CÁCH SỬA ĐÚNG

### ⚡ GIẢI PHÁP: Chạy file ĐÚNG

```
1. Mở phpMyAdmin: http://localhost/phpmyadmin
2. Click vào database "VietnamFoodGuide" ở menu bên trái
   (Nếu chưa có, tạo mới: New → VietnamFoodGuide → utf8mb4_unicode_ci → Create)
3. Click tab "SQL"
4. Copy TOÀN BỘ nội dung file: xampp_api/setup_complete.sql
5. Paste vào và click "Go"
6. ✅ XONG! Database đã có đầy đủ bảng và dữ liệu!
```

### 📋 SO SÁNH 2 FILE

| File | Mục đích | Khi nào dùng |
|------|----------|--------------|
| `setup_complete.sql` | **TẠO MỚI** tất cả bảng + dữ liệu | ✅ Setup từ đầu |
| `update_database_complete.sql` | **CẬP NHẬT** thêm cột mới | ❌ Chỉ khi đã có database cũ |

**Cho setup mới:** Chỉ cần `setup_complete.sql` - File này đã có SẴN tất cả cột Geofence!

---

## 🧪 KIỂM TRA

Sau khi chạy xong, kiểm tra:

```sql
-- Kiểm tra database
SHOW DATABASES LIKE 'VietnamFoodGuide';

-- Kiểm tra bảng Foods
SHOW TABLES FROM VietnamFoodGuide;

-- Kiểm tra cột mới
DESCRIBE VietnamFoodGuide.Foods;
```

Kết quả mong đợi:
```
✅ Database: VietnamFoodGuide
✅ Table: Foods
✅ Columns: Id, Name, City, ..., Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes
```

---

## 📋 CHECKLIST

- [ ] Database `VietnamFoodGuide` đã tồn tại
- [ ] Đã chọn database trong phpMyAdmin
- [ ] Chạy file `update_database_complete.sql`
- [ ] Kiểm tra bảng Foods có 5 cột mới
- [ ] ✅ Không còn lỗi!

---

## 🆘 NẾU VẪN LỖI

### Lỗi: Database không tồn tại

```sql
-- Tạo database mới
CREATE DATABASE VietnamFoodGuide 
CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Sau đó chạy file setup_complete.sql để tạo tất cả bảng
```

### Lỗi: Bảng Foods không tồn tại

```sql
-- Chạy file setup_complete.sql để tạo tất cả bảng
-- File: xampp_api/setup_complete.sql
```

### Lỗi: Quyền truy cập

```
1. Kiểm tra user MySQL có quyền truy cập database
2. Thử đăng nhập lại phpMyAdmin
3. Restart MySQL trong XAMPP
```

---

## 🎯 TÓM TẮT NHANH

**Lỗi:** Chưa chọn database  
**Fix:** Chọn database `VietnamFoodGuide` trong phpMyAdmin trước khi chạy SQL  
**Hoặc:** Chạy lại file SQL đã được sửa (có lệnh `USE VietnamFoodGuide;`)

✅ **Đã sửa file `update_database_complete.sql` - Chạy lại là được!**
