# 🧪 TEST ĐĂNG NHẬP

## ✅ KIỂM TRA ĐÃ LÀM

### 1. File api.php
- ✅ Tồn tại: `C:/xampp/htdocs/vfg-api/api.php`
- ✅ Có chức năng login (dòng 200-260)

### 2. API hoạt động
- ✅ URL: `http://localhost/vfg-api/api.php`
- ✅ Status: 200 OK
- ✅ Trả về dữ liệu JSON

### 3. File SQLite
- ✅ Đã tạo: `VietnamFoodGuide/bin/Debug/net48/Data/foods.db`
- ✅ App có thể hoạt động offline

---

## 🔍 NGUYÊN NHÂN KHÔNG ĐĂNG NHẬP ĐƯỢC

### Có thể do:

1. **Database chưa có users**
   - Bạn chưa chạy `setup_complete.sql`
   - Bảng Users trống

2. **Password hash sai**
   - Password trong database không đúng format BCrypt
   - Cần fix password hash

3. **XAMPP MySQL không chạy**
   - Chỉ bật Apache, chưa bật MySQL
   - API không kết nối được database

---

## ✅ CÁCH FIX

### Bước 1: Kiểm tra XAMPP
```
1. Mở XAMPP Control Panel
2. Kiểm tra:
   ✅ Apache: Running (màu xanh)
   ✅ MySQL: Running (màu xanh)
3. Nếu MySQL chưa chạy → Click "Start"
```

### Bước 2: Kiểm tra Database
```
1. Mở phpMyAdmin: http://localhost/phpmyadmin
2. Chọn database "VietnamFoodGuide"
3. Click bảng "Users"
4. Kiểm tra có users không?
   - Nếu KHÔNG → Chạy setup_complete.sql
   - Nếu CÓ → Kiểm tra PasswordHash
```

### Bước 3: Fix Password Hash (nếu cần)
```
Chạy trong phpMyAdmin:

-- Fix password cho admin
UPDATE Users 
SET PasswordHash = '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi' 
WHERE Username = 'admin';

-- Fix password cho user123
UPDATE Users 
SET PasswordHash = '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi' 
WHERE Username = 'user123';
```

### Bước 4: Test Login từ Browser
```
Mở browser và test:

URL: http://localhost/vfg-api/api.php?action=login
Method: POST
Body (JSON):
{
  "username": "admin",
  "password": "admin123"
}

Kết quả mong đợi:
{
  "success": true,
  "user": {
    "id": 1,
    "username": "admin",
    "role": "Admin"
  },
  "message": "Đăng nhập thành công"
}
```

---

## 🧪 TEST NHANH BẰNG POWERSHELL

```powershell
# Test login
$body = @{
    username = "admin"
    password = "admin123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost/vfg-api/api.php?action=login" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

---

## 📋 CHECKLIST

- [ ] XAMPP Apache đang chạy
- [ ] XAMPP MySQL đang chạy
- [ ] Database "VietnamFoodGuide" tồn tại
- [ ] Bảng Users có dữ liệu
- [ ] Password hash đúng format BCrypt
- [ ] API trả về status 200
- [ ] File api.php tồn tại trong C:/xampp/htdocs/vfg-api/

---

## 🆘 NẾU VẪN KHÔNG ĐĂNG NHẬP ĐƯỢC

### Kiểm tra Output Window trong Visual Studio:
```
1. Chạy app (F5)
2. View > Output
3. Tìm dòng log:
   [ApiFoodService] Loading foods from API...
   [LoginWindow] Login attempt...
4. Xem có lỗi gì không
```

### Kiểm tra API trực tiếp:
```
http://localhost/vfg-api/api.php?action=users

Kết quả mong đợi: Danh sách users
```

---

## 🎯 TÓM TẮT

**File cần thiết:**
- ✅ `api.php` - QUAN TRỌNG! Chứa tất cả API
- ✅ `utilities.php` - Công cụ phụ (không bắt buộc)

**Không xóa `api.php`!** File này cần thiết cho:
- Đăng nhập
- Đăng ký
- Lấy danh sách quán ăn
- Thêm/xóa yêu thích
- Tất cả chức năng của app

**`utilities.php` chỉ dùng để:**
- Kiểm tra database (command line)
- Test API (command line)
- Backup database (command line)
- KHÔNG phải API cho app!
