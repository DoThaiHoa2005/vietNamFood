# ========================================
# SCRIPT TẢI BẢN ĐỒ OFFLINE TỰ ĐỘNG
# Khu vực: Vĩnh Khánh, Quận 4, TP.HCM
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TẢI BẢN ĐỒ OFFLINE - VĨNH KHÁNH" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Cấu hình
$outputDir = "VietnamFoodGuide/bin/Debug/net48/Data/MapTiles"
$zooms = 13..16  # Zoom levels
$minLat = 10.7450
$maxLat = 10.7650
$minLng = 106.6850
$maxLng = 106.7050

# Tạo thư mục output
Write-Host "📁 Tạo thư mục: $outputDir" -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

# Hàm chuyển đổi lat/lng sang tile coordinates
function LatLngToTile($lat, $lng, $zoom) {
    $n = [Math]::Pow(2, $zoom)
    $x = [Math]::Floor(($lng + 180) / 360 * $n)
    $latRad = $lat * [Math]::PI / 180
    $y = [Math]::Floor((1 - [Math]::Log([Math]::Tan($latRad) + 1 / [Math]::Cos($latRad)) / [Math]::PI) / 2 * $n)
    return @{x=[int]$x; y=[int]$y}
}

# Tính tổng số tiles
$totalTiles = 0
foreach ($zoom in $zooms) {
    $minTile = LatLngToTile $minLat $minLng $zoom
    $maxTile = LatLngToTile $maxLat $maxLng $zoom
    $tilesX = $maxTile.x - $minTile.x + 1
    $tilesY = $maxTile.y - $minTile.y + 1
    $totalTiles += $tilesX * $tilesY
}

Write-Host "📊 Tổng số tiles cần tải: $totalTiles" -ForegroundColor Green
Write-Host "📦 Kích thước ước tính: $([Math]::Round($totalTiles * 20 / 1024, 2)) MB" -ForegroundColor Green
Write-Host "⏱️  Thời gian ước tính: $([Math]::Round($totalTiles * 0.15 / 60, 1)) phút" -ForegroundColor Green
Write-Host ""

$confirm = Read-Host "Bạn có muốn tiếp tục? (Y/N)"
if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Host "❌ Đã hủy" -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "🚀 Bắt đầu tải..." -ForegroundColor Cyan
Write-Host ""

$downloaded = 0
$skipped = 0
$errors = 0
$startTime = Get-Date

foreach ($zoom in $zooms) {
    $minTile = LatLngToTile $minLat $minLng $zoom
    $maxTile = LatLngToTile $maxLat $maxLng $zoom
    
    Write-Host "📍 Zoom $zoom : Tiles ($($minTile.x),$($minTile.y)) to ($($maxTile.x),$($maxTile.y))" -ForegroundColor Yellow
    
    for ($x = $minTile.x; $x -le $maxTile.x; $x++) {
        $xDir = "$outputDir/$zoom/$x"
        New-Item -ItemType Directory -Force -Path $xDir | Out-Null
        
        for ($y = $minTile.y; $y -le $maxTile.y; $y++) {
            $tilePath = "$xDir/$y.png"
            
            # Skip nếu đã tồn tại
            if (Test-Path $tilePath) {
                $skipped++
                $progress = [Math]::Round((($downloaded + $skipped + $errors) * 100.0) / $totalTiles, 1)
                Write-Host "⏭️  Skip: $zoom/$x/$y (exists) - Progress: $progress%" -ForegroundColor Gray
                continue
            }
            
            $url = "https://tile.openstreetmap.org/$zoom/$x/$y.png"
            
            try {
                # Download tile
                Invoke-WebRequest -Uri $url -OutFile $tilePath -UserAgent "VietnamFoodGuide/1.0" -TimeoutSec 10
                $downloaded++
                $progress = [Math]::Round((($downloaded + $skipped + $errors) * 100.0) / $totalTiles, 1)
                Write-Host "✅ Downloaded: $zoom/$x/$y - Progress: $progress% ($downloaded/$totalTiles)" -ForegroundColor Green
                
                # Delay để không spam server
                Start-Sleep -Milliseconds 100
            } catch {
                $errors++
                $progress = [Math]::Round((($downloaded + $skipped + $errors) * 100.0) / $totalTiles, 1)
                Write-Host "❌ Error: $zoom/$x/$y - $($_.Exception.Message) - Progress: $progress%" -ForegroundColor Red
            }
        }
    }
    
    Write-Host ""
}

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  KẾT QUẢ" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ Downloaded: $downloaded tiles" -ForegroundColor Green
Write-Host "⏭️  Skipped: $skipped tiles" -ForegroundColor Gray
Write-Host "❌ Errors: $errors tiles" -ForegroundColor Red
Write-Host "⏱️  Thời gian: $([Math]::Round($duration / 60, 1)) phút" -ForegroundColor Yellow
Write-Host ""

# Tính kích thước thực tế
$totalSize = 0
if (Test-Path $outputDir) {
    Get-ChildItem -Path $outputDir -Recurse -File | ForEach-Object {
        $totalSize += $_.Length
    }
    $sizeMB = [Math]::Round($totalSize / 1024 / 1024, 2)
    Write-Host "📦 Kích thước thực tế: $sizeMB MB" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎉 HOÀN THÀNH!" -ForegroundColor Green
Write-Host ""
Write-Host "📂 Tiles đã lưu tại: $outputDir" -ForegroundColor Yellow
Write-Host ""
Write-Host "🧪 Test ngay:" -ForegroundColor Cyan
Write-Host "   1. Tắt WiFi" -ForegroundColor White
Write-Host "   2. Mở app VietnamFoodGuide" -ForegroundColor White
Write-Host "   3. Chọn quán ăn" -ForegroundColor White
Write-Host "   4. Bấm 'Xem bản đồ'" -ForegroundColor White
Write-Host "   5. ✅ Thấy đường xá chi tiết!" -ForegroundColor White
Write-Host ""
