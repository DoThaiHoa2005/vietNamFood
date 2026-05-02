<?php
// ============================================================
// Vietnam Food Guide - PHP API cho XAMPP
// Đặt vào: C:\xampp\htdocs\vfg-api\api.php
// ============================================================

header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS");
header("Access-Control-Allow-Headers: Content-Type");

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') { http_response_code(200); exit; }

// ============================================================
// CẤU HÌNH KẾT NỐI DATABASE
// ============================================================

// --- LỰA CHỌN 1: KHI CHẠY TRÊN HOSTING (Đã ngắt vì bị chặn) ---
/*
$host = 'sql301.infinityfree.com'; // MySQL Hostname (Lấy từ Hosting Panel)
$db   = 'if0_41694117_vietnamfoodguide';        // Tên Database đầy đủ (Ví dụ: if0_38626359_vfg_db)
$user = 'if0_41694117';               // DB Username (Ví dụ: if0_38626359)
$pass = 'Thaihoa27092005';   // DB Password (Mật khẩu tài khoản host)
$charset = 'utf8mb4';
*/

// --- LỰA CHỌN 2: KHI CHẠY TRÊN XAMPP (Dùng với ngrok) ---
$host = 'localhost'; $db = 'VietnamFoodGuide'; $user = 'root'; $pass = ''; $charset = 'utf8mb4';


try {
    $pdo = new PDO("mysql:host=$host;dbname=$db;charset=$charset", $user, $pass, [
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
    ]);
} catch (PDOException $e) {
    http_response_code(500);
    header("Content-Type: application/json; charset=utf-8");
    echo json_encode(["error" => "DB connection failed: " . $e->getMessage()]);
    exit;
}

$action = $_GET['action'] ?? 'foods';
$method = $_SERVER['REQUEST_METHOD'];

// ---- Upload ảnh ----
if ($action === 'upload' && $method === 'POST') {
    $uploadDir = __DIR__ . '/uploads/';
    if (!is_dir($uploadDir)) mkdir($uploadDir, 0755, true);

    if (!isset($_FILES['image'])) {
        http_response_code(400);
        echo json_encode(["error" => "Không có file"]);
        exit;
    }

    $file = $_FILES['image'];
    $ext  = strtolower(pathinfo($file['name'], PATHINFO_EXTENSION));
    $allowed = ['jpg','jpeg','png','gif','webp'];

    if (!in_array($ext, $allowed)) {
        http_response_code(400);
        echo json_encode(["error" => "Định dạng không hợp lệ"]);
        exit;
    }

    $filename = uniqid('food_') . '.' . $ext;
    $dest = $uploadDir . $filename;

    if (move_uploaded_file($file['tmp_name'], $dest)) {
        header("Content-Type: application/json; charset=utf-8");
        echo json_encode([
            "url"      => "http://localhost/vfg-api/uploads/" . $filename,
            "filename" => $filename
        ]);
    } else {
        http_response_code(500);
        echo json_encode(["error" => "Upload thất bại"]);
    }
    exit;
}

header("Content-Type: application/json; charset=utf-8");

switch ($action) {
    // ---- Danh sách + thêm mới ----
    case 'foods':
        if ($method === 'GET') {
            $stmt = $pdo->query("SELECT * FROM Foods ORDER BY Rating DESC");
            echo json_encode($stmt->fetchAll());
        } elseif ($method === 'POST') {
            $d = json_decode(file_get_contents('php://input'), true);
            $stmt = $pdo->prepare("INSERT INTO Foods
                (Name,City,Category,Description_VI,Description_EN,Description_CN,Latitude,Longitude,Rating,ImagePath,Radius,Priority,AudioUrl,NarrationScript,CooldownMinutes)
                VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)");
            $stmt->execute([
                $d['Name'], $d['City'], $d['Category'],
                $d['Description_VI'], $d['Description_EN'], $d['Description_CN'],
                $d['Latitude'], $d['Longitude'], $d['Rating'], $d['ImagePath'],
                $d['Radius'] ?? 30.0, $d['Priority'] ?? 5, $d['AudioUrl'] ?? null,
                $d['NarrationScript'] ?? null, $d['CooldownMinutes'] ?? 5
            ]);
            echo json_encode(["id" => $pdo->lastInsertId(), "message" => "Thêm thành công"]);
        }
        break;

    // ---- Sửa + xóa theo ID ----
    case 'food':
        $id = (int)($_GET['id'] ?? 0);
        if ($method === 'PUT') {
            $d = json_decode(file_get_contents('php://input'), true);
            $stmt = $pdo->prepare("UPDATE Foods SET
                Name=?, City=?, Category=?,
                Description_VI=?, Description_EN=?, Description_CN=?,
                Latitude=?, Longitude=?, Rating=?, ImagePath=?,
                Radius=?, Priority=?, AudioUrl=?, NarrationScript=?, CooldownMinutes=?
                WHERE Id=?");
            $stmt->execute([
                $d['Name'], $d['City'], $d['Category'],
                $d['Description_VI'], $d['Description_EN'], $d['Description_CN'],
                $d['Latitude'], $d['Longitude'], $d['Rating'], $d['ImagePath'],
                $d['Radius'] ?? 30.0, $d['Priority'] ?? 5, $d['AudioUrl'] ?? null,
                $d['NarrationScript'] ?? null, $d['CooldownMinutes'] ?? 5,
                $id
            ]);
            echo json_encode(["message" => "Cập nhật thành công"]);
        } elseif ($method === 'DELETE') {
            $pdo->prepare("DELETE FROM Foods WHERE Id=?")->execute([$id]);
            echo json_encode(["message" => "Đã xóa"]);
        }
        break;

    case 'stats':
        $total = $pdo->query("SELECT COUNT(*) FROM Foods")->fetchColumn();
        $cats  = $pdo->query("SELECT COUNT(DISTINCT Category) FROM Foods")->fetchColumn();
        $avg   = $pdo->query("SELECT ROUND(AVG(Rating),1) FROM Foods")->fetchColumn();
        $users = $pdo->query("SELECT COUNT(*) FROM Users")->fetchColumn();
        $favorites = $pdo->query("SELECT COUNT(*) FROM Favorites")->fetchColumn();
        echo json_encode(["totalFoods"=>$total,"totalCategories"=>$cats,"avgRating"=>$avg,"totalUsers"=>$users,"totalFavorites"=>$favorites]);
        break;

    case 'users':
        $stmt = $pdo->query("SELECT Id, Username, Role, IsLocked, CreatedDate FROM Users ORDER BY Id");
        echo json_encode($stmt->fetchAll());
        break;

    case 'init_users':
        // Endpoint để tự động tạo users nếu chưa có
        if ($method === 'POST') {
            try {
                // Kiểm tra xem đã có users chưa
                $count = $pdo->query("SELECT COUNT(*) FROM Users")->fetchColumn();
                
                if ($count == 0) {
                    // Tạo users mặc định
                    $adminHash = password_hash('admin123', PASSWORD_BCRYPT);
                    $userHash = password_hash('user123', PASSWORD_BCRYPT);
                    
                    $stmt = $pdo->prepare("INSERT INTO Users (Username, PasswordHash, Role, CreatedDate) VALUES (?, ?, ?, NOW())");
                    $stmt->execute(['admin', $adminHash, 'Admin']);
                    $stmt->execute(['user123', $userHash, 'User']);
                    
                    echo json_encode([
                        "success" => true,
                        "message" => "Đã tạo 2 users mặc định",
                        "users" => ["admin", "user123"]
                    ]);
                } else {
                    // Fix password hash cho users hiện có
                    $adminHash = password_hash('admin123', PASSWORD_BCRYPT);
                    $userHash = password_hash('user123', PASSWORD_BCRYPT);
                    
                    $stmt = $pdo->prepare("UPDATE Users SET PasswordHash = ? WHERE Username = ?");
                    $stmt->execute([$adminHash, 'admin']);
                    $stmt->execute([$userHash, 'user123']);
                    
                    echo json_encode([
                        "success" => true,
                        "message" => "Đã fix password hash cho users hiện có",
                        "count" => $count
                    ]);
                }
            } catch (Exception $e) {
                http_response_code(500);
                echo json_encode(["error" => $e->getMessage()]);
            }
        }
        break;

    case 'login':
        if ($method === 'POST') {
            $data = json_decode(file_get_contents('php://input'), true);
            $username = $data['username'] ?? '';
            $password = $data['password'] ?? '';
            
            if (empty($username) || empty($password)) {
                http_response_code(400);
                echo json_encode(["error" => "Username và password không được để trống"]);
                break;
            }
            
            $stmt = $pdo->prepare("SELECT * FROM Users WHERE Username = ?");
            $stmt->execute([$username]);
            $user = $stmt->fetch();
            
            if (!$user) {
                http_response_code(401);
                echo json_encode(["error" => "Tài khoản không tồn tại"]);
                break;
            }
            
            // Kiểm tra tài khoản có bị khóa không
            if ($user['IsLocked']) {
                http_response_code(403);
                echo json_encode(["error" => "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên."]);
                break;
            }
            
            // Kiểm tra password với BCrypt
            $passwordValid = password_verify($password, $user['PasswordHash']);
            
            // Nếu hash sai, thử fix tự động với password mặc định
            if (!$passwordValid) {
                // Kiểm tra xem có phải password mặc định không
                $defaultPasswords = [
                    'admin' => 'admin123',
                    'user123' => 'user123'
                ];
                
                if (isset($defaultPasswords[$username]) && $password === $defaultPasswords[$username]) {
                    // Fix password hash ngay
                    $newHash = password_hash($password, PASSWORD_BCRYPT);
                    $updateStmt = $pdo->prepare("UPDATE Users SET PasswordHash = ? WHERE Id = ?");
                    $updateStmt->execute([$newHash, $user['Id']]);
                    
                    // Đăng nhập thành công sau khi fix
                    $passwordValid = true;
                }
            }
            
            if ($passwordValid) {
                // Tạo session token
                $token = bin2hex(random_bytes(32)); // Token ngẫu nhiên 64 ký tự
                $expiresAt = date('Y-m-d H:i:s', strtotime('+7 days')); // Hết hạn sau 7 ngày
                
                // Lưu session vào database
                $stmt = $pdo->prepare("INSERT INTO Sessions (UserId, Token, ExpiresAt, CreatedDate) VALUES (?, ?, ?, NOW())");
                $stmt->execute([$user['Id'], $token, $expiresAt]);
                
                // Cập nhật LastActiveTime cho user
                $stmt = $pdo->prepare("UPDATE Users SET LastActiveTime = NOW() WHERE Id = ?");
                $stmt->execute([$user['Id']]);
                
                // Đăng nhập thành công
                echo json_encode([
                    "success" => true,
                    "user" => [
                        "id" => $user['Id'],
                        "username" => $user['Username'],
                        "role" => $user['Role']
                    ],
                    "token" => $token,
                    "expiresAt" => $expiresAt,
                    "message" => "Đăng nhập thành công"
                ]);
            } else {
                http_response_code(401);
                echo json_encode(["error" => "Mật khẩu không đúng"]);
            }
        }
        break;

    case 'register':
        if ($method === 'POST') {
            $data = json_decode(file_get_contents('php://input'), true);
            $username = trim($data['username'] ?? '');
            $password = $data['password'] ?? '';
            $email = trim($data['email'] ?? '');
            
            if (empty($username) || empty($password)) {
                http_response_code(400);
                echo json_encode(["error" => "Username và password không được để trống"]);
                break;
            }
            
            if (strlen($password) < 6) {
                http_response_code(400);
                echo json_encode(["error" => "Password phải có ít nhất 6 ký tự"]);
                break;
            }
            
            // Kiểm tra username đã tồn tại chưa
            $stmt = $pdo->prepare("SELECT Id FROM Users WHERE Username = ?");
            $stmt->execute([$username]);
            if ($stmt->fetch()) {
                http_response_code(400);
                echo json_encode(["error" => "Username đã tồn tại"]);
                break;
            }
            
            // Tạo tài khoản mới (mặc định role = User)
            $passwordHash = password_hash($password, PASSWORD_BCRYPT);
            $stmt = $pdo->prepare("INSERT INTO Users (Username, PasswordHash, Role, CreatedDate) VALUES (?, ?, 'User', NOW())");
            $stmt->execute([$username, $passwordHash]);
            
            echo json_encode([
                "success" => true,
                "message" => "Đăng ký thành công! Vui lòng đăng nhập.",
                "userId" => $pdo->lastInsertId()
            ]);
        }
        break;

    case 'update_user_role':
        if ($method === 'PUT') {
            $data = json_decode(file_get_contents('php://input'), true);
            $userId = (int)($data['userId'] ?? 0);
            $newRole = $data['role'] ?? '';
            
            if (!in_array($newRole, ['User', 'Admin'])) {
                http_response_code(400);
                echo json_encode(["error" => "Role không hợp lệ"]);
                break;
            }
            
            $stmt = $pdo->prepare("UPDATE Users SET Role = ? WHERE Id = ?");
            $stmt->execute([$newRole, $userId]);
            
            echo json_encode([
                "success" => true,
                "message" => "Cập nhật role thành công"
            ]);
        }
        break;

    case 'toggle_user_lock':
        // Khóa/Mở khóa tài khoản user
        if ($method === 'PUT') {
            $data = json_decode(file_get_contents('php://input'), true);
            $userId = (int)($data['userId'] ?? 0);
            
            if ($userId <= 0) {
                http_response_code(400);
                echo json_encode(["error" => "UserId không hợp lệ"]);
                break;
            }
            
            // Lấy trạng thái hiện tại
            $stmt = $pdo->prepare("SELECT IsLocked FROM Users WHERE Id = ?");
            $stmt->execute([$userId]);
            $user = $stmt->fetch();
            
            if (!$user) {
                http_response_code(404);
                echo json_encode(["error" => "User không tồn tại"]);
                break;
            }
            
            // Đảo ngược trạng thái
            $newStatus = !$user['IsLocked'];
            $stmt = $pdo->prepare("UPDATE Users SET IsLocked = ? WHERE Id = ?");
            $stmt->execute([$newStatus, $userId]);
            
            echo json_encode([
                "success" => true,
                "isLocked" => $newStatus,
                "message" => $newStatus ? "Đã khóa tài khoản" : "Đã mở khóa tài khoản"
            ]);
        }
        break;

    case 'delete_user':
        // Xóa user (chỉ xóa được User, không xóa được Admin)
        if ($method === 'DELETE') {
            $userId = (int)($_GET['userId'] ?? 0);
            
            if ($userId <= 0) {
                http_response_code(400);
                echo json_encode(["error" => "UserId không hợp lệ"]);
                break;
            }
            
            // Kiểm tra role
            $stmt = $pdo->prepare("SELECT Role FROM Users WHERE Id = ?");
            $stmt->execute([$userId]);
            $user = $stmt->fetch();
            
            if (!$user) {
                http_response_code(404);
                echo json_encode(["error" => "User không tồn tại"]);
                break;
            }
            
            // Không cho xóa Admin
            if ($user['Role'] === 'Admin') {
                http_response_code(403);
                echo json_encode(["error" => "Không thể xóa tài khoản Admin. Chỉ có thể khóa."]);
                break;
            }
            
            // Xóa user (CASCADE sẽ tự động xóa Favorites, Sessions, UserTracking)
            $stmt = $pdo->prepare("DELETE FROM Users WHERE Id = ?");
            $stmt->execute([$userId]);
            
            echo json_encode([
                "success" => true,
                "message" => "Đã xóa user thành công"
            ]);
        }
        break;

    // ============================================================
    // USER TRACKING ENDPOINTS
    // ============================================================
    
    case 'getTracking':
        // Lấy tất cả tracking data
        $stmt = $pdo->query("SELECT * FROM UserTracking ORDER BY LastUpdate DESC");
        echo json_encode([
            "success" => true,
            "data" => $stmt->fetchAll()
        ]);
        break;

    case 'getUsers':
        // Lấy danh sách users (bao gồm thông tin QR và App)
        $stmt = $pdo->query("SELECT Id, Username, Role, QRScanned, AppInstalled, LastActiveTime, CreatedDate FROM Users ORDER BY Id");
        echo json_encode([
            "success" => true,
            "data" => $stmt->fetchAll()
        ]);
        break;

    case 'getSessions':
        // Lấy sessions với tracking info (join UserTracking)
        $stmt = $pdo->query("
            SELECT 
                u.Id as user_id,
                u.Username,
                t.CurrentLat as current_lat,
                t.CurrentLng as current_lng,
                t.DestinationLat as destination_lat,
                t.DestinationLng as destination_lng,
                t.DestinationName as current_destination,
                t.IsNavigating,
                t.IsActive as is_active,
                t.LastUpdate as last_update
            FROM Users u
            LEFT JOIN UserTracking t ON u.Id = t.UserId
            WHERE t.IsActive = 1
            ORDER BY t.LastUpdate DESC
        ");
        echo json_encode([
            "success" => true,
            "data" => $stmt->fetchAll()
        ]);
        break;

    case 'getFavorites':
        // Lấy danh sách favorites
        $stmt = $pdo->query("SELECT * FROM Favorites ORDER BY CreatedDate DESC");
        echo json_encode([
            "success" => true,
            "data" => $stmt->fetchAll()
        ]);
        break;

    case 'updateTracking':
        // Cập nhật vị trí tracking của user
        if ($method === 'POST') {
            $data = json_decode(file_get_contents('php://input'), true);
            $userId = (int)($data['userId'] ?? 0);
            $currentLat = (float)($data['currentLat'] ?? 0);
            $currentLng = (float)($data['currentLng'] ?? 0);
            $destLat = (float)($data['destinationLat'] ?? null);
            $destLng = (float)($data['destinationLng'] ?? null);
            $destName = $data['destinationName'] ?? null;
            $isNavigating = (bool)($data['isNavigating'] ?? false);
            $isActive = (bool)($data['isActive'] ?? true);
            
            if ($userId <= 0) {
                http_response_code(400);
                echo json_encode(["error" => "UserId không hợp lệ"]);
                break;
            }
            
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
                    $isNavigating ? 1 : 0, $isActive ? 1 : 0,
                    $userId
                ]);
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
                    $isNavigating ? 1 : 0, $isActive ? 1 : 0
                ]);
            }
            
            echo json_encode([
                "success" => true,
                "message" => "Cập nhật tracking thành công"
            ]);
        }
        break;

    case 'getAppStats':
        // Lấy thống kê về QR scan và app install
        // Đếm từ table qr_scans để chính xác hơn
        $qrScanned = $pdo->query("SELECT COUNT(*) FROM qr_scans")->fetchColumn();
        $appInstalled = $pdo->query("SELECT COUNT(*) FROM Users WHERE AppInstalled = 1")->fetchColumn();
        $currentlyActive = $pdo->query("SELECT COUNT(*) FROM Users WHERE LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE)")->fetchColumn();
        
        echo json_encode([
            "success" => true,
            "data" => [
                "qrScanned" => (int)$qrScanned,
                "appInstalled" => (int)$appInstalled,
                "currentlyActive" => (int)$currentlyActive
            ]
        ]);
        break;

    case 'updateUserActivity':
        // Cập nhật hoạt động của user (khi mở app)
        if ($method === 'POST') {
            $data = json_decode(file_get_contents('php://input'), true);
            $userId = (int)($data['userId'] ?? 0);
            $qrScanned = (bool)($data['qrScanned'] ?? false);
            $appInstalled = (bool)($data['appInstalled'] ?? false);
            
            if ($userId <= 0) {
                http_response_code(400);
                echo json_encode(["error" => "UserId không hợp lệ"]);
                break;
            }
            
            $stmt = $pdo->prepare("
                UPDATE Users SET 
                    QRScanned = ?,
                    AppInstalled = ?,
                    LastActiveTime = NOW()
                WHERE Id = ?
            ");
            $stmt->execute([$qrScanned, $appInstalled, $userId]);
            
            echo json_encode([
                "success" => true,
                "message" => "Cập nhật hoạt động user thành công"
            ]);
        }
        break;

    case 'addFavorite':
        // Thêm yêu thích
        if ($method === 'POST') {
            $data = json_decode(file_get_contents('php://input'), true);
            $userId = (int)($data['userId'] ?? 0);
            $foodId = (int)($data['foodId'] ?? 0);
            
            if ($userId <= 0 || $foodId <= 0) {
                http_response_code(400);
                echo json_encode(["success" => false, "message" => "UserId và FoodId không hợp lệ"]);
                break;
            }
            
            // Kiểm tra xem đã yêu thích chưa
            $stmt = $pdo->prepare("SELECT Id FROM Favorites WHERE UserId = ? AND FoodId = ?");
            $stmt->execute([$userId, $foodId]);
            if ($stmt->fetch()) {
                // Nếu đã yêu thích rồi thì trả về success: true để App cập nhật giao diện
                echo json_encode(["success" => true, "message" => "Đã yêu thích rồi"]);
                break;
            }
            
            // Thêm yêu thích mới
            $stmt = $pdo->prepare("INSERT INTO Favorites (UserId, FoodId, CreatedDate) VALUES (?, ?, NOW())");
            $stmt->execute([$userId, $foodId]);
            
            echo json_encode([
                "success" => true,
                "message" => "Thêm yêu thích thành công"
            ]);
        }
        break;

    case 'removeFavorite':
        // Xóa yêu thích
        if ($method === 'DELETE') {
            $userId = (int)($_GET['userId'] ?? 0);
            $foodId = (int)($_GET['foodId'] ?? 0);
            
            if ($userId <= 0 || $foodId <= 0) {
                http_response_code(400);
                echo json_encode(["success" => false, "message" => "UserId và FoodId không hợp lệ"]);
                break;
            }
            
            $stmt = $pdo->prepare("DELETE FROM Favorites WHERE UserId = ? AND FoodId = ?");
            $stmt->execute([$userId, $foodId]);
            
            echo json_encode([
                "success" => true,
                "message" => "Xóa yêu thích thành công"
            ]);
        }
        break;

    case 'isFavorite':
        // Kiểm tra có yêu thích không
        $userId = (int)($_GET['userId'] ?? 0);
        $foodId = (int)($_GET['foodId'] ?? 0);
        
        if ($userId <= 0 || $foodId <= 0) {
            echo json_encode(["success" => false]);
            break;
        }
        
        $stmt = $pdo->prepare("SELECT Id FROM Favorites WHERE UserId = ? AND FoodId = ?");
        $stmt->execute([$userId, $foodId]);
        $exists = $stmt->fetch();
        
        echo json_encode([
            "success" => $exists ? true : false
        ]);
        break;

    case 'getUserFavorites':
        // Lấy danh sách yêu thích của user
        $userId = (int)($_GET['userId'] ?? 0);
        
        if ($userId <= 0) {
            echo json_encode(["success" => false, "data" => []]);
            break;
        }
        
        $stmt = $pdo->prepare("
            SELECT f.* FROM Foods f
            INNER JOIN Favorites fav ON f.Id = fav.FoodId
            WHERE fav.UserId = ?
            ORDER BY fav.CreatedDate DESC
        ");
        $stmt->execute([$userId]);
        $foods = $stmt->fetchAll();
        
        echo json_encode([
            "success" => true,
            "data" => $foods
        ]);
        break;

    case 'deleteFavorite':
        // Xóa favorite theo ID
        if ($method === 'DELETE') {
            $id = (int)($_GET['id'] ?? 0);
            
            if ($id <= 0) {
                http_response_code(400);
                echo json_encode(["error" => "ID không hợp lệ"]);
                break;
            }
            
            $stmt = $pdo->prepare("DELETE FROM Favorites WHERE Id = ?");
            $stmt->execute([$id]);
            
            echo json_encode([
                "success" => true,
                "message" => "Đã xóa favorite"
            ]);
        }
        break;

    case 'setUserOffline':
        // Đặt user offline (IsActive = false)
        if ($method === 'POST') {
            $data = json_decode(file_get_contents('php://input'), true);
            $userId = (int)($data['userId'] ?? 0);
            
            if ($userId <= 0) {
                http_response_code(400);
                echo json_encode(["error" => "UserId không hợp lệ"]);
                break;
            }
            
            $stmt = $pdo->prepare("UPDATE UserTracking SET IsActive = 0, IsNavigating = 0, DestinationName = '', DestinationLat = 0, DestinationLng = 0, LastUpdate = NOW() WHERE UserId = ?");
            $stmt->execute([$userId]);
            
            echo json_encode([
                "success" => true,
                "message" => "User đã offline"
            ]);
        }
        break;

    // ============================================================
    // QR SCAN ENDPOINTS
    // ============================================================
    
    case 'saveQRScan':
        // Lưu thông tin quét QR
        if ($method === 'POST') {
            $data = json_decode(file_get_contents('php://input'), true);
            $deviceId = $data['deviceId'] ?? '';
            $qrCode = $data['qrCode'] ?? '';
            $deviceName = $data['deviceName'] ?? '';
            $osVersion = $data['osVersion'] ?? '';
            
            if (empty($deviceId) || empty($qrCode)) {
                http_response_code(400);
                echo json_encode(["success" => false, "message" => "DeviceId và QRCode không được để trống"]);
                break;
            }
            
            // Kiểm tra xem device đã quét chưa
            $stmt = $pdo->prepare("SELECT Id FROM qr_scans WHERE device_id = ?");
            $stmt->execute([$deviceId]);
            if ($stmt->fetch()) {
                echo json_encode([
                    "success" => true,
                    "message" => "Device đã quét QR trước đó",
                    "alreadyScanned" => true
                ]);
                break;
            }
            
            // Lưu thông tin quét QR
            $stmt = $pdo->prepare("
                INSERT INTO qr_scans (device_id, qr_code, device_name, os_version, scan_date)
                VALUES (?, ?, ?, ?, NOW())
            ");
            $stmt->execute([$deviceId, $qrCode, $deviceName, $osVersion]);
            
            echo json_encode([
                "success" => true,
                "message" => "Lưu QR scan thành công",
                "scanId" => $pdo->lastInsertId()
            ]);
        }
        break;

    case 'checkQRScan':
        // Kiểm tra xem device đã quét QR chưa
        $deviceId = $_GET['deviceId'] ?? '';
        
        if (empty($deviceId)) {
            echo json_encode(["hasScanned" => false]);
            break;
        }
        
        $stmt = $pdo->prepare("SELECT Id, scan_date FROM qr_scans WHERE device_id = ?");
        $stmt->execute([$deviceId]);
        $scan = $stmt->fetch();
        
        echo json_encode([
            "hasScanned" => $scan ? true : false,
            "scanDate" => $scan ? $scan['scan_date'] : null
        ]);
        break;

    case 'getQRScans':
        // Lấy danh sách tất cả QR scans (cho admin dashboard)
        $stmt = $pdo->query("SELECT * FROM qr_scans ORDER BY scan_date DESC");
        echo json_encode([
            "success" => true,
            "data" => $stmt->fetchAll()
        ]);
        break;

    case 'getQRStats':
        // Lấy thống kê QR scans
        $totalScans = $pdo->query("SELECT COUNT(*) FROM qr_scans")->fetchColumn();
        $todayScans = $pdo->query("SELECT COUNT(*) FROM qr_scans WHERE DATE(scan_date) = CURDATE()")->fetchColumn();
        $weekScans = $pdo->query("SELECT COUNT(*) FROM qr_scans WHERE scan_date >= DATE_SUB(NOW(), INTERVAL 7 DAY)")->fetchColumn();
        
        echo json_encode([
            "success" => true,
            "data" => [
                "totalScans" => (int)$totalScans,
                "todayScans" => (int)$todayScans,
                "weekScans" => (int)$weekScans
            ]
        ]);
        break;

    // ============================================================
    // SESSION MANAGEMENT ENDPOINTS
    // ============================================================
    
    case 'validateSession':
        // Kiểm tra session còn hạn không
        $token = $_GET['token'] ?? '';
        
        if (empty($token)) {
            http_response_code(400);
            echo json_encode(["error" => "Token không được để trống"]);
            break;
        }
        
        $stmt = $pdo->prepare("
            SELECT s.*, u.Username, u.Role 
            FROM Sessions s
            JOIN Users u ON s.UserId = u.Id
            WHERE s.Token = ? AND s.ExpiresAt > NOW()
        ");
        $stmt->execute([$token]);
        $session = $stmt->fetch();
        
        if ($session) {
            // Cập nhật LastActiveTime
            $stmt = $pdo->prepare("UPDATE Users SET LastActiveTime = NOW() WHERE Id = ?");
            $stmt->execute([$session['UserId']]);
            
            echo json_encode([
                "success" => true,
                "user" => [
                    "id" => $session['UserId'],
                    "username" => $session['Username'],
                    "role" => $session['Role']
                ]
            ]);
        } else {
            http_response_code(401);
            echo json_encode(["error" => "Session không hợp lệ hoặc đã hết hạn"]);
        }
        break;
    
    case 'updateActivity':
        // Cập nhật LastActiveTime (gọi mỗi 30 giây từ app)
        if ($method === 'POST') {
            $data = json_decode(file_get_contents('php://input'), true);
            $token = $data['token'] ?? '';
            
            if (empty($token)) {
                http_response_code(400);
                echo json_encode(["error" => "Token không được để trống"]);
                break;
            }
            
            // Kiểm tra session còn hạn
            $stmt = $pdo->prepare("SELECT UserId FROM Sessions WHERE Token = ? AND ExpiresAt > NOW()");
            $stmt->execute([$token]);
            $session = $stmt->fetch();
            
            if ($session) {
                // Cập nhật LastActiveTime
                $stmt = $pdo->prepare("UPDATE Users SET LastActiveTime = NOW() WHERE Id = ?");
                $stmt->execute([$session['UserId']]);
                
                echo json_encode([
                    "success" => true,
                    "message" => "Activity updated"
                ]);
            } else {
                http_response_code(401);
                echo json_encode(["error" => "Session không hợp lệ"]);
            }
        }
        break;
    
    case 'logout':
        // Xóa session khi logout
        if ($method === 'POST') {
            $data = json_decode(file_get_contents('php://input'), true);
            $token = $data['token'] ?? '';
            
            if (!empty($token)) {
                // Xóa session
                $stmt = $pdo->prepare("DELETE FROM Sessions WHERE Token = ?");
                $stmt->execute([$token]);
            }
            
            echo json_encode([
                "success" => true,
                "message" => "Đăng xuất thành công"
            ]);
        }
        break;
    
    case 'getActiveSessions':
        // Lấy danh sách sessions đang active (cho admin dashboard)
        $stmt = $pdo->query("
            SELECT 
                u.Id as user_id,
                u.Username,
                u.Role,
                u.LastActiveTime,
                COUNT(s.Id) as session_count,
                MAX(s.CreatedDate) as last_login
            FROM Users u
            LEFT JOIN Sessions s ON u.Id = s.UserId AND s.ExpiresAt > NOW()
            WHERE u.LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE)
            GROUP BY u.Id, u.Username, u.Role, u.LastActiveTime
            ORDER BY u.LastActiveTime DESC
        ");
        
        echo json_encode([
            "success" => true,
            "data" => $stmt->fetchAll()
        ]);
        break;
    
    case 'getOnlineCount':
        // Đếm số người online (active trong 5 phút)
        $count = $pdo->query("
            SELECT COUNT(DISTINCT u.Id) 
            FROM Users u
            WHERE u.LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE)
        ")->fetchColumn();
        
        echo json_encode([
            "success" => true,
            "count" => (int)$count
        ]);
        break;

    default:
        http_response_code(404);
        echo json_encode(["error" => "Action không hợp lệ"]);
}

