# 🚀 HƯỚNG DẪN SETUP DATABASE ĐƠN GIẢN

## ⚡ CHỈ CẦN 3 BƯỚC - 2 PHÚT

### BƯỚC 1: Mở phpMyAdmin
```
http://localhost/phpmyadmin
```

### BƯỚC 2: Tạo Database (nếu chưa có)
```
1. Click "New" ở menu trái
2. Tên database: VietnamFoodGuide
3. Collation: utf8mb4_unicode_ci
4. Click "Create"
```

### BƯỚC 3: Chạy File SQL
```
1. Click vào database "VietnamFoodGuide" vừa tạo
2. Click tab "SQL"
3. Copy TOÀN BỘ file: xampp_api/setup_complete.sql
4. Paste vào và click "Go"
5. ✅ XONG!
```

---

## 📋 2 FILE SQL - KHI NÀO DÙNG?

### ✅ `setup_complete.sql` - SETUP TỪ ĐẦU
**Dùng khi:** Setup database lần đầu tiên

**Tạo:**
- 6 bảng (Users, Foods, Favorites, Sessions, UserTracking, qr_scans)
- 8 users mẫu
- 11 quán ăn với **ĐẦY ĐỦ** các trường mới (Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes)
- Dữ liệu mẫu

**File này đã có SẴN tất cả cột mới!**

---

### ⚠️ `update_database_complete.sql` - CẬP NHẬT DATABASE CŨ
**Dùng khi:** Bạn đã có database từ trước và muốn thêm cột mới

**Làm gì:**
- ALTER TABLE Foods ADD COLUMN Radius...
- ALTER TABLE Foods ADD COLUMN Priority...
- (Thêm 5 cột mới vào bảng Foods hiện tại)

**KHÔNG dùng file này cho setup mới!**

---

## 🎯 TÓM TẮT

| Tình huống | File cần dùng | Lý do |
|------------|---------------|-------|
| Setup lần đầu | `setup_complete.sql` | Tạo tất cả từ đầu |
| Database đã có, cần thêm cột | `update_database_complete.sql` | Chỉ thêm cột mới |
| Bị lỗi "Table doesn't exist" | `setup_complete.sql` | Bạn đang dùng sai file! |

---

## ✅ KIỂM TRA SAU KHI SETUP

```sql
-- Kiểm tra bảng
SHOW TABLES;
-- Kết quả: 6 tables

-- Kiểm tra cột Foods
DESCRIBE Foods;
-- Phải có: Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes

-- Kiểm tra dữ liệu
SELECT COUNT(*) FROM Foods;
-- Kết quả: 11 quán ăn
```

---

## 🆘 TROUBLESHOOTING

### Lỗi: "Table doesn't exist"
**Nguyên nhân:** Bạn đang chạy `update_database_complete.sql` thay vì `setup_complete.sql`  
**Fix:** Chạy lại với file `setup_complete.sql`

### Lỗi: "Database doesn't exist"
**Fix:** Tạo database trước (Bước 2)

### Lỗi: "Column already exists"
**Nguyên nhân:** Bạn đã chạy file rồi  
**Fix:** Không cần làm gì, database đã OK!

---

## 🎉 HOÀN TẤT

Sau khi setup xong:

1. **Test API:**
   ```
   http://localhost/vfg-api/api.php?action=foods
   ```

2. **Đăng nhập app:**
   ```
   Username: admin
   Password: admin123
   ```

3. **✅ Enjoy!**

---

**Thời gian:** 2 phút  
**Độ khó:** ⭐☆☆☆☆ (Rất dễ)  
**File cần:** `setup_complete.sql` (CHỈ 1 FILE!)
