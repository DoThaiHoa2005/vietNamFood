# 🍲 Vietnam Food Guide - Ứng Dụng Hướng Dẫn Ẩm Thực

## 📋 Giới thiệu

Ứng dụng hướng dẫn ẩm thực Việt Nam với:
- **WPF Desktop App** - Ứng dụng Windows
- **Web Admin Dashboard** - Quản lý dữ liệu
- **XAMPP API** - Backend PHP + MySQL

## 🚀 Cài Đặt Nhanh - 3 Bước

### Bước 1: Chạy XAMPP
```
Mở XAMPP Control Panel
→ Start Apache ✅
→ Start MySQL ✅
```

### Bước 2: Setup Database
```
1. Mở: http://localhost/phpmyadmin
2. Click tab "SQL"
3. Copy toàn bộ file: xampp_api/setup_complete.sql
4. Paste và click "Go"
```

### Bước 3: Đăng Nhập
```
Web Admin: http://localhost/admin_dashboard.html
WPF App: Chạy trong Visual Studio (F5)

Username: admin
Password: admin123
```

## 🔑 Tài Khoản Mặc Định

| Username | Password | Role  |
|----------|----------|-------|
| admin    | admin123 | Admin |
| user123  | admin123 | User  |

## ✨ Tính Năng

### Web Admin Dashboard
- ✅ Quản lý quán ăn (thêm/sửa/xóa)
- ✅ Upload hình ảnh
- ✅ Chỉnh sửa 3 ngôn ngữ (VI, EN, CN)
- ✅ Quản lý users
- ✅ Thống kê

### WPF Desktop App
- ✅ Đăng nhập từ XAMPP
- ✅ Xem danh sách quán
- ✅ Thuyết minh âm thanh (3 ngôn ngữ)
- ✅ Bản đồ với đường đi thực tế
- ✅ Tự động thuyết minh khi gần quán
- ✅ Nút chỉ đường và dừng chỉ đường
- ✅ Hoạt động offline

## 📁 Cấu Trúc Dự Án

```
VietnamFoodGuide/
├── VietnamFoodGuide/              # WPF App
│   ├── Views/                     # Các màn hình
│   ├── Services/                  # Các service
│   ├── Models/                    # Data models
│   └── Data/                      # Database context
│
├── xampp_api/                     # Backend API
│   ├── api.php                    # Main API
│   └── setup_complete.sql         # Database setup
│
├── admin_dashboard.html           # Web Admin
│
└── README.md                      # File này
```

## 📚 Tài Liệu

- `HUONG_DAN_DON_GIAN_NHAT.md` - Hướng dẫn đơn giản nhất
- `START_HERE.md` - Bắt đầu tại đây
- `HUONG_DAN_HOAN_CHINH.md` - Hướng dẫn chi tiết
- `TROUBLESHOOTING.md` - Khắc phục sự cố

## 🔧 Yêu Cầu Hệ Thống

- Windows 10+
- .NET Framework 4.8+
- Visual Studio 2019+
- XAMPP (Apache + MySQL + PHP)

## 🧪 Test Hệ Thống

### Test API:
```
http://localhost/vfg-api/api.php?action=users
```

### Test Web Admin:
```
http://localhost/admin_dashboard.html
```

### Test WPF App:
```
Chạy trong Visual Studio (F5)
```

## ❓ Troubleshooting

### Lỗi: "Không thể kết nối đến server"
→ Kiểm tra XAMPP đã chạy chưa

### Lỗi: "Mật khẩu không đúng"
→ Chạy lại file setup_complete.sql

### Lỗi: "Database không tồn tại"
→ Tạo database trong phpMyAdmin

## 📞 Hỗ Trợ

Nếu gặp vấn đề:
1. Đọc file `TROUBLESHOOTING.md`
2. Kiểm tra XAMPP logs
3. Xem Output trong Visual Studio

---

**Phát triển bởi: Vietnam Food Guide Team**
**Version: 1.0.0**
**License: MIT**
