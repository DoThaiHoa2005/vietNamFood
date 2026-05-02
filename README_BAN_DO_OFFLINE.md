# 🗺️ BẢN ĐỒ OFFLINE - VIETNAM FOOD GUIDE

## 🎯 Tính Năng

Khi **không có mạng**, bản đồ vẫn hiển thị **đường xá chi tiết** như Google Maps.

## ⚡ CÁCH SỬ DỤNG NHANH (3 BƯỚC)

### Bước 1: Tải Tiles Bản Đồ

Chạy script PowerShell:

```bash
powershell -ExecutionPolicy Bypass -File download_map_tiles.ps1
```

Hoặc dùng app (nếu đã thêm button):
- Mở app
- Bấm "📥 Tải Bản Đồ Offline"
- Đợi 100%

### Bước 2: Build App

```bash
dotnet build VietnamFoodGuide/VietnamFoodGuide.csproj
```

### Bước 3: Test

1. **Tắt WiFi** ❌
2. Mở app
3. Đăng nhập: `admin` / `admin123`
4. Chọn quán ăn
5. Bấm "Xem bản đồ"
6. ✅ **Thấy đường xá chi tiết!**

## 📊 Kết Quả

### Trước (Không Có Tiles):
- Chỉ thấy đường thẳng
- Không có đường xá
- Canvas đơn giản

### Sau (Có Tiles): ⭐
- ✅ Đường xá chi tiết
- ✅ Tên đường
- ✅ Địa danh (công viên, bệnh viện, v.v.)
- ✅ Zoom/Drag mượt mà
- ✅ Giống Google Maps

## 📁 Cấu Trúc Files

```
VietnamFoodGuide/
├── bin/Debug/net48/Data/MapTiles/  ← Tiles offline
│   ├── 13/
│   ├── 14/
│   ├── 15/
│   └── 16/
├── Services/
│   ├── NetworkService.cs           ← Detect mạng
│   └── OfflineMapService.cs        ← Quản lý tiles
├── Views/
│   ├── MapWindow.xaml.cs           ← Logic bản đồ
│   ├── MapWindow_Offline.html      ← Bản đồ offline
│   └── OfflineMapDownloadWindow.*  ← UI tải tiles
└── download_map_tiles.ps1          ← Script tải tiles
```

## 🔧 Cách Hoạt Động

### 1. Detect Mạng
```csharp
bool isOnline = await NetworkService.IsInternetAvailableAsync();
```

### 2. Chọn Bản Đồ
```csharp
if (isOnline) {
    LoadMap(); // Google Maps
} else {
    LoadOfflineMap(); // Tiles offline
}
```

### 3. Load Tiles
```javascript
// Thử local trước
img.src = 'file:///' + tilesPath + '/{z}/{x}/{y}.png';

// Fallback online
img.onerror = function() {
    img.src = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
};
```

## 📦 Thông Tin Tiles

| Thông Tin | Giá Trị |
|-----------|---------|
| Khu vực | Vĩnh Khánh, Quận 4 |
| Zoom levels | 13-16 |
| Số tiles | ~2500-5000 |
| Kích thước | ~50-100 MB |
| Thời gian tải | ~5-10 phút |
| Nguồn | OpenStreetMap |

## 🚨 Lỗi Thường Gặp

### Lỗi 1: Không Thấy Bản Đồ
**Giải pháp**: Kiểm tra thư mục `Data/MapTiles/` có tiles chưa

### Lỗi 2: Thấy Placeholder Xám
**Giải pháp**: Tiles không load được, kiểm tra đường dẫn

### Lỗi 3: Bản Đồ Bị Trắng
**Giải pháp**: Bấm F12 xem console log

## 📚 Tài Liệu Chi Tiết

- `HUONG_DAN_BAN_DO_OFFLINE_DON_GIAN.md` - Hướng dẫn đầy đủ
- `TEST_BAN_DO_OFFLINE_NGAY.md` - Test nhanh
- `HUONG_DAN_BAN_DO_OFFLINE_CHI_TIET.md` - Chi tiết kỹ thuật
- `OFFLINE_MAP_COMPLETE.md` - Tổng hợp tính năng

## 🎓 Video Hướng Dẫn

1. Chạy `download_map_tiles.ps1`
2. Đợi tải xong
3. Tắt WiFi
4. Mở app
5. Xem bản đồ
6. ✅ Thấy đường xá!

## 💡 Tips

- Chỉ tải zoom 13-16 (đủ dùng)
- Tải qua đêm nếu khu vực lớn
- Xóa tiles cũ sau 3-6 tháng
- Backup tiles để không phải tải lại

## 🤝 Đóng Góp

Nếu gặp lỗi hoặc có ý tưởng cải tiến:
1. Mở issue trên GitHub
2. Gửi screenshot console log (F12)
3. Gửi ảnh thư mục tiles

## 📄 License

MIT License - Tự do sử dụng cho đồ án

---

**Tác Giả**: Kiro AI Assistant  
**Ngày**: May 2, 2026  
**Version**: 2.0 - Offline Map Complete
