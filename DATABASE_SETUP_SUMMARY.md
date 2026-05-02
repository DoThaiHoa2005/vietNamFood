# 📚 TÓM TẮT SETUP DATABASE

## 🎯 ĐIỀU QUAN TRỌNG NHẤT

```
┌─────────────────────────────────────────────────────────┐
│                                                          │
│  SETUP MỚI: CHỈ CHẠY setup_complete.sql                │
│                                                          │
│  ✅ File này đã có SẴN tất cả cột Geofence!             │
│  ✅ Không cần chạy file nào khác!                       │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

## 📖 TÀI LIỆU HƯỚNG DẪN

### 🚀 Cho người mới (ĐỌC ĐẦU TIÊN!)

1. **SETUP_DATABASE_1_FILE_DUY_NHAT.md** ⭐⭐⭐⭐⭐
   - Hướng dẫn setup nhanh nhất
   - Chỉ 3 bước, 2 phút
   - Dễ hiểu nhất

2. **HUONG_DAN_SETUP_DATABASE_DON_GIAN.md** ⭐⭐⭐⭐
   - Hướng dẫn đơn giản
   - Có bảng so sánh 2 file
   - Có troubleshooting

3. **SO_SANH_2_FILE_SQL.md** ⭐⭐⭐
   - So sánh chi tiết 2 file SQL
   - Giải thích khi nào dùng file nào
   - Có bảng so sánh

---

### 🔧 Cho người gặp lỗi

4. **FIX_DATABASE_ERROR.md**
   - Fix lỗi "Table doesn't exist"
   - Giải thích nguyên nhân
   - Hướng dẫn fix

5. **SETUP_DATABASE_TU_DAU.md**
   - Setup database từ đầu chi tiết
   - Có kiểm tra từng bước
   - Có troubleshooting

---

## 🗂️ 2 FILE SQL

### ✅ File 1: `setup_complete.sql`

**Dùng cho:** SETUP MỚI

**Tạo:**
- 6 bảng (Users, Foods, Favorites, Sessions, UserTracking, qr_scans)
- 8 users mẫu
- 11 quán ăn
- 15 favorites
- Dữ liệu tracking

**Cột Foods:**
```sql
CREATE TABLE Foods (
    Id INT PRIMARY KEY,
    Name VARCHAR(255),
    City VARCHAR(255),
    Category VARCHAR(100),
    Description_VI TEXT,
    Description_EN TEXT,
    Description_CN TEXT,
    Latitude DOUBLE,
    Longitude DOUBLE,
    Rating DOUBLE,
    ImagePath VARCHAR(500),
    Radius DOUBLE DEFAULT 30.0,        ← Có sẵn!
    Priority INT DEFAULT 5,            ← Có sẵn!
    AudioUrl VARCHAR(500),             ← Có sẵn!
    NarrationScript TEXT,              ← Có sẵn!
    CooldownMinutes INT DEFAULT 5      ← Có sẵn!
)
```

**Khi nào dùng:**
- ✅ Setup database lần đầu
- ✅ Tạo database mới
- ✅ Reset database

---

### ⚠️ File 2: `update_database_complete.sql`

**Dùng cho:** CẬP NHẬT DATABASE CŨ

**Làm:**
```sql
-- Giả định bảng Foods ĐÃ TỒN TẠI
ALTER TABLE Foods ADD COLUMN Radius DOUBLE DEFAULT 30.0;
ALTER TABLE Foods ADD COLUMN Priority INT DEFAULT 5;
ALTER TABLE Foods ADD COLUMN AudioUrl VARCHAR(500);
ALTER TABLE Foods ADD COLUMN NarrationScript TEXT;
ALTER TABLE Foods ADD COLUMN CooldownMinutes INT DEFAULT 5;

-- Cập nhật dữ liệu
UPDATE Foods SET Radius = 30.0, Priority = 7 WHERE Id = 1;
...
```

**Khi nào dùng:**
- ⚠️ Bạn đã có database từ trước
- ⚠️ Bạn muốn GIỮ dữ liệu hiện tại
- ⚠️ Bạn chỉ muốn THÊM cột mới

**KHÔNG dùng cho setup mới!**

---

## 🎯 HƯỚNG DẪN NHANH

### Tình huống 1: Setup lần đầu

```
1. Mở phpMyAdmin
2. Tạo database "VietnamFoodGuide"
3. Chạy: setup_complete.sql
4. ✅ XONG!
```

**Đọc:** `SETUP_DATABASE_1_FILE_DUY_NHAT.md`

---

### Tình huống 2: Đã có database cũ

```
1. Mở phpMyAdmin
2. Chọn database "VietnamFoodGuide"
3. Chạy: update_database_complete.sql
4. ✅ XONG!
```

**Đọc:** `SO_SANH_2_FILE_SQL.md`

---

### Tình huống 3: Bị lỗi "Table doesn't exist"

```
Nguyên nhân: Bạn đang chạy SAI FILE!
Fix: Chạy setup_complete.sql
```

**Đọc:** `FIX_DATABASE_ERROR.md`

---

## 📊 BẢNG SO SÁNH NHANH

| Tình huống | File cần dùng | Tài liệu |
|------------|---------------|----------|
| 🆕 Setup lần đầu | `setup_complete.sql` | SETUP_DATABASE_1_FILE_DUY_NHAT.md |
| 🔄 Cập nhật DB cũ | `update_database_complete.sql` | SO_SANH_2_FILE_SQL.md |
| ❌ Lỗi "Table doesn't exist" | `setup_complete.sql` | FIX_DATABASE_ERROR.md |
| 🔄 Reset database | `setup_complete.sql` | SETUP_DATABASE_TU_DAU.md |
| 💾 Giữ dữ liệu hiện tại | `update_database_complete.sql` | SO_SANH_2_FILE_SQL.md |

---

## ✅ CHECKLIST

### Sau khi setup xong:

- [ ] Database "VietnamFoodGuide" đã tồn tại
- [ ] Có 6 bảng: Users, Foods, Favorites, Sessions, UserTracking, qr_scans
- [ ] Bảng Foods có 16 cột (bao gồm 5 cột Geofence)
- [ ] Có 11 quán ăn trong bảng Foods
- [ ] Có 8 users trong bảng Users
- [ ] API hoạt động: http://localhost/vfg-api/api.php?action=foods
- [ ] App chạy được và hiển thị dữ liệu

---

## 🧪 KIỂM TRA

```sql
-- Kiểm tra bảng
SHOW TABLES;
-- Kết quả: 6 tables

-- Kiểm tra cột Foods
DESCRIBE Foods;
-- Phải có: Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes

-- Kiểm tra dữ liệu
SELECT COUNT(*) FROM Foods;
-- Kết quả: 11

SELECT Name, Radius, Priority FROM Foods LIMIT 3;
-- Kết quả:
-- Bánh Mì Trần Văn Hành | 30.0 | 7
-- Bún Chả Hàng Cót      | 35.0 | 8
-- Phở Đặc Biệt          | 40.0 | 9
```

---

## 🆘 HỖ TRỢ

### Lỗi thường gặp:

1. **"Table doesn't exist"**
   - Đọc: `FIX_DATABASE_ERROR.md`
   - Fix: Chạy `setup_complete.sql`

2. **"Database doesn't exist"**
   - Fix: Tạo database trước

3. **"Column already exists"**
   - Fix: Không cần làm gì, database đã OK

4. **API không hoạt động**
   - Kiểm tra XAMPP đang chạy
   - Kiểm tra đường dẫn: C:/xampp/htdocs/vfg-api/

---

## 📚 TÀI LIỆU LIÊN QUAN

### Trong project:
- `README.md` - Tổng quan project
- `HUONG_DAN_SU_DUNG.md` - Hướng dẫn sử dụng đầy đủ
- `HUONG_DAN_HOAN_CHINH_NOI_BAI.md` - Hướng dẫn nội bài
- `HUONG_DAN_DEPLOY_ONLINE.md` - Deploy online

### Database:
- `SETUP_DATABASE_1_FILE_DUY_NHAT.md` ⭐ Đọc đầu tiên!
- `HUONG_DAN_SETUP_DATABASE_DON_GIAN.md`
- `SO_SANH_2_FILE_SQL.md`
- `FIX_DATABASE_ERROR.md`
- `SETUP_DATABASE_TU_DAU.md`

---

## 🎯 KẾT LUẬN

```
┌─────────────────────────────────────────────────────────┐
│                                                          │
│  SETUP MỚI: CHỈ CHẠY setup_complete.sql                │
│                                                          │
│  ✅ File này đã có SẴN tất cả!                          │
│  ✅ Không cần chạy file nào khác!                       │
│  ✅ Không cần chạy update_database_complete.sql!        │
│                                                          │
│  Đọc: SETUP_DATABASE_1_FILE_DUY_NHAT.md                │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

**Thời gian setup:** 2 phút  
**Độ khó:** ⭐☆☆☆☆ (Rất dễ)  
**File cần:** `setup_complete.sql` (CHỈ 1 FILE!)

✅ **Chúc bạn setup thành công!**
