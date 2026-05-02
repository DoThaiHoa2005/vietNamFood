# 🎯 SETUP DATABASE - CHỈ 1 FILE DUY NHẤT!

## ⚡ 3 BƯỚC - 2 PHÚT

```
┌─────────────────────────────────────────────────────────┐
│  SETUP MỚI: CHỈ CHẠY setup_complete.sql                │
│  ✅ File này đã có SẴN tất cả!                          │
└─────────────────────────────────────────────────────────┘
```

---

## 📋 BƯỚC 1: Mở phpMyAdmin

```
http://localhost/phpmyadmin
```

---

## 📋 BƯỚC 2: Tạo Database

```
1. Click "New" ở menu trái
2. Tên: VietnamFoodGuide
3. Collation: utf8mb4_unicode_ci
4. Click "Create"
```

---

## 📋 BƯỚC 3: Chạy SQL

```
1. Click vào database "VietnamFoodGuide"
2. Click tab "SQL"
3. Copy TOÀN BỘ file: xampp_api/setup_complete.sql
4. Paste và click "Go"
5. ✅ XONG!
```

---

## ✅ KẾT QUẢ

File `setup_complete.sql` đã tạo:

```
✅ 6 bảng:
   - Users (8 users mẫu)
   - Foods (11 quán ăn)
   - Favorites (15 favorites)
   - Sessions
   - UserTracking
   - qr_scans

✅ Bảng Foods có ĐẦY ĐỦ các cột:
   - Id, Name, City, Category
   - Description_VI, Description_EN, Description_CN
   - Latitude, Longitude, Rating, ImagePath
   - Radius ← Có sẵn!
   - Priority ← Có sẵn!
   - AudioUrl ← Có sẵn!
   - NarrationScript ← Có sẵn!
   - CooldownMinutes ← Có sẵn!
```

---

## ❌ KHÔNG CẦN CHẠY FILE NÀY

```
❌ update_database_complete.sql
```

**Lý do:**
- File này CHỈ dùng để cập nhật database CŨ
- Nó giả định bảng Foods ĐÃ TỒN TẠI
- Nó chỉ ALTER TABLE thêm cột mới
- **KHÔNG dùng cho setup mới!**

---

## 🔍 SO SÁNH

### ✅ setup_complete.sql (DÙNG FILE NÀY!)
```sql
CREATE TABLE Foods (
    Id INT PRIMARY KEY,
    Name VARCHAR(255),
    ...
    Radius DOUBLE DEFAULT 30.0,        ← Có sẵn!
    Priority INT DEFAULT 5,            ← Có sẵn!
    AudioUrl VARCHAR(500),             ← Có sẵn!
    NarrationScript TEXT,              ← Có sẵn!
    CooldownMinutes INT DEFAULT 5      ← Có sẵn!
)

INSERT INTO Foods VALUES (...)         ← Có dữ liệu!
```

### ❌ update_database_complete.sql (KHÔNG DÙNG!)
```sql
-- Giả định bảng Foods ĐÃ TỒN TẠI
ALTER TABLE Foods ADD COLUMN Radius...
ALTER TABLE Foods ADD COLUMN Priority...
-- ❌ Nếu bảng chưa có → LỖI!
```

---

## 🆘 TROUBLESHOOTING

### Lỗi: "Table 'vietnamfoodguide.foods' doesn't exist"

**Nguyên nhân:**
```
Bạn đang chạy update_database_complete.sql (SAI!)
```

**Fix:**
```
Chạy setup_complete.sql (ĐÚNG!)
```

---

### Lỗi: "Database doesn't exist"

**Fix:**
```
Tạo database trước (Bước 2)
```

---

### Lỗi: "Column already exists"

**Nguyên nhân:**
```
Bạn đã chạy file rồi
```

**Fix:**
```
Không cần làm gì, database đã OK!
```

---

## 🧪 KIỂM TRA

Sau khi chạy xong, kiểm tra:

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

SELECT Name, Radius, Priority FROM Foods LIMIT 3;
-- Kết quả:
-- Bánh Mì Trần Văn Hành | 30.0 | 7
-- Bún Chả Hàng Cót      | 35.0 | 8
-- Phở Đặc Biệt          | 40.0 | 9
```

---

## 🎉 HOÀN TẤT

Sau khi setup xong:

### 1. Test API
```
http://localhost/vfg-api/api.php?action=foods
```

### 2. Chạy App
```
F5 trong Visual Studio
```

### 3. Đăng nhập
```
Username: admin
Password: admin123
```

### 4. ✅ Enjoy!

---

## 📊 TÓM TẮT

```
┌─────────────────────────────────────────────────────────┐
│  SETUP MỚI                                              │
│  ├─ File: setup_complete.sql                           │
│  ├─ Tạo: Tất cả bảng + dữ liệu                         │
│  └─ Cột: Có sẵn Radius, Priority, AudioUrl, ...        │
│                                                          │
│  CẬP NHẬT DATABASE CŨ                                   │
│  ├─ File: update_database_complete.sql                 │
│  ├─ Làm: Chỉ thêm cột mới                              │
│  └─ Dùng: Khi đã có database từ trước                  │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 KẾT LUẬN

**Cho setup mới:**
```
CHỈ CHẠY: setup_complete.sql
KHÔNG CHẠY: update_database_complete.sql
```

**Đơn giản vậy thôi!**

---

**Thời gian:** 2 phút  
**Độ khó:** ⭐☆☆☆☆ (Rất dễ)  
**File cần:** `setup_complete.sql` (CHỈ 1 FILE!)

✅ **Chúc bạn setup thành công!**
