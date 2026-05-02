# Sửa Lỗi Kết Nối API - "< is an invalid start of a value"

## Lỗi Gặp Phải

```
Error: Lỗi kết nối: '<' is an invalid start of a value. 
Path: $ | LineNumber: 0 | BytePositionInLine: 0.
```

## Nguyên Nhân

Lỗi này xảy ra khi API trả về **HTML** thay vì **JSON**. Các nguyên nhân phổ biến:

1. **XAMPP không chạy** → API không khả dụng
2. **Ngrok URL đã hết hạn** → Không kết nối được đến API
3. **Database chưa được tạo** → API trả về error page
4. **PHP syntax error** → API crash và trả về HTML error

## Giải Pháp Đã Áp Dụng

### 1. Cải Thiện Error Handling

#### File: `VietnamFoodGuide/Services/FavoritesApiService.cs`

Thêm kiểm tra xem response có phải HTML không:

```csharp
// Check if response is HTML (error page)
if (responseText.TrimStart().StartsWith("<") || responseText.Contains("<!DOCTYPE"))
{
    System.Diagnostics.Debug.WriteLine($"[AddFavorite] ERROR: API returned HTML instead of JSON");
    return (false, "Lỗi kết nối API. Vui lòng kiểm tra:\n1. XAMPP đã bật chưa?\n2. Ngrok URL còn hoạt động không?\n3. Database đã tạo chưa?");
}
```

#### File: `VietnamFoodGuide/Services/ApiFoodService.cs`

Thêm logging chi tiết và fallback tốt hơn:

```csharp
// Check if response is HTML (error page)
if (json.TrimStart().StartsWith("<") || json.Contains("<!DOCTYPE"))
{
    System.Diagnostics.Debug.WriteLine("[ApiFoodService] ERROR: API returned HTML instead of JSON");
    System.Diagnostics.Debug.WriteLine("[ApiFoodService] Possible causes:");
    System.Diagnostics.Debug.WriteLine("  1. XAMPP is not running");
    System.Diagnostics.Debug.WriteLine("  2. Ngrok URL expired");
    System.Diagnostics.Debug.WriteLine("  3. Database not created");
    System.Diagnostics.Debug.WriteLine("[ApiFoodService] Fallback to foods.json");
    return _fallbackService.LoadFoods();
}
```

### 2. Better Exception Handling

Phân biệt các loại lỗi:
- `HttpRequestException` → Không kết nối được
- `JsonException` → Response không phải JSON
- `Exception` → Lỗi khác

## Cách Khắc Phục

### Bước 1: Kiểm Tra XAMPP

1. Mở **XAMPP Control Panel**
2. Đảm bảo **Apache** và **MySQL** đang chạy (màu xanh)
3. Nếu chưa chạy, nhấn **Start** cho cả 2

### Bước 2: Kiểm Tra Database

1. Mở **phpMyAdmin**: http://localhost/phpmyadmin
2. Kiểm tra database `VietnamFoodGuide` đã tồn tại chưa
3. Nếu chưa có, chạy file `xampp_api/setup_complete.sql`:
   ```sql
   -- Import file này vào phpMyAdmin
   ```

### Bước 3: Kiểm Tra Ngrok

1. Mở Command Prompt
2. Chạy ngrok:
   ```bash
   ngrok http 80
   ```
3. Copy URL mới (ví dụ: `https://abcd-1234.ngrok-free.app`)
4. Cập nhật `AppConfig.cs`:
   ```csharp
   public static readonly string Domain = "https://YOUR-NEW-URL.ngrok-free.app/vfg-api";
   ```

### Bước 4: Test API Trực Tiếp

Mở trình duyệt và test:

**Test 1: Get Foods**
```
https://YOUR-NGROK-URL.ngrok-free.app/vfg-api/api.php?action=foods
```
**Kỳ vọng:** Trả về JSON array của foods

**Test 2: Check Connection**
```
http://localhost/vfg-api/api.php?action=foods
```
**Kỳ vọng:** Trả về JSON array của foods

### Bước 5: Rebuild và Test

1. Đóng app hiện tại
2. Rebuild project
3. Chạy lại app
4. Kiểm tra Output window để xem logs

## Debug Logs

Khi chạy app, mở **Output** window trong Visual Studio để xem logs:

### Logs Thành Công:
```
[ApiFoodService] Loading foods from API: https://xxx.ngrok-free.app/vfg-api/api.php?action=foods
[ApiFoodService] API Response (first 200 chars): [{"Id":1,"Name":"Bánh Mì...
[ApiFoodService] Successfully loaded 20 foods from API
```

### Logs Lỗi (HTML Response):
```
[ApiFoodService] ERROR: API returned HTML instead of JSON
[ApiFoodService] Possible causes:
  1. XAMPP is not running
  2. Ngrok URL expired
  3. Database not created
[ApiFoodService] Fallback to foods.json
```

### Logs Lỗi (Connection Failed):
```
[ApiFoodService] HTTP Error: No connection could be made...
[ApiFoodService] Cannot connect to API. Check XAMPP and ngrok.
[ApiFoodService] Fallback to foods.json
```

## Fallback Mechanism

Khi API không khả dụng, app tự động fallback về `foods.json`:

✅ **Ưu điểm:**
- App vẫn hoạt động offline
- Người dùng vẫn xem được danh sách món ăn
- Không bị crash

⚠️ **Hạn chế:**
- Không thêm/sửa/xóa được dữ liệu
- Không sync được favorites với database
- Dữ liệu có thể cũ

## Checklist Khắc Phục

- [ ] XAMPP đang chạy (Apache + MySQL)
- [ ] Database `VietnamFoodGuide` đã tạo
- [ ] Ngrok đang chạy và URL còn hiệu lực
- [ ] AppConfig.cs đã cập nhật URL mới
- [ ] Test API trực tiếp trên browser thành công
- [ ] Rebuild project
- [ ] Kiểm tra Output logs

## Kết Quả

### Trước Khi Sửa:
- ❌ App crash với lỗi JSON parse
- ❌ Không có thông báo lỗi rõ ràng
- ❌ Không fallback về JSON

### Sau Khi Sửa:
- ✅ App không crash
- ✅ Thông báo lỗi rõ ràng
- ✅ Tự động fallback về foods.json
- ✅ Logs chi tiết để debug
- ✅ Người dùng vẫn sử dụng được app

## Build Status
✅ Build thành công  
✅ Không có lỗi compile  
✅ Sẵn sàng để test  

## Files Đã Sửa
- `VietnamFoodGuide/Services/FavoritesApiService.cs` - Better error handling
- `VietnamFoodGuide/Services/ApiFoodService.cs` - HTML detection + detailed logging
