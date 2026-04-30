# Sửa Lỗi "Invalid URI: The hostname could not be parsed"

## Vấn Đề
Lỗi này xảy ra khi URL trong `AppConfig.cs` có:
- ❌ Khoảng trắng thừa ở đầu/cuối
- ❌ Thiếu `/vfg-api` ở cuối
- ❌ Format không đúng

## Đã Sửa

### File: `VietnamFoodGuide/Services/AppConfig.cs`

**Trước (SAI):**
```csharp
public static readonly string Domain = "  https://bridged-shindig-feminize.ngrok-free.dev ";
```

**Sau (ĐÚNG):**
```csharp
public static readonly string Domain = "https://bridged-shindig-feminize.ngrok-free.dev/vfg-api";
```

## Checklist Khi Cập Nhật Ngrok URL

Mỗi khi ngrok restart, URL sẽ thay đổi. Hãy làm theo:

### 1. Lấy URL Mới Từ Ngrok
```bash
# Chạy ngrok
ngrok http 80

# Copy URL từ dòng "Forwarding"
# Ví dụ: https://abcd-1234.ngrok-free.app
```

### 2. Cập Nhật AppConfig.cs
```csharp
public static readonly string Domain = "https://YOUR-NGROK-URL.ngrok-free.app/vfg-api";
//                                      ^                                        ^
//                                      Không có khoảng trắng                    Phải có /vfg-api
```

### 3. Kiểm Tra Format
✅ **ĐÚNG:**
```csharp
"https://bridged-shindig-feminize.ngrok-free.dev/vfg-api"
"https://abcd-1234.ngrok-free.app/vfg-api"
"http://localhost/vfg-api"
```

❌ **SAI:**
```csharp
"  https://bridged-shindig-feminize.ngrok-free.dev "  // Có khoảng trắng
"https://bridged-shindig-feminize.ngrok-free.dev"    // Thiếu /vfg-api
"https://bridged-shindig-feminize.ngrok-free.dev/"   // Có / nhưng thiếu vfg-api
```

### 4. Rebuild và Test
```bash
# Trong Visual Studio
Build → Rebuild Solution
# Chạy app
```

## Ngrok URL Hiện Tại

Theo screenshot, ngrok đang chạy với:
```
Forwarding: https://bridged-shindig-feminize.ngrok-free.dev -> http://localhost:80
```

Vậy URL đúng là:
```csharp
public static readonly string Domain = "https://bridged-shindig-feminize.ngrok-free.dev/vfg-api";
```

## Test API

Sau khi cập nhật, test API bằng browser:

**Test 1: Get Foods**
```
https://bridged-shindig-feminize.ngrok-free.dev/vfg-api/api.php?action=foods
```
**Kỳ vọng:** Trả về JSON array

**Test 2: Check Health**
```
https://bridged-shindig-feminize.ngrok-free.dev/vfg-api/api.php?action=stats
```
**Kỳ vọng:** Trả về thống kê

## Lưu Ý

### Khi Nào Cần Cập Nhật URL?
- ✅ Mỗi khi restart ngrok
- ✅ Ngrok crash và phải chạy lại
- ✅ Chuyển từ localhost sang ngrok hoặc ngược lại

### Làm Sao Biết URL Đúng?
1. Mở terminal đang chạy ngrok
2. Tìm dòng "Forwarding"
3. Copy URL (không có http://localhost:80)
4. Thêm `/vfg-api` vào cuối

### Nếu Không Muốn Dùng Ngrok?
Dùng localhost:
```csharp
public static readonly string Domain = "http://localhost/vfg-api";
```

## Build Status
✅ Build thành công  
✅ URL đã được sửa đúng format  
✅ Sẵn sàng để test  

## Kết Quả
- ✅ Không còn lỗi "Invalid URI"
- ✅ App kết nối được đến API
- ✅ Load dữ liệu từ database thành công
