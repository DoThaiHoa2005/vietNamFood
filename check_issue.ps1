# ============================================================
# KIỂM TRA VẤN ĐỀ "KHÔNG CÓ DỮ LIỆU QUÁN ĂN"
# ============================================================

Write-Host "🔍 KIỂM TRA VẤN ĐỀ" -ForegroundColor Cyan
Write-Host "=" * 80
Write-Host ""

# 1. Kiểm tra XAMPP
Write-Host "📋 BƯỚC 1: Kiểm tra XAMPP" -ForegroundColor Yellow
Write-Host "-" * 80

$apacheRunning = Get-Process -Name "httpd" -ErrorAction SilentlyContinue
$mysqlRunning = Get-Process -Name "mysqld" -ErrorAction SilentlyContinue

if ($apacheRunning) {
    Write-Host "✅ Apache đang chạy" -ForegroundColor Green
} else {
    Write-Host "❌ Apache KHÔNG chạy!" -ForegroundColor Red
    Write-Host "   → Mở XAMPP Control Panel và Start Apache" -ForegroundColor Yellow
}

if ($mysqlRunning) {
    Write-Host "✅ MySQL đang chạy" -ForegroundColor Green
} else {
    Write-Host "❌ MySQL KHÔNG chạy!" -ForegroundColor Red
    Write-Host "   → Mở XAMPP Control Panel và Start MySQL" -ForegroundColor Yellow
}

Write-Host ""

# 2. Kiểm tra file api.php
Write-Host "📋 BƯỚC 2: Kiểm tra file API" -ForegroundColor Yellow
Write-Host "-" * 80

$apiFile = "C:\xampp\htdocs\vfg-api\api.php"
if (Test-Path $apiFile) {
    Write-Host "✅ File api.php tồn tại" -ForegroundColor Green
} else {
    Write-Host "❌ File api.php KHÔNG tồn tại!" -ForegroundColor Red
    Write-Host "   → Copy thư mục xampp_api vào C:\xampp\htdocs\vfg-api\" -ForegroundColor Yellow
}

Write-Host ""

# 3. Test API
Write-Host "📋 BƯỚC 3: Test API" -ForegroundColor Yellow
Write-Host "-" * 80

try {
    $response = Invoke-WebRequest -Uri "http://localhost/vfg-api/api.php?action=foods" -UseBasicParsing -TimeoutSec 5
    
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ API hoạt động (Status: 200)" -ForegroundColor Green
        
        $data = $response.Content | ConvertFrom-Json
        $count = $data.Count
        
        if ($count -gt 0) {
            Write-Host "✅ API trả về $count quán ăn" -ForegroundColor Green
        } else {
            Write-Host "❌ API trả về 0 quán ăn!" -ForegroundColor Red
            Write-Host "   → Database chưa có dữ liệu" -ForegroundColor Yellow
            Write-Host "   → Chạy file: xampp_api/setup_complete.sql" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "❌ Không thể kết nối API!" -ForegroundColor Red
    Write-Host "   Lỗi: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "   Nguyên nhân có thể:" -ForegroundColor Yellow
    Write-Host "   1. Apache không chạy" -ForegroundColor White
    Write-Host "   2. MySQL không chạy" -ForegroundColor White
    Write-Host "   3. File api.php không tồn tại" -ForegroundColor White
    Write-Host "   4. Database chưa được tạo" -ForegroundColor White
}

Write-Host ""

# 4. Kiểm tra SQLite
Write-Host "📋 BƯỚC 4: Kiểm tra SQLite" -ForegroundColor Yellow
Write-Host "-" * 80

$sqliteDb = "VietnamFoodGuide\bin\Debug\net48\Data\foods.db"
if (Test-Path $sqliteDb) {
    $fileInfo = Get-Item $sqliteDb
    Write-Host "✅ File foods.db tồn tại" -ForegroundColor Green
    Write-Host "   Kích thước: $($fileInfo.Length) bytes" -ForegroundColor White
    Write-Host "   Lần sửa cuối: $($fileInfo.LastWriteTime)" -ForegroundColor White
    
    if ($fileInfo.Length -lt 10000) {
        Write-Host "⚠️  File quá nhỏ, có thể chưa có dữ liệu" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ File foods.db KHÔNG tồn tại!" -ForegroundColor Red
    Write-Host "   → Chạy app một lần để tạo file" -ForegroundColor Yellow
}

Write-Host ""

# 5. Tổng kết
Write-Host "=" * 80
Write-Host "📊 TỔNG KẾT" -ForegroundColor Cyan
Write-Host "=" * 80
Write-Host ""

$issues = @()

if (-not $apacheRunning) { $issues += "Apache không chạy" }
if (-not $mysqlRunning) { $issues += "MySQL không chạy" }
if (-not (Test-Path $apiFile)) { $issues += "File api.php không tồn tại" }

if ($issues.Count -eq 0) {
    Write-Host "✅ Tất cả kiểm tra OK!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔧 VẤN ĐỀ CÓ THỂ DO:" -ForegroundColor Yellow
    Write-Host "   1. Database chưa có dữ liệu" -ForegroundColor White
    Write-Host "      → Chạy: xampp_api/setup_complete.sql trong phpMyAdmin" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   2. App chưa kết nối được API" -ForegroundColor White
    Write-Host "      → Kiểm tra Output Window trong Visual Studio" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   3. SQLite chưa có dữ liệu" -ForegroundColor White
    Write-Host "      → Chạy app khi API hoạt động để sync dữ liệu" -ForegroundColor Cyan
} else {
    Write-Host "❌ CÓ VẤN ĐỀ CẦN FIX:" -ForegroundColor Red
    Write-Host ""
    foreach ($issue in $issues) {
        Write-Host "   • $issue" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "🔧 CÁCH FIX:" -ForegroundColor Yellow
    Write-Host "   1. Mở XAMPP Control Panel" -ForegroundColor White
    Write-Host "   2. Start Apache và MySQL" -ForegroundColor White
    Write-Host "   3. Copy xampp_api vào C:\xampp\htdocs\vfg-api\" -ForegroundColor White
    Write-Host "   4. Chạy setup_complete.sql trong phpMyAdmin" -ForegroundColor White
}

Write-Host ""
Write-Host "=" * 80
Write-Host ""
