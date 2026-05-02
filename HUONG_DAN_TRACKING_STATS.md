# 📊 Hướng Dẫn: Tracking Stats Logic

## 🎯 Yêu Cầu Mới

User muốn các stats trong User Tracking section hiển thị **chính xác** dựa trên dữ liệu thực tế:

1. **Quét QR Code**: Chỉ đếm users đã thực sự quét QR (`QRScanned = TRUE`)
2. **Đã Cài App**: Chỉ đếm users đã thực sự cài app (`AppInstalled = TRUE`)
3. **Đang Online**: Chỉ đếm users đã đăng nhập và active trong 5 phút
4. **Đang Chỉ Đường**: Chỉ đếm users đang thực sự navigate (`IsNavigating = TRUE` AND online)

## ✅ Logic Đã Sửa

### 1. **Quét QR Code** (📱)
```sql
SELECT COUNT(*) FROM Users WHERE QRScanned = TRUE
```
- Chỉ đếm users có `QRScanned = 1` trong database
- Khi user quét QR lần đầu → set `QRScanned = TRUE`

### 2. **Đã Cài App** (📲)
```sql
SELECT COUNT(*) FROM Users WHERE AppInstalled = TRUE
```
- Chỉ đếm users có `AppInstalled = 1` trong database
- Khi user cài app và mở lần đầu → set `AppInstalled = TRUE`

### 3. **Đang Online** (🟢)
```javascript
const online = mergedData.filter(t => t.isOnline && t.IsActive).length;
```
- Đếm users có:
  - `LastActiveTime > NOW() - 5 minutes` (đã đăng nhập)
  - `UserTracking.IsActive = TRUE` (có tracking active)

### 4. **Đang Chỉ Đường** (🧭)
```javascript
const navigating = mergedData.filter(t => t.isOnline && t.IsNavigating && t.IsActive).length;
```
- Đếm users có:
  - `LastActiveTime > NOW() - 5 minutes` (đang online)
  - `UserTracking.IsNavigating = TRUE` (đang navigate)
  - `UserTracking.IsActive = TRUE` (tracking active)

## 📊 Dữ Liệu Mẫu Mới

### **Users Table**
| Username | QRScanned | AppInstalled | LastActiveTime | Trạng Thái |
|----------|-----------|--------------|----------------|------------|
| admin | ✅ TRUE | ✅ TRUE | NOW() | 🟢 Online |
| user123 | ✅ TRUE | ✅ TRUE | NOW() | 🟢 Online |
| testuser | ✅ TRUE | ❌ FALSE | 2 giờ trước | ⚪ Offline |
| khach001 | ✅ TRUE | ❌ FALSE | NULL | ⚪ Offline |
| khach002 | ❌ FALSE | ❌ FALSE | 30 phút trước | ⚪ Offline |
| khach003 | ❌ FALSE | ❌ FALSE | NULL | ⚪ Offline |
| nguoiyeuthich | ✅ TRUE | ✅ TRUE | 1 giờ trước | ⚪ Offline |
| dukhach01 | ✅ TRUE | ✅ TRUE | 3 phút trước | 🟢 Online |

### **UserTracking Table**
| UserId | Username | IsNavigating | IsActive | Trạng Thái |
|--------|----------|--------------|----------|------------|
| 2 | user123 | ✅ TRUE | ✅ TRUE | 🧭 Đang chỉ đường |
| 3 | testuser | ✅ TRUE | ✅ TRUE | ⚪ Offline (không đếm) |
| 8 | dukhach01 | ❌ FALSE | ✅ TRUE | 🟢 Online (không navigate) |

## 📈 Kết Quả Mong Đợi

Với dữ liệu mẫu trên, stats sẽ hiển thị:

```
📱 Quét QR Code: 6 người dùng
   (admin, user123, testuser, khach001, nguoiyeuthich, dukhach01)

📲 Đã Cài App: 4 người dùng
   (admin, user123, nguoiyeuthich, dukhach01)

🟢 Đang Online: 3 người dùng
   (admin, user123, dukhach01)

🧭 Đang Chỉ Đường: 1 người dùng
   (user123 - vì đang online VÀ IsNavigating = TRUE)
```

## 🔄 Cách Cập Nhật Dữ Liệu

### **Khi User Quét QR**
```php
// API endpoint: saveQRScan
UPDATE Users SET QRScanned = TRUE WHERE Id = ?
```

### **Khi User Cài App**
```php
// API endpoint: updateUserActivity
UPDATE Users SET AppInstalled = TRUE WHERE Id = ?
```

### **Khi User Đăng Nhập**
```php
// API endpoint: login
UPDATE Users SET LastActiveTime = NOW() WHERE Id = ?
```

### **Khi User Bắt Đầu Navigate**
```php
// API endpoint: updateTracking
UPDATE UserTracking SET IsNavigating = TRUE WHERE UserId = ?
```

### **Khi User Dừng Navigate**
```php
// API endpoint: updateTracking
UPDATE UserTracking SET IsNavigating = FALSE WHERE UserId = ?
```

## 🧪 Test Scenarios

### **Scenario 1: User Mới Quét QR**
1. User quét QR code lần đầu
2. API gọi `saveQRScan` → set `QRScanned = TRUE`
3. Stats "Quét QR Code" tăng lên +1

### **Scenario 2: User Cài App**
1. User cài app và mở lần đầu
2. API gọi `updateUserActivity` → set `AppInstalled = TRUE`
3. Stats "Đã Cài App" tăng lên +1

### **Scenario 3: User Đăng Nhập**
1. User đăng nhập thành công
2. API gọi `login` → set `LastActiveTime = NOW()`
3. Stats "Đang Online" tăng lên +1

### **Scenario 4: User Bắt Đầu Navigate**
1. User đang online và bắt đầu chỉ đường
2. API gọi `updateTracking` → set `IsNavigating = TRUE`
3. Stats "Đang Chỉ Đường" tăng lên +1

### **Scenario 5: User Offline**
1. User không active > 5 phút
2. `LastActiveTime` cũ hơn 5 phút
3. User biến mất khỏi "Đang Online" và "Đang Chỉ Đường"

## 📝 Lưu Ý Quan Trọng

1. **QRScanned và AppInstalled**: Là trạng thái **vĩnh viễn** (một khi TRUE thì không đổi lại FALSE)
2. **LastActiveTime**: Được cập nhật **mỗi 30 giây** khi app đang chạy
3. **IsNavigating**: Là trạng thái **tạm thời** (TRUE khi đang chỉ đường, FALSE khi dừng)
4. **Online Status**: Chỉ check **5 phút gần nhất** (không phải toàn bộ lịch sử)

## 🚀 Cách Setup Lại Database

**Chỉ cần chạy 1 file SQL duy nhất:**

```sql
-- File: xampp_api/setup_complete.sql
```

### **Cách chạy:**
1. Mở phpMyAdmin: `http://localhost/phpmyadmin`
2. Click tab **"SQL"**
3. Copy toàn bộ nội dung file `xampp_api/setup_complete.sql`
4. Paste vào ô SQL
5. Click **"Go"**
6. ✅ XONG!

File này đã bao gồm tất cả:
- Tạo database VietnamFoodGuide
- Tạo tất cả bảng
- Thêm dữ liệu mẫu đã được cấu hình đúng
- Không cần chạy file nào khác

---

**File đã sửa:**
- `admin_dashboard.html` - Cập nhật logic đếm navigating
- `xampp_api/setup_complete.sql` - Cập nhật dữ liệu mẫu

**Ngày sửa:** 2026-04-30
**Trạng Thái:** ✅ HOÀN THÀNH
