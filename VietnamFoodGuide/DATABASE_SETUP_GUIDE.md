# Hướng Dẫn Cài Đặt Database

## File SQL Duy Nhất

**File:** `xampp_api/vietnamfoodguide.sql`

File này chứa toàn bộ cấu trúc database và dữ liệu mẫu, bao gồm:
- ✅ Tất cả bảng (Users, Foods, Favorites, Sessions, UserTracking, qr_scans)
- ✅ Dữ liệu mẫu (12 quán ăn, 2 users: admin và user123)
- ✅ Indexes và Foreign Keys
- ✅ Cột `HasInstalledApp` và `FirstLoginDate` cho tracking cài đặt app

---

## Cách Cài Đặt

### Phương Pháp 1: Sử dụng phpMyAdmin (Khuyến nghị)

1. **Mở phpMyAdmin**
   - Truy cập: `http://localhost/phpmyadmin`

2. **Tạo Database Mới**
   - Click tab "Databases"
   - Nhập tên: `vietnamfoodguide`
   - Collation: `utf8mb4_unicode_ci`
   - Click "Create"

3. **Import File SQL**
   - Click vào database `vietnamfoodguide` vừa tạo
   - Click tab "Import"
   - Click "Choose File" và chọn `xampp_api/vietnamfoodguide.sql`
   - Click "Go" ở cuối trang
   - Đợi import hoàn tất

4. **Kiểm Tra**
   - Click tab "Structure" để xem các bảng
   - Bạn sẽ thấy 6 bảng:
     - `favorites`
     - `foods` (12 quán ăn)
     - `qr_scans`
     - `sessions`
     - `users` (2 users)
     - `usertracking`

---

### Phương Pháp 2: Sử dụng MySQL Command Line

```bash
# 1. Mở Command Prompt hoặc Terminal
# 2. Chạy lệnh sau:

mysql -u root -p

# 3. Nhập password (mặc định XAMPP là rỗng, nhấn Enter)

# 4. Tạo database
CREATE DATABASE vietnamfoodguide CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

# 5. Sử dụng database
USE vietnamfoodguide;

# 6. Import file SQL
SOURCE C:/xampp/htdocs/vfg-api/vietnamfoodguide.sql;

# 7. Kiểm tra
SHOW TABLES;
SELECT COUNT(*) FROM Foods;
SELECT COUNT(*) FROM Users;
```

---

### Phương Pháp 3: Sử dụng MySQL Workbench

1. Mở MySQL Workbench
2. Kết nối đến MySQL Server (localhost)
3. Click "File" → "Run SQL Script"
4. Chọn file `xampp_api/vietnamfoodguide.sql`
5. Chọn "Default Schema": `vietnamfoodguide` (hoặc tạo mới)
6. Click "Run"

---

## Cấu Trúc Database

### Bảng `users`
| Cột | Kiểu | Mô Tả |
|-----|------|-------|
| Id | INT | Primary Key, Auto Increment |
| Username | VARCHAR(100) | Tên đăng nhập (UNIQUE) |
| PasswordHash | VARCHAR(255) | Mật khẩu đã hash (BCrypt) |
| Role | VARCHAR(20) | Vai trò: Admin hoặc User |
| CreatedDate | DATETIME | Ngày tạo tài khoản |
| LastActiveTime | DATETIME | Lần hoạt động cuối |
| IsLocked | BOOLEAN | Tài khoản bị khóa? |
| **HasInstalledApp** | **BOOLEAN** | **Đã cài app?** |
| **FirstLoginDate** | **DATETIME** | **Ngày đăng nhập lần đầu** |

**Dữ liệu mẫu:**
- `admin` / `admin123` (Role: Admin)
- `user123` / `user123` (Role: User)

---

### Bảng `foods`
| Cột | Kiểu | Mô Tả |
|-----|------|-------|
| Id | INT | Primary Key, Auto Increment |
| Name | VARCHAR(255) | Tên quán ăn |
| City | VARCHAR(255) | Khu vực |
| Category | VARCHAR(100) | Danh mục (Hải sản, Ốc, Bún...) |
| Description_VI | TEXT | Mô tả tiếng Việt |
| Description_EN | TEXT | Mô tả tiếng Anh |
| Description_CN | TEXT | Mô tả tiếng Trung |
| Latitude | DOUBLE | Vĩ độ |
| Longitude | DOUBLE | Kinh độ |
| Rating | DOUBLE | Đánh giá (0-5) |
| ImagePath | VARCHAR(500) | Đường dẫn ảnh |
| Radius | DOUBLE | Bán kính geofence (mét) |
| Priority | INT | Ưu tiên phát audio (1-10) |
| AudioUrl_VI | VARCHAR(500) | URL audio tiếng Việt |
| AudioUrl_EN | VARCHAR(500) | URL audio tiếng Anh |
| AudioUrl_CN | VARCHAR(500) | URL audio tiếng Trung |
| CooldownMinutes | INT | Thời gian chờ phát lại (phút) |

**Dữ liệu mẫu:** 12 quán ăn ở Vĩnh Khánh, Quận 4

---

### Bảng `favorites`
| Cột | Kiểu | Mô Tả |
|-----|------|-------|
| Id | INT | Primary Key, Auto Increment |
| UserId | INT | Foreign Key → users.Id |
| FoodId | INT | Foreign Key → foods.Id |
| CreatedDate | DATETIME | Ngày thêm yêu thích |

**UNIQUE:** (UserId, FoodId) - Mỗi user chỉ yêu thích 1 quán 1 lần

---

### Bảng `sessions`
| Cột | Kiểu | Mô Tả |
|-----|------|-------|
| Id | INT | Primary Key, Auto Increment |
| UserId | INT | Foreign Key → users.Id |
| Token | VARCHAR(255) | Session token (64 ký tự hex) |
| ExpiresAt | DATETIME | Thời gian hết hạn |
| CreatedDate | DATETIME | Ngày tạo session |

**Thời gian hết hạn:** 7 ngày

---

### Bảng `usertracking`
| Cột | Kiểu | Mô Tả |
|-----|------|-------|
| Id | INT | Primary Key, Auto Increment |
| UserId | INT | Foreign Key → users.Id (UNIQUE) |
| CurrentLat | DOUBLE | Vĩ độ hiện tại |
| CurrentLng | DOUBLE | Kinh độ hiện tại |
| DestinationLat | DOUBLE | Vĩ độ điểm đến |
| DestinationLng | DOUBLE | Kinh độ điểm đến |
| DestinationName | VARCHAR(255) | Tên điểm đến |
| IsNavigating | BOOLEAN | Đang chỉ đường? |
| IsActive | BOOLEAN | Đang online? |
| LastUpdate | DATETIME | Lần cập nhật cuối |
| CreatedDate | DATETIME | Ngày tạo |

**UNIQUE:** UserId - Mỗi user chỉ có 1 bản ghi tracking

---

### Bảng `qr_scans`
| Cột | Kiểu | Mô Tả |
|-----|------|-------|
| id | INT | Primary Key, Auto Increment |
| device_id | VARCHAR(255) | Device ID (UNIQUE) |
| qr_code | VARCHAR(500) | Mã QR đã quét |
| scan_date | DATETIME | Ngày quét |
| device_name | VARCHAR(255) | Tên thiết bị |
| os_version | VARCHAR(100) | Phiên bản OS |

**UNIQUE:** device_id - Mỗi thiết bị chỉ quét 1 lần

---

## Kiểm Tra Sau Khi Cài Đặt

### Query Kiểm Tra Cơ Bản

```sql
-- Kiểm tra số lượng bảng
SHOW TABLES;
-- Kết quả: 6 bảng

-- Kiểm tra số lượng quán ăn
SELECT COUNT(*) as TotalFoods FROM foods;
-- Kết quả: 12

-- Kiểm tra số lượng users
SELECT COUNT(*) as TotalUsers FROM users;
-- Kết quả: 2

-- Kiểm tra cột mới
DESCRIBE users;
-- Phải có: HasInstalledApp, FirstLoginDate

-- Xem dữ liệu users
SELECT Id, Username, Role, HasInstalledApp, FirstLoginDate FROM users;
```

### Query Kiểm Tra Chi Tiết

```sql
-- Kiểm tra indexes
SHOW INDEX FROM users;
-- Phải có: idx_has_installed_app

-- Kiểm tra foreign keys
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    CONSTRAINT_NAME,
    REFERENCED_TABLE_NAME,
    REFERENCED_COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'vietnamfoodguide'
AND REFERENCED_TABLE_NAME IS NOT NULL;

-- Kiểm tra quán ăn có audio URL
SELECT Id, Name, AudioUrl_VI, AudioUrl_EN, AudioUrl_CN 
FROM foods 
LIMIT 5;
```

---

## Cập Nhật Database (Nếu Đã Có Database Cũ)

### Cách 1: Xóa và Tạo Lại (Mất Dữ Liệu)

```sql
-- ⚠️ CẢNH BÁO: Lệnh này sẽ XÓA TẤT CẢ dữ liệu!
DROP DATABASE IF EXISTS vietnamfoodguide;
CREATE DATABASE vietnamfoodguide CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE vietnamfoodguide;
SOURCE C:/xampp/htdocs/vfg-api/vietnamfoodguide.sql;
```

### Cách 2: Thêm Cột Mới (Giữ Dữ Liệu)

```sql
-- Chỉ thêm 2 cột mới vào bảng users
USE vietnamfoodguide;

ALTER TABLE users 
ADD COLUMN HasInstalledApp TINYINT(1) DEFAULT 0 
COMMENT 'TRUE = Đã cài app và đăng nhập lần đầu';

ALTER TABLE users 
ADD COLUMN FirstLoginDate DATETIME DEFAULT NULL 
COMMENT 'Ngày đăng nhập lần đầu từ app';

ALTER TABLE users 
ADD INDEX idx_has_installed_app (HasInstalledApp);

-- Kiểm tra
DESCRIBE users;
```

---

## Backup Database

### Backup Toàn Bộ Database

```bash
# Sử dụng mysqldump
mysqldump -u root -p vietnamfoodguide > backup_vietnamfoodguide_$(date +%Y%m%d).sql

# Hoặc trong phpMyAdmin:
# 1. Chọn database vietnamfoodguide
# 2. Click tab "Export"
# 3. Chọn "Quick" hoặc "Custom"
# 4. Click "Go"
```

### Restore Từ Backup

```bash
mysql -u root -p vietnamfoodguide < backup_vietnamfoodguide_20260503.sql
```

---

## Troubleshooting

### Lỗi: Database already exists

**Giải pháp:**
```sql
DROP DATABASE vietnamfoodguide;
CREATE DATABASE vietnamfoodguide CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### Lỗi: Access denied for user 'root'

**Giải pháp:**
1. Kiểm tra XAMPP MySQL đang chạy
2. Kiểm tra password (mặc định XAMPP là rỗng)
3. Thử: `mysql -u root` (không cần -p)

### Lỗi: Column 'HasInstalledApp' doesn't exist

**Nguyên nhân:** Import file SQL cũ

**Giải pháp:**
1. Tải lại file `vietnamfoodguide.sql` mới nhất
2. Hoặc chạy:
```sql
ALTER TABLE users ADD COLUMN HasInstalledApp TINYINT(1) DEFAULT 0;
ALTER TABLE users ADD COLUMN FirstLoginDate DATETIME DEFAULT NULL;
```

### Lỗi: Duplicate entry for key 'unique_device'

**Nguyên nhân:** Đã có dữ liệu trong bảng qr_scans

**Giải pháp:**
```sql
TRUNCATE TABLE qr_scans;
```

---

## Thông Tin Quan Trọng

### Mật Khẩu Mặc Định

| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Admin |
| user123 | user123 | User |

**⚠️ LƯU Ý:** Đổi mật khẩu ngay sau khi cài đặt!

### Cấu Hình API

File `xampp_api/api.php` cần cấu hình:
```php
$host = 'localhost';
$db   = 'vietnamfoodguide';  // ← Tên database
$user = 'root';               // ← Username MySQL
$pass = '';                   // ← Password MySQL (mặc định rỗng)
```

### Đường Dẫn File

- **SQL File:** `xampp_api/vietnamfoodguide.sql`
- **API File:** `xampp_api/api.php`
- **Admin Dashboard:** `admin_dashboard.html`

---

## Tóm Tắt

✅ **Chỉ cần 1 file SQL:** `vietnamfoodguide.sql`
✅ **Bao gồm tất cả:** Bảng, dữ liệu, indexes, foreign keys
✅ **Tracking cài đặt app:** Cột `HasInstalledApp` và `FirstLoginDate`
✅ **Dữ liệu mẫu:** 12 quán ăn, 2 users
✅ **Sẵn sàng sử dụng:** Import và chạy ngay

**Các file SQL khác đã bị xóa:**
- ❌ `setup_complete.sql` (đã merge)
- ❌ `add_app_install_tracking.sql` (đã merge)
