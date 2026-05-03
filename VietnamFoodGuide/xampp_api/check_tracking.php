<?php
// Kiểm tra tracking data trong database
header("Content-Type: text/html; charset=utf-8");

$host = 'localhost';
$db = 'VietnamFoodGuide';
$user = 'root';
$pass = '';

try {
    $pdo = new PDO("mysql:host=$host;dbname=$db;charset=utf8mb4", $user, $pass);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    
    echo "<h2>🔍 KIỂM TRA TRACKING DATA</h2>";
    echo "<style>table{border-collapse:collapse;width:100%;}th,td{border:1px solid #ddd;padding:8px;text-align:left;}th{background:#4CAF50;color:white;}</style>";
    
    // 1. Kiểm tra Users
    echo "<h3>👥 Users</h3>";
    $stmt = $pdo->query("SELECT Id, Username, Role, LastActiveTime, IsLocked FROM Users");
    $users = $stmt->fetchAll(PDO::FETCH_ASSOC);
    
    echo "<table>";
    echo "<tr><th>ID</th><th>Username</th><th>Role</th><th>LastActiveTime</th><th>IsLocked</th><th>Online?</th></tr>";
    foreach ($users as $u) {
        $isOnline = $u['LastActiveTime'] && (strtotime($u['LastActiveTime']) > strtotime('-5 minutes'));
        $onlineStatus = $isOnline ? '🟢 Online' : '⚪ Offline';
        echo "<tr>";
        echo "<td>{$u['Id']}</td>";
        echo "<td>{$u['Username']}</td>";
        echo "<td>{$u['Role']}</td>";
        echo "<td>{$u['LastActiveTime']}</td>";
        echo "<td>" . ($u['IsLocked'] ? '🔒 Locked' : '✅ Active') . "</td>";
        echo "<td>{$onlineStatus}</td>";
        echo "</tr>";
    }
    echo "</table>";
    
    // 2. Kiểm tra UserTracking
    echo "<h3>📍 UserTracking</h3>";
    $stmt = $pdo->query("
        SELECT 
            t.*,
            u.Username,
            u.LastActiveTime,
            CASE 
                WHEN u.LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE) THEN 1
                ELSE 0
            END as IsUserOnline
        FROM UserTracking t
        JOIN Users u ON t.UserId = u.Id
        ORDER BY t.LastUpdate DESC
    ");
    $tracking = $stmt->fetchAll(PDO::FETCH_ASSOC);
    
    if (count($tracking) > 0) {
        echo "<table>";
        echo "<tr><th>UserId</th><th>Username</th><th>IsNavigating</th><th>IsActive</th><th>User Online?</th><th>Destination</th><th>DestLat</th><th>DestLng</th><th>LastUpdate</th></tr>";
        foreach ($tracking as $t) {
            $isOnline = $t['IsUserOnline'] == 1;
            $shouldCount = $t['IsNavigating'] && $t['IsActive'] && $isOnline;
            $rowColor = $shouldCount ? 'background:#d4edda;' : '';
            
            echo "<tr style='{$rowColor}'>";
            echo "<td>{$t['UserId']}</td>";
            echo "<td>{$t['Username']}</td>";
            echo "<td>" . ($t['IsNavigating'] ? '🧭 TRUE' : '❌ FALSE') . "</td>";
            echo "<td>" . ($t['IsActive'] ? '✅ TRUE' : '❌ FALSE') . "</td>";
            echo "<td>" . ($isOnline ? '🟢 Online' : '⚪ Offline') . "</td>";
            echo "<td>{$t['DestinationName']}</td>";
            echo "<td>{$t['DestinationLat']}</td>";
            echo "<td>{$t['DestinationLng']}</td>";
            echo "<td>{$t['LastUpdate']}</td>";
            echo "</tr>";
        }
        echo "</table>";
        
        // Đếm số đang navigate
        $navigatingCount = 0;
        foreach ($tracking as $t) {
            if ($t['IsNavigating'] && $t['IsActive'] && $t['IsUserOnline']) {
                $navigatingCount++;
            }
        }
        
        echo "<h3>📊 Kết Quả Đếm</h3>";
        echo "<p style='font-size:20px;'><strong>🧭 Đang Chỉ Đường: {$navigatingCount}</strong></p>";
        echo "<p><em>Điều kiện: IsNavigating = TRUE AND IsActive = TRUE AND User Online (LastActiveTime < 5 phút)</em></p>";
        
    } else {
        echo "<p>❌ Chưa có dữ liệu tracking</p>";
    }
    
    // 3. Kiểm tra API stats
    echo "<h3>📊 API Stats (getAppStats)</h3>";
    $qrScanned = $pdo->query("SELECT COUNT(*) FROM Users WHERE QRScanned = 1")->fetchColumn();
    $appInstalled = $pdo->query("SELECT COUNT(*) FROM Users WHERE AppInstalled = 1")->fetchColumn();
    $currentlyActive = $pdo->query("SELECT COUNT(*) FROM Users WHERE LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE)")->fetchColumn();
    
    echo "<ul>";
    echo "<li>📱 QR Scanned: {$qrScanned}</li>";
    echo "<li>📲 App Installed: {$appInstalled}</li>";
    echo "<li>🟢 Currently Active: {$currentlyActive}</li>";
    echo "</ul>";
    
    // 4. Test query đếm navigating
    echo "<h3>🧪 Test Query Đếm Navigating</h3>";
    $stmt = $pdo->query("
        SELECT COUNT(*) as count
        FROM UserTracking t
        JOIN Users u ON t.UserId = u.Id
        WHERE t.IsNavigating = TRUE 
        AND t.IsActive = TRUE
        AND u.LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE)
    ");
    $result = $stmt->fetch(PDO::FETCH_ASSOC);
    echo "<p style='font-size:20px;'><strong>Query Result: {$result['count']}</strong></p>";
    
    echo "<hr>";
    echo "<p><a href='check_tracking.php'>🔄 Refresh</a> | <a href='api.php?action=getTracking'>📡 API getTracking</a> | <a href='api.php?action=getAppStats'>📊 API getAppStats</a></p>";
    
} catch (PDOException $e) {
    echo "<h2>❌ Lỗi kết nối database</h2>";
    echo "<p>" . $e->getMessage() . "</p>";
}
?>
