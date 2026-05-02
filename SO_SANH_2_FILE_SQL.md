# 📊 SO SÁNH 2 FILE SQL

## ⚡ TÓM TẮT NHANH

| | `setup_complete.sql` | `update_database_complete.sql` |
|---|---|---|
| **Mục đích** | TẠO MỚI database từ đầu | CẬP NHẬT database đã có |
| **Khi nào dùng** | ✅ Setup lần đầu | ⚠️ Chỉ khi đã có DB cũ |
| **Tạo bảng** | ✅ CREATE TABLE | ❌ Không |
| **Thêm cột** | ✅ Có sẵn trong CREATE | ✅ ALTER TABLE ADD COLUMN |
| **Dữ liệu mẫu** | ✅ 11 quán ăn, 8 users | ❌ Không |
| **Cột Geofence** | ✅ Có sẵn | ✅ Thêm vào |
| **Thời gian** | 2 phút | 1 phút |

---

## 📋 CHI TIẾT FILE 1: `setup_complete.sql`

### Dùng cho: SETUP MỚI

### Làm gì:
```sql
-- 1. Tạo 6 bảng
CREATE TABLE Users (...)
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
CREATE TABLE Favorites (...)
CREATE TABLE Sessions (...)
CREATE TABLE UserTracking (...)
CREATE TABLE qr_scans (...)

-- 2. Thêm dữ liệu mẫu
INSERT INTO Users ... (8 users)
INSERT INTO Foods ... (11 quán ăn)
INSERT INTO Favorites ... (15 favorites)
```

### Kết quả:
- ✅ Database hoàn chỉnh
- ✅ Có đầy đủ bảng
- ✅ Có dữ liệu mẫu
- ✅ Có SẴN tất cả cột Geofence

### Khi nào dùng:
- ✅ Setup database lần đầu
- ✅ Tạo database mới từ đầu
- ✅ Reset database về trạng thái ban đầu

---

## 📋 CHI TIẾT FILE 2: `update_database_complete.sql`

### Dùng cho: CẬP NHẬT DATABASE CŨ

### Làm gì:
```sql
-- Giả định bảng Foods ĐÃ TỒN TẠI
USE VietnamFoodGuide;

-- Thêm cột mới vào bảng hiện tại
ALTER TABLE Foods ADD COLUMN Radius DOUBLE DEFAULT 30.0;
ALTER TABLE Foods ADD COLUMN Priority INT DEFAULT 5;
ALTER TABLE Foods ADD COLUMN AudioUrl VARCHAR(500);
ALTER TABLE Foods ADD COLUMN NarrationScript TEXT;
ALTER TABLE Foods ADD COLUMN CooldownMinutes INT DEFAULT 5;

-- Cập nhật dữ liệu hiện tại
UPDATE Foods SET Radius = 30.0 WHERE Id = 1;
UPDATE Foods SET Priority = 7 WHERE Id = 1;
...
```

### Kết quả:
- ✅ Thêm 5 cột mới vào bảng Foods
- ✅ Cập nhật giá trị cho các quán ăn hiện tại
- ❌ KHÔNG tạo bảng mới
- ❌ KHÔNG thêm dữ liệu mới

### Khi nào dùng:
- ⚠️ Bạn đã có database từ trước
- ⚠️ Bạn muốn GIỮ dữ liệu hiện tại
- ⚠️ Bạn chỉ muốn THÊM cột mới

---

## ❌ LỖI THƯỜNG GẶP

### Lỗi: "Table 'vietnamfoodguide.foods' doesn't exist"

**Nguyên nhân:**
```
Bạn đang chạy update_database_complete.sql cho database MỚI!
File này giả định bảng Foods ĐÃ TỒN TẠI.
```

**Fix:**
```
Chạy setup_complete.sql thay vì update_database_complete.sql
```

---

## ✅ HƯỚNG DẪN ĐÚNG

### Tình huống 1: Setup lần đầu
```
1. Mở phpMyAdmin
2. Tạo database "VietnamFoodGuide"
3. Chạy file: setup_complete.sql
4. ✅ XONG!
```

### Tình huống 2: Đã có database cũ, cần thêm cột
```
1. Mở phpMyAdmin
2. Chọn database "VietnamFoodGuide"
3. Chạy file: update_database_complete.sql
4. ✅ XONG!
```

### Tình huống 3: Bị lỗi "Table doesn't exist"
```
Bạn đang dùng SAI FILE!
→ Chạy setup_complete.sql
```

---

## 🎯 KẾT LUẬN

### Cho người mới:
```
CHỈ CẦN NHỚ: Chạy setup_complete.sql
File này đã có SẴN tất cả!
```

### Cho người đã có database:
```
Chạy update_database_complete.sql để thêm cột mới
Giữ nguyên dữ liệu hiện tại
```

---

## 📊 BẢNG SO SÁNH NHANH

| Tình huống | File cần dùng | Lý do |
|------------|---------------|-------|
| 🆕 Setup lần đầu | `setup_complete.sql` | Tạo tất cả từ đầu |
| 🔄 Cập nhật DB cũ | `update_database_complete.sql` | Chỉ thêm cột mới |
| ❌ Lỗi "Table doesn't exist" | `setup_complete.sql` | Bạn dùng sai file! |
| 🔄 Reset database | `setup_complete.sql` | Tạo lại từ đầu |
| 💾 Giữ dữ liệu hiện tại | `update_database_complete.sql` | Không xóa data |

---

## 🆘 VẪN CHƯA RÕ?

### Câu hỏi: "Tôi nên chạy file nào?"

**Trả lời:**
```
Bạn đã có database VietnamFoodGuide chưa?
├─ CHƯA → Chạy setup_complete.sql
└─ RỒI → Chạy update_database_complete.sql
```

### Câu hỏi: "Tôi chạy cả 2 file được không?"

**Trả lời:**
```
KHÔNG CẦN!
- Setup mới: CHỈ chạy setup_complete.sql
- Cập nhật: CHỈ chạy update_database_complete.sql
```

### Câu hỏi: "File nào có cột Geofence?"

**Trả lời:**
```
CẢ 2 FILE đều có!
- setup_complete.sql: Có SẴN trong CREATE TABLE
- update_database_complete.sql: Thêm vào bằng ALTER TABLE
```

---

**Tóm tắt 1 câu:**
- **Setup mới:** `setup_complete.sql` (có sẵn tất cả)
- **Cập nhật:** `update_database_complete.sql` (chỉ thêm cột)

✅ **Đơn giản vậy thôi!**
