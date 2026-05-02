# ============================================================
# XEM DỮ LIỆU TRONG foods.db
# ============================================================

Write-Host "📊 XEM DỮ LIỆU FOODS.DB" -ForegroundColor Cyan
Write-Host "=" * 80
Write-Host ""

$dbPath = "VietnamFoodGuide\bin\Debug\net48\Data\foods.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "❌ Không tìm thấy file: $dbPath" -ForegroundColor Red
    exit
}

Write-Host "✅ File tồn tại: $dbPath" -ForegroundColor Green
$fileInfo = Get-Item $dbPath
Write-Host "📁 Kích thước: $($fileInfo.Length) bytes" -ForegroundColor White
Write-Host "📅 Lần sửa cuối: $($fileInfo.LastWriteTime)" -ForegroundColor White
Write-Host ""

# Load System.Data.SQLite
$sqliteDll = "VietnamFoodGuide\bin\Debug\net48\System.Data.SQLite.dll"

if (-not (Test-Path $sqliteDll)) {
    Write-Host "❌ Không tìm thấy System.Data.SQLite.dll" -ForegroundColor Red
    Write-Host "   Hãy build project trước!" -ForegroundColor Yellow
    exit
}

Add-Type -Path $sqliteDll

# Kết nối database
$connectionString = "Data Source=$dbPath;Version=3;"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)

try {
    $connection.Open()
    Write-Host "✅ Kết nối database thành công!" -ForegroundColor Green
    Write-Host ""
    
    # Lấy danh sách tables
    Write-Host "📋 DANH SÁCH TABLES:" -ForegroundColor Yellow
    Write-Host "-" * 80
    
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'"
    $reader = $cmd.ExecuteReader()
    
    while ($reader.Read()) {
        Write-Host "  - $($reader["name"])" -ForegroundColor White
    }
    $reader.Close()
    
    Write-Host ""
    
    # Đếm số quán ăn
    Write-Host "📊 THỐNG KÊ:" -ForegroundColor Yellow
    Write-Host "-" * 80
    
    $cmd.CommandText = "SELECT COUNT(*) as Total FROM Foods"
    $total = $cmd.ExecuteScalar()
    Write-Host "  Tổng quán ăn: $total" -ForegroundColor White
    
    Write-Host ""
    
    # Lấy cấu trúc bảng Foods
    Write-Host "🏗️ CẤU TRÚC BẢNG FOODS:" -ForegroundColor Yellow
    Write-Host "-" * 80
    
    $cmd.CommandText = "PRAGMA table_info(Foods)"
    $reader = $cmd.ExecuteReader()
    
    Write-Host ("{0,-5} {1,-25} {2,-15} {3,-10}" -f "ID", "Tên cột", "Kiểu dữ liệu", "Not Null") -ForegroundColor Cyan
    Write-Host "-" * 80
    
    while ($reader.Read()) {
        $id = $reader["cid"]
        $name = $reader["name"]
        $type = $reader["type"]
        $notNull = if ($reader["notnull"] -eq 1) { "YES" } else { "NO" }
        
        Write-Host ("{0,-5} {1,-25} {2,-15} {3,-10}" -f $id, $name, $type, $notNull) -ForegroundColor White
    }
    $reader.Close()
    
    Write-Host ""
    
    # Lấy 5 quán ăn đầu tiên
    Write-Host "🍜 TOP 5 QUÁN ĂN (theo Rating):" -ForegroundColor Yellow
    Write-Host "-" * 80
    
    $cmd.CommandText = @"
SELECT 
    Id, 
    Name, 
    Category, 
    Rating, 
    Radius, 
    Priority,
    CooldownMinutes
FROM Foods 
ORDER BY Rating DESC, Priority DESC 
LIMIT 5
"@
    
    $reader = $cmd.ExecuteReader()
    
    Write-Host ("{0,-3} {1,-30} {2,-15} {3,-6} {4,-8} {5,-8} {6,-10}" -f "ID", "Tên", "Loại", "Rating", "Radius", "Priority", "Cooldown") -ForegroundColor Cyan
    Write-Host "-" * 80
    
    while ($reader.Read()) {
        $id = $reader["Id"]
        $name = $reader["Name"]
        $category = $reader["Category"]
        $rating = $reader["Rating"]
        $radius = $reader["Radius"]
        $priority = $reader["Priority"]
        $cooldown = $reader["CooldownMinutes"]
        
        Write-Host ("{0,-3} {1,-30} {2,-15} {3,-6} {4,-8} {5,-8} {6,-10}" -f $id, $name, $category, $rating, $radius, $priority, $cooldown) -ForegroundColor White
    }
    $reader.Close()
    
    Write-Host ""
    
    # Thống kê theo category
    Write-Host "📊 THỐNG KÊ THEO LOẠI:" -ForegroundColor Yellow
    Write-Host "-" * 80
    
    $cmd.CommandText = @"
SELECT 
    Category, 
    COUNT(*) as Total,
    ROUND(AVG(Rating), 1) as AvgRating
FROM Foods 
GROUP BY Category
ORDER BY Total DESC
"@
    
    $reader = $cmd.ExecuteReader()
    
    Write-Host ("{0,-20} {1,-10} {2,-15}" -f "Loại", "Số lượng", "Rating TB") -ForegroundColor Cyan
    Write-Host "-" * 80
    
    while ($reader.Read()) {
        $category = $reader["Category"]
        $total = $reader["Total"]
        $avgRating = $reader["AvgRating"]
        
        Write-Host ("{0,-20} {1,-10} {2,-15}" -f $category, $total, $avgRating) -ForegroundColor White
    }
    $reader.Close()
    
    Write-Host ""
    Write-Host "=" * 80
    Write-Host "✅ Hoàn tất!" -ForegroundColor Green
    Write-Host ""
    Write-Host "💡 Để xem chi tiết hơn, dùng DB Browser for SQLite:" -ForegroundColor Yellow
    Write-Host "   https://sqlitebrowser.org/" -ForegroundColor Cyan
    Write-Host ""
    
} catch {
    Write-Host "❌ Lỗi: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}
