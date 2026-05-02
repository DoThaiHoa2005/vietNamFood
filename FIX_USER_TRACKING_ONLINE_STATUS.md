# ✅ FIX: User Tracking - Hiển Thị Trạng Thái Online

## 🎯 Yêu Cầu
User muốn:
1. **BỎ** card "Đang Online" trong Dashboard (đã xong trước đó)
2. **HIỂN THỊ** trạng thái online trong User Tracking section
3. **CHỈ HIỂN THỊ** users đã đăng nhập (LastActiveTime < 5 phút)
4. **HIỂN THỊ** vị trí thực tế của user từ bảng UserTracking

## ✅ Đã Sửa

### 1. **Thêm Logic Kiểm Tra Online Status**
```javascript
// Helper function để check user có online không
function isUserOnline(lastActiveTime) {
    if (!lastActiveTime) return false;
    const lastActive = new Date(lastActiveTime);
    const now = new Date();
    const diffMinutes = (now - lastActive) / 1000 / 60;
    return diffMinutes < 5; // Online nếu active trong 5 phút
}
```

### 2. **Merge Users với Tracking Data**
```javascript
// Trong loadTrackingData(), merge users với tracking
const mergedData = tracking.map(track => {
    const user = users.find(u => u.Id === track.UserId);
    return {
        ...track,
        Username: user ? user.Username : `User ${track.UserId}`,
        LastActiveTime: user ? user.LastActiveTime : null,
        isOnline: user && user.LastActiveTime ? isUserOnline(user.LastActiveTime) : false
    };
});
```

### 3. **Cập Nhật User List - Chỉ Hiển Thị Online Users**
```javascript
function updateTrackingUserList(mergedData) {
    // Chỉ hiển thị users đã đăng nhập (online)
    const onlineUsers = mergedData.filter(t => t.isOnline && t.IsActive);
    
    // Hiển thị với:
    // - 🟢 Green dot indicator
    // - "Online" badge
    // - Vị trí thực tế (CurrentLat, CurrentLng)
    // - Thời gian cập nhật tracking
    // - Thời gian hoạt động cuối (LastActiveTime)
    // - Điểm đến (nếu đang navigate)
}
```

### 4. **Cập Nhật Map - Green Markers cho Online Users**
```javascript
function updateTrackingMap(mergedData) {
    // Chỉ hiển thị markers cho online users
    const onlineUsers = mergedData.filter(t => t.isOnline && t.IsActive && t.CurrentLat && t.CurrentLng);
    
    // Marker màu xanh lá (#2d7a4f) thay vì đỏ
    // Popup hiển thị:
    // - 🟢 Green dot + "Online" badge
    // - Vị trí chính xác
    // - Thời gian cập nhật
    // - Thời gian hoạt động cuối
    // - Điểm đến (nếu có)
}
```

### 5. **Cập Nhật Stats - Đếm Đúng Online Users**
```javascript
function updateTrackingStats(mergedData, stats) {
    // Chỉ đếm users đã đăng nhập VÀ có tracking active
    const online = mergedData.filter(t => t.isOnline && t.IsActive).length;
    const navigating = mergedData.filter(t => t.isOnline && t.IsNavigating && t.IsActive).length;
}
```

## 📊 Hiển Thị Trong User Tracking

### **User List (Bên Trái)**
```
🟢 admin [Online]
📍 Vị trí: 10.77690, 106.70090
🕐 Cập nhật: 14:30:25
💚 Hoạt động: 14:30:20
🎯 Đang đi đến: Phở Đặc Biệt Trần Hưng Đạo
```

### **Map (Bên Phải)**
- **Green markers** (🟢) cho users đang online
- **Blue markers** (🎯) cho điểm đến
- **Dashed line** nối user với điểm đến
- **Popup** hiển thị đầy đủ thông tin khi click

## 🔄 Logic Online Status

### **User được coi là ONLINE khi:**
1. ✅ Đã đăng nhập (có session token)
2. ✅ `LastActiveTime` < 5 phút (được cập nhật mỗi 30 giây từ app)
3. ✅ `UserTracking.IsActive = TRUE`

### **User được coi là OFFLINE khi:**
1. ❌ Chưa đăng nhập
2. ❌ `LastActiveTime` > 5 phút (không còn active)
3. ❌ `UserTracking.IsActive = FALSE`

## 📱 Cách Hoạt Động

### **Từ App (C#)**
```csharp
// Mỗi 30 giây, app gọi API updateActivity
POST /api.php?action=updateActivity
{
    "token": "session_token_here"
}

// API sẽ cập nhật Users.LastActiveTime = NOW()
```

### **Từ Admin Dashboard**
```javascript
// Mỗi 5 giây, dashboard tự động refresh
setInterval(loadTrackingData, 5000);

// Load data:
// 1. Lấy Users (có LastActiveTime)
// 2. Lấy UserTracking (có vị trí)
// 3. Merge và check online status
// 4. Hiển thị chỉ users online
```

## 🎨 Màu Sắc

| Trạng Thái | Màu | Hex Code |
|-----------|-----|----------|
| Online | 🟢 Xanh lá | #2d7a4f |
| Offline | ⚪ Xám | #999999 |
| Destination | 🔵 Xanh dương | #457B9D |
| Route Line | 🟢 Xanh lá | #2d7a4f |

## 📝 Lưu Ý

1. **Dashboard Stats**: Card "Đang Online" đã bị xóa, chỉ hiển thị trong User Tracking section
2. **Auto Refresh**: Tracking data tự động refresh mỗi 5 giây
3. **Session Timeout**: Session hết hạn sau 7 ngày, nhưng online status chỉ check 5 phút
4. **Activity Update**: App phải gọi `updateActivity` mỗi 30 giây để duy trì online status

## ✅ Kết Quả

- ✅ Chỉ hiển thị users đã đăng nhập trong User Tracking
- ✅ Green dot (🟢) và "Online" badge cho users online
- ✅ Hiển thị vị trí thực tế từ UserTracking table
- ✅ Hiển thị thời gian hoạt động cuối (LastActiveTime)
- ✅ Hiển thị điểm đến nếu user đang navigate
- ✅ Map với green markers cho online users
- ✅ Stats đếm đúng số users online

## 🚀 Test

1. Đăng nhập vào app với tài khoản `admin` / `admin123`
2. Mở Admin Dashboard → User Tracking
3. Sẽ thấy user `admin` hiển thị với:
   - 🟢 Green dot
   - "Online" badge
   - Vị trí thực tế
   - Thời gian hoạt động
4. Nếu đóng app hoặc không active > 5 phút → user biến mất khỏi list

---

**File đã sửa:** `admin_dashboard.html`
**Ngày sửa:** 2026-04-30
**Trạng thái:** ✅ HOÀN THÀNH
