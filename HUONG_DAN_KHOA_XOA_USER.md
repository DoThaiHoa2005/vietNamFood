# 🔐 HƯỚNG DẪN: Khóa & Xóa User

## ✅ Chức Năng Mới

### 1️⃣ **Khóa/Mở Khóa Tài Khoản**
- Tài khoản bị khóa **KHÔNG THỂ** đăng nhập
- Có thể khóa cả **Admin** và **User**
- Có thể mở khóa lại bất cứ lúc nào

### 2️⃣ **Xóa User**
- Chỉ xóa được tài khoản **User**
- **KHÔNG THỂ** xóa tài khoản **Admin**
- Xóa vĩnh viễn, không thể hoàn tác

## 🎯 Cách Sử Dụng

### **Trong Admin Dashboard**

1. Đăng nhập với tài khoản `admin`
2. Click vào menu **"Người Dùng"**
3. Xem danh sách users

### **Khóa Tài Khoản**

1. Click nút **🔒 Khóa** (màu đỏ) bên cạnh user
2. Xác nhận khóa
3. User sẽ không đăng nhập được nữa
4. Trạng thái hiển thị: **🔒 Locked** (màu đỏ)

### **Mở Khóa Tài Khoản**

1. Click nút **🔓 Mở khóa** (màu xanh) bên cạnh user bị khóa
2. Xác nhận mở khóa
3. User có thể đăng nhập lại
4. Trạng thái hiển thị: **✅ Active** (màu xanh)

### **Xóa User**

1. Click nút **🗑️ Xóa** (màu đỏ) bên cạnh user
2. Xác nhận xóa (cảnh báo: không thể hoàn tác!)
3. User bị xóa vĩnh viễn khỏi database
4. **LƯU Ý**: Chỉ hiển thị nút xóa cho **User**, không hiển thị cho **Admin**

## 📊 Hiển Thị Trong Bảng

| Username | Role | Trạng Thái | Đổi Role | Hành Động |
|----------|------|------------|----------|-----------|
| admin | Admin | ✅ Active | [Dropdown] | 🔒 Khóa |
| user123 | User | ✅ Active | [Dropdown] | 🔒 Khóa 🗑️ Xóa |
| khach01 | User | 🔒 Locked | [Dropdown] | 🔓 Mở khóa 🗑️ Xóa |

### **Màu Sắc**

- **Active** (✅): Màu xanh lá, tài khoản hoạt động bình thường
- **Locked** (🔒): Màu đỏ, tài khoản bị khóa, nền vàng nhạt

## 🔄 Khi User Bị Khóa

### **Khi Đăng Nhập**

User bị khóa sẽ thấy thông báo:
```
❌ Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.
```

### **Trong Database**

```sql
-- User bị khóa
UPDATE Users SET IsLocked = TRUE WHERE Username = 'user123';

-- User được mở khóa
UPDATE Users SET IsLocked = FALSE WHERE Username = 'user123';
```

## 🗑️ Khi Xóa User

### **Điều Kiện**

- ✅ Có thể xóa: Tài khoản **User**
- ❌ Không thể xóa: Tài khoản **Admin**

### **Khi Cố Xóa Admin**

Sẽ thấy thông báo:
```
❌ Không thể xóa tài khoản Admin. Chỉ có thể khóa.
```

### **Dữ Liệu Bị Xóa**

Khi xóa user, các dữ liệu liên quan cũng bị xóa (CASCADE):
- ✅ Sessions (phiên đăng nhập)
- ✅ Favorites (yêu thích)
- ✅ UserTracking (tracking)

## 📝 API Endpoints

### **1. Khóa/Mở Khóa User**

```http
PUT /api.php?action=toggle_user_lock
Content-Type: application/json

{
  "userId": 2
}
```

**Response:**
```json
{
  "success": true,
  "isLocked": true,
  "message": "Đã khóa tài khoản"
}
```

### **2. Xóa User**

```http
DELETE /api.php?action=delete_user&userId=2
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Đã xóa user thành công"
}
```

**Response (Error - Admin):**
```json
{
  "error": "Không thể xóa tài khoản Admin. Chỉ có thể khóa."
}
```

### **3. Check Locked Khi Login**

```http
POST /api.php?action=login
Content-Type: application/json

{
  "username": "user123",
  "password": "user123"
}
```

**Response (Locked):**
```json
{
  "error": "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên."
}
```

## 🧪 Test Scenarios

### **Scenario 1: Khóa User**

1. Admin khóa tài khoản `user123`
2. User123 cố đăng nhập
3. Thấy thông báo: "Tài khoản đã bị khóa"
4. Admin mở khóa lại
5. User123 đăng nhập thành công

### **Scenario 2: Xóa User**

1. Admin xóa tài khoản `user123`
2. User123 cố đăng nhập
3. Thấy thông báo: "Tài khoản không tồn tại"
4. Không thể khôi phục (đã xóa vĩnh viễn)

### **Scenario 3: Cố Xóa Admin**

1. Admin cố xóa tài khoản `admin`
2. Thấy thông báo: "Không thể xóa tài khoản Admin"
3. Chỉ có thể khóa admin, không xóa được

## 🔍 Kiểm Tra Trong Database

### **Xem Users Bị Khóa**

```sql
SELECT Username, Role, IsLocked, CreatedDate
FROM Users
WHERE IsLocked = TRUE;
```

### **Xem Tất Cả Users**

```sql
SELECT 
    Username, 
    Role, 
    CASE WHEN IsLocked = TRUE THEN '🔒 Locked' ELSE '✅ Active' END as Status,
    CreatedDate
FROM Users
ORDER BY Id;
```

## ⚠️ Lưu Ý Quan Trọng

1. **Admin không thể bị xóa** - Chỉ có thể khóa/mở khóa
2. **Xóa user là vĩnh viễn** - Không thể hoàn tác
3. **User bị khóa không thể đăng nhập** - Nhưng dữ liệu vẫn còn
4. **Xóa user sẽ xóa tất cả dữ liệu liên quan** - Favorites, Sessions, Tracking

## 📂 Files Đã Sửa

1. ✅ `xampp_api/setup_complete.sql` - Thêm cột `IsLocked`
2. ✅ `xampp_api/api.php` - Thêm 2 endpoints mới + check locked khi login
3. ✅ `admin_dashboard.html` - Thêm nút Khóa/Xóa + functions

---

**Ngày tạo:** 2026-04-30
**Trạng thái:** ✅ HOÀN THÀNH
