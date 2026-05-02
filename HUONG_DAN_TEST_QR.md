# Hướng Dẫn Test Chức Năng QR Scanner ✅

## 📋 Tổng Quan

Tài liệu này hướng dẫn cách test chức năng quét QR code trong app Vietnam Food Guide và kiểm tra xem dữ liệu có được cập nhật lên database và hiển thị trong admin dashboard không.

---

## 🎯 Mục Tiêu Test

1. ✅ Quét QR code thành công
2. ✅ Dữ liệu được lưu vào database (table `qr_scans`)
3. ✅ Admin dashboard hiển thị số lượng QR đã quét
4. ✅ Không cho phép quét lại (1 device chỉ quét 1 lần)

---

## 📁 Files Đã Tạo

### 1. **generate_qr.html** (Tạo mã QR test)
**Đường dẫn**: `xampp_api/generate_qr.html`

**Chức năng**:
- Tạo mã QR với code: `VFG-TEST-2024`
- Tải xuống mã QR dưới dạng PNG
- In mã QR
- Hướng dẫn test chi tiết

**Cách mở**:
```
http://localhost/vfg-api/generate_qr.html
```

### 2. **admin_dashboard.html** (Đã cập nhật)
**Thay đổi**:
- ✅ Thêm card "Đã Quét QR" trong stats grid
- ✅ Hiển thị số lượng người đã quét QR
- ✅ Gọi API `getQRStats` để lấy dữ liệu

---

## 🔧 Cấu Hình Database

### Table `qr_scans` (Đã có sẵn)
```sql
CREATE TABLE IF NOT EXISTS qr_scans (
    id INT AUTO_INCREMENT PRIMARY KEY,
    device_id VARCHAR(255) NOT NULL,
    qr_code VARCHAR(500) NOT NULL,
    scan_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    device_name VARCHAR(255) NULL,
    os_version VARCHAR(100) NULL,
    UNIQUE KEY unique_device (device_id),
    INDEX idx_scan_date (scan_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

**Lưu ý**: Table này đã được tạo trong `setup_complete.sql`

---

## 🚀 Các Bước Test

### Bước 1: Tạo Mã QR Test

1. Mở trình duyệt và truy cập:
   ```
   http://localhost/vfg-api/generate_qr.html
   ```

2. Bạn sẽ thấy:
   - Mã QR lớn ở giữa màn hình
   - Code: `VFG-TEST-2024`
   - 2 nút: "Tải xuống QR" và "In mã QR"

3. **Tùy chọn**:
   - **Tải xuống**: Bấm "📥 Tải xuống QR" để lưu file PNG
   - **In**: Bấm "🖨️ In mã QR" để in ra giấy
   - **Hiển thị trên màn hình**: Giữ nguyên trên màn hình để quét trực tiếp

---

### Bước 2: Mở App Vietnam Food Guide

1. Build và chạy app (nếu chưa chạy):
   ```powershell
   dotnet build --configuration Debug
   dotnet run --project VietnamFoodGuide/VietnamFoodGuide.csproj
   ```

2. Đăng nhập vào app (nếu cần)

---

### Bước 3: Quét Mã QR

#### Cách 1: Từ Banner QR (Nếu chưa quét)
1. Khi mở app lần đầu, sẽ có banner "Quét mã QR để nhận ưu đãi"
2. Bấm nút **"Quét Ngay"**
3. MainWindow sẽ đóng, QRScannerWindow sẽ mở

#### Cách 2: Từ Menu/Button
1. Bấm vào nút **"QR Scanner"** trong menu
2. MainWindow sẽ đóng, QRScannerWindow sẽ mở

#### Quét QR:
1. Camera sẽ tự động bật
2. Hướng camera vào mã QR trên màn hình hoặc giấy in
3. Đợi app tự động nhận diện (1-2 giây)

---

### Bước 4: Kiểm Tra Kết Quả Trong App

**Khi quét thành công**:
1. ✅ Overlay màu xanh xuất hiện với icon ✓
2. ✅ Text "Quét thành công!"
3. ✅ Tự động chuyển về MainWindow sau 2 giây
4. ✅ Banner QR biến mất (không hiển thị nữa)

**Khi quét lại (đã quét trước đó)**:
1. ✅ Overlay màu xanh xuất hiện
2. ✅ Text "Bạn đã quét QR trước đó!"
3. ✅ Tự động chuyển về MainWindow

---

### Bước 5: Kiểm Tra Database

#### Cách 1: Qua phpMyAdmin
1. Mở phpMyAdmin: `http://localhost/phpmyadmin`
2. Chọn database `VietnamFoodGuide`
3. Mở table `qr_scans`
4. Kiểm tra dữ liệu:

| id | device_id | qr_code | scan_date | device_name | os_version |
|----|-----------|---------|-----------|-------------|------------|
| 1  | ABC123... | VFG-TEST-2024 | 2024-... | DESKTOP-... | Windows... |

#### Cách 2: Qua SQL Query
```sql
SELECT * FROM qr_scans ORDER BY scan_date DESC;
```

**Kết quả mong đợi**:
- ✅ Có 1 record mới
- ✅ `device_id` là unique ID của máy bạn
- ✅ `qr_code` = "VFG-TEST-2024"
- ✅ `scan_date` là thời gian vừa quét
- ✅ `device_name` là tên máy tính
- ✅ `os_version` là phiên bản Windows

---

### Bước 6: Kiểm Tra Admin Dashboard

1. Mở Admin Dashboard:
   ```
   http://localhost/admin_dashboard.html
   ```

2. Đăng nhập (nếu cần):
   - Username: `admin`
   - Password: `admin123`

3. Kiểm tra Stats Cards:
   - ✅ Card **"Đã Quét QR"** hiển thị số **1** (hoặc nhiều hơn nếu đã test nhiều lần)
   - ✅ Icon QR code màu xanh lá
   - ✅ Text "Đã Quét QR"

4. **Refresh** trang để xem số liệu cập nhật

---

## 🔍 Kiểm Tra Chi Tiết

### Test Case 1: Quét Lần Đầu
**Input**: Quét mã QR `VFG-TEST-2024` lần đầu tiên

**Expected Output**:
- ✅ App hiển thị "Quét thành công!"
- ✅ Database có 1 record mới trong `qr_scans`
- ✅ Admin dashboard hiển thị số 1 ở card "Đã Quét QR"
- ✅ Banner QR trong MainWindow biến mất

---

### Test Case 2: Quét Lại (Cùng Device)
**Input**: Quét mã QR `VFG-TEST-2024` lần thứ 2 trên cùng máy

**Expected Output**:
- ✅ App hiển thị "Bạn đã quét QR trước đó!"
- ✅ Database **KHÔNG** thêm record mới (vẫn 1 record)
- ✅ Admin dashboard vẫn hiển thị số 1
- ✅ Banner QR vẫn không hiển thị

---

### Test Case 3: Quét Từ Device Khác
**Input**: Quét mã QR `VFG-TEST-2024` từ máy tính khác

**Expected Output**:
- ✅ App hiển thị "Quét thành công!"
- ✅ Database có thêm 1 record mới (tổng 2 records)
- ✅ Admin dashboard hiển thị số 2
- ✅ Banner QR biến mất trên máy thứ 2

---

## 📊 API Endpoints Liên Quan

### 1. **saveQRScan** (POST)
**URL**: `http://localhost/vfg-api/api.php?action=saveQRScan`

**Request Body**:
```json
{
  "deviceId": "ABC123...",
  "qrCode": "VFG-TEST-2024",
  "deviceName": "DESKTOP-ABC",
  "osVersion": "Microsoft Windows NT 10.0..."
}
```

**Response (Success)**:
```json
{
  "success": true,
  "message": "Lưu QR scan thành công",
  "scanId": 1
}
```

**Response (Already Scanned)**:
```json
{
  "success": true,
  "message": "Device đã quét QR trước đó",
  "alreadyScanned": true
}
```

---

### 2. **checkQRScan** (GET)
**URL**: `http://localhost/vfg-api/api.php?action=checkQRScan&deviceId=ABC123...`

**Response**:
```json
{
  "hasScanned": true,
  "scanDate": "2024-05-01 14:30:00"
}
```

---

### 3. **getQRStats** (GET)
**URL**: `http://localhost/vfg-api/api.php?action=getQRStats`

**Response**:
```json
{
  "success": true,
  "data": {
    "totalScans": 5,
    "todayScans": 2,
    "weekScans": 5
  }
}
```

---

### 4. **getQRScans** (GET)
**URL**: `http://localhost/vfg-api/api.php?action=getQRScans`

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "device_id": "ABC123...",
      "qr_code": "VFG-TEST-2024",
      "scan_date": "2024-05-01 14:30:00",
      "device_name": "DESKTOP-ABC",
      "os_version": "Windows 10"
    }
  ]
}
```

---

## 🐛 Troubleshooting

### Vấn đề 1: Không quét được QR
**Nguyên nhân**:
- Camera không hoạt động
- WebView2 chưa cài đặt

**Giải pháp**:
1. Kiểm tra camera: Settings → Privacy → Camera
2. Cài WebView2: https://go.microsoft.com/fwlink/p/?LinkId=2124703

---

### Vấn đề 2: Quét thành công nhưng không lưu vào database
**Nguyên nhân**:
- API không kết nối được
- XAMPP chưa chạy
- Database chưa có table `qr_scans`

**Giải pháp**:
1. Kiểm tra XAMPP: Apache và MySQL đang chạy
2. Kiểm tra API: `http://localhost/vfg-api/api.php?action=stats`
3. Chạy lại `setup_complete.sql` trong phpMyAdmin

---

### Vấn đề 3: Admin dashboard không hiển thị số QR
**Nguyên nhân**:
- API `getQRStats` lỗi
- JavaScript không load được

**Giải pháp**:
1. Mở Console (F12) trong trình duyệt
2. Kiểm tra lỗi JavaScript
3. Test API trực tiếp: `http://localhost/vfg-api/api.php?action=getQRStats`

---

## ✅ Checklist Test Hoàn Chỉnh

- [ ] Mở `generate_qr.html` và thấy mã QR
- [ ] Tải xuống hoặc in mã QR thành công
- [ ] Mở app Vietnam Food Guide
- [ ] Bấm "Quét QR" từ banner hoặc menu
- [ ] Camera bật và quét được mã QR
- [ ] App hiển thị "Quét thành công!"
- [ ] Tự động chuyển về MainWindow
- [ ] Banner QR biến mất
- [ ] Kiểm tra database có record mới trong `qr_scans`
- [ ] Mở admin dashboard
- [ ] Card "Đã Quét QR" hiển thị số 1
- [ ] Quét lại và thấy "Bạn đã quét QR trước đó!"
- [ ] Database không thêm record mới (vẫn 1)
- [ ] Admin dashboard vẫn hiển thị số 1

---

## 📝 Ghi Chú

### Device ID
- Device ID được tạo từ: `Environment.MachineName + Environment.UserName + Environment.OSVersion`
- Mỗi máy tính có 1 Device ID duy nhất
- Device ID được hash để bảo mật

### QR Code Format
- Hiện tại: `VFG-TEST-2024` (test)
- Production: Có thể dùng format khác như `VFG-PROMO-{code}` hoặc URL

### Security
- Table `qr_scans` có UNIQUE constraint trên `device_id`
- Không cho phép 1 device quét nhiều lần
- Có thể thêm logic kiểm tra thời gian (ví dụ: chỉ quét 1 lần/ngày)

---

## 🎉 Kết Luận

Sau khi hoàn thành tất cả các bước test, bạn đã xác nhận:
- ✅ Chức năng quét QR hoạt động tốt
- ✅ Dữ liệu được lưu vào database chính xác
- ✅ Admin dashboard hiển thị thống kê đúng
- ✅ Không cho phép quét lại (1 device = 1 lần)

**App sẵn sàng cho production!** 🚀
