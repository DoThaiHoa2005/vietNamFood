# Script kiểm tra trạng thái audio generation

Write-Host "🔍 Kiểm tra trạng thái tạo audio..." -ForegroundColor Cyan
Write-Host ""

# 1. Kiểm tra thư mục
Write-Host "📁 Kiểm tra thư mục Assets/Audio..." -ForegroundColor Yellow
$audioFolder = "VietnamFoodGuide/Assets/Audio"

if (Test-Path $audioFolder) {
    Write-Host "✅ Thư mục tồn tại: $audioFolder" -ForegroundColor Green
    
    # Đếm file MP3
    $mp3Files = Get-ChildItem $audioFolder -Filter "*.mp3" -ErrorAction SilentlyContinue
    Write-Host "📊 Số file MP3: $($mp3Files.Count)" -ForegroundColor Cyan
    
    if ($mp3Files.Count -gt 0) {
        Write-Host ""
        Write-Host "📋 Danh sách file:" -ForegroundColor Green
        $mp3Files | ForEach-Object {
            $sizeKB = [math]::Round($_.Length / 1KB, 2)
            Write-Host "  ✅ $($_.Name) - $sizeKB KB"
        }
    } else {
        Write-Host "❌ KHÔNG CÓ FILE MP3 NÀO!" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Thư mục không tồn tại: $audioFolder" -ForegroundColor Red
    Write-Host "Tạo thư mục..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $audioFolder -Force | Out-Null
    Write-Host "✅ Đã tạo thư mục" -ForegroundColor Green
}

Write-Host ""
Write-Host "─────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

# 2. Kiểm tra XAMPP
Write-Host "🔧 Kiểm tra XAMPP..." -ForegroundColor Yellow
$mysqlProcess = Get-Process | Where-Object {$_.Name -like "*mysql*"}
$apacheProcess = Get-Process | Where-Object {$_.Name -like "*httpd*" -or $_.Name -like "*apache*"}

if ($mysqlProcess) {
    Write-Host "✅ MySQL đang chạy" -ForegroundColor Green
} else {
    Write-Host "❌ MySQL KHÔNG chạy - Mở XAMPP và Start MySQL!" -ForegroundColor Red
}

if ($apacheProcess) {
    Write-Host "✅ Apache đang chạy" -ForegroundColor Green
} else {
    Write-Host "⚠️ Apache không chạy (không bắt buộc cho audio)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "─────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

# 3. Kiểm tra kết nối internet
Write-Host "🌐 Kiểm tra kết nối internet..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://translate.google.com" -TimeoutSec 5 -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Kết nối internet OK" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ KHÔNG CÓ INTERNET - Cần internet để dùng Google TTS!" -ForegroundColor Red
}

Write-Host ""
Write-Host "─────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

# 4. Test download audio từ Google
Write-Host "🧪 Test download audio từ Google..." -ForegroundColor Yellow
try {
    $testText = "Xin chào"
    $encodedText = [System.Uri]::EscapeDataString($testText)
    $testUrl = "https://translate.google.com/translate_tts?ie=UTF-8&tl=vi&client=tw-ob&q=$encodedText"
    
    Write-Host "📥 Đang download test audio..." -ForegroundColor Cyan
    $testFile = "test_audio.mp3"
    Invoke-WebRequest -Uri $testUrl -OutFile $testFile -TimeoutSec 10
    
    if (Test-Path $testFile) {
        $size = (Get-Item $testFile).Length
        Write-Host "✅ Download thành công! Kích thước: $size bytes" -ForegroundColor Green
        
        # Cleanup
        Remove-Item $testFile -Force
        Write-Host "🗑️ Đã xóa file test" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Lỗi download: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "⚠️ Google có thể đang block request!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "─────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

# 5. Kiểm tra database
Write-Host "🗄️ Kiểm tra database..." -ForegroundColor Yellow
Write-Host "⚠️ Cần kiểm tra thủ công trong phpMyAdmin:" -ForegroundColor Yellow
Write-Host "  1. Mở http://localhost/phpmyadmin" -ForegroundColor Cyan
Write-Host "  2. Chọn database 'vietnamfoodguide'" -ForegroundColor Cyan
Write-Host "  3. Chạy query: SELECT COUNT(*) FROM Foods;" -ForegroundColor Cyan
Write-Host "  4. Nếu COUNT = 0 → Không có dữ liệu!" -ForegroundColor Cyan

Write-Host ""
Write-Host "─────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

# 6. Hướng dẫn debug
Write-Host "🐛 Cách debug:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1️⃣ Chạy app từ Visual Studio (F5)" -ForegroundColor Cyan
Write-Host "2️⃣ Mở Output window (View → Output)" -ForegroundColor Cyan
Write-Host "3️⃣ Click nút 🎵 → Click 'Tạo Audio'" -ForegroundColor Cyan
Write-Host "4️⃣ Xem log trong Output window" -ForegroundColor Cyan
Write-Host "5️⃣ Tìm các dòng bắt đầu với [GoogleTTS]" -ForegroundColor Cyan
Write-Host ""
Write-Host "Các log quan trọng:" -ForegroundColor Yellow
Write-Host "  🚀 [GoogleTTS] Bắt đầu tạo audio..." -ForegroundColor Gray
Write-Host "  📝 [GoogleTTS] Tìm thấy X quán ăn" -ForegroundColor Gray
Write-Host "  🍽️ [GoogleTTS] Đang tạo audio cho: Tên quán" -ForegroundColor Gray
Write-Host "  ✅ [GoogleTTS] Đã tạo MP3: ..." -ForegroundColor Gray
Write-Host "  ❌ [GoogleTTS] Lỗi: ..." -ForegroundColor Gray

Write-Host ""
Write-Host "─────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

# Tổng kết
Write-Host "📊 TÓM TẮT:" -ForegroundColor Cyan
Write-Host ""

$issues = @()
if (-not (Test-Path $audioFolder)) { $issues += "❌ Thư mục Audio không tồn tại" }
if ($mp3Files.Count -eq 0) { $issues += "❌ Không có file MP3 nào" }
if (-not $mysqlProcess) { $issues += "❌ MySQL không chạy" }

if ($issues.Count -eq 0) {
    Write-Host "✅ Mọi thứ OK! Có $($mp3Files.Count) file MP3" -ForegroundColor Green
} else {
    Write-Host "⚠️ Phát hiện $($issues.Count) vấn đề:" -ForegroundColor Yellow
    $issues | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
}

Write-Host ""
Write-Host "Nhấn Enter để thoát..." -ForegroundColor Gray
Read-Host
