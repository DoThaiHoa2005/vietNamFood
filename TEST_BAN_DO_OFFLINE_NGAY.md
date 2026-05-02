# ⚡ TEST BẢN ĐỒ OFFLINE NGAY (5 PHÚT)

## 🎯 Mục Tiêu
Test bản đồ offline **NGAY BÂY GIỜ** không cần tải tiles (dùng online fallback)

## ✅ Bước 1: Build App (30 giây)

```bash
dotnet build VietnamFoodGuide/VietnamFoodGuide.csproj
```

## ✅ Bước 2: Chạy App (10 giây)

```bash
cd VietnamFoodGuide/bin/Debug/net48
./VietnamFoodGuide.exe
```

## ✅ Bước 3: Test Offline Mode (1 phút)

### Test A: Có Mạng
1. ✅ Bật WiFi
2. Đăng nhập: `admin` / `admin123`
3. Chọn quán ăn bất kỳ
4. Bấm "Xem bản đồ"
5. **Kết quả**: Thấy Google Maps đầy đủ

### Test B: Không Mạng (Chưa Tải Tiles)
1. ❌ Tắt WiFi
2. Đăng nhập: `admin` / `admin123`
3. Chọn quán ăn bất kỳ
4. Bấm "Xem bản đồ"
5. **Kết quả**: 
   - Banner: "📡 Chế độ Offline"
   - Thấy canvas với grid
   - Thấy 2 markers (user + quán)
   - Thấy đường thẳng nối
   - Thấy khoảng cách

### Test C: Không Mạng (Đã Tải Tiles) ⭐
1. ❌ Tắt WiFi
2. **Tải tiles trước** (xem hướng dẫn dưới)
3. Đăng nhập: `admin` / `admin123`
4. Chọn quán ăn bất kỳ
5. Bấm "Xem bản đồ"
6. **Kết quả**: 
   - Banner: "📡 Chế độ Offline"
   - ✅ **Thấy đường xá chi tiết!**
   - ✅ **Thấy tên đường!**
   - ✅ **Zoom/Drag mượt!**

## 🚀 Cách Tải Tiles Nhanh (5 Phút)

### Option 1: Dùng Script PowerShell (Tự Động)

Tạo file `download_tiles.ps1`:

```powershell
# Download tiles cho Vĩnh Khánh
$outputDir = "VietnamFoodGuide/bin/Debug/net48/Data/MapTiles"
New-Item -ItemType Directory -Force -Path $outputDir

# Zoom levels
$zooms = 13..16

# Bounds (Vĩnh Khánh)
$minLat = 10.7450
$maxLat = 10.7650
$minLng = 106.6850
$maxLng = 106.7050

function LatLngToTile($lat, $lng, $zoom) {
    $n = [Math]::Pow(2, $zoom)
    $x = [Math]::Floor(($lng + 180) / 360 * $n)
    $y = [Math]::Floor((1 - [Math]::Log([Math]::Tan($lat * [Math]::PI / 180) + 1 / [Math]::Cos($lat * [Math]::PI / 180)) / [Math]::PI) / 2 * $n)
    return @{x=$x; y=$y}
}

$total = 0
$downloaded = 0

foreach ($zoom in $zooms) {
    $minTile = LatLngToTile $minLat $minLng $zoom
    $maxTile = LatLngToTile $maxLat $maxLng $zoom
    
    Write-Host "Zoom $zoom : Tiles ($($minTile.x),$($minTile.y)) to ($($maxTile.x),$($maxTile.y))"
    
    for ($x = $minTile.x; $x -le $maxTile.x; $x++) {
        $xDir = "$outputDir/$zoom/$x"
        New-Item -ItemType Directory -Force -Path $xDir | Out-Null
        
        for ($y = $minTile.y; $y -le $maxTile.y; $y++) {
            $tilePath = "$xDir/$y.png"
            
            if (Test-Path $tilePath) {
                Write-Host "Skip: $zoom/$x/$y (exists)"
                continue
            }
            
            $url = "https://tile.openstreetmap.org/$zoom/$x/$y.png"
            
            try {
                Invoke-WebRequest -Uri $url -OutFile $tilePath -UserAgent "VietnamFoodGuide/1.0"
                $downloaded++
                Write-Host "Downloaded: $zoom/$x/$y ($downloaded tiles)"
                Start-Sleep -Milliseconds 100
            } catch {
                Write-Host "Error: $zoom/$x/$y - $($_.Exception.Message)"
            }
        }
    }
}

Write-Host "Done! Downloaded $downloaded tiles"
```

Chạy:
```bash
powershell -ExecutionPolicy Bypass -File download_tiles.ps1
```

### Option 2: Dùng App (Đã Code Sẵn)

1. Thêm button vào MainWindow.xaml:
```xml
<Button Content="📥 Tải Bản Đồ Offline" 
        Click="BtnDownloadMap_Click"
        Width="200" Height="40"/>
```

2. Thêm code vào MainWindow.xaml.cs:
```csharp
private void BtnDownloadMap_Click(object sender, RoutedEventArgs e)
{
    var window = new OfflineMapDownloadWindow();
    window.ShowDialog();
}
```

3. Build và chạy
4. Bấm button "📥 Tải Bản Đồ Offline"
5. Đợi progress bar 100%

## 🎨 Kết Quả Mong Đợi

### Trước (Không Có Tiles):
```
┌─────────────────────────────────┐
│  📡 Chế độ Offline              │
│                                 │
│  [Grid màu xám]                 │
│  🔵 (User)                      │
│  - - - - (Line)                 │
│  🔴 (Quán)                      │
│                                 │
│  Khoảng cách: 2.5 km            │
└─────────────────────────────────┘
```

### Sau (Có Tiles): ⭐
```
┌─────────────────────────────────┐
│  📡 Chế độ Offline              │
│                                 │
│  [Bản đồ với đường xá]          │
│  Đường Nguyễn Tất Thành         │
│  🔵 (User)                      │
│  ━━━━━ (Đường đi)               │
│  🔴 Quán Ốc Vĩnh Khánh          │
│  Công viên, Bệnh viện...        │
│                                 │
│  Khoảng cách: 2.5 km            │
└─────────────────────────────────┘
```

## 🔍 Debug Nếu Không Thấy Bản Đồ

### 1. Bấm F12 trong WebView2
Xem console log:
```
✅ Offline map initialized
✅ User: 10.7769, 106.7009
✅ Destination: 10.7550, 106.6950
✅ Zoom: 14
✅ Tiles path: E:/IT/C#/.../Data/MapTiles
```

### 2. Kiểm Tra Thư Mục Tiles
```bash
dir VietnamFoodGuide/bin/Debug/net48/Data/MapTiles
```

Phải thấy:
```
13/
14/
15/
16/
```

### 3. Kiểm Tra File Tiles
```bash
dir VietnamFoodGuide/bin/Debug/net48/Data/MapTiles/14/6789
```

Phải thấy:
```
4321.png
4322.png
4323.png
...
```

## ⚡ Tóm Tắt

**Để thấy bản đồ offline như trong ảnh:**

1. ✅ Code đã sẵn sàng (không cần sửa gì)
2. ✅ Tải tiles (dùng script hoặc app)
3. ✅ Tắt WiFi
4. ✅ Mở bản đồ
5. ✅ **THẤY ĐƯỜNG XÁ CHI TIẾT!**

**Không cần code thêm!** Chỉ cần tải tiles!

---

**Tác Giả**: Kiro AI Assistant  
**Ngày**: May 2, 2026
