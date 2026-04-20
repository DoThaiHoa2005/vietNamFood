# 🚀 Hướng dẫn Setup XAMPP cho Vietnam Food Guide

## Bước 1: Cài đặt XAMPP

1. Tải XAMPP từ: https://www.apachefriends.org/
2. Cài đặt XAMPP vào `C:\xampp\`
3. Mở XAMPP Control Panel

## Bước 2: Copy API vào htdocs

```bash
# Copy toàn bộ thư mục xampp_api vào htdocs
copy xampp_api C:\xampp\htdocs\vfg-api
```

Hoặc thủ công:
- Copy thư mục `xampp_api/` vào `C:\xampp\htdocs\`
- Đổi tên thành `vfg-api`

Cấu trúc sau khi copy:
```
C:\xampp\htdocs\vfg-api\
├── api.php
├── import_foods.sql
├── SETUP_XAMPP.md
└── uploads/  (sẽ tự tạo khi upload ảnh)
```

## Bước 3: Khởi động XAMPP

1. Mở **XAMPP Control Panel**
2. Click **Start** cho **Apache**
3. Click **Start** cho **MySQL**
4. Đợi đến khi cả 2 đều hiện màu xanh

## Bước 4: Import Database

### Cách 1: Dùng phpMyAdmin (Khuyến nghị)

1. Mở trình duyệt, truy cập: http://localhost/phpmyadmin
2. Click tab **SQL** ở trên cùng
3. Mở file `C:\xampp\htdocs\vfg-api\import_foods.sql` bằng Notepad
4. Copy toàn bộ nội dung
5. Paste vào ô SQL trong phpMyAdmin
6. Click nút **Go** (hoặc **Thực hiện**)
7. Đợi đến khi thấy thông báo "Query OK"

### Cách 2: Dùng MySQL Command Line

```bash
cd C:\xampp\mysql\bin
mysql -u root -p
# Nhấn Enter (không cần password mặc định)

# Trong MySQL prompt:
source C:/xampp/htdocs/vfg-api/import_foods.sql
exit
```

## Bước 5: Kiểm tra Database

1. Vào phpMyAdmin: http://localhost/phpmyadmin
2. Click vào database **VietnamFoodGuide** bên trái
3. Kiểm tra các bảng:
   - ✅ **Users** (2 rows: admin, user123)
   - ✅ **Foods** (11 rows: tất cả quán ăn)
   - ✅ **Favorites** (0 rows - trống)
   - ✅ **Sessions** (0 rows - trống)

## Bước 6: Test API

Mở trình duyệt và test các endpoint:

### Test 1: Lấy danh sách quán ăn
```
http://localhost/vfg-api/api.php?action=foods
```
Kết quả: JSON array với 11 quán ăn

### Test 2: Lấy thống kê
```
http://localhost/vfg-api/api.php?action=stats
```
Kết quả:
```json
{
  "totalFoods": 11,
  "totalCategories": 6,
  "avgRating": 4.6,
  "totalUsers": 2
}
```

### Test 3: Lấy danh sách users
```
http://localhost/vfg-api/api.php?action=users
```

## Bước 7: Mở Admin Dashboard

1. Mở file `admin_dashboard.html` bằng trình duyệt
2. Kiểm tra:
   - ✅ Trạng thái hiện "✅ Online" (màu xanh)
   - ✅ Tổng quán ăn: 11
   - ✅ Danh mục: 6
   - ✅ Đánh giá trung bình: ~4.6
3. Click tab **🍜 Quán Ăn**
4. Thấy bảng với 11 quán ăn có thumbnail ảnh
5. Click nút **✏️ Sửa** trên bất kỳ quán nào
6. Modal mở ra với đầy đủ thông tin:
   - Tên, thành phố, danh mục, rating, tọa độ
   - Ảnh preview
   - 3 tab ngôn ngữ: 🇻🇳 Tiếng Việt / 🇺🇸 English / 🇨🇳 中文

## Bước 8: Test chỉnh sửa

1. Click **✏️ Sửa** trên quán "Bánh Mì Trần Văn Hành"
2. Thay đổi rating từ 4.7 → 5.0
3. Chuyển sang tab **🇺🇸 English**
4. Sửa mô tả tiếng Anh
5. Click **💾 Lưu thay đổi**
6. Thấy toast "✅ Cập nhật thành công!"
7. Reload trang → rating đã đổi thành 5.0

## Bước 9: Test upload ảnh

1. Click **+ Thêm quán ăn mới**
2. Điền thông tin cơ bản
3. Kéo thả ảnh vào vùng upload (hoặc click chọn file)
4. Thấy preview ảnh ngay lập tức
5. Click **💾 Lưu thay đổi**
6. Ảnh được upload vào `C:\xampp\htdocs\vfg-api\uploads\`
7. URL ảnh: `http://localhost/vfg-api/uploads/food_xxxxx.jpg`

## ⚠️ Troubleshooting

### Lỗi: "Không thể tải dữ liệu"
- Kiểm tra Apache và MySQL đã Start chưa
- Kiểm tra URL API: http://localhost/vfg-api/api.php?action=foods
- Xem Console (F12) để check lỗi CORS hoặc 404

### Lỗi: "DB connection failed"
- MySQL chưa start
- Hoặc password root đã đổi → sửa trong `api.php` dòng `$pass = '';`

### Lỗi: Upload ảnh thất bại
- Kiểm tra quyền ghi folder `C:\xampp\htdocs\vfg-api\uploads\`
- Tạo thủ công folder `uploads` nếu chưa có

### Admin Dashboard hiện "⚠️ Offline"
- XAMPP chưa chạy
- Hoặc đường dẫn API sai → kiểm tra `const API_BASE` trong admin_dashboard.html
- Nếu vẫn offline, dashboard sẽ tự động fallback đọc `foods.json` local

## 🎯 Kết quả mong đợi

Sau khi setup xong:
- ✅ Admin dashboard kết nối được XAMPP MySQL
- ✅ Hiển thị 11 quán ăn với thumbnail
- ✅ Click Sửa → modal mở với 3 tab ngôn ngữ
- ✅ Chỉnh sửa description VI/EN/CN
- ✅ Upload ảnh mới hoặc nhập URL
- ✅ Lưu → dữ liệu cập nhật vào MySQL
- ✅ WPF app đọc từ MySQL (nếu có kết nối)

## 📝 Thông tin đăng nhập

### Admin Dashboard
- Không cần đăng nhập (public admin panel)

### MySQL
- Host: localhost
- Port: 3306
- User: root
- Password: (để trống)
- Database: VietnamFoodGuide

### phpMyAdmin
- URL: http://localhost/phpmyadmin
- User: root
- Password: (để trống)

## 🔗 Các URL quan trọng

- API Base: `http://localhost/vfg-api/api.php`
- phpMyAdmin: `http://localhost/phpmyadmin`
- Upload folder: `http://localhost/vfg-api/uploads/`
- Admin Dashboard: `file:///path/to/admin_dashboard.html`
