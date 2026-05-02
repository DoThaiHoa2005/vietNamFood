# 🎯 CÁCH HOẠT ĐỘNG - BẮT ĐẦU TỪ 0

## ✅ Sau Khi Setup Database

Tất cả stats sẽ hiển thị **0**:

```
📱 Quét QR Code: 0
📲 Đã Cài App: 0
🟢 Đang Online: 0
🧭 Đang Chỉ Đường: 0
❤️ Yêu Thích: 0
```

## 🔄 Khi Nào Số Liệu Tăng Lên?

### 1️⃣ **Quét QR Code** (📱)

**Khi nào tăng:**
- User quét QR code lần đầu tiên
- App gọi API `saveQRScan`
- Database set `Users.QRScanned = TRUE`

**Kết quả:**
```
📱 Quét QR Code: 0 → 1
```

**Code trong App:**
```csharp
// Khi user quét QR thành công
await apiService.SaveQRScan(deviceId, qrCode);
// → API set QRScanned = TRUE
```

---

### 2️⃣ **Đã Cài App** (📲)

**Khi nào tăng:**
- User cài app và mở lần đầu
- App gọi API `updateUserActivity` với `appInstalled = true`
- Database set `Users.AppInstalled = TRUE`

**Kết quả:**
```
📲 Đã Cài App: 0 → 1
```

**Code trong App:**
```csharp
// Khi app khởi động lần đầu
await apiService.UpdateUserActivity(userId, qrScanned: true, appInstalled: true);
// → API set AppInstalled = TRUE
```

---

### 3️⃣ **Đang Online** (🟢)

**Khi nào tăng:**
- User đăng nhập thành công
- API `login` set `Users.LastActiveTime = NOW()`
- App gọi `updateActivity` mỗi 30 giây để duy trì online

**Kết quả:**
```
🟢 Đang Online: 0 → 1
```

**Code trong App:**
```csharp
// Khi đăng nhập
var (success, user, error) = await apiService.LoginAsync(username, password);
// → API set LastActiveTime = NOW()

// Mỗi 30 giây
await apiService.UpdateActivity(sessionToken);
// → API update LastActiveTime = NOW()
```

**Khi nào giảm:**
- User không active > 5 phút
- `LastActiveTime` cũ hơn 5 phút
- User tự động offline

---

### 4️⃣ **Đang Chỉ Đường** (🧭)

**Khi nào tăng:**
- User đang online (đã đăng nhập)
- User bắt đầu navigate đến một quán ăn
- App gọi API `updateTracking` với `isNavigating = true`
- Database set `UserTracking.IsNavigating = TRUE`

**Kết quả:**
```
🧭 Đang Chỉ Đường: 0 → 1
```

**Code trong App:**
```csharp
// Khi user click "Chỉ đường"
await apiService.UpdateTracking(
    userId: currentUser.Id,
    currentLat: myLat,
    currentLng: myLng,
    destinationLat: food.Latitude,
    destinationLng: food.Longitude,
    destinationName: food.Name,
    isNavigating: true,
    isActive: true
);
// → API set IsNavigating = TRUE
```

**Khi nào giảm:**
- User dừng navigate
- User offline (không active > 5 phút)
- App gọi API với `isNavigating = false`

---

### 5️⃣ **Yêu Thích** (❤️)

**Khi nào tăng:**
- User click nút yêu thích (❤️) trên một quán ăn
- App gọi API `addFavorite`
- Database thêm record vào bảng `Favorites`

**Kết quả:**
```
❤️ Yêu Thích: 0 → 1
```

**Code trong App:**
```csharp
// Khi user click nút yêu thích
await apiService.AddFavorite(userId, foodId);
// → API INSERT INTO Favorites
```

**Khi nào giảm:**
- User bỏ yêu thích (click lại nút ❤️)
- App gọi API `removeFavorite`
- Database xóa record khỏi bảng `Favorites`

---

## 📊 Ví Dụ Thực Tế

### **Scenario: User Mới Sử Dụng App**

#### **Bước 1: Quét QR Code**
```
User quét QR → QRScanned = TRUE
📱 Quét QR Code: 0 → 1
```

#### **Bước 2: Cài App**
```
User cài app và mở → AppInstalled = TRUE
📲 Đã Cài App: 0 → 1
```

#### **Bước 3: Đăng Nhập**
```
User đăng nhập → LastActiveTime = NOW()
🟢 Đang Online: 0 → 1
```

#### **Bước 4: Yêu Thích Quán**
```
User click ❤️ trên "Phở Đặc Biệt" → INSERT Favorites
❤️ Yêu Thích: 0 → 1
```

#### **Bước 5: Bắt Đầu Chỉ Đường**
```
User click "Chỉ đường" → IsNavigating = TRUE
🧭 Đang Chỉ Đường: 0 → 1
```

#### **Kết Quả Cuối:**
```
📱 Quét QR Code: 1
📲 Đã Cài App: 1
🟢 Đang Online: 1
🧭 Đang Chỉ Đường: 1
❤️ Yêu Thích: 1
```

---

## 🔍 Kiểm Tra Trong Database

### **Xem Users**
```sql
SELECT Username, QRScanned, AppInstalled, LastActiveTime 
FROM Users 
WHERE QRScanned = TRUE OR AppInstalled = TRUE;
```

### **Xem Tracking**
```sql
SELECT u.Username, t.IsNavigating, t.DestinationName, t.LastUpdate
FROM UserTracking t
JOIN Users u ON t.UserId = u.Id
WHERE t.IsActive = TRUE;
```

### **Xem Favorites**
```sql
SELECT u.Username, f.Name as FoodName, fav.CreatedDate
FROM Favorites fav
JOIN Users u ON fav.UserId = u.Id
JOIN Foods f ON fav.FoodId = f.Id
ORDER BY fav.CreatedDate DESC;
```

---

## 🎓 Tóm Tắt

| Hành Động | API Endpoint | Database Update | Stats Tăng |
|-----------|--------------|-----------------|------------|
| Quét QR | `saveQRScan` | `QRScanned = TRUE` | 📱 +1 |
| Cài App | `updateUserActivity` | `AppInstalled = TRUE` | 📲 +1 |
| Đăng Nhập | `login` | `LastActiveTime = NOW()` | 🟢 +1 |
| Bắt Đầu Navigate | `updateTracking` | `IsNavigating = TRUE` | 🧭 +1 |
| Yêu Thích | `addFavorite` | `INSERT Favorites` | ❤️ +1 |

**Tất cả bắt đầu từ 0, chỉ tăng khi user thực sự thực hiện hành động!**

---

**File:** `CACH_HOAT_DONG_BAT_DAU_TU_0.md`
**Ngày tạo:** 2026-04-30
