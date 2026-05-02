# 🧪 TEST ĐANG CHỈ ĐƯỜNG - HƯỚNG DẪN CHI TIẾT

## 📋 CHUẨN BỊ

### 1. Build lại app
```bash
cd VietnamFoodGuide
dotnet build
```

### 2. Đảm bảo XAMPP đang chạy
- ✅ Apache đang chạy
- ✅ MySQL đang chạy

### 3. Mở các công cụ kiểm tra
- 🌐 Browser: `http://localhost/vfg-api/check_tracking.php`
- 🌐 Browser: `http://localhost/vfg-api/admin_dashboard.html` (đăng nhập với `admin` / `admin123`)

---

## 🚀 BƯỚC 1: CHẠY APP VÀ ĐĂNG NHẬP

1. **Đóng tất cả app cũ** (nếu đang chạy)
2. **Run app mới** (F5 trong Visual Studio)
3. **Đăng nhập** với tài khoản `user123` / `user123`
4. **Mở Output window** trong Visual Studio (View → Output)

---

## 🧭 BƯỚC 2: BẮT ĐẦU CHỈ ĐƯỜNG

### 2.1. Trong App:
1. Click vào một quán ăn bất kỳ trên bản đồ
2. Click nút **"🚀 Bắt đầu chỉ đường"**

### 2.2. Kiểm tra Output Window:

Bạn phải thấy các dòng log sau (theo thứ tự):

```
📨 [C#] RAW MESSAGE: {"type":"navStateChanged","navigating":true,"destinationName":"Phở...","destinationLat":10.78567,"destinationLng":106.70189}
📋 [C#] Message type: navStateChanged
🧭 [C#] Nhận navStateChanged message!
🧭 [C#] Parsed navigating: True
🧭 [C#] destinationName: Phở Đặc Biệt Trần Hưng Đạo
🧭 [C#] destinationLat: 10.78567
🧭 [C#] destinationLng: 106.70189
🧭 [C#] Navigation state changed: True, Goal: Phở... (10.78567, 106.70189)
🚀 [C#] GỬI NGAY LẬP TỨC lên server (forceImmediate = true)
🔍 [C#] KIỂM TRA: _isNavigating = True, _destinationLat = 10.78567, _destinationLng = 106.70189
📤 [Tracking] JSON gửi đi: {"userId":2,"currentLat":10.78567,"currentLng":106.70189,"destinationLat":10.78567,"destinationLng":106.70189,"destinationName":"Phở...","isNavigating":true,"isActive":true}
📥 [Tracking] API response: {"success":true,"message":"Cập nhật tracking thành công"}
📡 [Tracking] Đã gửi vị trí User user123 (10.78567, 106.70189) lên server - Navigate: True (Force: True)
```

### ✅ CHECKLIST Output Window:
- [ ] Có dòng `🧭 [C#] Parsed navigating: True`
- [ ] Có dòng `📤 [Tracking] JSON gửi đi: {...,"isNavigating":true,...}`
- [ ] Có dòng `📥 [Tracking] API response: {"success":true,...}`
- [ ] **KHÔNG CÓ** dòng `❌ Exception` hoặc `❌ Lỗi`

---

## 🔍 BƯỚC 3: KIỂM TRA DATABASE

### 3.1. Mở check_tracking.php:
```
http://localhost/vfg-api/check_tracking.php
```

### 3.2. Kiểm tra bảng UserTracking:

Bạn phải thấy:

| UserId | Username | IsNavigating | IsActive | User Online? | Destination | DestLat | DestLng |
|--------|----------|--------------|----------|--------------|-------------|---------|---------|
| 2 | user123 | 🧭 TRUE | ✅ TRUE | 🟢 Online | Phở... | 10.78567 | 106.70189 |

### 3.3. Kiểm tra Kết Quả Đếm:

```
🧭 Đang Chỉ Đường: 1
```

### ✅ CHECKLIST Database:
- [ ] `IsNavigating = 🧭 TRUE` (không phải ❌ FALSE)
- [ ] `IsActive = ✅ TRUE`
- [ ] `User Online? = 🟢 Online`
- [ ] `DestinationLat` có giá trị (không phải 0 hoặc NULL)
- [ ] `DestinationLng` có giá trị (không phải 0 hoặc NULL)
- [ ] `DestinationName` có tên quán ăn (không phải rỗng)
- [ ] **Kết Quả Đếm: 🧭 Đang Chỉ Đường: 1**

---

## 📊 BƯỚC 4: KIỂM TRA ADMIN DASHBOARD

### 4.1. Mở Admin Dashboard:
```
http://localhost/vfg-api/admin_dashboard.html
```

### 4.2. Đăng nhập:
- Username: `admin`
- Password: `admin123`

### 4.3. Vào tab "User Tracking"

### 4.4. Kiểm tra stats:

Bạn phải thấy:

```
📊 Tracking Stats
🟢 Online: 1
🧭 Đang Chỉ Đường: 1
```

### 4.5. Kiểm tra bảng User Tracking:

Bạn phải thấy user `user123` với:
- Status: 🟢 Online
- Navigating: ✅ Yes
- Destination: Phở... (10.78567, 106.70189)

### ✅ CHECKLIST Admin Dashboard:
- [ ] Stats hiển thị: **"Đang Chỉ Đường: 1"**
- [ ] User `user123` có status 🟢 Online
- [ ] User `user123` có Navigating: ✅ Yes
- [ ] Destination có tên quán ăn và tọa độ

---

## ⏹️ BƯỚC 5: TEST DỪNG CHỈ ĐƯỜNG

### 5.1. Trong App:
1. Click nút **"⏹ Dừng"**

### 5.2. Kiểm tra Output Window:

Bạn phải thấy:

```
📨 [C#] RAW MESSAGE: {"type":"navStateChanged","navigating":false,"destinationName":"","destinationLat":0,"destinationLng":0}
📋 [C#] Message type: navStateChanged
🧭 [C#] Nhận navStateChanged message!
🧭 [C#] Parsed navigating: False
🧭 [C#] Navigation state changed: False, Goal:  (0.00000, 0.00000)
🚀 [C#] GỬI NGAY LẬP TỨC lên server (forceImmediate = true)
📤 [Tracking] JSON gửi đi: {"userId":2,"currentLat":...,"currentLng":...,"destinationLat":0,"destinationLng":0,"destinationName":"","isNavigating":false,"isActive":true}
📥 [Tracking] API response: {"success":true,"message":"Cập nhật tracking thành công"}
📡 [Tracking] Đã gửi vị trí User user123 (...) lên server - Navigate: False (Force: True)
```

### 5.3. Refresh check_tracking.php:

Bạn phải thấy:

| UserId | Username | IsNavigating | IsActive | User Online? | Destination | DestLat | DestLng |
|--------|----------|--------------|----------|--------------|-------------|---------|---------|
| 2 | user123 | ❌ FALSE | ✅ TRUE | 🟢 Online | | 0 | 0 |

```
🧭 Đang Chỉ Đường: 0
```

### 5.4. Refresh Admin Dashboard:

Stats phải hiển thị:

```
🟢 Online: 1
🧭 Đang Chỉ Đường: 0
```

### ✅ CHECKLIST Dừng Chỉ Đường:
- [ ] Output có dòng `🧭 [C#] Parsed navigating: False`
- [ ] Database: `IsNavigating = ❌ FALSE`
- [ ] Database: `Kết Quả Đếm: 🧭 Đang Chỉ Đường: 0`
- [ ] Admin Dashboard: **"Đang Chỉ Đường: 0"**

---

## 🐛 NẾU CÓ VẤN ĐỀ

### ❌ Vấn Đề 1: Output Window có JSON Exception

**Triệu chứng:**
```
❌ Exception thrown: 'System.Text.Json.JsonException'
```

**Nguyên nhân:** Code cũ chưa được build lại

**Giải pháp:**
1. Đóng app
2. Clean solution: `dotnet clean`
3. Build lại: `dotnet build`
4. Run lại (F5)

---

### ❌ Vấn Đề 2: IsNavigating vẫn là FALSE trong database

**Triệu chứng:**
- Output window có dòng `📤 [Tracking] JSON gửi đi: {...,"isNavigating":true,...}`
- Nhưng database vẫn hiển thị `IsNavigating = ❌ FALSE`

**Kiểm tra:**

1. **Kiểm tra API có nhận được request không:**
   - Mở `xampp_api/api.php`
   - Thêm log ở đầu case `updateTracking`:
   ```php
   case 'updateTracking':
       error_log("📥 [API] Nhận updateTracking request");
       $data = json_decode(file_get_contents('php://input'), true);
       error_log("📥 [API] Data: " . json_encode($data));
       // ... rest of code
   ```
   - Xem log trong `C:\xampp\apache\logs\error.log`

2. **Kiểm tra database connection:**
   - Mở `http://localhost/vfg-api/check_database.php`
   - Phải thấy "✅ Kết nối database thành công"

3. **Kiểm tra user có tồn tại không:**
   ```sql
   SELECT * FROM Users WHERE Username = 'user123';
   ```

---

### ❌ Vấn Đề 3: Admin Dashboard vẫn hiển thị 0

**Triệu chứng:**
- Database có `IsNavigating = TRUE`
- Nhưng Admin Dashboard vẫn hiển thị "Đang Chỉ Đường: 0"

**Kiểm tra:**

1. **Hard refresh Admin Dashboard:**
   - Ctrl + Shift + R (hoặc Ctrl + F5)

2. **Kiểm tra console trong browser:**
   - F12 → Console tab
   - Xem có lỗi JavaScript không

3. **Kiểm tra API getTracking:**
   - Mở `http://localhost/vfg-api/api.php?action=getTracking`
   - Phải thấy user với `IsNavigating: true`

4. **Kiểm tra logic đếm trong admin_dashboard.html:**
   - Mở F12 → Console
   - Gõ:
   ```javascript
   console.log('Tracking data:', window.trackingData);
   console.log('Navigating count:', window.trackingData.filter(t => t.isOnline && t.IsNavigating && t.IsActive).length);
   ```

---

## 📸 CHỤP MÀN HÌNH NẾU VẪN LỖI

Nếu sau khi làm theo tất cả các bước trên mà vẫn không được, hãy chụp màn hình:

1. **Output Window** (toàn bộ log từ lúc bấm "Bắt đầu")
2. **check_tracking.php** (toàn bộ trang)
3. **Admin Dashboard** (tab User Tracking)
4. **Browser Console** (F12 → Console tab trong Admin Dashboard)

Gửi cho tôi để tôi kiểm tra!

---

## ✅ KẾT QUẢ MONG ĐỢI

Nếu tất cả đều OK, bạn sẽ thấy:

### ✅ Output Window:
```
🧭 [C#] Parsed navigating: True
📤 [Tracking] JSON gửi đi: {...,"isNavigating":true,...}
📥 [Tracking] API response: {"success":true,...}
```

### ✅ Database (check_tracking.php):
```
IsNavigating = 🧭 TRUE
Kết Quả Đếm: 🧭 Đang Chỉ Đường: 1
```

### ✅ Admin Dashboard:
```
🧭 Đang Chỉ Đường: 1
```

---

**🎉 NẾU TẤT CẢ ĐỀU ✅ → HOÀN THÀNH!**

---

**Ngày tạo:** 2026-04-30  
**Mục đích:** Test fix "Đang Chỉ Đường" không hiện số
