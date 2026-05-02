# 📱 HƯỚNG DẪN HOÀN CHỈNH: QR SCANNER SYSTEM

## 🎯 TỔNG QUAN

Hệ thống QR Scanner cho phép:
1. ✅ Quét QR code trước khi vào app
2. ✅ Lưu thông tin người đã quét vào database
3. ✅ Báo lên admin dashboard số người đã quét
4. ✅ Kiểm tra đã quét rồi thì không cần quét lại
5. ✅ Chọn ảnh QR từ thư viện ảnh
6. ✅ Giao diện chuyên nghiệp với đa ngôn ngữ

## 📋 CÁC FILE ĐÃ TẠO/SỬA

### 1. **Model - QRScan Entity**
**File**: `VietnamFoodGuide/Models/Entities/QRScan.cs`
- Định nghĩa model cho bảng `qr_scans`
- Các trường: Id, DeviceId, QRCode, ScanDate, DeviceName, OSVersion

### 2. **Database Context**
**File**: `VietnamFoodGuide/Data/ApplicationDbContext.cs`
- Thêm `DbSet<QRScan> QRScans`

### 3. **QR Scanner Window - XAML**
**File**: `VietnamFoodGuide/Views/QRScannerWindow.xaml`
- Giao diện chuyên nghiệp với WebView2
- Camera preview
- Nút chọn ảnh QR
- Nút bỏ qua
- Success overlay
- Loading overlay
- Hướng dẫn sử dụng
- Hỗ trợ đa ngôn ngữ (vi/en/zh)

### 4. **QR Scanner Window - Code Behind**
**File**: `VietnamFoodGuide/Views/QRScannerWindow.xaml.cs`
- Sử dụng thư viện `html5-qrcode` (JavaScript)
- Tự động quét QR từ camera
- Chọn ảnh QR từ file
- Lưu vào local storage
- Gửi lên API
- Kiểm tra đã quét chưa
- Lấy Device ID duy nhất

### 5. **Storage Service**
**File**: `VietnamFoodGuide/Services/StorageService.cs`
- Thêm methods:
  - `SaveQRScanned()` - Lưu trạng thái đã quét
  - `HasScannedQR()` - Kiểm tra đã quét chưa
  - `GetQRScanDate()` - Lấy ngày quét

### 6. **App Startup**
**File**: `VietnamFoodGuide/App.xaml.cs`
- Thay đổi: Hiển thị QRScannerWindow trước MainWindow
- Luồng: QRScannerWindow → MainWindow (sau khi quét hoặc bỏ qua)

### 7. **API Endpoints**
**File**: `xampp_api/api.php`
- `saveQRScan` (POST) - Lưu thông tin quét QR
- `checkQRScan` (GET) - Kiểm tra device đã quét chưa
- `getQRScans` (GET) - Lấy danh sách tất cả QR scans
- `getQRStats` (GET) - Lấy thống kê QR scans

### 8. **Database Schema**
**File**: `xampp_api/create_qr_scans_table.sql`
- Script tạo bảng `qr_scans`

## 🔧 CÀI ĐẶT

### Bước 1: Tạo bảng database

```sql
-- Mở phpMyAdmin: http://localhost/phpmyadmin
-- Chọn database "VietnamFoodGuide"
-- Chạy script sau:

CREATE TABLE IF NOT EXISTS `qr_scans` (
  `id` INT(11) NOT NULL AUTO_INCREMENT,
  `device_id` VARCHAR(255) NOT NULL,
  `qr_code` VARCHAR(500) NOT NULL,
  `scan_date` DATETIME NOT NULL,
  `device_name` VARCHAR(255) DEFAULT NULL,
  `os_version` VARCHAR(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `unique_device` (`device_id`),
  KEY `idx_scan_date` (`scan_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### Bước 2: Build project

```bash
dotnet build VietnamFoodGuide/VietnamFoodGuide.csproj --configuration Release
```

### Bước 3: Chạy XAMPP

1. Mở XAMPP Control Panel
2. Start Apache
3. Start MySQL
4. Kiểm tra API: http://localhost/vfg-api/api.php?action=getQRStats

### Bước 4: Tạo QR Code

Tạo QR code với nội dung bất kỳ, ví dụ:
- `VIETNAM_FOOD_GUIDE_ACCESS_2024`
- `VFG_PROMO_CODE_12345`
- URL: `https://vietnamfoodguide.com/access`

Công cụ tạo QR:
- https://www.qr-code-generator.com/
- https://www.the-qrcode-generator.com/

## 🎮 CÁCH SỬ DỤNG

### Lần đầu tiên:

1. **Mở app** → Hiển thị QR Scanner Window
2. **Quét QR**:
   - **Option 1**: Đưa QR code vào camera
   - **Option 2**: Click "📁 Chọn ảnh QR" → Chọn ảnh QR từ máy
3. **Sau khi quét thành công**:
   - Hiển thị "✅ Quét thành công!"
   - Tự động chuyển đến MainWindow sau 2 giây
   - Lưu vào local storage
   - Gửi lên API server

### Lần sau:

1. **Mở app** → Tự động kiểm tra đã quét chưa
2. **Nếu đã quét** → Bỏ qua QR Scanner, vào thẳng MainWindow
3. **Nếu chưa quét** → Hiển thị QR Scanner

### Bỏ qua quét QR:

1. Click nút "⏭️ Bỏ qua"
2. Xác nhận → Vào MainWindow
3. Có thể quét sau trong phần Tài khoản

## 📊 ADMIN DASHBOARD

Admin có thể xem thống kê QR scans tại:
- **URL**: http://localhost/vfg-api/admin_dashboard.html
- **Section**: Dashboard → QR Scan Statistics

Thống kê hiển thị:
- 📱 **Tổng số lượt quét**: Tổng số device đã quét
- 📅 **Hôm nay**: Số lượt quét hôm nay
- 📈 **Tuần này**: Số lượt quét trong 7 ngày qua

## 🔄 LUỒNG HOẠT ĐỘNG

```
App Start
    ↓
QRScannerWindow.CheckIfAlreadyScanned()
    ↓
    ├─ Check Local Storage (StorageService.HasScannedQR())
    │   ↓
    │   ├─ YES → ShowSuccessAndProceed() → MainWindow
    │   └─ NO → Continue
    ↓
    ├─ Check API (GET /api.php?action=checkQRScan&deviceId=xxx)
    │   ↓
    │   ├─ YES → Save to Local → ShowSuccessAndProceed() → MainWindow
    │   └─ NO → Continue
    ↓
Initialize QR Scanner (WebView2 + html5-qrcode library)
    ↓
User Actions:
    ├─ Scan with Camera → OnQRScanned() → SaveQRScan()
    ├─ Select Image → decodeQRFromImage() → OnQRScanned() → SaveQRScan()
    └─ Skip → Confirm → MainWindow
    ↓
SaveQRScan()
    ├─ Save to Local Storage (StorageService.SaveQRScanned())
    └─ Save to API (POST /api.php?action=saveQRScan)
    ↓
ShowSuccessAndProceed()
    ↓
Wait 2 seconds
    ↓
Open MainWindow
    ↓
Close QRScannerWindow
```

## 🌐 ĐA NGÔN NGỮ

QR Scanner Window hỗ trợ 3 ngôn ngữ:

### Tiếng Việt (vi):
- Tiêu đề: "📱 Quét Mã QR"
- Phụ đề: "Quét mã QR để truy cập"
- Hướng dẫn: "1. Đưa mã QR vào khung hình..."
- Nút: "📁 Chọn ảnh QR", "⏭️ Bỏ qua"

### English (en):
- Title: "📱 Scan QR Code"
- Subtitle: "Scan QR Code to Access"
- Instructions: "1. Place QR code in frame..."
- Buttons: "📁 Select QR Image", "⏭️ Skip"

### 中文 (zh):
- 标题: "📱 扫描二维码"
- 副标题: "扫描二维码以访问"
- 说明: "1. 将二维码放入框架..."
- 按钮: "📁 选择二维码图片", "⏭️ 跳过"

## 🔐 BẢO MẬT

### Device ID Generation:
1. **Ưu tiên**: Lấy UUID từ hardware (Win32_ComputerSystemProduct)
2. **Fallback**: MachineName + UserName

### Unique Constraint:
- Mỗi device chỉ được lưu 1 lần trong database
- UNIQUE KEY trên `device_id`

### Local Storage:
- File: `%AppData%\VietnamFoodGuide\qr_scanned.txt`
- Nội dung: Ngày giờ quét (yyyy-MM-dd HH:mm:ss)

## 📱 TÍNH NĂNG NỔI BẬT

### 1. **Auto-detect Camera**
- Tự động phát hiện và bật camera
- Fallback message nếu không có camera

### 2. **Image Selection**
- Hỗ trợ: JPG, JPEG, PNG, BMP, GIF
- Decode QR từ ảnh tĩnh

### 3. **Real-time Scanning**
- FPS: 10 frames/second
- QR box: 250x250px
- Tự động dừng sau khi quét thành công

### 4. **Offline Support**
- Lưu local storage trước
- Gửi API sau (không block UI)
- Vẫn hoạt động nếu API lỗi

### 5. **Professional UI**
- Modern design với rounded corners
- Drop shadow effects
- Smooth animations
- Loading states
- Success overlay
- Status messages

## 🧪 TESTING

### Test Case 1: Quét QR lần đầu
1. Xóa file: `%AppData%\VietnamFoodGuide\qr_scanned.txt`
2. Xóa record trong database: `DELETE FROM qr_scans WHERE device_id = 'YOUR_DEVICE_ID'`
3. Mở app
4. ✅ **Kết quả**: Hiển thị QR Scanner

### Test Case 2: Quét QR thành công
1. Đưa QR code vào camera
2. ✅ **Kết quả**: 
   - Hiển thị "✅ Quét thành công!"
   - Chuyển đến MainWindow sau 2s
   - File `qr_scanned.txt` được tạo
   - Record được thêm vào database

### Test Case 3: Mở app lần 2
1. Mở app sau khi đã quét
2. ✅ **Kết quả**: Bỏ qua QR Scanner, vào thẳng MainWindow

### Test Case 4: Chọn ảnh QR
1. Click "📁 Chọn ảnh QR"
2. Chọn file ảnh có QR code
3. ✅ **Kết quả**: Tự động decode và lưu

### Test Case 5: Bỏ qua
1. Click "⏭️ Bỏ qua"
2. Xác nhận
3. ✅ **Kết quả**: Vào MainWindow (không lưu QR scan)

### Test Case 6: Đa ngôn ngữ
1. Thay đổi ngôn ngữ trong MainWindow
2. Quay lại QR Scanner
3. ✅ **Kết quả**: UI cập nhật theo ngôn ngữ đã chọn

## 📈 THỐNG KÊ ADMIN

Admin có thể xem:
- **Tổng số lượt quét**: `SELECT COUNT(*) FROM qr_scans`
- **Hôm nay**: `WHERE DATE(scan_date) = CURDATE()`
- **Tuần này**: `WHERE scan_date >= DATE_SUB(NOW(), INTERVAL 7 DAY)`
- **Danh sách chi tiết**: Device ID, QR Code, Scan Date, Device Name, OS Version

## 🚀 TRIỂN KHAI

### Development:
```bash
# Build
dotnet build --configuration Debug

# Run
dotnet run --project VietnamFoodGuide/VietnamFoodGuide.csproj
```

### Production:
```bash
# Build Release
dotnet build --configuration Release

# Publish
dotnet publish -c Release -r win-x64 --self-contained true

# Output: VietnamFoodGuide/bin/Release/net48/win-x64/publish/
```

## 🔧 TROUBLESHOOTING

### Lỗi: Camera không hoạt động
- **Nguyên nhân**: Quyền camera bị từ chối
- **Giải pháp**: Cho phép camera trong Windows Settings → Privacy → Camera

### Lỗi: WebView2 Runtime not found
- **Nguyên nhân**: Chưa cài WebView2 Runtime
- **Giải pháp**: Download tại https://go.microsoft.com/fwlink/p/?LinkId=2124703

### Lỗi: API connection failed
- **Nguyên nhân**: XAMPP chưa chạy hoặc API URL sai
- **Giải pháp**: 
  1. Kiểm tra XAMPP đang chạy
  2. Kiểm tra `AppConfig.ApiBaseUrl`
  3. Test API: http://localhost/vfg-api/api.php?action=getQRStats

### Lỗi: Database table not found
- **Nguyên nhân**: Chưa tạo bảng `qr_scans`
- **Giải pháp**: Chạy script `create_qr_scans_table.sql` trong phpMyAdmin

## 📝 GHI CHÚ

- QR Scanner chỉ hiển thị 1 lần duy nhất (trừ khi xóa local storage)
- Device ID là duy nhất cho mỗi máy
- Hỗ trợ offline: Lưu local trước, sync API sau
- Admin có thể xem tất cả QR scans trong dashboard
- Có thể custom QR code content theo nhu cầu

---
**Date**: 2026-04-29
**Status**: ✅ COMPLETED
**Version**: 1.0.0
