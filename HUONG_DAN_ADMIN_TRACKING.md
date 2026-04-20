# 📊 HƯỚNG DẪN ADMIN DASHBOARD - USER TRACKING

## 🎯 Tính năng

Admin Dashboard cho phép theo dõi người dùng đang sử dụng app theo thời gian thực:

### 1. Thống kê tổng quan
- **Đang Online**: Số người dùng đang active
- **Tổng Người Dùng**: Tổng số tài khoản
- **Đang Chỉ Đường**: Số người đang navigation
- **Yêu Thích**: Tổng số quán được yêu thích

### 2. Danh sách người dùng
- Hiển thị tất cả người dùng đang online
- Vị trí hiện tại (lat, lng)
- Thời gian cập nhật cuối
- Điểm đến (nếu đang chỉ đường)

### 3. Bản đồ tracking
- Marker xanh: Vị trí người dùng
- Marker đỏ: Điểm đến
- Đường nét đứt: Route đang đi
- Click vào người dùng để focus

## 🚀 Cách sử dụng

### Bước 1: Mở Admin Dashboard
```
Mở file: admin_dashboard_tracking.html
```

### Bước 2: Kiểm tra API
Dashboard sẽ tự động kết nối đến:
```
http://localhost/xampp_api/api.php
```

### Bước 3: Xem dữ liệu
- Dashboard tự động refresh mỗi 5 giây
- Click "🔄 Làm Mới" để refresh thủ công
- Click vào người dùng trong danh sách để focus trên bản đồ

## 📡 API Endpoints

Dashboard sử dụng các API sau:

### 1. Get Users
```
GET /api.php?action=getUsers
```
Trả về danh sách tất cả người dùng

### 2. Get Sessions
```
GET /api.php?action=getSessions
```
Trả về session tracking (vị trí, điểm đến)

### 3. Get Favorites
```
GET /api.php?action=getFavorites
```
Trả về danh sách yêu thích

## 🔧 Cấu hình

### Thay đổi API URL
Sửa trong file `admin_dashboard_tracking.html`:
```javascript
const API_URL = 'http://localhost/xampp_api/api.php';
```

### Thay đổi thời gian refresh
```javascript
// Mặc định: 5 giây
setInterval(refreshData, 5000);

// Thay đổi thành 10 giây:
setInterval(refreshData, 10000);
```

## 📊 Dữ liệu hiển thị

### Session Data Structure
```json
{
  "session_id": 1,
  "user_id": 1,
  "current_lat": 10.7769,
  "current_lng": 106.7009,
  "destination_lat": 10.7850,
  "destination_lng": 106.7050,
  "current_destination": "Phở Thìn",
  "is_active": true,
  "last_update": "2024-01-15 10:30:00"
}
```

### User Data Structure
```json
{
  "user_id": 1,
  "username": "user123",
  "role": "user",
  "created_at": "2024-01-01 00:00:00"
}
```

## 🎨 Giao diện

### Màu sắc
- **Xanh (#1a73e8)**: Người dùng, primary
- **Đỏ (#ea4335)**: Điểm đến
- **Xanh lá (#34a853)**: Online status
- **Xám (#9aa0a6)**: Offline status

### Icons
- 👤 Người dùng
- 🎯 Điểm đến
- 📍 Vị trí
- 🕐 Thời gian
- 🔄 Làm mới

## ⚠️ Lưu ý

1. **API phải chạy**: Đảm bảo XAMPP đang chạy và API hoạt động
2. **CORS**: Nếu gặp lỗi CORS, thêm header trong `api.php`:
   ```php
   header('Access-Control-Allow-Origin: *');
   ```
3. **Dữ liệu thực**: Hiện tại dùng mock data, cần tích hợp với app thực tế
4. **Performance**: Với nhiều người dùng, cân nhắc tăng thời gian refresh

## 🔗 Tích hợp với App

Để app gửi dữ liệu tracking lên server:

### 1. Thêm API endpoint trong C#
```csharp
// Gửi vị trí hiện tại
public async Task UpdateLocation(double lat, double lng)
{
    var data = new {
        user_id = _currentUserId,
        lat = lat,
        lng = lng
    };
    
    await _httpClient.PostAsJsonAsync(
        "http://localhost/xampp_api/api.php?action=updateLocation",
        data
    );
}
```

### 2. Gọi định kỳ trong MapWindow
```csharp
private DispatcherTimer _trackingTimer = new DispatcherTimer();

private void StartTracking()
{
    _trackingTimer.Interval = TimeSpan.FromSeconds(5);
    _trackingTimer.Tick += async (s, e) => {
        await UpdateLocation(_userLat, _userLng);
    };
    _trackingTimer.Start();
}
```

## 🎉 Kết quả

Admin có thể:
- ✅ Xem tất cả người dùng đang online
- ✅ Theo dõi vị trí real-time
- ✅ Xem người dùng đang đi đâu
- ✅ Thống kê tổng quan
- ✅ Focus vào từng người dùng

Giống Google Maps Timeline cho admin!
