# Sửa Lỗi Map - Đường Đi Thực Tế Thay Vì Đường Chim Bay

## Vấn Đề
Map hiện tại hiển thị đường chim bay (straight line) thay vì đường đi thực tế theo đường phố.

## Nguyên Nhân
- API routing (OSRM) có timeout quá ngắn (8 giây)
- Không có fallback API khi OSRM thất bại
- Khi API thất bại, app tự động dùng đường chim bay mà không thông báo rõ ràng

## Giải Pháp Đã Áp Dụng

### 1. Tăng Timeout và Cải Thiện Error Handling
- **Trước:** Timeout 8 giây
- **Sau:** Timeout 15 giây cho mỗi API

### 2. Thêm Fallback API
Hệ thống routing mới có 2 tầng:

**Tầng 1: OSRM (Open Source Routing Machine)**
- URL: `https://router.project-osrm.org/route/v1/driving/`
- Ưu điểm: Nhanh, chính xác, tốt cho Việt Nam
- Timeout: 15 giây

**Tầng 2: OpenRouteService (Fallback)**
- URL: `https://api.openrouteservice.org/v2/directions/driving-car`
- Ưu điểm: Ổn định, có dữ liệu toàn cầu
- Timeout: 15 giây
- Tự động convert format từ ORS sang OSRM để tương thích

**Tầng 3: Đường Chim Bay (Last Resort)**
- Chỉ dùng khi cả 2 API đều thất bại
- Hiển thị rõ ràng là "đường chim bay - không chính xác"
- Đường line màu đỏ, nét đứt (dash) để phân biệt

### 3. Cải Thiện UI/UX

#### Đường Đi Thực Tế (API thành công)
- **Màu:** Xanh dương (#1A73E8)
- **Độ dày:** 6px
- **Kiểu:** Nét liền
- **Hiển thị:** "🚗 X.X km • XX phút"

#### Đường Chim Bay (API thất bại)
- **Màu:** Đỏ (#EA4335)
- **Độ dày:** 6px
- **Kiểu:** Nét đứt (dashArray: '10, 10')
- **Opacity:** 0.6 (mờ hơn)
- **Hiển thị:** "⚠️ ~X.X km • ~XX phút (đường chim bay - không chính xác)"

### 4. Logging Chi Tiết
```javascript
console.log('🌐 [Route] Thử OSRM API...');
console.log('✅ [Route] OSRM thành công trong X ms');
console.warn('⚠️ [Route] OSRM failed - Thử OpenRouteService...');
console.log('✅ [Route] OpenRouteService thành công trong X ms');
console.error('❌ [Route] Cả 2 API đều thất bại. Fallback về đường chim bay');
```

## Kết Quả

### Trước Khi Sửa
❌ Luôn hiển thị đường chim bay  
❌ Không rõ ràng là đường không chính xác  
❌ Timeout quá ngắn (8s)  
❌ Không có fallback API  

### Sau Khi Sửa
✅ Ưu tiên đường đi thực tế theo đường phố  
✅ Có 2 API routing để đảm bảo độ tin cậy  
✅ Timeout đủ dài (15s mỗi API)  
✅ Hiển thị rõ ràng khi dùng đường chim bay  
✅ Đường chim bay có màu đỏ, nét đứt để phân biệt  
✅ Logging chi tiết để debug  

## Cách Kiểm Tra

1. **Mở MapWindow** và chọn một quán ăn
2. **Quan sát đường line:**
   - **Xanh dương, nét liền** = Đường đi thực tế ✅
   - **Đỏ, nét đứt** = Đường chim bay (API thất bại) ⚠️
3. **Kiểm tra subtitle:**
   - "🚗 X.X km • XX phút" = Thành công
   - "⚠️ ~X.X km • ~XX phút (đường chim bay - không chính xác)" = Fallback
4. **Mở F12 Developer Tools** để xem console logs

## Lưu Ý Kỹ Thuật

### API Routing
- **OSRM:** Miễn phí, không cần API key, giới hạn rate limit
- **OpenRouteService:** Miễn phí, không cần API key (có thể cần key cho production)
- **Cả 2 API** đều hỗ trợ turn-by-turn navigation với steps chi tiết

### Format Conversion
OpenRouteService trả về format khác OSRM, code đã tự động convert:
```javascript
routeSteps = steps.map(function(s){
  return {
    maneuver: {
      type: s.type===0?'depart':(s.type===10?'arrive':'turn'),
      modifier: s.type===1?'left':(s.type===2?'right':'straight'),
      location: [s.way_points[0][0], s.way_points[0][1]]
    },
    name: s.name||'',
    distance: s.distance
  };
});
```

### Cache
Route được cache theo key: `startLat,startLng->destLat,destLng`
- Giảm số lần gọi API
- Tăng tốc độ load
- Cache trong memory (mất khi reload page)

## File Đã Sửa
- `VietnamFoodGuide/Views/MapWindow.xaml.cs`
  - Hàm `calculateRoute()` - Thêm fallback API và cải thiện error handling
  - Hàm `drawOfflineRoute()` - Cải thiện UI cho đường chim bay

## Build Status
✅ Build thành công  
✅ Không có lỗi compile  
✅ Sẵn sàng để test  

## Hướng Dẫn Test
1. Đóng app đang chạy
2. Rebuild project
3. Chạy app
4. Mở MapWindow và chọn quán ăn
5. Kiểm tra đường line có màu xanh dương và theo đường phố không
6. Nếu thấy đường đỏ nét đứt = API thất bại, cần kiểm tra kết nối internet

## Tương Lai
Nếu cần độ tin cậy cao hơn, có thể:
- Đăng ký API key cho OpenRouteService (5000 requests/day miễn phí)
- Thêm GraphHopper API làm tầng fallback thứ 3
- Tự host OSRM server riêng cho Việt Nam
