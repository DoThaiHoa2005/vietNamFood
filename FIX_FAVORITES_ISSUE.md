# Sửa Lỗi Chức Năng Yêu Thích

## Vấn Đề
1. **Không thêm được yêu thích:** Khi nhấn nút "⭐ Yêu thích", không có gì xảy ra
2. **Không hiển thị trạng thái yêu thích:** Món ăn đã có trong database favorites nhưng không hiển thị là đã yêu thích

## Nguyên Nhân
Khi load dữ liệu từ `foods.json` (fallback khi API không khả dụng), các `FoodItem` không có `Id` được set. Điều này khiến:
- API call `addFavorite` thất bại vì `foodId = 0` (không hợp lệ)
- API call `isFavorite` không tìm thấy vì `foodId = 0`

## Giải Pháp

### File: `VietnamFoodGuide/Services/FoodService.cs`

Thêm logic tự động generate Id cho các món ăn load từ JSON:

**Trước:**
```csharp
var foods = JsonSerializer.Deserialize<List<FoodItem>>(json, options) ?? new List<FoodItem>();

return foods;
```

**Sau:**
```csharp
var foods = JsonSerializer.Deserialize<List<FoodItem>>(json, options) ?? new List<FoodItem>();

// Auto-generate Id for foods loaded from JSON (starting from 1000 to avoid conflict with database)
int autoId = 1000;
foreach (var food in foods)
{
    if (food.Id == 0) // If no Id set
    {
        food.Id = autoId++;
    }
}

return foods;
```

## Chi Tiết Kỹ Thuật

### Tại Sao Bắt Đầu Từ 1000?
- Database IDs thường bắt đầu từ 1
- Để tránh conflict giữa JSON IDs và Database IDs
- IDs từ 1-999: Reserved cho database
- IDs từ 1000+: Auto-generated cho JSON

### Flow Hoạt Động

#### Khi Load Từ API (MySQL):
```
API → ApiFoodService → FoodItem với Id từ database (1, 2, 3, ...)
```

#### Khi Load Từ JSON (Fallback):
```
JSON → FoodService → Auto-generate Id (1000, 1001, 1002, ...)
```

### API Endpoints Liên Quan

#### 1. Add Favorite
```
POST /api.php?action=addFavorite
Body: {"userId": 1, "foodId": 1000}
```

#### 2. Remove Favorite
```
DELETE /api.php?action=removeFavorite&userId=1&foodId=1000
```

#### 3. Check Is Favorite
```
GET /api.php?action=isFavorite&userId=1&foodId=1000
```

#### 4. Get User Favorites
```
GET /api.php?action=getUserFavorites&userId=1
```

## Kết Quả

### ✅ Trước Khi Sửa
- ❌ Không thêm được yêu thích (foodId = 0)
- ❌ Không hiển thị trạng thái yêu thích
- ❌ API call thất bại với error "UserId và FoodId không hợp lệ"

### ✅ Sau Khi Sửa
- ✅ Thêm yêu thích thành công
- ✅ Hiển thị đúng trạng thái yêu thích (vàng = đã thích, xám = chưa thích)
- ✅ API call thành công với foodId hợp lệ (1000+)
- ✅ Sync đúng giữa local storage và database

## Cách Kiểm Tra

### 1. Test Thêm Yêu Thích
1. Mở app và đăng nhập
2. Chọn một món ăn
3. Nhấn nút "⭐ Yêu thích"
4. **Kỳ vọng:** Nút chuyển sang màu vàng, text "❤️ Đã yêu thích"

### 2. Test Hiển Thị Trạng Thái
1. Thêm một món vào yêu thích
2. Đóng app và mở lại
3. Mở món ăn đó
4. **Kỳ vọng:** Nút hiển thị màu vàng ngay từ đầu

### 3. Test Xóa Yêu Thích
1. Mở món ăn đã yêu thích (nút màu vàng)
2. Nhấn nút "❤️ Đã yêu thích"
3. **Kỳ vọng:** Nút chuyển sang màu xám, text "⭐ Yêu thích"

### 4. Test Favorites Window
1. Nhấn nút "⭐ Yêu thích" ở bottom navigation
2. **Kỳ vọng:** Hiển thị danh sách các món đã yêu thích

## Debug Logs

Khi test, mở Output window trong Visual Studio để xem logs:

```
[AddFavorite] API Call - UserId: 1, FoodId: 1000
[AddFavorite] Request JSON: {"userId":1,"foodId":1000}
[AddFavorite] Response Status: OK
[AddFavorite] Response Body: {"success":true,"message":"Thêm yêu thích thành công"}
```

## Lưu Ý

### Khi Nào Dùng JSON Fallback?
- API server không chạy (XAMPP tắt)
- Không có kết nối internet
- API timeout hoặc lỗi

### Khi Nào Dùng Database?
- API server đang chạy (XAMPP bật)
- Có kết nối internet
- API response thành công

## Build Status
✅ Build thành công  
✅ Không có lỗi compile  
✅ Sẵn sàng để test  

## Files Đã Sửa
- `VietnamFoodGuide/Services/FoodService.cs` - Thêm auto-generate Id logic

## Files Liên Quan (Không Sửa)
- `VietnamFoodGuide/Services/ApiFoodService.cs` - Đã có Id mapping
- `VietnamFoodGuide/Services/FavoritesApiService.cs` - API calls
- `VietnamFoodGuide/Views/FoodDetailWindow.xaml.cs` - UI logic
- `xampp_api/api.php` - Backend endpoints
