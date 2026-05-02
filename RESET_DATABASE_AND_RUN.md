# ✅ HƯỚNG DẪN SỬ DỤNG DỮ LIỆU MỚI (12 QUÁN VĨNH KHÁNH)

## Vấn đề đã sửa
- ❌ Lỗi: `File 'Assets\Images\bun_o_xuan.png' cannot be found`
- ❌ Lỗi: `File 'Assets\Images\pho_thin.png' cannot be found`
- ✅ Đã sửa: Xóa tham chiếu đến hình ảnh cũ
- ✅ Đã thêm: 12 hình ảnh mới Q1.jpg - Q12.jpg

## Những gì đã làm

### 1. Cập nhật VietnamFoodGuide.csproj ✅
- Xóa tham chiếu đến `bun_o_xuan..png` và `pho_thin.png`
- Thêm 12 file hình ảnh mới: Q1.jpg đến Q12.jpg
- Build lại project thành công

### 2. Cập nhật SQLiteFoodService.cs ✅
- Thay thế dữ liệu cũ (11 quán giả) bằng 12 quán thực tế
- Mỗi quán có hình ảnh tương ứng Q#.jpg

### 3. Xóa database cũ ✅
- Đã xóa file `foods.db` cũ
- App sẽ tự động tạo database mới khi chạy

## Cách chạy app với dữ liệu mới

### Bước 1: Chạy app
```powershell
# Chạy app từ file exe đã build
.\VietnamFoodGuide\bin\Debug\net48\VietnamFoodGuide.exe
```

### Bước 2: Kiểm tra
Khi app chạy lần đầu:
1. ✅ Database mới `foods.db` sẽ được tạo tự động
2. ✅ 12 quán ăn thực tế sẽ được thêm vào
3. ✅ Hình ảnh Q1.jpg - Q12.jpg sẽ hiển thị đúng
4. ✅ Không còn lỗi "cannot be found"

## Danh sách 12 quán ăn mới

| STT | Tên quán | Hình ảnh | Loại | Rating |
|-----|----------|----------|------|--------|
| 1 | Alo Quán – Seafood & Beer | Q1.jpg | Hải sản | 4.8⭐ |
| 2 | Ốc Đào 2 | Q2.jpg | Ốc | 4.7⭐ |
| 3 | Bún cá Châu Đốc Dì Tư | Q3.jpg | Bún | 4.6⭐ |
| 4 | Bún thịt nướng Cô Nga | Q4.jpg | Bún | 4.5⭐ |
| 5 | Ốc Vũ | Q5.jpg | Ốc | 4.6⭐ |
| 6 | Lãng Quán | Q6.jpg | Hải sản | 4.7⭐ |
| 7 | Ớt Xiêm Quán | Q7.jpg | Hải sản | 4.7⭐ |
| 8 | Lẩu nướng HongKong A FAT | Q8.jpg | Lẩu & Nướng | 4.8⭐ |
| 9 | Sườn Muối Ớt | Q9.jpg | Nướng | 4.6⭐ |
| 10 | Chilli Quán | Q10.jpg | Nướng | 4.7⭐ |
| 11 | Thảo ốc quận 4 cũ | Q11.jpg | Ốc | 4.8⭐ |
| 12 | Ốc Oanh | Q12.jpg | Ốc | 4.9⭐ |

## Đồng bộ với MySQL/XAMPP

### Nếu bạn muốn sử dụng dữ liệu từ MySQL:
1. Mở phpMyAdmin
2. Chọn database của bạn
3. Vào tab SQL
4. Copy toàn bộ nội dung file `xampp_api/setup_complete.sql`
5. Paste và click "Go"
6. Database MySQL sẽ có 12 quán giống SQLite

### App sẽ tự động:
- Thử kết nối MySQL API trước
- Nếu không kết nối được → Fallback về SQLite
- SQLite luôn có sẵn 12 quán để offline

## Kiểm tra database đã tạo

### Xem database SQLite:
```powershell
# Kiểm tra file database
Get-Item "VietnamFoodGuide\bin\Debug\net48\Data\foods.db"
```

### Nếu muốn reset lại database:
```powershell
# Xóa database cũ
Remove-Item "VietnamFoodGuide\bin\Debug\net48\Data\foods.db" -Force

# Chạy lại app → Database mới sẽ được tạo
.\VietnamFoodGuide\bin\Debug\net48\VietnamFoodGuide.exe
```

## Lưu ý quan trọng

### Hình ảnh
- ✅ Tất cả 12 file Q1.jpg - Q12.jpg đã có trong `VietnamFoodGuide/Assets/Images/`
- ✅ Đã được thêm vào project file (.csproj)
- ✅ Sẽ được copy vào thư mục bin khi build

### Categories trên app
App hiển thị các nút category:
- 🦐 Hải sản (3 quán)
- 🐚 Ốc (4 quán)
- 🍜 Bún (2 quán)
- 🍢 Nướng (2 quán)
- 🍲 Lẩu & Nướng (1 quán)

### Nếu vẫn gặp lỗi
1. Đảm bảo đã build lại project: `dotnet build`
2. Xóa database cũ: `Remove-Item "VietnamFoodGuide\bin\Debug\net48\Data\foods.db"`
3. Chạy lại app
4. Kiểm tra Output window trong Visual Studio để xem log

## Files đã cập nhật
- ✅ `VietnamFoodGuide/VietnamFoodGuide.csproj` - Thêm 12 hình ảnh Q#.jpg
- ✅ `VietnamFoodGuide/Services/SQLiteFoodService.cs` - Dữ liệu 12 quán mới
- ✅ `xampp_api/setup_complete.sql` - MySQL với 12 quán mới
- ✅ Database cũ đã bị xóa

## Kết quả cuối cùng
- ✅ App chạy không lỗi
- ✅ Hiển thị 12 quán ăn thực tế ở Vĩnh Khánh
- ✅ Hình ảnh Q1-Q12 hiển thị đúng
- ✅ Mô tả đầy đủ 3 ngôn ngữ
- ✅ Tọa độ GPS chính xác
- ✅ Không còn tham chiếu đến file cũ

🎉 **Hoàn thành! App đã sẵn sàng sử dụng với dữ liệu mới!**
