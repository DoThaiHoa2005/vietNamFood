# 🐛 DEBUG: Đang Chỉ Đường Không Hiện Số

## 🔍 Các Bước Kiểm Tra

### **Bước 1: Kiểm Tra Database**

Mở browser và truy cập:
```
http://localhost/vfg-api/check_tracking.php
```

Xem:
- ✅ Có users nào đang online không? (LastActiveTime < 5 phút)
- ✅ Có tracking data không?
- ✅ IsNavigating có = TRUE không?
- ✅ Số đếm cuối cùng là bao nhiêu?

### **Bước 2: Test Gửi Tracking Data Thủ Công**

Mở browser và truy cập:
```
http://localhost/vfg-api/test_tracking.php
```

File này sẽ:
1. Set user123 (ID=2) là online
2. Tạo tracking record với IsNavigating = TRUE
3. Hiển thị kết quả

Sau đó refresh Admin Dashboard xem có hiện số không.

### **Bước 3: Kiểm Tra Console Log**

1. Mở Admin Dashboard
2. Nhấn F12 để mở Developer Tools
3. Vào tab "Console"
4. Refresh trang
5. Xem log `📊 [Tracking Stats]`

Kiểm tra:
```javascript
{
  totalMerged: 1,  // Có bao nhiêu tracking records
  online: 1,       // Có bao nhiêu users online
  navigating: 0,   // ← Số này phải > 0 nếu đang navigate
  mergedData: [    // Chi tiết từng record
    {
      UserId: 2,
      Username: "user123",
      IsNavigating: true,  // ← Phải là true
      IsActive: true,      // ← Phải là true
      isOnline: true,      // ← Phải là true
      ...
    }
  ]
}
```

### **Bước 4: Kiểm Tra App Có Gửi Data Không**

1. Mở app và đăng nhập với `user123`
2. Click vào một quán ăn
3. Click "🚀 Bắt đầu chỉ đường"
4. Mở Output window trong Visual Studio (View → Output)
5. Tìm dòng log:

```
📡 [Tracking] Đã gửi vị trí User user123 (10.77690, 106.70090) lên server - Navigate: True
```

Nếu thấy `Navigate: True` → App đã gửi đúng

### **Bước 5: Kiểm Tra API Có Nhận Data Không**

Mở browser và truy cập:
```
http://localhost/vfg-api/api.php?action=getTracking
```

Xem response:
```json
{
  "success": true,
  "data": [
    {
      "Id": 1,
      "UserId": 2,
      "CurrentLat": 10.7769,
      "CurrentLng": 106.7009,
      "DestinationLat": 10.78567,
      "DestinationLng": 106.70189,
      "DestinationName": "Phở Đặc Biệt Trần Hưng Đạo",
      "IsNavigating": 1,  // ← Phải là 1 (TRUE)
      "IsActive": 1,      // ← Phải là 1 (TRUE)
      "LastUpdate": "2026-04-30 14:30:25"
    }
  ]
}
```

## 🔧 Các Vấn Đề Thường Gặp

### **Vấn Đề 1: IsNavigating = 0 (FALSE)**

**Nguyên nhân:** App không gửi `isNavigating: true` lên server

**Giải pháp:**
1. Kiểm tra biến `_isNavigating` trong MapWindow.xaml.cs
2. Kiểm tra JavaScript có gửi `navigating: true` không
3. Rebuild app: `dotnet build`

### **Vấn Đề 2: User Không Online**

**Nguyên nhân:** `LastActiveTime` cũ hơn 5 phút

**Giải pháp:**
1. Đăng nhập lại app
2. Kiểm tra activity timer có chạy không (mỗi 30 giây)
3. Kiểm tra API `updateActivity` có hoạt động không

### **Vấn Đề 3: Không Có Tracking Data**

**Nguyên nhân:** API `updateTracking` không được gọi

**Giải pháp:**
1. Kiểm tra `ReportPositionToServer()` có được gọi không
2. Kiểm tra có lỗi trong Output window không
3. Kiểm tra API endpoint có đúng không

### **Vấn Đề 4: Admin Dashboard Không Refresh**

**Nguyên nhân:** Auto-refresh không hoạt động

**Giải pháp:**
1. Click nút "🔄 Làm Mới" thủ công
2. Kiểm tra interval có chạy không (mỗi 5 giây)
3. Refresh trang browser (F5)

## 📊 Checklist Debug

- [ ] Database có tracking data với IsNavigating = TRUE
- [ ] User có LastActiveTime < 5 phút (online)
- [ ] App log hiển thị "Navigate: True"
- [ ] API getTracking trả về IsNavigating = 1
- [ ] Console log hiển thị navigating > 0
- [ ] Admin Dashboard hiển thị số đúng

## 🧪 Test Nhanh

### **Test 1: Gửi Data Thủ Công**

```bash
# Mở browser
http://localhost/vfg-api/test_tracking.php

# Refresh Admin Dashboard
# Phải thấy "Đang Chỉ Đường: 1"
```

### **Test 2: Kiểm Tra Query SQL**

```sql
-- Chạy trong phpMyAdmin
SELECT 
    u.Username,
    t.IsNavigating,
    t.IsActive,
    u.LastActiveTime,
    CASE 
        WHEN u.LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE) THEN 'Online'
        ELSE 'Offline'
    END as Status
FROM UserTracking t
JOIN Users u ON t.UserId = u.Id
WHERE t.IsNavigating = TRUE AND t.IsActive = TRUE;

-- Nếu có kết quả → Phải hiển thị trong dashboard
```

## 📞 Nếu Vẫn Không Được

1. Chụp màn hình `check_tracking.php`
2. Chụp màn hình Console log trong Admin Dashboard
3. Chụp màn hình Output window trong Visual Studio
4. Gửi cho tôi để debug tiếp

---

**File debug:** `DEBUG_DANG_CHI_DUONG.md`
**Files test:**
- `xampp_api/check_tracking.php` - Kiểm tra database
- `xampp_api/test_tracking.php` - Test gửi data thủ công
