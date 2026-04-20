# 🎯 Hướng dẫn Test và Quản lý Admin

## 📍 PHẦN 1: Test Giọng Nói Chỉ Đường & Thuyết Minh

### Cách test:

1. **Mở app VietnamFoodGuide**
2. **Click vào 1 quán ăn** bất kỳ
3. **Chọn điểm xuất phát** (GPS/Tìm kiếm/Mặc định)
4. **Bấm "🚀 Bắt đầu"** để bắt đầu navigation
5. **Bấm nút "🚶 Test Mode"** ở góc trên bên phải
6. **App sẽ tự động giả lập di chuyển** theo route

### Kết quả:
- ✅ **Giọng nói chỉ đường**: Mỗi khi đến gần điểm rẽ, app sẽ đọc hướng dẫn
- ✅ **Thuyết minh tự động**: Khi đi gần quán ăn (20-40m), app sẽ thuyết minh về quán đó
- ✅ **Cập nhật real-time**: Bản đồ tự động zoom theo vị trí

### Tốc độ giả lập:
- Di chuyển: ~10m/giây (giống đi bộ)
- Cập nhật: Mỗi 1 giây

### Dừng test:
- Bấm nút **"⏹ Stop Test"** để dừng giả lập

---

## 👥 PHẦN 2: Admin Dashboard - Quản lý Người dùng

### Mở trang admin:

```bash
# Mở file trong trình duyệt
admin_dashboard_tracking.html
```

### Tính năng:

#### 1. **Thống kê tổng quan**
- 👥 Tổng số người dùng
- ✅ Số người đang online
- 📱 Tổng lượt quét QR code
- 🗺️ Số người đang dùng chỉ đường

#### 2. **Bản đồ Real-time**
- 🗺️ Hiển thị vị trí tất cả người dùng
- 🟢 Màu xanh: Online
- 🔴 Màu xám: Offline
- 📍 Click vào marker để xem chi tiết

#### 3. **Danh sách người dùng**
- Tên người dùng
- Vị trí GPS (lat, lng)
- Trạng thái (Online/Offline)
- Thời gian hoạt động cuối

#### 4. **Tự động cập nhật**
- Làm mới mỗi 5 giây
- Hoặc bấm nút "🔄 Làm mới"

---

## 🔗 PHẦN 3: Tích hợp API (Tương lai)

### Để tracking thực tế, cần:

#### 1. **Thêm API endpoint** trong `xampp_api/api.php`:

```php
// Lưu vị trí người dùng
if ($action === 'update_location') {
    $userId = $_POST['user_id'];
    $lat = $_POST['lat'];
    $lng = $_POST['lng'];
    $isNavigating = $_POST['is_navigating'] ?? 0;
    
    $stmt = $conn->prepare("
        INSERT INTO user_locations (user_id, latitude, longitude, is_navigating, updated_at)
        VALUES (?, ?, ?, ?, NOW())
        ON DUPLICATE KEY UPDATE 
            latitude = VALUES(latitude),
            longitude = VALUES(longitude),
            is_navigating = VALUES(is_navigating),
            updated_at = NOW()
    ");
    $stmt->bind_param("iddi", $userId, $lat, $lng, $isNavigating);
    $stmt->execute();
    
    echo json_encode(['success' => true]);
}

// Lấy danh sách người dùng và vị trí
if ($action === 'get_users_locations') {
    $result = $conn->query("
        SELECT u.id, u.username, u.email, 
               ul.latitude, ul.longitude, ul.is_navigating,
               ul.updated_at,
               TIMESTAMPDIFF(MINUTE, ul.updated_at, NOW()) as minutes_ago
        FROM users u
        LEFT JOIN user_locations ul ON u.id = ul.user_id
        WHERE ul.updated_at > DATE_SUB(NOW(), INTERVAL 1 HOUR)
        ORDER BY ul.updated_at DESC
    ");
    
    $users = [];
    while ($row = $result->fetch_assoc()) {
        $users[] = [
            'id' => $row['id'],
            'name' => $row['username'],
            'email' => $row['email'],
            'lat' => (float)$row['latitude'],
            'lng' => (float)$row['longitude'],
            'online' => $row['minutes_ago'] < 5,
            'navigating' => (bool)$row['is_navigating'],
            'lastSeen' => $row['updated_at']
        ];
    }
    
    echo json_encode(['users' => $users]);
}
```

#### 2. **Tạo bảng database**:

```sql
CREATE TABLE user_locations (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    latitude DECIMAL(10, 8) NOT NULL,
    longitude DECIMAL(11, 8) NOT NULL,
    is_navigating TINYINT(1) DEFAULT 0,
    updated_at DATETIME NOT NULL,
    UNIQUE KEY unique_user (user_id),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_updated_at (updated_at)
);

CREATE TABLE qr_scans (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT,
    scanned_at DATETIME NOT NULL,
    device_info VARCHAR(255),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_scanned_at (scanned_at)
);
```

#### 3. **Cập nhật MapWindow.xaml.cs** để gửi vị trí:

Thêm vào `OnWebMessage`:

```csharp
else if (type == "updateUserPosition")
{
    // Gửi vị trí lên server
    if (msg.TryGetValue("lat", out var latStr) && 
        msg.TryGetValue("lng", out var lngStr))
    {
        _ = UpdateUserLocationAsync(latStr, lngStr, isNavigating);
    }
}

private async Task UpdateUserLocationAsync(string lat, string lng, bool isNavigating)
{
    try
    {
        using var client = new HttpClient();
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("action", "update_location"),
            new KeyValuePair<string, string>("user_id", CurrentUserId.ToString()),
            new KeyValuePair<string, string>("lat", lat),
            new KeyValuePair<string, string>("lng", lng),
            new KeyValuePair<string, string>("is_navigating", isNavigating ? "1" : "0")
        });
        
        await client.PostAsync("http://localhost/xampp_api/api.php", content);
    }
    catch { }
}
```

#### 4. **Cập nhật admin_dashboard_tracking.html**:

Thay function `generateMockData()` bằng:

```javascript
async function fetchRealData() {
    try {
        const response = await fetch('http://localhost/xampp_api/api.php?action=get_users_locations');
        const data = await response.json();
        return data.users;
    } catch (error) {
        console.error('Lỗi lấy dữ liệu:', error);
        return generateMockData(); // Fallback
    }
}

async function refreshData() {
    const users = await fetchRealData();
    updateStats(users);
    updateMap(users);
    updateUserList(users);
}
```

---

## 📊 Thống kê QR Code

### Để tracking lượt quét QR:

1. **Tạo QR code** link đến app download
2. **Thêm tracking parameter**: `?ref=qr_code_001`
3. **Khi app mở lần đầu**, gửi request:

```csharp
private async Task TrackQRScanAsync(string refCode)
{
    using var client = new HttpClient();
    var content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("action", "track_qr_scan"),
        new KeyValuePair<string, string>("ref_code", refCode),
        new KeyValuePair<string, string>("device_info", GetDeviceInfo())
    });
    
    await client.PostAsync("http://localhost/xampp_api/api.php", content);
}
```

---

## 🎯 Tóm tắt

### ✅ Đã có:
1. Test Mode để giả lập di chuyển
2. Giọng nói chỉ đường tự động
3. Thuyết minh quán ăn tự động
4. Admin dashboard với mock data

### 🔄 Cần làm thêm (nếu muốn real tracking):
1. Tạo bảng database `user_locations` và `qr_scans`
2. Thêm API endpoints trong `api.php`
3. Cập nhật MapWindow.xaml.cs để gửi vị trí
4. Cập nhật admin dashboard để lấy dữ liệu thực

### 🚀 Cách test ngay:
1. Build app: `dotnet build VietnamFoodGuide/VietnamFoodGuide.csproj`
2. Chạy app và bấm Test Mode
3. Mở `admin_dashboard_tracking.html` trong browser
4. Xem bản đồ và thống kê

---

**Lưu ý**: Hiện tại admin dashboard dùng mock data. Để có dữ liệu thực, cần implement API như hướng dẫn ở PHẦN 3.
