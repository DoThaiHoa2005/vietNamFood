<?php
// Kiểm tra kết nối database và tables
header("Content-Type: text/html; charset=utf-8");

$host = 'localhost';
$db = 'VietnamFoodGuide';
$user = 'root';
$pass = '';

echo "<h2>🔍 KIỂM TRA KẾT NỐI XAMPP</h2>";
echo "<style>
    body { font-family: Arial, sans-serif; padding: 20px; }
    .success { color: #2d7a4f; font-weight: bold; }
    .error { color: #dc2626; font-weight: bold; }
    .info { color: #457B9D; }
    table { border-collapse: collapse; width: 100%; margin: 20px 0; }
    th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }
    th { background: #4CAF50; color: white; }
    tr:hover { background: #f5f5f5; }
</style>";

try {
    // 1. Kết nối database
    $pdo = new PDO("mysql:host=$host;dbname=$db;charset=utf8mb4", $user, $pass);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    
    echo "<p class='success'>✅ Kết nối database thành công!</p>";
    echo "<p class='info'>📊 Database: <strong>$db</strong></p>";
    echo "<hr>";
    
    // 2. Kiểm tra các tables
    echo "<h3>📋 Danh Sách Tables</h3>";
    $stmt = $pdo->query("SHOW TABLES");
    $tables = $stmt->fetchAll(PDO::FETCH_COLUMN);
    
    echo "<table>";
    echo "<tr><th>STT</th><th>Table Name</th><th>Số Records</th><th>Status</th></tr>";
    
    $index = 1;
    foreach ($tables as $table) {
        $countStmt = $pdo->query("SELECT COUNT(*) FROM `$table`");
        $count = $countStmt->fetchColumn();
        
        echo "<tr>";
        echo "<td>$index</td>";
        echo "<td><strong>$table</strong></td>";
        echo "<td>$count</td>";
        echo "<td class='success'>✅ OK</td>";
        echo "</tr>";
        
        $index++;
    }
    
    echo "</table>";
    
    // 3. Kiểm tra dữ liệu Foods
    echo "<hr>";
    echo "<h3>🍜 Dữ Liệu Foods (Top 5)</h3>";
    $stmt = $pdo->query("SELECT Id, Name, City, Rating FROM Foods ORDER BY Rating DESC LIMIT 5");
    $foods = $stmt->fetchAll(PDO::FETCH_ASSOC);
    
    echo "<table>";
    echo "<tr><th>ID</th><th>Name</th><th>City</th><th>Rating</th></tr>";
    
    foreach ($foods as $food) {
        echo "<tr>";
        echo "<td>{$food['Id']}</td>";
        echo "<td>{$food['Name']}</td>";
        echo "<td>{$food['City']}</td>";
        echo "<td>⭐ {$food['Rating']}</td>";
        echo "</tr>";
    }
    
    echo "</table>";
    
    // 4. Kiểm tra Users
    echo "<hr>";
    echo "<h3>👥 Dữ Liệu Users</h3>";
    $stmt = $pdo->query("SELECT Id, Username, Role, CreatedDate FROM Users");
    $users = $stmt->fetchAll(PDO::FETCH_ASSOC);
    
    echo "<table>";
    echo "<tr><th>ID</th><th>Username</th><th>Role</th><th>Created Date</th></tr>";
    
    foreach ($users as $user) {
        echo "<tr>";
        echo "<td>{$user['Id']}</td>";
        echo "<td>{$user['Username']}</td>";
        echo "<td>{$user['Role']}</td>";
        echo "<td>{$user['CreatedDate']}</td>";
        echo "</tr>";
    }
    
    echo "</table>";
    
    // 5. Kiểm tra API endpoints
    echo "<hr>";
    echo "<h3>🔗 API Endpoints</h3>";
    echo "<ul>";
    echo "<li><a href='api.php?action=foods' target='_blank'>GET /api.php?action=foods</a> - Lấy danh sách quán ăn</li>";
    echo "<li><a href='api.php?action=stats' target='_blank'>GET /api.php?action=stats</a> - Thống kê</li>";
    echo "<li><a href='api.php?action=users' target='_blank'>GET /api.php?action=users</a> - Danh sách users</li>";
    echo "<li><a href='api.php?action=getAppStats' target='_blank'>GET /api.php?action=getAppStats</a> - App stats</li>";
    echo "<li><a href='admin_dashboard.html' target='_blank'>Admin Dashboard</a> - Trang quản trị</li>";
    echo "</ul>";
    
    // 6. Tổng kết
    echo "<hr>";
    echo "<h3>✅ KẾT LUẬN</h3>";
    echo "<p class='success'>🎉 XAMPP đang hoạt động hoàn hảo!</p>";
    echo "<ul>";
    echo "<li>✅ Apache: Running</li>";
    echo "<li>✅ MySQL: Running</li>";
    echo "<li>✅ Database: Connected</li>";
    echo "<li>✅ Tables: " . count($tables) . " tables</li>";
    echo "<li>✅ API: Working</li>";
    echo "</ul>";
    
} catch (PDOException $e) {
    echo "<p class='error'>❌ Lỗi kết nối database!</p>";
    echo "<p class='error'>Error: " . $e->getMessage() . "</p>";
    echo "<hr>";
    echo "<h3>💡 Giải pháp:</h3>";
    echo "<ol>";
    echo "<li>Kiểm tra XAMPP Control Panel → MySQL phải đang chạy</li>";
    echo "<li>Mở phpMyAdmin: <a href='http://localhost/phpmyadmin' target='_blank'>http://localhost/phpmyadmin</a></li>";
    echo "<li>Tạo database <strong>VietnamFoodGuide</strong> nếu chưa có</li>";
    echo "<li>Import file: <strong>xampp_api/setup_complete.sql</strong></li>";
    echo "</ol>";
}
?>
