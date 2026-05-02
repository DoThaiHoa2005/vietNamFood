# ✅ FIX: Test Mode Tự Động Báo Đang Chỉ Đường

## 🎯 Yêu Cầu

Khi bấm nút **"🧪 Test Mode"** (di chuyển giả lập), app phải:
1. Tự động bật navigation (nếu chưa bật)
2. Gửi `IsNavigating = TRUE` lên server
3. Admin Dashboard hiển thị "Đang Chỉ Đường: 1"

## 🐛 Vấn Đề Trước Đây

Test Mode yêu cầu phải bật navigation trước:
```javascript
if(!isNavigating || !fullRouteCoords || fullRouteCoords.length === 0){
  alert('Vui lòng bắt đầu dẫn đường (🚀 Bắt đầu) trước khi bật Test Mode!');
  return;
}
```

→ User phải click 2 lần:
1. Click "🚀 Bắt đầu chỉ đường"
2. Click "🧪 Test Mode"

## ✅ Giải Pháp

Test Mode tự động bật navigation nếu chưa bật:

```javascript
function toggleTestMode(){
  testModeActive = !testModeActive;
  
  if(testModeActive){
    // Kiểm tra có điểm đến chưa
    if(!currentDest){
      alert('Vui lòng chọn điểm đến trước!');
      testModeActive = false;
      return;
    }
    
    // Tự động bật navigation nếu chưa bật
    if(!isNavigating){
      console.log('🧪 Test Mode: Tự động bật navigation');
      startNavigation();  // ← TỰ ĐỘNG BẬT
      
      // Đợi 1 giây để route được tính
      setTimeout(function(){
        if(!isNavigating || !fullRouteCoords || fullRouteCoords.length === 0){
          alert('Không thể tính route. Vui lòng thử lại!');
          testModeActive = false;
          return;
        }
      }, 1000);
    }
    
    // Bắt đầu Test Mode
    btn.innerHTML = '⏹ Stop Test';
    btn.style.background = '#f44336';
    console.log('🧪 Test Mode: bắt đầu giả lập di chuyển theo route');
    testRouteIndex = 0;
    userInteractedWithMap = false;
    
    // Di chuyển giả lập mỗi 1.2 giây
    testModeInterval = setInterval(function(){
      // Di chuyển theo route...
    }, 1200);
  }
}
```

## 🔄 Luồng Hoạt Động Mới

### **Khi User Click "🧪 Test Mode":**

1. **Kiểm tra điểm đến:**
   - Nếu chưa chọn điểm đến → Alert "Vui lòng chọn điểm đến trước!"
   - Nếu đã chọn → Tiếp tục

2. **Kiểm tra navigation:**
   - Nếu `isNavigating = false` → Tự động gọi `startNavigation()`
   - `startNavigation()` sẽ:
     - Tính route
     - Set `isNavigating = true`
     - Gửi message `navStateChanged` lên C#:
       ```javascript
       window.chrome.webview.postMessage(JSON.stringify({
         type: 'navStateChanged',
         navigating: true,
         destinationName: currentDest.name,
         destinationLat: currentDest.lat,
         destinationLng: currentDest.lng
       }));
       ```

3. **C# nhận message:**
   ```csharp
   _isNavigating = true;
   _destinationName = "Phở Đặc Biệt";
   _destinationLat = 10.78567;
   _destinationLng = 106.70189;
   ReportPositionToServer(_userLat, _userLng);
   ```

4. **Gửi lên server:**
   ```csharp
   POST /api.php?action=updateTracking
   {
     "userId": 2,
     "currentLat": 10.7769,
     "currentLng": 106.7009,
     "destinationLat": 10.78567,
     "destinationLng": 106.70189,
     "destinationName": "Phở Đặc Biệt",
     "isNavigating": true,
     "isActive": true
   }
   ```

5. **Database cập nhật:**
   ```sql
   UPDATE UserTracking SET
     IsNavigating = TRUE,
     DestinationLat = 10.78567,
     DestinationLng = 106.70189,
     DestinationName = 'Phở Đặc Biệt'
   WHERE UserId = 2
   ```

6. **Admin Dashboard refresh:**
   - Thấy "Đang Chỉ Đường: 1" ✅

### **Khi Test Mode Di Chuyển:**

- Mỗi 1.2 giây, marker di chuyển theo route
- Mỗi lần di chuyển, gọi `updateUserPosition(lat, lng)`
- C# nhận message và gọi `ReportPositionToServer()`
- Server cập nhật `CurrentLat`, `CurrentLng` (nhưng `IsNavigating` vẫn = TRUE)

### **Khi User Click "⏹ Stop Test":**

- Test Mode dừng
- **NHƯNG** navigation vẫn tiếp tục (không tự động dừng)
- Nếu muốn dừng navigation → Click "⏹ Dừng" ở nút navigation

## 🧪 Cách Test

### **Test 1: Bật Test Mode Khi Chưa Navigate**

1. Mở app, đăng nhập với `user123`
2. Click vào một quán ăn (ví dụ: Phở Đặc Biệt)
3. **KHÔNG** click "🚀 Bắt đầu chỉ đường"
4. Click "🧪 Test Mode" ngay
5. → App tự động bật navigation
6. → Marker bắt đầu di chuyển
7. Mở Admin Dashboard → Thấy "Đang Chỉ Đường: 1" ✅

### **Test 2: Bật Test Mode Khi Đã Navigate**

1. Mở app, đăng nhập với `user123`
2. Click vào một quán ăn
3. Click "🚀 Bắt đầu chỉ đường"
4. Click "🧪 Test Mode"
5. → Marker bắt đầu di chuyển
6. Mở Admin Dashboard → Thấy "Đang Chỉ Đường: 1" ✅

### **Test 3: Dừng Test Mode**

1. Trong khi Test Mode đang chạy
2. Click "⏹ Stop Test"
3. → Test Mode dừng
4. → Navigation vẫn tiếp tục (nút vẫn hiển thị "⏹ Dừng")
5. Admin Dashboard vẫn hiển thị "Đang Chỉ Đường: 1" ✅

### **Test 4: Dừng Navigation**

1. Click "⏹ Dừng" (nút navigation)
2. → Navigation dừng
3. → `IsNavigating = FALSE` gửi lên server
4. Admin Dashboard hiển thị "Đang Chỉ Đường: 0" ✅

## 📊 Kết Quả Mong Đợi

### **Trước Khi Sửa:**
```
User: Click "🧪 Test Mode"
App: ❌ Alert "Vui lòng bắt đầu dẫn đường trước!"
Admin Dashboard: Đang Chỉ Đường: 0
```

### **Sau Khi Sửa:**
```
User: Click "🧪 Test Mode"
App: ✅ Tự động bật navigation + di chuyển
Admin Dashboard: Đang Chỉ Đường: 1 ✅
```

## 📝 Lưu Ý

1. **Test Mode tự động bật navigation** - Không cần click "🚀 Bắt đầu" trước
2. **Dừng Test Mode ≠ Dừng Navigation** - Navigation vẫn tiếp tục sau khi dừng Test Mode
3. **Phải chọn điểm đến trước** - Không thể bật Test Mode nếu chưa chọn điểm đến

## 📂 Files Đã Sửa

1. ✅ `VietnamFoodGuide/Views/MapWindow.xaml.cs`
   - Sửa function `toggleTestMode()`
   - Tự động gọi `startNavigation()` nếu chưa bật

---

**Ngày sửa:** 2026-04-30
**Trạng thái:** ✅ HOÀN THÀNH
**Build:** ✅ Thành công
