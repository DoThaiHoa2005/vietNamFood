# ✅ FIX: Đang Chỉ Đường Không Hiện Số

## 🐛 Vấn Đề

Khi user đang chỉ đường trong app, Admin Dashboard không hiển thị số "Đang Chỉ Đường" tăng lên.

## 🔍 Nguyên Nhân

App C# gửi API `updateTracking` nhưng **THIẾU** 2 thông tin quan trọng:
- ❌ `destinationLat` (vĩ độ điểm đến)
- ❌ `destinationLng` (kinh độ điểm đến)

API cần cả 2 thông tin này để lưu vào database.

## ✅ Đã Sửa

### **1. Thêm Biến Lưu Destination**

```csharp
// VietnamFoodGuide/Views/MapWindow.xaml.cs
private string _destinationName = "";
private double _destinationLat = 0;  // ← MỚI
private double _destinationLng = 0;  // ← MỚI
```

### **2. Gửi Destination Lat/Lng Lên Server**

```csharp
var data = new
{
    userId = user.Id,
    currentLat = lat,
    currentLng = lng,
    destinationLat = _destinationLat,  // ← MỚI
    destinationLng = _destinationLng,  // ← MỚI
    destinationName = _destinationName,
    isNavigating = _isNavigating,
    isActive = true
};
```

### **3. JavaScript Gửi Destination Khi Bắt Đầu Navigate**

```javascript
// Khi bắt đầu chỉ đường
window.chrome.webview.postMessage(JSON.stringify({
    type: 'navStateChanged',
    navigating: true,
    destinationName: currentDest.name,
    destinationLat: currentDest.lat,  // ← MỚI
    destinationLng: currentDest.lng   // ← MỚI
}));
```

### **4. JavaScript Gửi Message Khi Dừng Navigate**

```javascript
// Khi dừng chỉ đường
window.chrome.webview.postMessage(JSON.stringify({
    type: 'navStateChanged',
    navigating: false,
    destinationName: '',
    destinationLat: 0,  // ← MỚI
    destinationLng: 0   // ← MỚI
}));
```

### **5. C# Nhận Destination Lat/Lng Từ JavaScript**

```csharp
if (msg.TryGetValue("destinationLat", out var latStr) && double.TryParse(latStr, out var lat)) 
    _destinationLat = lat;
if (msg.TryGetValue("destinationLng", out var lngStr) && double.TryParse(lngStr, out var lng)) 
    _destinationLng = lng;
```

## 🔄 Luồng Hoạt Động

### **Khi User Bắt Đầu Chỉ Đường:**

1. User click "🚀 Bắt đầu chỉ đường" trong MapWindow
2. JavaScript gửi message `navStateChanged` với:
   - `navigating: true`
   - `destinationName: "Phở Đặc Biệt"`
   - `destinationLat: 10.78567`
   - `destinationLng: 106.70189`
3. C# nhận message và lưu vào biến:
   - `_isNavigating = true`
   - `_destinationName = "Phở Đặc Biệt"`
   - `_destinationLat = 10.78567`
   - `_destinationLng = 106.70189`
4. C# gọi `ReportPositionToServer()` gửi lên API
5. API lưu vào database:
   ```sql
   UPDATE UserTracking SET
       IsNavigating = TRUE,
       DestinationLat = 10.78567,
       DestinationLng = 106.70189,
       DestinationName = 'Phở Đặc Biệt'
   WHERE UserId = 2
   ```
6. Admin Dashboard refresh → Thấy "Đang Chỉ Đường: 1"

### **Khi User Dừng Chỉ Đường:**

1. User click "⏹ Dừng" trong MapWindow
2. JavaScript gửi message `navStateChanged` với:
   - `navigating: false`
   - `destinationName: ""`
   - `destinationLat: 0`
   - `destinationLng: 0`
3. C# nhận message và reset biến:
   - `_isNavigating = false`
   - `_destinationName = ""`
   - `_destinationLat = 0`
   - `_destinationLng = 0`
4. C# gọi `ReportPositionToServer()` gửi lên API
5. API cập nhật database:
   ```sql
   UPDATE UserTracking SET
       IsNavigating = FALSE,
       DestinationLat = NULL,
       DestinationLng = NULL,
       DestinationName = NULL
   WHERE UserId = 2
   ```
6. Admin Dashboard refresh → Thấy "Đang Chỉ Đường: 0"

## 🧪 Cách Test

### **1. Chạy Lại App**

```bash
# Build lại app
cd VietnamFoodGuide
dotnet build
```

### **2. Test Chỉ Đường**

1. Đăng nhập với tài khoản `user123`
2. Click vào một quán ăn
3. Click "🚀 Bắt đầu chỉ đường"
4. Mở Admin Dashboard (đăng nhập với `admin`)
5. Vào "User Tracking" section
6. Xem stats: **"Đang Chỉ Đường: 1"** ✅

### **3. Test Dừng Chỉ Đường**

1. Trong app, click "⏹ Dừng"
2. Refresh Admin Dashboard
3. Xem stats: **"Đang Chỉ Đường: 0"** ✅

### **4. Kiểm Tra Database**

```sql
-- Xem users đang navigate
SELECT 
    u.Username,
    t.IsNavigating,
    t.DestinationName,
    t.DestinationLat,
    t.DestinationLng,
    t.LastUpdate
FROM UserTracking t
JOIN Users u ON t.UserId = u.Id
WHERE t.IsNavigating = TRUE AND t.IsActive = TRUE;
```

## 📊 Kết Quả Mong Đợi

### **Trước Khi Sửa:**
```
🧭 Đang Chỉ Đường: 0  ← Luôn là 0 dù đang navigate
```

### **Sau Khi Sửa:**
```
🧭 Đang Chỉ Đường: 1  ← Hiển thị đúng khi đang navigate
```

## 📂 Files Đã Sửa

1. ✅ `VietnamFoodGuide/Views/MapWindow.xaml.cs`
   - Thêm biến `_destinationLat`, `_destinationLng`
   - Gửi destination lat/lng lên server
   - Nhận destination lat/lng từ JavaScript
   - Gửi message khi stop navigation

---

**Ngày sửa:** 2026-04-30
**Trạng thái:** ✅ HOÀN THÀNH
**Build:** ✅ Thành công
