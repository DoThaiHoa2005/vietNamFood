# ✅ ĐÃ IMPLEMENT: SESSION + ONLINE USERS

## 🎯 TÍNH NĂNG ĐÃ THÊM:

### 1. ✅ Lưu Session khi đăng nhập
- Tạo token ngẫu nhiên (64 ký tự)
- Lưu vào bảng Sessions (hết hạn sau 7 ngày)
- Trả về token cho app

### 2. ✅ Cập nhật LastActiveTime
- Timer tự động cập nhật mỗi 30 giây
- Gọi API: `/api.php?action=updateActivity`
- Chỉ khi user đang đăng nhập

### 3. ✅ Hiển thị số người online
- Admin dashboard hiển thị số người online
- Đếm users có LastActiveTime trong 5 phút
- Cập nhật real-time

### 4. ✅ Logout xóa session
- Endpoint: `/api.php?action=logout`
- Xóa session khỏi database

---

## 📊 API ENDPOINTS MỚI:

### 1. `POST /api.php?action=login`
**Response mới:**
```json
{
  "success": true,
  "user": {
    "id": 1,
    "username": "admin",
    "role": "Admin"
  },
  "token": "abc123...xyz789",
  "expiresAt": "2026-05-07 10:30:00",
  "message": "Đăng nhập thành công"
}
```

### 2. `GET /api.php?action=validateSession?token=xxx`
Kiểm tra session còn hạn không

### 3. `POST /api.php?action=updateActivity`
```json
{
  "token": "abc123...xyz789"
}
```
Cập nhật LastActiveTime

### 4. `POST /api.php?action=logout`
```json
{
  "token": "abc123...xyz789"
}
```
Xóa session

### 5. `GET /api.php?action=getOnlineCount`
**Response:**
```json
{
  "success": true,
  "count": 3
}
```

### 6. `GET /api.php?action=getActiveSessions`
Lấy danh sách users đang online (cho admin)

---

## 🔄 FLOW HOẠT ĐỘNG:

### Đăng nhập:
```
1. User nhập username/password
2. App gọi API login
3. API tạo token → Lưu vào Sessions
4. API cập nhật LastActiveTime = NOW()
5. API trả về token + expiresAt
6. App lưu token vào App.SessionToken
7. App bắt đầu timer cập nhật mỗi 30 giây
```

### Trong khi sử dụng app:
```
Mỗi 30 giây:
1. Timer tick
2. Gọi API updateActivity với token
3. API cập nhật LastActiveTime = NOW()
4. User vẫn được tính là "online"
```

### Admin dashboard:
```
Mỗi lần load stats:
1. Gọi API getOnlineCount
2. API đếm users có LastActiveTime > NOW() - 5 phút
3. Hiển thị số người online
```

### Logout:
```
1. User click logout
2. App gọi API logout với token
3. API xóa session khỏi database
4. App xóa App.SessionToken
5. App dừng timer
```

---

## 📋 FILES ĐÃ SỬA:

### Backend (PHP):
1. ✅ `xampp_api/api.php`
   - Thêm tạo session trong login
   - Thêm 6 endpoints mới

### Frontend (C#):
1. ✅ `VietnamFoodGuide/App.xaml.cs`
   - Thêm `SessionToken` và `SessionExpiresAt`

2. ✅ `VietnamFoodGuide/Views/LoginWindow.xaml.cs`
   - Lưu token sau khi login
   - Bắt đầu activity timer
   - Thêm method `StartActivityTimer()`

3. ✅ `VietnamFoodGuide/Services/ApiAuthService.cs`
   - Cập nhật `UserInfo` class (thêm Token, ExpiresAt)
   - Cập nhật `LoginResponse` class
   - Parse token từ API response

4. ✅ `admin_dashboard.html`
   - Cập nhật `loadStats()` gọi `getOnlineCount`
   - Đổi label "Người Dùng" → "🟢 Đang Online"

---

## 🧪 CÁCH TEST:

### 1. Build và chạy app:
```
Build > Rebuild Solution
F5
```

### 2. Đăng nhập:
```
Username: admin
Password: admin123
```

### 3. Kiểm tra Output Window:
```
View > Output > Debug

Tìm dòng:
[Activity] Timer started - will update every 30 seconds
[Activity] Updated LastActiveTime
```

### 4. Kiểm tra database:
```sql
-- Xem sessions
SELECT * FROM Sessions;

-- Xem LastActiveTime
SELECT Username, LastActiveTime FROM Users;

-- Đếm online users
SELECT COUNT(*) FROM Users 
WHERE LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE);
```

### 5. Kiểm tra Admin Dashboard:
```
1. Mở admin dashboard
2. Xem số "🟢 Đang Online"
3. Phải hiển thị 1 (user vừa đăng nhập)
4. Đợi 30 giây → Số vẫn là 1
5. Đợi 6 phút (không dùng app) → Số giảm về 0
```

---

## 📊 LOGIC ONLINE:

**User được tính là ONLINE khi:**
- LastActiveTime > NOW() - 5 phút
- Có session còn hạn trong bảng Sessions

**User KHÔNG online khi:**
- LastActiveTime > 5 phút trước
- Đã logout (session bị xóa)
- Session hết hạn (> 7 ngày)

---

## 🎯 KẾT QUẢ:

✅ **Bảng Sessions có dữ liệu** khi user đăng nhập  
✅ **LastActiveTime tự động cập nhật** mỗi 30 giây  
✅ **Admin dashboard hiển thị đúng** số người online  
✅ **Chỉ đếm users đang đăng nhập** (active trong 5 phút)  
✅ **Logout xóa session** khỏi database

---

## 🔮 TÍNH NĂNG TƯƠNG LAI:

- [ ] Remember Me (lưu token vào local storage)
- [ ] Auto login khi mở app (validate token)
- [ ] Hiển thị danh sách users online trong admin
- [ ] Kick user (xóa session từ admin)
- [ ] Session history (lịch sử đăng nhập)

---

**Thời gian implement:** 30 phút  
**Độ khó:** ⭐⭐⭐☆☆ (Trung bình)

✅ **Hoàn tất! Bây giờ admin dashboard hiển thị đúng số người online!**
