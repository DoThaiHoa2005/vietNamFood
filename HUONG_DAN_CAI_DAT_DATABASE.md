# 🗄️ HƯỚNG DẪN CÀI ĐẶT DATABASE

## 📋 Tổng Quan

File này hướng dẫn cài đặt lại database VietnamFoodGuide với bảng UserTracking mới để hỗ trợ tính năng theo dõi người dùng trong Admin Dashboard.

---

## ⚡ CÀI ĐẶT NHANH (5 BƯỚC)

### Bước 1: Mở XAMPP Control Panel
- Khởi động **Apache** và **MySQL**
- Đảm bảo cả 2 service đang chạy (màu xanh)

### Bước 2: Mở phpMyAdmin
- Mở trình duyệt
- Truy cập: `http://localhost/phpmyadmin`

### Bước 3: Mở File SQL
- Mở file `xampp_api/setup_with_tracking.sql` bằng Notepad hoặc VS Code
- Nhấn **Ctrl+A** để chọn toàn bộ nội dung
- Nhấn **Ctrl+C** để copy

### Bước 4: Thực Thi SQL
- Trong phpMyAdmin, click tab **SQL** ở menu trên
- Nhấn **Ctrl+V** để paste toàn bộ nội dung đã copy
- Click nút **Go** (hoặc **Thực hiện**) ở góc dưới bên phải

### Bước 5: Kiểm Tra Kết Quả
Bạn sẽ thấy thông báo thành công:
```
✅ Setup hoàn tất với User Tracking!
👥 Users: 3 accounts
🍜 Foods: 11 restaurants
📍 Tracking: 2 active sessions
```

---

## 📊 CẤU TRÚC DATABASE MỚI

### Các Bảng Chính

1. **Users** - Quản lý người dùng
   - Id, Username, PasswordHash, Role, CreatedDate

2. **Foods** - Danh sách quán ăn
   - Id, Name, City, Category, Description (VI/EN/CN), Latitude, Longitude, Rating, ImagePath

3. **Favorites** - Yêu thích của người dùng
   - Id, UserId, FoodId, CreatedDate

4. **Sessions** - Phiên đăng nhập
   - Id, UserId, Token, ExpiresAt, CreatedDate

5. **UserTracking** ⭐ MỚI - Theo dõi vị trí người dùng
   - Id, UserId, CurrentLat, CurrentLng
   - DestinationLat, DestinationLng, DestinationName
   - IsNavigating, IsActive, LastUpdate, CreatedDate

---

## 👤 TÀI KHOẢN MẶC ĐỊNH

Sau khi cài đặt, bạn có thể đăng nhập với các tài khoản sau:

| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Admin |
| user123 | user123 | User |
| testuser | user123 | User |

---

## 🧪 KIỂM TRA API ENDPOINTS

### 1. Test Kết Nối Cơ Bản
Mở trình duyệt và truy cập:
```
http://localhost/xampp_api/api.php?action=getUsers
```

Kết quả mong đợi:
```json
{
  "success": true,
  "data": [
    {"Id": 1, "Username": "admin", "Role": "Admin"},
    {"Id": 2, "Username": "user123", "Role": "User"},
    {"Id": 3, "Username": "testuser", "Role": "User"}
  ]
}
```

### 2. Test Tracking Data
```
http://localhost/xampp_api/api.php?action=getTracking
```

Kết quả mong đợi:
```json
{
  "success": true,
  "data": [
    {
      "Id": 1,
      "UserId": 2,
      "CurrentLat": 10.7769,
      "CurrentLng": 106.7009,
      "DestinationName": "Phở Đắc Biệt Trần Hưng Đạo",
      "IsNavigating": true,
      "IsActive": true
    }
  ]
}
```

### 3. Test Foods Data
```
http://localhost/vfg-api/api.php?action=foods
```

---

## 🔍 XEM DỮ LIỆU TRONG PHPMYADMIN

### Xem Bảng UserTracking
1. Trong phpMyAdmin, click vào database **VietnamFoodGuide** ở sidebar trái
2. Click vào bảng **UserTracking**
3. Click tab **Browse** để xem dữ liệu

Bạn sẽ thấy 2 user đang navigate (mock data để test):
- User 2: Đang đi đến "Phở Đắc Biệt Trần Hưng Đạo"
- User 3: Đang đi đến "Cà Phê Truyền Thống Sài Gòn"

### Xem Tất Cả Quán Ăn
1. Click vào bảng **Foods**
2. Click tab **Browse**
3. Bạn sẽ thấy 11 quán ăn ở Vĩnh Khánh

---

## 🚀 MỞ ADMIN DASHBOARD

Sau khi cài đặt database xong:

1. Mở file `admin_dashboard.html` bằng trình duyệt
2. Click menu **User Tracking** ở sidebar
3. Bạn sẽ thấy:
   - Thống kê: 2 người đang online, 2 người đang chỉ đường
   - Danh sách người dùng online ở bên trái
   - Bản đồ hiển thị vị trí và đường đi ở bên phải

---

## ❌ XỬ LÝ LỖI THƯỜNG GẶP

### Lỗi: "Access denied for user"
**Nguyên nhân:** Sai thông tin kết nối MySQL

**Giải pháp:**
1. Mở file `xampp_api/api.php`
2. Kiểm tra thông tin kết nối:
```php
$host = 'localhost';
$username = 'root';
$password = '';  // Mặc định XAMPP không có password
$database = 'VietnamFoodGuide';
```

### Lỗi: "Table doesn't exist"
**Nguyên nhân:** Chưa chạy file SQL hoặc chạy sai database

**Giải pháp:**
1. Chạy lại file `setup_with_tracking.sql`
2. Đảm bảo đã chọn đúng database VietnamFoodGuide

### Lỗi: "Cannot connect to MySQL"
**Nguyên nhân:** MySQL chưa khởi động

**Giải pháp:**
1. Mở XAMPP Control Panel
2. Click **Start** ở dòng MySQL
3. Đợi đến khi status chuyển sang màu xanh

### Lỗi: API trả về "404 Not Found"
**Nguyên nhân:** Sai đường dẫn API

**Giải pháp:**
1. Kiểm tra file `api.php` có tồn tại trong thư mục `xampp_api/`
2. Đảm bảo XAMPP Apache đang chạy
3. Thử truy cập: `http://localhost/xampp_api/api.php`

---

## 📝 GHI CHÚ QUAN TRỌNG

### Về Dữ Liệu Mock
- File SQL có sẵn 2 user đang navigate để test
- Dữ liệu này sẽ tự động cập nhật `LastUpdate` mỗi khi có thay đổi
- Trong production, dữ liệu này sẽ được app WPF cập nhật real-time

### Về Auto Refresh
- Admin Dashboard tự động làm mới dữ liệu tracking mỗi 5 giây
- Bạn cũng có thể click nút "Làm Mới" để cập nhật thủ công

### Về Bảo Mật
- Password trong database đã được hash bằng BCrypt
- Không thể xem password gốc trong phpMyAdmin
- Để đổi password, cần dùng API hoặc hash lại bằng BCrypt

---

## 🎯 BƯỚC TIẾP THEO

Sau khi cài đặt database thành công:

1. ✅ Test các API endpoints
2. ✅ Mở Admin Dashboard và kiểm tra User Tracking
3. ✅ Chạy app WPF để test real-time tracking
4. ✅ Kiểm tra dữ liệu cập nhật trong phpMyAdmin

---

## 💡 MẸO HỮU ÍCH

### Backup Database
Trước khi cài đặt lại, nên backup database cũ:
1. Trong phpMyAdmin, chọn database VietnamFoodGuide
2. Click tab **Export**
3. Click **Go** để tải file backup

### Reset Database
Nếu muốn reset về trạng thái ban đầu:
1. Chạy lại file `setup_with_tracking.sql`
2. Tất cả dữ liệu sẽ được reset về mặc định

### Thêm Mock Data
Để thêm user tracking giả để test:
```sql
INSERT INTO UserTracking (UserId, CurrentLat, CurrentLng, DestinationLat, DestinationLng, DestinationName, IsNavigating, IsActive) 
VALUES (2, 10.7800, 106.7050, 10.78567, 106.70189, 'Phở Đắc Biệt', TRUE, TRUE);
```

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề:
1. Kiểm tra lại từng bước trong hướng dẫn
2. Xem phần "Xử lý lỗi thường gặp"
3. Kiểm tra log trong XAMPP Control Panel
4. Đảm bảo tất cả service đang chạy

---

**Chúc bạn cài đặt thành công! 🎉**
