# 🎯 BẮT ĐẦU TỪ 0 - Tracking Stats

## ✅ Dữ Liệu Mới

Database bây giờ **BẮT ĐẦU TỪ 0**:

```
📱 Quét QR Code: 0 người dùng
📲 Đã Cài App: 0 người dùng
🟢 Đang Online: 0 người dùng
🧭 Đang Chỉ Đường: 0 người dùng
```

## 🚀 Khi Nào Số Tăng Lên?

### **1. Quét QR Code (+1)**
Khi user quét QR code lần đầu:
```
📱 Quét QR Code: 0 → 1
```

### **2. Đã Cài App (+1)**
Khi user cài app và mở lần đầu:
```
📲 Đã Cài App: 0 → 1
```

### **3. Đang Online (+1)**
Khi user đăng nhập thành công:
```
🟢 Đang Online: 0 → 1
```

### **4. Đang Chỉ Đường (+1)**
Khi user đang online VÀ bắt đầu navigate:
```
🧭 Đang Chỉ Đường: 0 → 1
```

## 📊 Ví Dụ Thực Tế

### **Scenario 1: User Đăng Nhập Lần Đầu**
```
Bước 1: User mở app
→ 📱 Quét QR Code: 0
→ 📲 Đã Cài App: 0
→ 🟢 Đang Online: 0
→ 🧭 Đang Chỉ Đường: 0

Bước 2: User đăng nhập với username "user123"
→ 📱 Quét QR Code: 0
→ 📲 Đã Cài App: 0
→ 🟢 Đang Online: 1 ✅ (user123 đã đăng nhập)
→ 🧭 Đang Chỉ Đường: 0

Bước 3: User click vào quán ăn và bắt đầu chỉ đường
→ 📱 Quét QR Code: 0
→ 📲 Đã Cài App: 0
→ 🟢 Đang Online: 1
→ 🧭 Đang Chỉ Đường: 1 ✅ (user123 đang navigate)
```

### **Scenario 2: User Quét QR**
```
Bước 1: User quét QR code từ poster
→ 📱 Quét QR Code: 1 ✅ (user mới quét QR)
→ 📲 Đã Cài App: 0
→ 🟢 Đang Online: 0
→ 🧭 Đang Chỉ Đường: 0

Bước 2: User cài app từ link QR
→ 📱 Quét QR Code: 1
→ 📲 Đã Cài App: 1 ✅ (user đã cài app)
→ 🟢 Đang Online: 0
→ 🧭 Đang Chỉ Đường: 0

Bước 3: User mở app và đăng nhập
→ 📱 Quét QR Code: 1
→ 📲 Đã Cài App: 1
→ 🟢 Đang Online: 1 ✅ (user đã đăng nhập)
→ 🧭 Đang Chỉ Đường: 0
```

### **Scenario 3: Nhiều Users**
```
User A: Đăng nhập + Navigate
User B: Đăng nhập (không navigate)
User C: Chưa đăng nhập

Kết quả:
→ 📱 Quét QR Code: 0 (chưa ai quét)
→ 📲 Đã Cài App: 0 (chưa ai cài)
→ 🟢 Đang Online: 2 (User A + User B)
→ 🧭 Đang Chỉ Đường: 1 (chỉ User A)
```

## 🎓 Dữ Liệu Trong Database

### **Users Table (Sau khi setup)**
| Username | QRScanned | AppInstalled | LastActiveTime | Status |
|----------|-----------|--------------|----------------|--------|
| admin | ❌ FALSE | ❌ FALSE | NULL | ⚫ Chưa login |
| user123 | ❌ FALSE | ❌ FALSE | NULL | ⚫ Chưa login |
| testuser | ❌ FALSE | ❌ FALSE | NULL | ⚫ Chưa login |
| khach001 | ❌ FALSE | ❌ FALSE | NULL | ⚫ Chưa login |
| khach002 | ❌ FALSE | ❌ FALSE | NULL | ⚫ Chưa login |
| khach003 | ❌ FALSE | ❌ FALSE | NULL | ⚫ Chưa login |
| nguoiyeuthich | ❌ FALSE | ❌ FALSE | NULL | ⚫ Chưa login |
| dukhach01 | ❌ FALSE | ❌ FALSE | NULL | ⚫ Chưa login |

### **UserTracking Table (Sau khi setup)**
```
(Trống - chưa có ai tracking)
```

## 🔄 Cách Dữ Liệu Được Cập Nhật

### **1. Khi User Đăng Nhập**
```php
// API: login
UPDATE Users SET LastActiveTime = NOW() WHERE Id = ?
```
→ Stats "Đang Online" tăng lên

### **2. Khi User Quét QR**
```php
// API: saveQRScan
UPDATE Users SET QRScanned = TRUE WHERE Id = ?
```
→ Stats "Quét QR Code" tăng lên

### **3. Khi User Cài App**
```php
// API: updateUserActivity
UPDATE Users SET AppInstalled = TRUE WHERE Id = ?
```
→ Stats "Đã Cài App" tăng lên

### **4. Khi User Bắt Đầu Navigate**
```php
// API: updateTracking
INSERT/UPDATE UserTracking SET IsNavigating = TRUE WHERE UserId = ?
```
→ Stats "Đang Chỉ Đường" tăng lên

### **5. Khi User Offline (> 5 phút không active)**
```
LastActiveTime cũ hơn 5 phút
```
→ Stats "Đang Online" giảm xuống
→ Stats "Đang Chỉ Đường" giảm xuống (nếu đang navigate)

## ✅ Kết Quả Sau Khi Setup

```sql
✅ Setup hoàn tất - Bắt đầu từ 0!

👥 Users: 8 tài khoản
📱 QR Scanned: 0 người dùng (bắt đầu từ 0)
📲 App Installed: 0 người dùng (bắt đầu từ 0)
🟢 Currently Online: 0 người dùng (bắt đầu từ 0)
🧭 Currently Navigating: 0 người dùng (bắt đầu từ 0)
🍜 Foods: 11 quán ăn
❤️ Favorites: 15 yêu thích
📍 Tracking Active: 0 phiên (bắt đầu từ 0)
📱 QR Scans: 0 lượt quét (bắt đầu từ 0)
```

## 🧪 Test Ngay

### **Test 1: Đăng Nhập**
1. Mở app
2. Đăng nhập với `user123` / `user123`
3. Mở Admin Dashboard → User Tracking
4. Xem stats: **🟢 Đang Online: 1**

### **Test 2: Navigate**
1. Trong app, click vào một quán ăn
2. Click "Chỉ Đường"
3. Mở Admin Dashboard → User Tracking
4. Xem stats: **🧭 Đang Chỉ Đường: 1**

### **Test 3: Offline**
1. Đóng app hoặc đợi > 5 phút
2. Mở Admin Dashboard → User Tracking
3. Xem stats: **🟢 Đang Online: 0**

---

**File cần chạy:** `xampp_api/setup_complete.sql`
**Trạng thái ban đầu:** TẤT CẢ = 0
**Cập nhật:** Tự động khi user thực hiện hành động
