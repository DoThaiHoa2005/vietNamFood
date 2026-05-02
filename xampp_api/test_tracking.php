<?php
// Test gửi tracking data
header("Content-Type: application/json; charset=utf-8");

$host = 'localhost';
$db = 'VietnamFoodGuide';
$user = 'root';
$pass = '';

try {
    $pdo = new PDO("mysql:host=$host;dbname=$db;charset=utf8mb4", $user, $pass);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    
    // Test data: User ID 2 (user123) đang navigate
    $userId = 2;
    $currentLat = 10.7769;
    $currentLng = 106.7009;
    $destLat = 10.78567;
    $destLng = 106.70189;
    $destName = 'Phở Đặc Biệt Trần Hưng Đạo';
    $isNavigating = true;
    $isActive = true;
    
    // Cập nhật LastActiveTime cho user
    $stmt = $pdo->prepare("UPDATE Users SET LastActiveTime = NOW() WHERE Id = ?");
    $stmt->execute([$userId]);
    
    // Kiểm tra xem user đã có tracking record chưa
    $stmt = $pdo->prepare("SELECT Id FROM UserTracking WHERE UserId = ?");
    $stmt->execute([$userId]);
    $existing = $stmt->fetch();
    
    if ($existing) {
        // Update existing record
        $stmt = $pdo->prepare("
            UPDATE UserTracking SET
                CurrentLat = ?,
                CurrentLng = ?,
                DestinationLat = ?,
                DestinationLng = ?,
                DestinationName = ?,
                IsNavigating = ?,
                IsActive = ?,
                LastUpdate = NOW()
            WHERE UserId = ?
        ");
        $stmt->execute([
            $currentLat, $currentLng,
            $destLat, $destLng, $destName,
            $isNavigating, $isActive,
            $userId
        ]);
        $message = "Updated existing tracking record";
    } else {
        // Insert new record
        $stmt = $pdo->prepare("
            INSERT INTO UserTracking 
            (UserId, CurrentLat, CurrentLng, DestinationLat, DestinationLng, DestinationName, IsNavigating, IsActive, LastUpdate, CreatedDate)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, NOW(), NOW())
        ");
        $stmt->execute([
            $userId,
            $currentLat, $currentLng,
            $destLat, $destLng, $destName,
            $isNavigating, $isActive
        ]);
        $message = "Inserted new tracking record";
    }
    
    // Kiểm tra kết quả
    $stmt = $pdo->prepare("
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
        WHERE t.UserId = ?
    ");
    $stmt->execute([$userId]);
    $result = $stmt->fetch(PDO::FETCH_ASSOC);
    
    echo json_encode([
        "success" => true,
        "message" => $message,
        "data" => $result,
        "shouldCount" => $result['IsNavigating'] && $result['IsActive'] && $result['IsUserOnline']
    ], JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE);
    
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode([
        "success" => false,
        "error" => $e->getMessage()
    ]);
}
?>
