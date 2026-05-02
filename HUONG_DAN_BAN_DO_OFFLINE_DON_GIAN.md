# 🗺️ HƯỚNG DẪN BẢN ĐỒ OFFLINE ĐỠN GIẢN

## 🎯 Mục Tiêu
Khi **không có mạng**, bản đồ vẫn hiện đường xá chi tiết như Google Maps.

## ✅ Bước 1: Tải Tiles Bản Đồ (Quan Trọng!)

### Cách 1: Dùng Tool Tự Động (Khuyên Dùng)

1. **Download Mobile Atlas Creator (MOBAC)**
   - Link: https://mobac.sourceforge.io/
   - Giải nén và chạy `Mobile Atlas Creator.exe`

2. **Cấu hình MOBAC**
   - Map Source: Chọn "OpenStreetMap Mapnik"
   - Zoom Levels: Chọn 13, 14, 15, 16
   - Atlas Format: Chọn "OSMDroid ZIP"

3. **Chọn Khu Vực Vĩnh Khánh**
   - Zoom đến khu vực Vĩnh Khánh, Quận 4, TP.HCM
   - Vẽ hình chữ nhật bao quanh khu vực
   - Coordinates: 
     - Top-Left: 10.7650, 106.6850
     - Bottom-Right: 10.7450, 106.7050

4. **Tải Xuống**
   - Bấm "Add Selection"
   - Bấm "Create Atlas"
   - Đợi tải xong (~5-10 phút, ~50-100 MB)

5. **Copy Tiles vào App**
   ```
   Giải nén file ZIP vừa tải
   Copy thư mục tiles vào:
   VietnamFoodGuide/bin/Debug/net48/Data/MapTiles/
   ```

### Cách 2: Dùng Code Tự Động (Đã Tạo Sẵn)

1. **Mở OfflineMapDownloadWindow**
   - Trong MainWindow, thêm button:
   ```csharp
   private void BtnDownloadMap_Click(object sender, RoutedEventArgs e)
   {
       var window = new OfflineMapDownloadWindow();
       window.ShowDialog();
   }
   ```

2. **Tải Tiles**
   - Bấm button "📥 Tải Xuống"
   - Đợi progress bar đến 100%
   - Xong!

## ✅ Bước 2: Kiểm Tra Tiles Đã Tải

Mở thư mục:
```
VietnamFoodGuide/bin/Debug/net48/Data/MapTiles/
```

Cấu trúc phải như này:
```
MapTiles/
├── 13/
│   ├── 6789/
│   │   ├── 4321.png
│   │   ├── 4322.png
│   │   └── ...
│   └── ...
├── 14/
├── 15/
└── 16/
```

## ✅ Bước 3: Test Bản Đồ Offline

### Test 1: Có Mạng
1. Bật WiFi
2. Mở app, chọn quán ăn
3. Bấm "Xem bản đồ"
4. ✅ Thấy Google Maps đầy đủ tính năng

### Test 2: Không Có Mạng (Đã Tải Tiles)
1. **Tắt WiFi**
2. Mở app, chọn quán ăn
3. Bấm "Xem bản đồ"
4. ✅ **Thấy bản đồ với đường xá chi tiết!**
   - Có đường xá
   - Có tên đường
   - Có địa danh
   - Zoom in/out mượt
   - Drag được

### Test 3: Không Có Mạng (Chưa Tải Tiles)
1. Tắt WiFi
2. Chưa tải tiles
3. Mở bản đồ
4. ⚠️ Thấy placeholder màu xám (fallback)

## 🎨 So Sánh 3 Chế Độ

### 1. Online (Có Mạng)
```
┌─────────────────────────────────┐
│  🌐 Google Maps                 │
│  ✅ Routing chi tiết            │
│  ✅ Turn-by-turn navigation     │
│  ✅ Search địa điểm             │
│  ✅ Compass                     │
│  ✅ Real-time traffic           │
└─────────────────────────────────┘
```

### 2. Offline (Đã Tải Tiles) ⭐ MỤC TIÊU
```
┌─────────────────────────────────┐
│  📡 Bản Đồ Offline              │
│  ✅ Đường xá chi tiết           │
│  ✅ Tên đường                   │
│  ✅ Địa danh                    │
│  ✅ Zoom/Drag mượt              │
│  ❌ Không có routing API        │
│  ❌ Không có turn-by-turn       │
└─────────────────────────────────┘
```

### 3. Offline (Chưa Tải Tiles)
```
┌─────────────────────────────────┐
│  ⚠️ Bản Đồ Đơn Giản             │
│  ✅ Đường thẳng                 │
│  ✅ Khoảng cách                 │
│  ❌ Không có đường xá           │
└─────────────────────────────────┘
```

## 🔧 Cách Hoạt Động

### Code Tự Động Detect Mạng:

```csharp
// MapWindow.xaml.cs - InitializeWebView()

bool isOnline = await NetworkService.IsInternetAvailableAsync();

if (isOnline) {
    LoadMap(); // Google Maps online
} else {
    LoadOfflineMap(); // Bản đồ offline với tiles
}
```

### Bản Đồ Offline Load Tiles:

```javascript
// MapWindow_Offline.html

// Thử load từ local trước
img.src = 'file:///' + tilesPath + '/' + z + '/' + x + '/' + y + '.png';

// Nếu không có → fallback online
img.onerror = function() {
    img.src = 'https://tile.openstreetmap.org/' + z + '/' + x + '/' + y + '.png';
};
```

## 🚨 Lỗi Thường Gặp

### Lỗi 1: Không Thấy Bản Đồ
**Nguyên nhân**: Chưa tải tiles

**Giải pháp**:
1. Kiểm tra thư mục `Data/MapTiles/`
2. Phải có các thư mục 13, 14, 15, 16
3. Mỗi thư mục phải có file .png

### Lỗi 2: Thấy Placeholder Xám
**Nguyên nhân**: Tiles không load được

**Giải pháp**:
1. Kiểm tra đường dẫn tiles
2. Bấm F12 trong WebView2 xem console log
3. Kiểm tra file .png có mở được không

### Lỗi 3: Bản Đồ Bị Trắng
**Nguyên nhân**: JavaScript error

**Giải pháp**:
1. Bấm F12 xem console
2. Kiểm tra file `MapWindow_Offline.html`
3. Rebuild app

## 💡 Tips Quan Trọng

### 1. Kích Thước Tiles
- Zoom 13-16 cho khu vực nhỏ: ~50-100 MB
- Zoom 13-18 cho khu vực lớn: ~500 MB - 1 GB
- **Khuyên**: Chỉ tải zoom 13-16 là đủ

### 2. Tốc Độ Tải
- Tải từ OpenStreetMap: ~5-10 phút
- Delay 50ms giữa mỗi tile để không bị ban
- Có thể tải qua đêm nếu khu vực lớn

### 3. Cập Nhật Tiles
- Tiles cũ sau 3-6 tháng
- Xóa và tải lại để cập nhật
- Hoặc chỉ tải tiles mới cho khu vực thay đổi

## 🎯 Kết Luận

**Để bản đồ offline hiện như trong ảnh:**

1. ✅ **Tải tiles trước** (quan trọng nhất!)
2. ✅ Code đã sẵn sàng (tự động detect mạng)
3. ✅ Tắt WiFi để test
4. ✅ Thấy đường xá chi tiết!

**Không cần code thêm gì!** Chỉ cần tải tiles là xong.

---

## 📞 Hỗ Trợ

Nếu vẫn không thấy bản đồ:
1. Gửi screenshot console log (F12)
2. Gửi ảnh thư mục `Data/MapTiles/`
3. Mình sẽ debug chi tiết

**Tác Giả**: Kiro AI Assistant  
**Ngày**: May 2, 2026
