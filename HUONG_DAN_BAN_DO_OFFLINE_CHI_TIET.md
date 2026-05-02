# 🗺️ HƯỚNG DẪN BẢN ĐỒ OFFLINE CHI TIẾT

## 📋 Tổng Quan

Bản đồ offline giờ đây có thể hiển thị **giống Google Maps** với đường xá, tên địa điểm, và routing chi tiết khi bạn tải trước dữ liệu bản đồ.

## 🎯 Tính Năng

### ✅ Bản Đồ Offline Chi Tiết
- Hiển thị đường xá, tên đường
- Hiển thị địa danh (công viên, bệnh viện, trường học, v.v.)
- Sử dụng OpenStreetMap tiles
- Zoom in/out mượt mà
- Drag để di chuyển
- Markers cho user và destination

### ✅ Download Manager
- Window riêng để tải bản đồ offline
- Progress bar hiển thị tiến độ
- Hiển thị dung lượng đã tải
- Xóa bản đồ offline khi không cần

## 🔧 Các File Đã Tạo

### 1. **OfflineMapService.cs** - Service Quản Lý Offline Tiles
```
VietnamFoodGuide/Services/OfflineMapService.cs
```

**Chức năng:**
- Download map tiles từ OpenStreetMap
- Lưu tiles vào `Data/MapTiles/{zoom}/{x}/{y}.png`
- Tính toán tile coordinates từ lat/lng
- Quản lý dung lượng và số lượng tiles
- Xóa tiles khi không cần

**Methods:**
```csharp
// Download tiles cho một khu vực
await DownloadTilesForArea(minLat, minLng, maxLat, maxLng, minZoom, maxZoom, progress);

// Download tiles cho Vĩnh Khánh (preset)
await DownloadVinhKhanhArea(progress);

// Kiểm tra có tiles offline không
bool hasTiles = HasOfflineTiles(lat, lng, zoom);

// Lấy đường dẫn thư mục tiles
string path = GetTilesDirectory();

// Xóa tất cả tiles
ClearOfflineTiles();

// Tính kích thước (MB)
double sizeMB = GetOfflineTilesSize();
```

### 2. **MapWindow_Offline.html** - Offline Map với Leaflet.js
```
VietnamFoodGuide/Views/MapWindow_Offline.html
```

**Tính năng:**
- Sử dụng Leaflet.js để render bản đồ
- Load tiles từ local file system
- Fallback to online tiles nếu không có offline
- User marker (blue) và destination marker (red)
- Đường thẳng nối 2 điểm
- Tính khoảng cách
- Multi-language support

**Placeholders:**
- `{USER_LAT}`, `{USER_LNG}` - Vị trí user
- `{DEST_LAT}`, `{DEST_LNG}` - Vị trí quán
- `{DEST_NAME}` - Tên quán
- `{CURRENT_LANG}` - Ngôn ngữ (vi/en/zh)
- `{TILES_PATH}` - Đường dẫn đến thư mục tiles

### 3. **OfflineMapDownloadWindow.xaml** - UI Tải Bản Đồ
```
VietnamFoodGuide/Views/OfflineMapDownloadWindow.xaml
VietnamFoodGuide/Views/OfflineMapDownloadWindow.xaml.cs
```

**Tính năng:**
- Button "Tải Xuống" để download tiles
- Progress bar hiển thị tiến độ (0-100%)
- Hiển thị dung lượng đã tải (MB)
- Hiển thị số lượng tiles
- Button "Xóa" để xóa tiles
- Button "Đóng" để đóng window

## 🚀 Cách Sử Dụng

### Bước 1: Mở Window Tải Bản Đồ

Thêm menu item hoặc button trong MainWindow:

```csharp
private void OpenOfflineMapDownload_Click(object sender, RoutedEventArgs e)
{
    var window = new OfflineMapDownloadWindow();
    window.ShowDialog();
}
```

### Bước 2: Tải Bản Đồ Offline

1. Bấm button "📥 Tải Xuống"
2. Xác nhận tải (kích thước ~50-100 MB)
3. Đợi quá trình tải hoàn tất (5-10 phút)
4. Thông báo "Thành công" khi hoàn tất

### Bước 3: Sử Dụng Bản Đồ Offline

1. Tắt WiFi/Ethernet (test offline)
2. Mở app và chọn quán ăn
3. Bấm "Xem bản đồ"
4. ✅ Bản đồ offline sẽ hiển thị với đường xá chi tiết

## 📊 Cấu Trúc Thư Mục Tiles

```
VietnamFoodGuide/bin/Debug/net48/Data/MapTiles/
├── 13/                    ← Zoom level 13
│   ├── 6789/              ← X coordinate
│   │   ├── 4321.png       ← Y coordinate (tile image)
│   │   ├── 4322.png
│   │   └── ...
│   └── ...
├── 14/                    ← Zoom level 14
│   └── ...
├── 15/                    ← Zoom level 15
│   └── ...
└── 16/                    ← Zoom level 16
    └── ...
```

## 🎨 So Sánh Bản Đồ

### Online Mode (Có Mạng):
```
┌─────────────────────────────────┐
│  Google Maps (Full Features)   │
│  + Routing API                  │
│  + Turn-by-turn navigation      │
│  + Real-time traffic            │
│  + Search địa điểm              │
│  + Compass                      │
└─────────────────────────────────┘
```

### Offline Mode (Không Mạng - Chưa Tải):
```
┌─────────────────────────────────┐
│  Canvas Map (Simple)            │
│  + Đường thẳng                  │
│  + Khoảng cách đường chim bay   │
│  + Zoom/Drag limited            │
└─────────────────────────────────┘
```

### Offline Mode (Không Mạng - Đã Tải):
```
┌─────────────────────────────────┐
│  Leaflet Map (Detailed)         │
│  + Đường xá, tên đường          │
│  + Địa danh (công viên, v.v.)   │
│  + Zoom/Drag mượt mà            │
│  + Giống Google Maps            │
│  - Không có routing API         │
│  - Không có turn-by-turn        │
└─────────────────────────────────┘
```

## 🧪 Test Scenarios

### Test 1: Tải Bản Đồ Offline
```
1. Bật WiFi
2. Mở OfflineMapDownloadWindow
3. Bấm "Tải Xuống"
4. Đợi progress bar đến 100%
Expected: 
- Thông báo "Thành công"
- Dung lượng hiển thị ~50-100 MB
- Số tiles hiển thị ~2500-5000
```

### Test 2: Xem Bản Đồ Offline (Đã Tải)
```
1. Tải bản đồ offline (Test 1)
2. Tắt WiFi
3. Mở app, chọn quán ăn
4. Bấm "Xem bản đồ"
Expected:
- Banner "📡 Chế độ Offline - Bản đồ từ dữ liệu đã tải"
- Hiển thị đường xá chi tiết
- Có thể zoom in/out
- Có thể drag bản đồ
- Markers hiển thị đúng vị trí
```

### Test 3: Xem Bản Đồ Offline (Chưa Tải)
```
1. Chưa tải bản đồ offline
2. Tắt WiFi
3. Mở app, chọn quán ăn
4. Bấm "Xem bản đồ"
Expected:
- Fallback to online tiles (sẽ không load được)
- Hoặc hiển thị canvas map đơn giản
```

### Test 4: Xóa Bản Đồ Offline
```
1. Đã tải bản đồ offline
2. Mở OfflineMapDownloadWindow
3. Bấm "Xóa"
4. Xác nhận
Expected:
- Thông báo "Đã xóa"
- Dung lượng về 0 MB
- Số tiles về 0
```

## 📐 Kích Thước và Hiệu Suất

### Khu Vực Vĩnh Khánh:
- **Bounds**: 
  - Min: 10.7450, 106.6850
  - Max: 10.7650, 106.7050
- **Zoom Levels**: 13-16
- **Số Tiles**: ~2500-5000 tiles
- **Kích Thước**: ~50-100 MB
- **Thời Gian Tải**: ~5-10 phút (tùy tốc độ mạng)

### Tối Ưu:
- Delay 50ms giữa mỗi tile để không spam server
- Sử dụng HttpClient với timeout 10 giây
- Skip tiles đã tồn tại (không tải lại)
- Progress callback để update UI

## ⚠️ Lưu Ý Quan Trọng

### 1. OpenStreetMap Tile Usage Policy
- **Không được** sử dụng cho commercial app mà không có permission
- **Phải** có User-Agent header khi download
- **Nên** cache tiles locally (đã làm)
- **Không được** download quá nhiều tiles cùng lúc

### 2. File System
- Tiles được lưu trong `Data/MapTiles/`
- Cần quyền write vào thư mục
- Kiểm tra dung lượng đĩa trước khi tải

### 3. WebView2 File Protocol
- Leaflet.js load tiles từ `file:///` protocol
- Cần enable file access trong WebView2
- Có thể không hoạt động trên một số hệ thống

## 🔜 Cải Tiến Tương Lai

### 1. Offline Routing
- Download OSRM routing data
- Sử dụng local routing algorithm
- Turn-by-turn navigation offline

### 2. Multiple Areas
- Cho phép chọn nhiều khu vực để tải
- Quản lý nhiều tile sets
- Auto-detect khu vực cần thiết

### 3. Auto Update
- Kiểm tra tiles cũ (>30 ngày)
- Tự động update tiles mới
- Background download

### 4. Compression
- Nén tiles để giảm dung lượng
- Sử dụng WebP thay vì PNG
- Giảm 30-50% kích thước

## 🎯 Kết Luận

Bây giờ app có 3 chế độ bản đồ:

1. **Online Full** - Google Maps đầy đủ tính năng
2. **Offline Detailed** - OpenStreetMap với đường xá chi tiết (cần tải trước)
3. **Offline Simple** - Canvas map đơn giản (fallback)

User experience tốt nhất: Tải bản đồ offline trước khi đi du lịch!

---

**Tác Giả**: Kiro AI Assistant  
**Ngày**: May 2, 2026  
**Phiên Bản**: 2.0 - Offline Map với Tiles Chi Tiết
