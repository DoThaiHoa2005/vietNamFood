# Script tự động setup API cho XAMPP
# Chạy: .\setup_api.ps1

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "SETUP API CHO VIETNAM FOOD GUIDE" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra XAMPP folder
$xamppPath = "C:\xampp"
if (-not (Test-Path $xamppPath)) {
    Write-Host "❌ Không tìm thấy XAMPP tại $xamppPath" -ForegroundColor Red
    Write-Host "Vui lòng cài đặt XAMPP trước!" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Tìm thấy XAMPP tại $xamppPath" -ForegroundColor Green

# Kiểm tra htdocs
$htdocsPath = "$xamppPath\htdocs"
if (-not (Test-Path $htdocsPath)) {
    Write-Host "❌ Không tìm thấy folder htdocs" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Tìm thấy htdocs tại $htdocsPath" -ForegroundColor Green

# Copy API folder
$sourcePath = ".\xampp_api"
$destPath = "$htdocsPath\vfg-api"

if (-not (Test-Path $sourcePath)) {
    Write-Host "❌ Không tìm thấy folder xampp_api trong project" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "📁 Đang copy API folder..." -ForegroundColor Yellow

# Xóa folder cũ nếu có
if (Test-Path $destPath) {
    Write-Host "⚠️  Folder vfg-api đã tồn tại, đang xóa..." -ForegroundColor Yellow
    Remove-Item -Path $destPath -Recurse -Force
}

# Copy folder mới
Copy-Item -Path $sourcePath -Destination $destPath -Recurse
Write-Host "✅ Đã copy API folder vào $destPath" -ForegroundColor Green

# Tạo uploads folder
$uploadsPath = "$destPath\uploads"
if (-not (Test-Path $uploadsPath)) {
    New-Item -Path $uploadsPath -ItemType Directory | Out-Null
    Write-Host "✅ Đã tạo folder uploads" -ForegroundColor Green
}

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "SETUP HOÀN TẤT!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 CÁC BƯỚC TIẾP THEO:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Mở XAMPP Control Panel" -ForegroundColor White
Write-Host "2. Start Apache và MySQL" -ForegroundColor White
Write-Host "3. Mở trình duyệt: http://localhost/phpmyadmin" -ForegroundColor White
Write-Host "4. Chọn database 'VietnamFoodGuide'" -ForegroundColor White
Write-Host "5. Click tab 'SQL'" -ForegroundColor White
Write-Host "6. Copy nội dung file 'xampp_api\setup_complete.sql' và paste vào" -ForegroundColor White
Write-Host "7. Click 'Go' để chạy" -ForegroundColor White
Write-Host ""
Write-Host "🧪 TEST API:" -ForegroundColor Yellow
Write-Host "Mở trình duyệt: http://localhost/vfg-api/api.php?action=stats" -ForegroundColor Cyan
Write-Host ""
Write-Host "🎯 SAU ĐÓ:" -ForegroundColor Yellow
Write-Host "Build lại project WPF và test đăng ký!" -ForegroundColor White
Write-Host ""
