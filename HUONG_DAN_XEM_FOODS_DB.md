# 📊 HƯỚNG DẪN XEM FILE foods.db

## 📍 VỊ TRÍ FILE

```
VietnamFoodGuide\bin\Debug\net48\Data\foods.db
```

---

## 🔧 CÁCH 1: DB Browser for SQLite (KHUYẾN NGHỊ ⭐⭐⭐⭐⭐)

### Tải về và cài đặt:

1. **Tải về:**
   ```
   https://sqlitebrowser.org/dl/
   ```
   Chọn: **DB Browser for SQLite - Standard installer for 64-bit Windows**

2. **Cài đặt:**
   - Chạy file `.exe` vừa tải
   - Next > Next > Install
   - Finish

3. **Mở file foods.db:**
   ```
   1. Mở DB Browser for SQLite
   2. File > Open Database
   3. Chọn file: VietnamFoodGuide\bin\Debug\net48\Data\foods.db
   4. Click "Open"
   ```

4. **Xem dữ liệu:**
   ```
   1. Click tab "Browse Data"
   2. Chọn table "Foods" trong dropdown
   3. ✅ Xem tất cả dữ liệu!
   ```

### Tính năng:

- ✅ Xem dữ liệu dạng bảng
- ✅ Sửa dữ liệu trực tiếp
- ✅ Thêm/xóa records
- ✅ Chạy SQL queries (tab "Execute SQL")
- ✅ Export dữ liệu (CSV, JSON, SQL)
- ✅ Xem cấu trúc bảng (tab "Database Structure")

### Screenshot workflow:

```
DB Browser for SQLite
├── Tab "Database Structure" → Xem cấu trúc bảng
├── Tab "Browse Data" → Xem dữ liệu
├── Tab "Execute SQL" → Chạy queries
└── Tab "Edit Pragmas" → Cấu hình database
```

---

## 🔧 CÁCH 2: Visual Studio Code + SQLite Extension

### Cài đặt:

1. **Mở VS Code**

2. **Cài extension:**
   ```
   1. Ctrl+Shift+X (mở Extensions)
   2. Tìm: "SQLite"
   3. Cài extension "SQLite" by alexcvzz
   4. Reload VS Code
   ```

3. **Mở database:**
   ```
   1. Ctrl+Shift+P
   2. Gõ: "SQLite: Open Database"
   3. Chọn file: VietnamFoodGuide\bin\Debug\net48\Data\foods.db
   ```

4. **Xem dữ liệu:**
   ```
   1. Click icon "SQLITE EXPLORER" ở sidebar trái
   2. Mở database "foods.db"
   3. Click vào table "Foods"
   4. ✅ Xem dữ liệu!
   ```

### Tính năng:

- ✅ Xem dữ liệu trong VS Code
- ✅ Chạy SQL queries
- ✅ Export dữ liệu
- ✅ Tích hợp với editor

---

## 💻 CÁCH 3: Python Script (XEM NHANH)

### Yêu cầu:
- Python 3.x đã cài đặt

### Chạy script:

```bash
python view_foods_db.py
```

### Kết quả:

```
📊 XEM DỮ LIỆU FOODS.DB
================================================================================

✅ File tồn tại: VietnamFoodGuide\bin\Debug\net48\Data\foods.db
📁 Kích thước: 20,480 bytes
📅 Lần sửa cuối: 2026-04-30 12:56:35

✅ Kết nối database thành công!

📋 DANH SÁCH TABLES:
--------------------------------------------------------------------------------
  - Foods

📊 THỐNG KÊ:
--------------------------------------------------------------------------------
  Tổng quán ăn: 11

🏗️ CẤU TRÚC BẢNG FOODS:
--------------------------------------------------------------------------------
ID    Tên cột                   Kiểu dữ liệu    Not Null  
--------------------------------------------------------------------------------
0     Id                        INTEGER         NO        
1     Name                      TEXT            YES       
2     City                      TEXT            NO        
3     Category                  TEXT            NO        
...

🍜 TOP 5 QUÁN ĂN (theo Rating):
--------------------------------------------------------------------------------
ID  Tên                            Loại            Rating Radius   Priority Cooldown  
--------------------------------------------------------------------------------
6   Cà Phê Truyền Thống Sài Gòn   Thức uống       4.9    25.0     6        5         
5   Phở Đặc Biệt Trần Hưng Đạo    Phở             4.8    40.0     9        5         
...
```

---

## 🔧 CÁCH 4: PowerShell Script

### Chạy script:

```powershell
powershell -ExecutionPolicy Bypass -File view_foods_db.ps1
```

Hoặc:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\view_foods_db.ps1
```

---

## 🔧 CÁCH 5: SQLite Command Line (Nâng cao)

### Cài đặt SQLite CLI:

1. **Tải về:**
   ```
   https://www.sqlite.org/download.html
   ```
   Chọn: **sqlite-tools-win32-x86-*.zip**

2. **Giải nén:**
   - Giải nén vào `C:\sqlite\`
   - Thêm `C:\sqlite\` vào PATH

3. **Chạy:**
   ```bash
   cd "VietnamFoodGuide\bin\Debug\net48\Data"
   sqlite3 foods.db
   ```

### Các lệnh SQLite:

```sql
-- Xem danh sách tables
.tables

-- Xem cấu trúc bảng
.schema Foods

-- Xem dữ liệu
SELECT * FROM Foods;

-- Đếm số quán
SELECT COUNT(*) FROM Foods;

-- Top 5 quán theo rating
SELECT Name, Rating FROM Foods ORDER BY Rating DESC LIMIT 5;

-- Thoát
.quit
```

---

## 📊 CÁC QUERY HỮU ÍCH

### 1. Xem tất cả quán ăn:
```sql
SELECT * FROM Foods;
```

### 2. Xem quán có rating cao nhất:
```sql
SELECT Name, Rating, Category 
FROM Foods 
ORDER BY Rating DESC 
LIMIT 5;
```

### 3. Xem quán theo loại:
```sql
SELECT Name, Category, Rating 
FROM Foods 
WHERE Category = 'Phở';
```

### 4. Thống kê theo category:
```sql
SELECT 
    Category, 
    COUNT(*) as Total,
    ROUND(AVG(Rating), 1) as AvgRating
FROM Foods 
GROUP BY Category;
```

### 5. Xem quán có Priority cao:
```sql
SELECT Name, Priority, Radius, Rating 
FROM Foods 
ORDER BY Priority DESC, Rating DESC;
```

### 6. Tìm quán theo tên:
```sql
SELECT * FROM Foods 
WHERE Name LIKE '%Phở%';
```

### 7. Xem quán có NarrationScript:
```sql
SELECT Name, NarrationScript 
FROM Foods 
WHERE NarrationScript IS NOT NULL;
```

---

## 🎯 SO SÁNH CÁC CÁCH

| Cách | Độ khó | Tính năng | Khuyến nghị |
|------|--------|-----------|-------------|
| DB Browser for SQLite | ⭐☆☆☆☆ | ⭐⭐⭐⭐⭐ | ✅ Tốt nhất |
| VS Code + Extension | ⭐⭐☆☆☆ | ⭐⭐⭐⭐☆ | ✅ Nếu đã dùng VS Code |
| Python Script | ⭐⭐☆☆☆ | ⭐⭐⭐☆☆ | ✅ Xem nhanh |
| PowerShell Script | ⭐⭐⭐☆☆ | ⭐⭐⭐☆☆ | ⚠️ Cần .NET |
| SQLite CLI | ⭐⭐⭐⭐☆ | ⭐⭐⭐⭐⭐ | ⚠️ Nâng cao |

---

## 💡 KHUYẾN NGHỊ

### Cho người mới:
```
Dùng DB Browser for SQLite
→ Dễ dùng nhất
→ Giao diện đẹp
→ Đầy đủ tính năng
```

### Cho developer:
```
Dùng VS Code + SQLite Extension
→ Tích hợp với editor
→ Chạy queries nhanh
→ Xem code và database cùng lúc
```

### Cho admin/tester:
```
Dùng Python Script
→ Xem nhanh thống kê
→ Không cần cài thêm
→ Tự động hóa được
```

---

## 🆘 TROUBLESHOOTING

### Lỗi: "File is encrypted or is not a database"
```
File bị hỏng hoặc không phải SQLite database
Fix: Xóa file và chạy lại app để tạo mới
```

### Lỗi: "Database is locked"
```
App đang mở file
Fix: Đóng app trước khi mở bằng tool khác
```

### Lỗi: "Unable to open database file"
```
File không tồn tại
Fix: Chạy app một lần để tạo file
```

---

## ✅ CHECKLIST

- [ ] Đã tải DB Browser for SQLite
- [ ] Đã cài đặt thành công
- [ ] Đã mở được file foods.db
- [ ] Đã xem được dữ liệu trong bảng Foods
- [ ] Đã thử chạy SQL queries

---

**Khuyến nghị:** Dùng **DB Browser for SQLite** - Dễ nhất và đầy đủ nhất!

📥 Download: https://sqlitebrowser.org/dl/
