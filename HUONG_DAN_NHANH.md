# 🚀 HƯỚNG DẪN NHANH - VIETNAM FOOD GUIDE

## 📋 SETUP BAN ĐẦU (Chỉ làm 1 lần)

### **Bước 1: Setup Database**
```sql
1. Mở phpMyAdmin: http://localhost/phpmyadmin
2. Chọn database: VietnamFoodGuide
3. Click tab "SQL"
4. Copy toàn bộ nội dung file: xampp_api/setup_complete.sql
5. Paste vào và click "Go"
6. Xong! ✅
```

**Kết quả:**
- ✅ 2 users: admin, user123 (password: admin123, user123)
- ✅ 11 quán ăn ở Vĩnh Khánh
- ✅ Tất cả stats = 0 (bắt đầu từ 0)

---

### **Bước 2: Chạy App**
```bash
1. Mở Visual Studio
2. Build Solution (Ctrl+Shift+B)
3. Run (F5)
4. Đăng nhập: user123 / user123
```

---

### **Bước 3: Mở Admin Dashboard**
```
1. Mở browser: http://localhost/vfg-api/admin_dashboard.html
2. Xem Dashboard → Tất cả stats = 0
3. Xem Tracking → Chưa có ai online
```

---

## 🎯 CÁC TÍNH NĂNG CHÍNH

### **1. Khóa/Xóa User**
```
Admin Dashboard → Users
- Click 🔒 → Khóa user (không đăng nhập được)
- Click 🔓 → Mở khóa
- Click 🗑️ → Xóa user (chỉ User, không xóa Admin)
```

### **2. Theo Dõi Người Dùng**
```
Admin Dashboard → Tracking
- Quét QR Code: Số người đã quét QR
- Đã Cài App: Số người đã cài app
- Đang Online: Số người đang online (< 5 phút)
- Đang Chỉ Đường: Số người đang navigate
```

### **3. Test Mode**
```
App → Click quán ăn → Click "🧪 Test Mode"
→ Tự động bật navigation
→ Di chuyển theo route
→ Admin Dashboard hiển thị "Đang Chỉ Đường: 1"
```

---

## 🔧 DEBUG NHANH

### **Kiểm Tra Database**
```
http://localhost/vfg-api/check_tracking.php
→ Xem users online, tracking data, IsNavigating
```

### **Test Gửi Data Thủ Công**
```
http://localhost/vfg-api/test_tracking.php
→ Tạo test data
→ Refresh Admin Dashboard
→ Phải thấy số tăng
```

### **Xem Console Log**
```
Admin Dashboard → F12 → Console
→ Tìm log: 📊 [Tracking Stats]
→ Xem navigating: 1 (phải > 0)
```

---

## 📊 FLOW HOẠT ĐỘNG

### **Flow 1: User Đăng Nhập**
```
User login → App gửi updateActivity
→ API cập nhật LastActiveTime
→ Admin Dashboard đếm "Đang Online"
```

### **Flow 2: User Bắt Đầu Chỉ Đường**
```
User click "Bắt đầu" → startNavigation()
→ Gửi navStateChanged message
→ C# set _isNavigating = true
→ ReportPositionToServer() gửi isNavigating: true
→ API lưu IsNavigating = TRUE
→ Admin Dashboard đếm "Đang Chỉ Đường"
```

### **Flow 3: Test Mode**
```
User click "Test Mode" → toggleTestMode()
→ Kiểm tra currentDest
→ Nếu !isNavigating → startNavigation()
→ (Giống Flow 2)
```

---

## 🐛 TROUBLESHOOTING

### **Vấn đề: "Đang Chỉ Đường" = 0**
```
1. Mở check_tracking.php → Xem IsNavigating có = TRUE không?
2. Mở Console → Xem navigating có > 0 không?
3. Mở Output window → Xem "Navigate: True" có hiện không?
4. Nếu không → Rebuild app và thử lại
```

### **Vấn đề: Không khóa được user**
```
1. Kiểm tra database có cột IsLocked không?
2. Chạy lại setup_complete.sql
3. Refresh Admin Dashboard
```

### **Vấn đề: Stats không tăng**
```
1. Kiểm tra API: http://localhost/vfg-api/api.php?action=getTracking
2. Kiểm tra database: check_tracking.php
3. Kiểm tra app có gửi data không: Output window
```

---

## 📁 FILES QUAN TRỌNG

### **Database**
- `xampp_api/setup_complete.sql` - Setup 1 lần là xong

### **API**
- `xampp_api/api.php` - Tất cả endpoints
- `xampp_api/check_tracking.php` - Debug tool
- `xampp_api/test_tracking.php` - Test tool

### **App**
- `VietnamFoodGuide/Views/MapWindow.xaml.cs` - Navigation logic

### **Admin**
- `admin_dashboard.html` - Giao diện quản lý

### **Docs**
- `TONG_KET_HOAN_THANH.md` - Tổng kết chi tiết
- `DEBUG_DANG_CHI_DUONG.md` - Debug navigation
- `HUONG_DAN_NHANH.md` - File này

---

## ✅ CHECKLIST HOÀN THÀNH

- [ ] Chạy setup_complete.sql
- [ ] Build app thành công
- [ ] Đăng nhập được với user123
- [ ] Mở được Admin Dashboard
- [ ] Thấy stats = 0 ban đầu
- [ ] Bắt đầu chỉ đường → Số tăng
- [ ] Test Mode hoạt động
- [ ] Khóa/Xóa user hoạt động

---

**Nếu tất cả ✅ → Hoàn thành! 🎉**  
**Nếu có ❌ → Xem TONG_KET_HOAN_THANH.md hoặc DEBUG_DANG_CHI_DUONG.md**
