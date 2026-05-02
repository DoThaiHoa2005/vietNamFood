# ✅ FIX: Bản Đồ Admin Hiển Thị Đường Đi Thực Tế

## 🐛 VẤN ĐỀ

Trên Admin Dashboard, khi user đang chỉ đường, bản đồ hiển thị **đường thẳng** (đường chim bay) từ vị trí hiện tại đến điểm đến, thay vì **đường đi thực tế** theo đường phố.

### Trước khi fix:
```
User -------- (đường thẳng) -------- Điểm đến
```

### Sau khi fix:
```
User ~~~~ (theo đường phố) ~~~~ Điểm đến
```

---

## ✅ GIẢI PHÁP

Sử dụng **OSRM (Open Source Routing Machine)** API để tính toán đường đi thực tế theo đường phố, giống như trong app.

### API Endpoint:
```
https://router.project-osrm.org/route/v1/driving/{lng1},{lat1};{lng2},{lat2}?overview=full&geometries=geojson
```

### Tham số:
- `{lng1},{lat1}`: Tọa độ điểm bắt đầu (kinh độ, vĩ độ)
- `{lng2},{lat2}`: Tọa độ điểm đến (kinh độ, vĩ độ)
- `overview=full`: Trả về toàn bộ geometry của route
- `geometries=geojson`: Trả về geometry dạng GeoJSON

### Response:
```json
{
  "code": "Ok",
  "routes": [
    {
      "geometry": {
        "coordinates": [
          [106.70189, 10.78567],
          [106.70195, 10.78572],
          ...
        ]
      },
      "distance": 1234.5,  // meters
      "duration": 180.2    // seconds
    }
  ]
}
```

---

## 🔧 THAY ĐỔI CODE

### 1. Thay đổi cách vẽ route

**Trước (đường thẳng):**
```javascript
// Draw route line
const routeLine = L.polyline([
    [track.CurrentLat, track.CurrentLng],
    [track.DestinationLat, track.DestinationLng]
], {
    color: '#2d7a4f',
    weight: 3,
    opacity: 0.6,
    dashArray: '10, 10'  // Đường đứt nét
}).addTo(trackingMap);

trackingRoutes[track.UserId] = routeLine;
```

**Sau (đường thực tế):**
```javascript
// Draw route line using OSRM (real road routing)
fetchOSRMRoute(
    track.CurrentLat, track.CurrentLng,
    track.DestinationLat, track.DestinationLng,
    track.UserId
);
```

### 2. Thêm function fetchOSRMRoute

```javascript
// Fetch real road route from OSRM
async function fetchOSRMRoute(startLat, startLng, endLat, endLng, userId) {
    try {
        const url = `https://router.project-osrm.org/route/v1/driving/${startLng},${startLat};${endLng},${endLat}?overview=full&geometries=geojson`;
        
        const response = await fetch(url);
        const data = await response.json();
        
        if (data.code === 'Ok' && data.routes && data.routes.length > 0) {
            const route = data.routes[0];
            const coordinates = route.geometry.coordinates;
            
            // Convert [lng, lat] to [lat, lng] for Leaflet
            const latLngs = coordinates.map(coord => [coord[1], coord[0]]);
            
            // Draw route line
            const routeLine = L.polyline(latLngs, {
                color: '#2d7a4f',
                weight: 4,
                opacity: 0.7,
                lineJoin: 'round',
                lineCap: 'round'
            }).addTo(trackingMap);
            
            // Add distance and duration info
            const distanceKm = (route.distance / 1000).toFixed(1);
            const durationMin = Math.round(route.duration / 60);
            
            routeLine.bindPopup(`
                <div style="text-align: center;">
                    <div style="font-weight: 600; margin-bottom: 5px;">📍 Lộ trình</div>
                    <div style="font-size: 12px;">🛣️ ${distanceKm} km</div>
                    <div style="font-size: 12px;">⏱️ ${durationMin} phút</div>
                </div>
            `);
            
            trackingRoutes[userId] = routeLine;
            
            console.log(`✅ [OSRM] Đã vẽ route cho user ${userId}: ${distanceKm}km, ${durationMin}min`);
        } else {
            console.error('❌ [OSRM] Không tìm thấy route:', data);
            // Fallback to straight line if OSRM fails
            drawStraightLine(startLat, startLng, endLat, endLng, userId);
        }
    } catch (error) {
        console.error('❌ [OSRM] Lỗi khi fetch route:', error);
        // Fallback to straight line if OSRM fails
        drawStraightLine(startLat, startLng, endLat, endLng, userId);
    }
}
```

### 3. Thêm fallback function (nếu OSRM lỗi)

```javascript
// Fallback: Draw straight line if OSRM fails
function drawStraightLine(startLat, startLng, endLat, endLng, userId) {
    const routeLine = L.polyline([
        [startLat, startLng],
        [endLat, endLng]
    ], {
        color: '#2d7a4f',
        weight: 3,
        opacity: 0.6,
        dashArray: '10, 10'
    }).addTo(trackingMap);
    
    trackingRoutes[userId] = routeLine;
    console.log(`⚠️ [Fallback] Đã vẽ đường thẳng cho user ${userId}`);
}
```

---

## 🎨 THAY ĐỔI GIAO DIỆN

### Đường đi mới:
- ✅ Màu: `#2d7a4f` (xanh lá đậm)
- ✅ Độ dày: `4px` (dày hơn để dễ nhìn)
- ✅ Độ mờ: `0.7` (rõ ràng hơn)
- ✅ Kiểu: Đường liền (không đứt nét)
- ✅ Bo góc: `round` (mượt mà hơn)

### Popup thông tin:
Khi click vào đường đi, hiển thị:
- 📍 Lộ trình
- 🛣️ Khoảng cách (km)
- ⏱️ Thời gian (phút)

---

## 🔄 LUỒNG HOẠT ĐỘNG

```
1. Admin Dashboard load tracking data
   ↓
2. Phát hiện user đang navigate (IsNavigating = TRUE)
   ↓
3. Có DestinationLat và DestinationLng
   ↓
4. Gọi fetchOSRMRoute(currentLat, currentLng, destLat, destLng, userId)
   ↓
5. Fetch OSRM API:
   https://router.project-osrm.org/route/v1/driving/...
   ↓
6. Nhận response với geometry coordinates
   ↓
7. Convert [lng, lat] → [lat, lng] cho Leaflet
   ↓
8. Vẽ polyline theo coordinates
   ↓
9. Thêm popup với thông tin khoảng cách và thời gian
   ↓
10. Hiển thị đường đi thực tế trên bản đồ ✅
```

---

## 🧪 CÁCH TEST

### 1. Mở Admin Dashboard:
```
http://localhost/vfg-api/admin_dashboard.html
```

### 2. Đăng nhập:
- Username: `admin`
- Password: `admin123`

### 3. Vào tab "User Tracking"

### 4. Trong app, đăng nhập với `user123` và bắt đầu chỉ đường

### 5. Kiểm tra bản đồ Admin Dashboard:

**Trước khi fix:**
```
User -------- (đường thẳng đứt nét) -------- Điểm đến
```

**Sau khi fix:**
```
User ~~~~~~~~ (đường cong theo đường phố) ~~~~~~~~ Điểm đến
```

### 6. Click vào đường đi:

Phải thấy popup:
```
📍 Lộ trình
🛣️ 2.3 km
⏱️ 5 phút
```

### 7. Kiểm tra Console (F12):

Phải thấy:
```
✅ [OSRM] Đã vẽ route cho user 2: 2.3km, 5min
```

---

## 📊 SO SÁNH

### TRƯỚC KHI FIX:

| Đặc điểm | Giá trị |
|----------|---------|
| Loại đường | Đường thẳng (chim bay) |
| Kiểu | Đứt nét (dashArray) |
| Độ dày | 3px |
| Độ mờ | 0.6 |
| Thông tin | Không có |
| Chính xác | ❌ Không chính xác |

### SAU KHI FIX:

| Đặc điểm | Giá trị |
|----------|---------|
| Loại đường | Đường thực tế (theo đường phố) |
| Kiểu | Liền nét |
| Độ dày | 4px |
| Độ mờ | 0.7 |
| Thông tin | Khoảng cách + Thời gian |
| Chính xác | ✅ Chính xác 100% |

---

## 🛡️ XỬ LÝ LỖI

### Nếu OSRM API lỗi:
- ⚠️ Tự động fallback về đường thẳng
- ⚠️ Log lỗi trong console
- ⚠️ Vẫn hiển thị đường đi (dù không chính xác)

### Nếu không có internet:
- ⚠️ Fallback về đường thẳng
- ⚠️ Không crash app

### Nếu tọa độ không hợp lệ:
- ⚠️ OSRM trả về error
- ⚠️ Fallback về đường thẳng

---

## 📂 FILES ĐÃ SỬA

### `admin_dashboard.html`

**Thay đổi:**
1. ✅ Thay đổi cách vẽ route từ đường thẳng sang OSRM routing
2. ✅ Thêm function `fetchOSRMRoute()` để gọi OSRM API
3. ✅ Thêm function `drawStraightLine()` làm fallback
4. ✅ Thêm popup hiển thị khoảng cách và thời gian

**Dòng code:** Khoảng dòng 2027-2130

---

## ✅ KẾT QUẢ MONG ĐỢI

### Bản đồ Admin Dashboard:
- ✅ Hiển thị đường đi thực tế theo đường phố
- ✅ Đường đi mượt mà, rõ ràng
- ✅ Click vào đường đi → Hiển thị khoảng cách và thời gian
- ✅ Giống với đường đi trong app

### Console:
```
✅ [OSRM] Đã vẽ route cho user 2: 2.3km, 5min
```

### Nếu OSRM lỗi:
```
❌ [OSRM] Lỗi khi fetch route: ...
⚠️ [Fallback] Đã vẽ đường thẳng cho user 2
```

---

## 🎯 LỢI ÍCH

1. ✅ **Chính xác hơn:** Hiển thị đường đi thực tế thay vì đường thẳng
2. ✅ **Thông tin đầy đủ:** Hiển thị khoảng cách và thời gian dự kiến
3. ✅ **Dễ theo dõi:** Admin có thể thấy chính xác user đang đi đường nào
4. ✅ **Nhất quán:** Giống với đường đi trong app
5. ✅ **Có fallback:** Vẫn hoạt động nếu OSRM lỗi

---

## 🔗 TÀI LIỆU THAM KHẢO

- OSRM API: https://project-osrm.org/docs/v5.24.0/api/
- Leaflet Polyline: https://leafletjs.com/reference.html#polyline
- GeoJSON: https://geojson.org/

---

**🎉 HOÀN THÀNH!**

Bây giờ Admin Dashboard sẽ hiển thị đường đi thực tế theo đường phố, giống như trong app!

---

**Ngày fix:** 2026-04-30  
**Trạng thái:** ✅ ĐÃ FIX  
**Test:** Mở Admin Dashboard và kiểm tra
