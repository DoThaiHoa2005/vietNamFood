# 🧪 HƯỚNG DẪN KIỂM TRA HIỂN THỊ ẢNH

## ✅ Đã hoàn thành

### 1. Sửa code
- ✅ `SQLiteFavoritesService.cs` - Sửa logic GetUserFavorites()
- ✅ `StringToImageSourceConverter.cs` - Tạo converter mới
- ✅ `MainWindow.xaml` - Thêm converter
- ✅ `FavoritesWindow.xaml` - Thêm converter
- ✅ `FoodDetailWindow.xaml` - Thêm converter
- ✅ `FoodDetailWindow.xaml.cs` - Sửa code-behind

### 2. Kiểm tra files
- ✅ Build thành công (no errors)
- ✅ No diagnostics errors
- ✅ 12 ảnh tồn tại: `Q1.jpg` - `Q12.jpg`
- ✅ Đường dẫn trong code: `Assets/Images/Q*.jpg` (đúng format)

## 🧪 Các bước kiểm tra

### Bước 1: Build và chạy app

```powershell
# Build project
cd "E:\IT\C#\Đồ án c#\VietnamFoodGuide"
dotnet build "VietnamFoodGuide\VietnamFoodGuide.csproj" --configuration Debug

# Chạy app
.\VietnamFoodGuide\bin\Debug\net48\VietnamFoodGuide.exe
```

### Bước 2: Kiểm tra MainWindow

**Mục tiêu**: Xác nhận ảnh quán hiển thị đúng

1. Mở app → MainWindow hiển thị
2. Kiểm tra danh sách quán ăn
3. **Kỳ vọng**: 
   - ✅ Mỗi quán có ảnh thumbnail bên trái
   - ✅ Ảnh hiển thị rõ ràng, không bị lỗi
   - ✅ Không có background màu xám trống

**Screenshot vị trí kiểm tra**:
```
┌─────────────────────────────────┐
│  [ẢNH]  │ Alo Quán - Seafood    │
│  Q1.jpg │ 📍 TP.HCM - Vĩnh Khánh│
│         │ ⭐ 4.5                 │
│         │ [Xem chi tiết]        │
└─────────────────────────────────┘
```

### Bước 3: Thêm vào yêu thích

**Mục tiêu**: Tạo dữ liệu test cho FavoritesWindow

1. Click vào quán bất kỳ (ví dụ: "Alo Quán - Seafood")
2. Trong FoodDetailWindow, click nút "⭐ Yêu thích"
3. Thêm 2-3 quán khác vào yêu thích
4. **Kỳ vọng**:
   - ✅ Nút đổi màu vàng: "⭐ Bỏ yêu thích"
   - ✅ Không có lỗi

### Bước 4: Kiểm tra FavoritesWindow

**Mục tiêu**: Xác nhận ảnh hiển thị trong danh sách yêu thích

1. Quay lại MainWindow (click "← Quay lại")
2. Click nút "⭐ Yêu thích" ở bottom navigation
3. FavoritesWindow mở ra
4. **Kỳ vọng**:
   - ✅ Danh sách các quán đã thêm vào yêu thích
   - ✅ Mỗi quán có ảnh thumbnail bên trái
   - ✅ Ảnh hiển thị rõ ràng (KHÔNG còn background xám trống)
   - ✅ Layout giống MainWindow

**Screenshot vị trí kiểm tra**:
```
┌─────────────────────────────────┐
│ ⭐ Yêu thích của tôi            │
│ 3 quán ăn                       │
├─────────────────────────────────┤
│  [ẢNH]  │ Alo Quán - Seafood    │ ← ẢNH PHẢI HIỂN THỊ
│  Q1.jpg │ 📍 TP.HCM - Vĩnh Khánh│
│         │ ⭐ 4.5                 │
│         │ [Xem chi tiết]        │
├─────────────────────────────────┤
│  [ẢNH]  │ Quán Ốc Đào 2         │ ← ẢNH PHẢI HIỂN THỊ
│  Q2.jpg │ 📍 TP.HCM - Vĩnh Khánh│
│         │ ⭐ 4.3                 │
│         │ [Xem chi tiết]        │
└─────────────────────────────────┘
```

### Bước 5: Kiểm tra FoodDetailWindow

**Mục tiêu**: Xác nhận ảnh hero hiển thị

1. Trong FavoritesWindow, click "Xem chi tiết" của quán bất kỳ
2. FoodDetailWindow mở ra
3. **Kỳ vọng**:
   - ✅ Ảnh hero lớn hiển thị ở đầu trang
   - ✅ Ảnh chiếm toàn bộ chiều rộng
   - ✅ Không có background xám trống

**Screenshot vị trí kiểm tra**:
```
┌─────────────────────────────────┐
│ ← Quay lại    Chi tiết món ăn   │
├─────────────────────────────────┤
│                                 │
│     [ẢNH HERO LỚN - Q1.jpg]    │ ← ẢNH PHẢI HIỂN THỊ
│                                 │
│         ⭐ 4.5                   │
├─────────────────────────────────┤
│ Alo Quán - Seafood              │
│ 📍 TP.HCM - Vĩnh Khánh          │
│                                 │
│ MÔ TẢ                           │
│ Địa chỉ đầu tiên trên phố...    │
└─────────────────────────────────┘
```

## ❌ Các lỗi có thể gặp

### Lỗi 1: Ảnh không hiển thị (background xám)

**Nguyên nhân**:
- Converter chưa được đăng ký trong XAML
- Image binding chưa sử dụng converter

**Giải pháp**:
```xml
<!-- Kiểm tra trong Window.Resources -->
<converters:StringToImageSourceConverter x:Key="ImageConverter"/>

<!-- Kiểm tra trong Image binding -->
<Image Source="{Binding Image, Converter={StaticResource ImageConverter}}" />
```

### Lỗi 2: Database không tồn tại

**Nguyên nhân**:
- Lần đầu chạy app, database chưa được tạo

**Giải pháp**:
- App sẽ tự động tạo database khi khởi động
- Đợi vài giây để database được khởi tạo
- Nếu vẫn lỗi, xóa file `foods.db` và `favorites.db` trong `bin\Debug\net48\` rồi chạy lại

### Lỗi 3: FavoritesWindow trống

**Nguyên nhân**:
- Chưa thêm quán nào vào yêu thích
- Hoặc đang ở chế độ Guest (chưa login)

**Giải pháp**:
- Thêm ít nhất 1 quán vào yêu thích từ MainWindow
- Nếu đã thêm mà vẫn trống, kiểm tra console log

## 🔍 Debug tips

### Kiểm tra console log

Trong Visual Studio, mở Output window (View → Output) và tìm các dòng log:

```
[SQLiteFoodService] Database initialized
[SQLiteFoodService] Seeded 12 foods
[SQLiteFavoritesService] Database initialized
[ImageConverter] Loading image: Assets/Images/Q1.jpg
```

### Kiểm tra file ảnh

```powershell
# Kiểm tra ảnh trong source
Get-ChildItem "VietnamFoodGuide\Assets\Images\Q*.jpg"

# Kiểm tra ảnh trong bin (sau build)
Get-ChildItem "VietnamFoodGuide\bin\Debug\net48\Assets\Images\Q*.jpg"
```

Kỳ vọng: 12 files (Q1.jpg - Q12.jpg)

### Kiểm tra database

```powershell
# Kiểm tra database tồn tại
Test-Path "VietnamFoodGuide\bin\Debug\net48\foods.db"
Test-Path "VietnamFoodGuide\bin\Debug\net48\favorites.db"
```

## ✅ Checklist hoàn chỉnh

- [ ] Build thành công (no errors)
- [ ] App khởi động được
- [ ] MainWindow hiển thị 12 quán với ảnh
- [ ] Click vào quán → FoodDetailWindow hiển thị ảnh hero
- [ ] Thêm quán vào yêu thích thành công
- [ ] FavoritesWindow hiển thị danh sách với ảnh
- [ ] Click "Xem chi tiết" trong FavoritesWindow → FoodDetailWindow hiển thị ảnh

## 🎯 Kết quả mong đợi

**TẤT CẢ 3 WINDOWS ĐỀU HIỂN THỊ ẢNH CHÍNH XÁC**

1. ✅ **MainWindow**: Ảnh thumbnail trong danh sách quán
2. ✅ **FavoritesWindow**: Ảnh thumbnail trong danh sách yêu thích
3. ✅ **FoodDetailWindow**: Ảnh hero lớn ở đầu trang

---
**Ngày tạo**: 2026-05-02
**Trạng thái**: ✅ SẴN SÀNG KIỂM TRA
