# 🧪 HƯỚNG DẪN TEST: Ảnh trong Favorites Window

## 🎯 Mục đích test

Kiểm tra xem ảnh quán có hiển thị đúng trong FavoritesWindow sau khi:
1. ✅ Bỏ nút "Xem chi tiết"
2. ✅ Cập nhật StringToImageSourceConverter
3. ✅ Thêm ImageCacheService

---

## 📋 Các bước test

### Bước 1: Chạy app

```powershell
cd "E:\IT\C#\Đồ án c#\VietnamFoodGuide"
.\VietnamFoodGuide\bin\Debug\net48\VietnamFoodGuide.exe
```

### Bước 2: Thêm quán vào yêu thích

1. App mở → MainWindow hiển thị
2. Scroll xuống xem danh sách 12 quán
3. **Kiểm tra**: Tất cả quán đều có ảnh ✅
4. Click vào quán bất kỳ (ví dụ: "Alo Quán - Seafood")
5. Trong FoodDetailWindow, click "⭐ Yêu thích"
6. Thêm 2-3 quán khác vào yêu thích

### Bước 3: Mở FavoritesWindow

1. Quay lại MainWindow (click "← Quay lại")
2. Click nút "⭐ Yêu thích" ở bottom navigation
3. FavoritesWindow mở ra

### Bước 4: Kiểm tra kết quả

**Kỳ vọng**:

```
┌─────────────────────────────────────────────────────────┐
│ ⭐ Yêu thích của tôi                          🔄        │
│ 3 món ăn                                                │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────┬─────────────────────────────────────┐     │
│  │  [ẢNH]  │ Alo Quán - Seafood & Beer           │     │
│  │  Q1.jpg │ 📍 TP.HCM - Vĩnh Khánh              │     │
│  │         │ ⭐ 4.5                               │     │
│  └─────────┴─────────────────────────────────────┘     │
│                                                         │
│  ┌─────────┬─────────────────────────────────────┐     │
│  │  [ẢNH]  │ Quán Ốc Đào 2                       │     │
│  │  Q2.jpg │ 📍 TP.HCM - Vĩnh Khánh              │     │
│  │         │ ⭐ 4.3                               │     │
│  └─────────┴─────────────────────────────────────┘     │
│                                                         │
│  ┌─────────┬─────────────────────────────────────┐     │
│  │  [ẢNH]  │ Bún Cá Châu Đốc Dì Tư               │     │
│  │  Q3.jpg │ 📍 TP.HCM - Vĩnh Khánh              │     │
│  │         │ ⭐ 4.6                               │     │
│  └─────────┴─────────────────────────────────────┘     │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

**Checklist**:
- [ ] ✅ Ảnh quán hiển thị rõ ràng (KHÔNG còn background xám)
- [ ] ✅ Tên quán hiển thị đầy đủ (font 16px, bold)
- [ ] ✅ Địa chỉ hiển thị với icon 📍
- [ ] ✅ Rating hiển thị với icon ⭐
- [ ] ❌ KHÔNG có nút "Xem chi tiết"
- [ ] ✅ Layout gọn gàng, dễ đọc

---

## ❌ Các lỗi có thể gặp

### Lỗi 1: Ảnh vẫn không hiển thị (background xám)

**Nguyên nhân**:
- Converter chưa được áp dụng đúng
- File ảnh không tồn tại

**Cách kiểm tra**:
```powershell
# Kiểm tra file ảnh
Get-ChildItem "VietnamFoodGuide\Assets\Images\Q*.jpg"

# Kiểm tra trong bin
Get-ChildItem "VietnamFoodGuide\bin\Debug\net48\Assets\Images\Q*.jpg"
```

**Kỳ vọng**: 12 files (Q1.jpg - Q12.jpg)

**Giải pháp**:
1. Rebuild project
2. Kiểm tra converter đã được đăng ký trong XAML
3. Xem console log trong Visual Studio

### Lỗi 2: Vẫn còn nút "Xem chi tiết"

**Nguyên nhân**:
- XAML chưa được cập nhật
- Đang chạy build cũ

**Giải pháp**:
1. Rebuild project
2. Xóa bin/obj folders
3. Build lại

### Lỗi 3: Layout bị lỗi

**Nguyên nhân**:
- XAML syntax error
- Missing resources

**Giải pháp**:
1. Check diagnostics trong Visual Studio
2. Rebuild project

---

## 🔍 Debug Tips

### Xem console log

Trong Visual Studio, mở Output window (View → Output):

```
[ImageConverter] Loading from relative path: E:\IT\C#\...\Assets\Images\Q1.jpg
[ImageConverter] Loading from relative path: E:\IT\C#\...\Assets\Images\Q2.jpg
[SQLiteFavoritesService] Loaded 3 favorites for user 1
```

### Kiểm tra binding

Nếu ảnh không hiển thị, kiểm tra:

1. **Image property có giá trị không?**
   ```csharp
   // Trong FavoritesWindow.xaml.cs - LoadFavorites()
   foreach (var food in favorites)
   {
       System.Diagnostics.Debug.WriteLine($"Food: {food.Name}, Image: {food.Image}");
   }
   ```

2. **Converter có được gọi không?**
   ```csharp
   // Trong StringToImageSourceConverter.cs - Convert()
   System.Diagnostics.Debug.WriteLine($"[ImageConverter] Converting: {path}");
   ```

---

## 📊 So sánh trước/sau

### Trước khi sửa ❌

```
┌─────────────────────────────────┐
│  [XÁM]  │ Ốc Oanh              │
│  (no    │ 📍 TP.HCM            │
│  image) │ ⭐ 4.9               │
│         │ [Xem chi tiết →]    │ ← Nút thừa
└─────────────────────────────────┘
```

### Sau khi sửa ✅

```
┌─────────────────────────────────┐
│  [ẢNH]  │ Ốc Oanh              │
│  Q12.jpg│ 📍 TP.HCM            │
│         │ ⭐ 4.9               │
│         │                      │ ← Không có nút
└─────────────────────────────────┘
```

---

## ✅ Kết quả mong đợi

**TẤT CẢ ẢNH PHẢI HIỂN THỊ ĐÚNG**

1. ✅ MainWindow: Ảnh hiển thị
2. ✅ FavoritesWindow: Ảnh hiển thị (ĐÃ SỬA)
3. ✅ FoodDetailWindow: Ảnh hero hiển thị
4. ✅ Không có nút "Xem chi tiết" trong FavoritesWindow

---

## 📸 Screenshot mẫu

Khi test thành công, bạn sẽ thấy:

**FavoritesWindow**:
- Header: "⭐ Yêu thích của tôi" + số lượng
- Danh sách: Mỗi item có ảnh + tên + địa chỉ + rating
- Không có nút "Xem chi tiết"
- Layout gọn gàng, dễ đọc

**MainWindow** (để so sánh):
- Cũng hiển thị ảnh tương tự
- Có nút "Xem chi tiết" (vẫn giữ nguyên)

---

**Ngày tạo**: 2026-05-02  
**Trạng thái**: ✅ SẴN SÀNG TEST
