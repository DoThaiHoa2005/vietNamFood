# ============================================================
# TEST LOGIN - Vietnam Food Guide
# ============================================================

Write-Host "🧪 TEST ĐĂNG NHẬP" -ForegroundColor Cyan
Write-Host "=" * 60
Write-Host ""

# Kiểm tra XAMPP
Write-Host "📋 BƯỚC 1: Kiểm tra XAMPP" -ForegroundColor Yellow
Write-Host "-" * 60

$apiFile = "C:\xampp\htdocs\vfg-api\api.php"
if (Test-Path $apiFile) {
    Write-Host "✅ File api.php tồn tại" -ForegroundColor Green
} else {
    Write-Host "❌ File api.php KHÔNG tồn tại!" -ForegroundColor Red
    Write-Host "   Đường dẫn: $apiFile" -ForegroundColor Red
    exit
}

Write-Host ""

# Test API connection
Write-Host "📋 BƯỚC 2: Test API Connection" -ForegroundColor Yellow
Write-Host "-" * 60

try {
    $response = Invoke-WebRequest -Uri "http://localhost/vfg-api/api.php?action=foods" -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ API hoạt động (Status: 200 OK)" -ForegroundColor Green
        $data = $response.Content | ConvertFrom-Json
        Write-Host "✅ Số quán ăn: $($data.Count)" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Không thể kết nối API!" -ForegroundColor Red
    Write-Host "   Lỗi: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔧 Kiểm tra:" -ForegroundColor Yellow
    Write-Host "   1. XAMPP Apache đang chạy?" -ForegroundColor White
    Write-Host "   2. XAMPP MySQL đang chạy?" -ForegroundColor White
    Write-Host "   3. File api.php tồn tại?" -ForegroundColor White
    exit
}

Write-Host ""

# Test Login
Write-Host "📋 BƯỚC 3: Test Login" -ForegroundColor Yellow
Write-Host "-" * 60

$loginData = @{
    username = "admin"
    password = "admin123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost/vfg-api/api.php?action=login" `
        -Method POST `
        -Body $loginData `
        -ContentType "application/json; charset=utf-8"
    
    if ($loginResponse.success) {
        Write-Host "✅ ĐĂNG NHẬP THÀNH CÔNG!" -ForegroundColor Green
        Write-Host ""
        Write-Host "👤 Thông tin user:" -ForegroundColor Cyan
        Write-Host "   Username: $($loginResponse.user.username)" -ForegroundColor White
        Write-Host "   Role: $($loginResponse.user.role)" -ForegroundColor White
        Write-Host "   User ID: $($loginResponse.user.id)" -ForegroundColor White
        Write-Host ""
        Write-Host "✅ App có thể đăng nhập được!" -ForegroundColor Green
    } else {
        Write-Host "❌ Đăng nhập thất bại!" -ForegroundColor Red
        Write-Host "   Lỗi: $($loginResponse.error)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Lỗi khi gọi API login!" -ForegroundColor Red
    Write-Host "   Lỗi: $($_.Exception.Message)" -ForegroundColor Red
    
    # Kiểm tra response
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host ""
        Write-Host "📄 Response từ server:" -ForegroundColor Yellow
        Write-Host $responseBody -ForegroundColor White
    }
    
    Write-Host ""
    Write-Host "🔧 Có thể do:" -ForegroundColor Yellow
    Write-Host "   1. Database chưa có users (chạy setup_complete.sql)" -ForegroundColor White
    Write-Host "   2. Password hash sai format" -ForegroundColor White
    Write-Host "   3. MySQL không chạy" -ForegroundColor White
}

Write-Host ""
Write-Host "=" * 60
Write-Host "🎯 Test hoàn tất!" -ForegroundColor Cyan
Write-Host ""

# Test với user123
Write-Host "📋 BƯỚC 4: Test Login với user123" -ForegroundColor Yellow
Write-Host "-" * 60

$loginData2 = @{
    username = "user123"
    password = "user123"
} | ConvertTo-Json

try {
    $loginResponse2 = Invoke-RestMethod -Uri "http://localhost/vfg-api/api.php?action=login" `
        -Method POST `
        -Body $loginData2 `
        -ContentType "application/json; charset=utf-8"
    
    if ($loginResponse2.success) {
        Write-Host "✅ User123 đăng nhập thành công!" -ForegroundColor Green
    } else {
        Write-Host "❌ User123 đăng nhập thất bại: $($loginResponse2.error)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Lỗi khi test user123: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=" * 60
Write-Host ""

# Tổng kết
Write-Host "📊 TỔNG KẾT:" -ForegroundColor Cyan
Write-Host "-" * 60
Write-Host "✅ File api.php: Tồn tại" -ForegroundColor Green
Write-Host "✅ API Connection: OK" -ForegroundColor Green
Write-Host "✅ Login Function: Kiểm tra ở trên" -ForegroundColor Yellow
Write-Host ""
Write-Host "🎯 Nếu login thành công → App có thể đăng nhập!" -ForegroundColor Green
Write-Host "❌ Nếu login thất bại → Xem hướng dẫn trong TEST_LOGIN.md" -ForegroundColor Yellow
Write-Host ""
