# Hướng Dẫn Tracking Cài Đặt App

## Tổng Quan

Hệ thống tracking cài đặt app giúp admin biết được:
- Có bao nhiêu user đã cài app và đăng nhập lần đầu
- Ngày đăng nhập lần đầu từ app
- Chỉ tính 1 lần duy nhất (lần đăng nhập đầu tiên)

---

## Cách Hoạt Động

### 1. Database Schema

**Bảng `Users` - Thêm 2 cột mới:**

| Cột | Kiểu | Mô Tả |
|-----|------|-------|
| `HasInstalledApp` | BOOLEAN | `TRUE` = Đã cài app và đăng nhập lần đầu |
| `FirstLoginDate` | DATETIME | Ngày đăng nhập lần đầu từ app |

**Index:**
- `idx_has_installed_app` trên cột `HasInstalledApp`

---

### 2. Luồng Hoạt Động

```
User cài app lần đầu
    ↓
Mở app và đăng nhập
    ↓
App gửi request login lên server
    ↓
Server kiểm tra: HasInstalledApp = FALSE?
    ↓
┌─────────────────────────────────────────┐
│ Lần đầu tiên đăng nhập?                 │
├─────────────────────────────────────────┤
│ ✅ YES → Cập nhật:                      │
│          HasInstalledApp = TRUE         │
│          FirstLoginDate = NOW()         │
│                                         │
│ ❌ NO  → Không làm gì (đã đánh dấu rồi)│
└─────────────────────────────────────────┘
    ↓
Admin Dashboard hiển thị số lượng
```

---

### 3. API Changes

#### **Endpoint: `POST /api.php?action=login`**

**Thay đổi:**
```php
// Sau khi verify password thành công
if ($passwordValid) {
    // ... tạo session token ...
    
    // ✅ Tracking cài đặt app (chỉ đánh dấu lần đầu tiên)
    if (!$user['HasInstalledApp']) {
        $stmt = $pdo->prepare("UPDATE Users SET HasInstalledApp = TRUE, FirstLoginDate = NOW() WHERE Id = ?");
        $stmt->execute([$user['Id']]);
    }
    
    // ... response ...
}
```

**Response mới:**
```json
{
  "success": true,
  "user": {
    "id": 2,
    "username": "user123",
    "role": "User",
    "hasInstalledApp": true
  },
  "token": "abc123...",
  "expiresAt": "2026-05-10 10:00:00",
  "message": "Đăng nhập thành công"
}
```

#### **Endpoint: `GET /api.php?action=getAppStats`**

**Thay đổi:**
```php
case 'getAppStats':
    $totalQR = $pdo->query("SELECT COUNT(*) FROM qr_scans")->fetchColumn();
    $totalInstalled = $pdo->query("SELECT COUNT(*) FROM Users WHERE HasInstalledApp = TRUE")->fetchColumn();
    // ...
    
    echo json_encode([
        "success" => true,
        "data" => [
            "totalQR" => 10,
            "totalInstalled" => 5,  // ← Số user đã cài app
            "totalUsers" => 20,
            "onlineCount" => 3,
            "navigatingCount" => 1
        ]
    ]);
```

---

### 4. Admin Dashboard

**Card hiển thị:**
```html
<div class="stat-card">
    <div class="stat-icon primary">
        <i class="fas fa-mobile-alt"></i>
    </div>
    <div class="stat-info">
        <h3 id="appInstalledUsers">0</h3>
        <p>Đã Cài App</p>
    </div>
</div>
```

**JavaScript cập nhật:**
```javascript
function updateTrackingStats(mergedData, stats) {
    document.getElementById('qrScannedUsers').textContent = stats.totalQR || 0;
    document.getElementById('appInstalledUsers').textContent = stats.totalInstalled || 0;
    document.getElementById('trackingOnline').textContent = stats.onlineCount || 0;
    document.getElementById('trackingNavigating').textContent = stats.navigatingCount || 0;
}
```

---

## Cài Đặt

### Bước 1: Chạy Migration SQL

**File:** `xampp_api/add_app_install_tracking.sql`

```sql
-- Thêm cột HasInstalledApp
ALTER TABLE Users ADD COLUMN HasInstalledApp BOOLEAN DEFAULT FALSE 
COMMENT "TRUE = Đã cài app và đăng nhập lần đầu";

-- Thêm cột FirstLoginDate
ALTER TABLE Users ADD COLUMN FirstLoginDate DATETIME NULL 
COMMENT "Ngày đăng nhập lần đầu từ app";

-- Thêm index
ALTER TABLE Users ADD INDEX idx_has_installed_app (HasInstalledApp);
```

**Cách chạy:**
1. Mở phpMyAdmin
2. Chọn database `VietnamFoodGuide`
3. Click tab "SQL"
4. Copy toàn bộ nội dung file `add_app_install_tracking.sql`
5. Paste và click "Go"

### Bước 2: Cập Nhật API

File `xampp_api/api.php` đã được cập nhật:
- ✅ Endpoint `login` - Thêm logic tracking
- ✅ Endpoint `getAppStats` - Thêm `totalInstalled`

### Bước 3: Cập Nhật Admin Dashboard

File `admin_dashboard.html` đã có sẵn:
- ✅ Card "Đã Cài App"
- ✅ JavaScript cập nhật giá trị

---

## Kiểm Tra

### Test 1: Đăng Nhập Lần Đầu

1. Tạo user mới hoặc reset `HasInstalledApp = FALSE` cho user test
2. Mở app và đăng nhập
3. Kiểm tra database:
   ```sql
   SELECT Id, Username, HasInstalledApp, FirstLoginDate 
   FROM Users 
   WHERE Username = 'user123';
   ```
4. ✅ Kết quả mong đợi:
   - `HasInstalledApp` = `1` (TRUE)
   - `FirstLoginDate` = thời gian hiện tại

### Test 2: Đăng Nhập Lần Thứ 2

1. Đăng xuất và đăng nhập lại
2. Kiểm tra database:
   ```sql
   SELECT Id, Username, HasInstalledApp, FirstLoginDate 
   FROM Users 
   WHERE Username = 'user123';
   ```
3. ✅ Kết quả mong đợi:
   - `HasInstalledApp` vẫn = `1` (không thay đổi)
   - `FirstLoginDate` vẫn giữ nguyên (không cập nhật)

### Test 3: Admin Dashboard

1. Mở `admin_dashboard.html`
2. Click menu "User Tracking"
3. Xem card "Đã Cài App"
4. ✅ Kết quả mong đợi:
   - Hiển thị số lượng user đã cài app
   - Số này tăng khi có user mới đăng nhập lần đầu
   - Số này KHÔNG tăng khi user cũ đăng nhập lại

---

## Query Hữu Ích

### Xem Danh Sách User Đã Cài App
```sql
SELECT 
    Id,
    Username,
    Role,
    FirstLoginDate,
    LastActiveTime
FROM Users
WHERE HasInstalledApp = TRUE
ORDER BY FirstLoginDate DESC;
```

### Thống Kê Theo Ngày
```sql
SELECT 
    DATE(FirstLoginDate) as Date,
    COUNT(*) as NewInstalls
FROM Users
WHERE HasInstalledApp = TRUE
GROUP BY DATE(FirstLoginDate)
ORDER BY Date DESC;
```

### Thống Kê Tổng Quan
```sql
SELECT 
    COUNT(*) as TotalUsers,
    SUM(CASE WHEN HasInstalledApp = TRUE THEN 1 ELSE 0 END) as InstalledUsers,
    SUM(CASE WHEN HasInstalledApp = FALSE THEN 1 ELSE 0 END) as NotInstalledUsers,
    ROUND(SUM(CASE WHEN HasInstalledApp = TRUE THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2) as InstallRate
FROM Users;
```

### Reset Tracking (Để Test)
```sql
-- Reset tất cả user về chưa cài app
UPDATE Users SET HasInstalledApp = FALSE, FirstLoginDate = NULL;

-- Reset một user cụ thể
UPDATE Users SET HasInstalledApp = FALSE, FirstLoginDate = NULL WHERE Username = 'user123';
```

---

## Lưu Ý

### 1. Chỉ Tính Lần Đầu Tiên
- Hệ thống chỉ đánh dấu lần đăng nhập đầu tiên
- Các lần đăng nhập sau không ảnh hưởng đến `HasInstalledApp` và `FirstLoginDate`

### 2. Không Phân Biệt Platform
- Không phân biệt Windows, Android, iOS
- Chỉ quan tâm user đã đăng nhập từ app chưa

### 3. Admin User
- Admin cũng được tính nếu đăng nhập từ app
- Có thể filter theo `Role` nếu chỉ muốn đếm user thường

### 4. Backup Data
- Trước khi chạy migration, nên backup database
- Có thể rollback nếu cần:
  ```sql
  ALTER TABLE Users DROP COLUMN HasInstalledApp;
  ALTER TABLE Users DROP COLUMN FirstLoginDate;
  ALTER TABLE Users DROP INDEX idx_has_installed_app;
  ```

---

## Troubleshooting

### Lỗi: Column 'HasInstalledApp' doesn't exist

**Nguyên nhân:** Chưa chạy migration SQL

**Giải pháp:**
1. Chạy file `xampp_api/add_app_install_tracking.sql` trong phpMyAdmin
2. Hoặc chạy lệnh:
   ```sql
   ALTER TABLE Users ADD COLUMN HasInstalledApp BOOLEAN DEFAULT FALSE;
   ALTER TABLE Users ADD COLUMN FirstLoginDate DATETIME NULL;
   ```

### Số liệu không cập nhật trên Dashboard

**Nguyên nhân:** Cache hoặc API không trả về đúng

**Giải pháp:**
1. Hard refresh browser (Ctrl + F5)
2. Kiểm tra API response:
   ```
   http://localhost/vfg-api/api.php?action=getAppStats
   ```
3. Xem có `totalInstalled` trong response không

### Tất cả user đều hiển thị HasInstalledApp = FALSE

**Nguyên nhân:** User đăng nhập trước khi cập nhật code

**Giải pháp:**
- User cần đăng nhập lại 1 lần để được đánh dấu
- Hoặc update thủ công:
  ```sql
  UPDATE Users SET HasInstalledApp = TRUE, FirstLoginDate = NOW() 
  WHERE LastActiveTime IS NOT NULL;
  ```

---

## Tóm Tắt Files Thay Đổi

| File | Thay Đổi |
|------|----------|
| `xampp_api/add_app_install_tracking.sql` | ✅ Migration SQL (mới) |
| `xampp_api/api.php` | ✅ Endpoint `login` - Thêm tracking |
| `xampp_api/api.php` | ✅ Endpoint `getAppStats` - Thêm `totalInstalled` |
| `admin_dashboard.html` | ✅ Đã có sẵn card "Đã Cài App" |

---

## Kết Luận

Hệ thống tracking cài đặt app đã hoàn chỉnh:
- ✅ Tracking lần đăng nhập đầu tiên
- ✅ Lưu ngày đăng nhập đầu tiên
- ✅ Hiển thị thống kê trên admin dashboard
- ✅ Chỉ tính 1 lần duy nhất

Admin có thể xem:
- Tổng số user đã cài app
- Ngày cài app của từng user
- Tỷ lệ cài đặt (install rate)
