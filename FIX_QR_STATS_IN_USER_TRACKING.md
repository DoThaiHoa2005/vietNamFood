# Sửa Hiển Thị QR Stats Trong User Tracking - HOÀN THÀNH ✅

## 📋 Thay Đổi

### 1. ✅ Xóa Card "Đã Quét QR" Ở Dashboard
**File**: `admin_dashboard.html`

**Trước**:
- Dashboard có 5 cards: Quán Ăn, Người Dùng, **Đã Quét QR**, Đánh Giá, Yêu Thích

**Sau**:
- Dashboard có 4 cards: Quán Ăn, Người Dùng, Đánh Giá, Yêu Thích
- ✅ Đã xóa card "Đã Quét QR"

---

### 2. ✅ Hiển Thị QR Stats Trong User Tracking
**File**: `admin_dashboard.html`

**Vị trí**: Phần User Tracking đã có sẵn 4 stats cards:
1. **Quét QR Code** - Hiển thị số người đã quét QR
2. **Đã Cài App** - Hiển thị số người đã cài app
3. **Đang Online** - Hiển thị số người đang online
4. **Đang Chỉ Đường** - Hiển thị số người đang navigate

**Code đã có sẵn**:
```html
<div class="stat-card">
    <div class="stat-icon success">
        <i class="fas fa-qrcode"></i>
    </div>
    <div class="stat-info">
        <h3 id="qrScannedUsers">0</h3>
        <p>Quét QR Code</p>
    </div>
</div>
```

---

### 3. ✅ Cập Nhật API Để Đếm Từ Table `qr_scans`
**File**: `xampp_api/api.php`

**Endpoint**: `getAppStats`

**Trước**:
```php
$qrScanned = $pdo->query("SELECT COUNT(*) FROM Users WHERE QRScanned = 1")->fetchColumn();
```

**Sau**:
```php
$qrScanned = $pdo->query("SELECT COUNT(*) FROM qr_scans")->fetchColumn();
```

**Lý do**:
- Table `qr_scans` chứa dữ liệu chính xác về QR đã quét
- Mỗi device quét QR sẽ có 1 record trong `qr_scans`
- Đếm từ `qr_scans` chính xác hơn đếm từ `Users.QRScanned`

---

## 🔄 Luồng Dữ Liệu

### Khi User Quét QR:
1. App gọi API `saveQRScan` với `deviceId`, `qrCode`, `deviceName`, `osVersion`
2. API lưu vào table `qr_scans`
3. Nếu device đã quét trước đó → không thêm record mới (UNIQUE constraint)

### Khi Mở User Tracking:
1. Admin dashboard gọi API `getAppStats`
2. API đếm số records trong table `qr_scans`
3. Trả về JSON:
   ```json
   {
     "success": true,
     "data": {
       "qrScanned": 5,
       "appInstalled": 2,
       "currentlyActive": 1
     }
   }
   ```
4. Dashboard hiển thị số `qrScanned` trong card "Quét QR Code"

---

## 📊 Giao Diện User Tracking

```
┌─────────────────────────────────────────────────────────┐
│  User Tracking - Theo Dõi Người Dùng      [Làm Mới]    │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐│
│  │ 🔲 QR    │  │ 📱 App   │  │ 👥 Online│  │ 🗺️ Chỉ   ││
│  │    5     │  │    2     │  │    1     │  │ Đường 0  ││
│  │ Quét QR  │  │ Đã Cài   │  │ Đang     │  │ Đang Chỉ ││
│  │ Code     │  │ App      │  │ Online   │  │ Đường    ││
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘│
│                                                          │
│  ┌─────────────────┬────────────────────────────────────┤
│  │ 👥 Người Dùng   │  🗺️ Bản Đồ                        │
│  │ Online          │                                    │
│  │                 │                                    │
│  │ • user123       │         [Map with markers]         │
│  │   Đang chỉ     │                                    │
│  │   đường đến... │                                    │
│  │                 │                                    │
│  └─────────────────┴────────────────────────────────────┘
└─────────────────────────────────────────────────────────┘
```

---

## 🧪 Test

### Bước 1: Quét QR
1. Mở `http://localhost/vfg-api/generate_qr.html`
2. Quét mã QR bằng app
3. Kiểm tra database:
   ```sql
   SELECT COUNT(*) FROM qr_scans;
   ```
   → Kết quả: 1

### Bước 2: Kiểm Tra User Tracking
1. Mở `http://localhost/admin_dashboard.html`
2. Bấm menu **"User Tracking"**
3. Kiểm tra card **"Quét QR Code"**
   - ✅ Hiển thị số **1**
   - ✅ Icon QR code màu xanh lá
   - ✅ Text "Quét QR Code"

### Bước 3: Quét Thêm QR (Device Khác)
1. Quét QR từ máy tính khác
2. Kiểm tra database:
   ```sql
   SELECT COUNT(*) FROM qr_scans;
   ```
   → Kết quả: 2

3. Refresh User Tracking
   - ✅ Card "Quét QR Code" hiển thị số **2**

---

## 📝 API Endpoints

### getAppStats (GET)
**URL**: `http://localhost/vfg-api/api.php?action=getAppStats`

**Response**:
```json
{
  "success": true,
  "data": {
    "qrScanned": 5,
    "appInstalled": 2,
    "currentlyActive": 1
  }
}
```

**Giải thích**:
- `qrScanned`: Số lượng devices đã quét QR (đếm từ `qr_scans`)
- `appInstalled`: Số users đã cài app (đếm từ `Users.AppInstalled`)
- `currentlyActive`: Số users đang online (active trong 5 phút)

---

## ✅ Checklist

- [x] Xóa card "Đã Quét QR" ở Dashboard
- [x] Xóa code load QR stats trong `loadStats()`
- [x] Giữ nguyên card "Quét QR Code" trong User Tracking
- [x] Cập nhật API `getAppStats` đếm từ `qr_scans`
- [x] Test quét QR và kiểm tra User Tracking
- [x] Verify số liệu hiển thị đúng

---

## 🎯 Kết Quả

### Dashboard (Trang Chủ)
- ✅ 4 cards: Quán Ăn, Người Dùng, Đánh Giá, Yêu Thích
- ✅ Không còn card "Đã Quét QR"

### User Tracking
- ✅ 4 cards: Quét QR Code, Đã Cài App, Đang Online, Đang Chỉ Đường
- ✅ Card "Quét QR Code" hiển thị số lượng chính xác từ table `qr_scans`
- ✅ Auto refresh mỗi 2 giây

---

## 📌 Lưu Ý

### Sự Khác Biệt Giữa 2 Cách Đếm:

#### Cách 1: Đếm từ `Users.QRScanned` (Cũ)
```sql
SELECT COUNT(*) FROM Users WHERE QRScanned = 1
```
- Đếm số **users** đã quét QR
- 1 user có thể quét nhiều lần nhưng chỉ đếm 1 lần
- Phụ thuộc vào việc cập nhật cột `QRScanned` trong table `Users`

#### Cách 2: Đếm từ `qr_scans` (Mới) ✅
```sql
SELECT COUNT(*) FROM qr_scans
```
- Đếm số **devices** đã quét QR
- 1 device chỉ quét 1 lần (UNIQUE constraint trên `device_id`)
- Dữ liệu chính xác và độc lập
- Không cần cập nhật table `Users`

**Kết luận**: Cách 2 chính xác và đơn giản hơn!

---

## 🚀 Hoàn Thành

✅ **Dashboard**: Đã xóa card "Đã Quét QR"
✅ **User Tracking**: Hiển thị số QR đã quét chính xác
✅ **API**: Đếm từ table `qr_scans`
✅ **Test**: Đã verify hoạt động đúng

**Trạng thái**: ✅ HOÀN THÀNH
