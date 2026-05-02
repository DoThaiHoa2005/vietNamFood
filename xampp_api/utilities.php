<?php
/**
 * ============================================================
 * FILE PHP TỔNG HỢP - TIỆN ÍCH CHO VIETNAM FOOD GUIDE
 * ============================================================
 * 
 * Chứa tất cả các utility functions:
 * 1. Cập nhật foods.json với các trường mới
 * 2. Kiểm tra database connection
 * 3. Backup và restore database
 * 4. Test API endpoints
 * 
 * Cách chạy:
 * php utilities.php [command]
 * 
 * Commands:
 * - update-json    : Cập nhật foods.json
 * - check-db       : Kiểm tra database
 * - backup-db      : Backup database
 * - test-api       : Test API endpoints
 * ============================================================
 */

// ============================================================
// CẤU HÌNH
// ============================================================

define('DB_HOST', 'localhost');
define('DB_NAME', 'VietnamFoodGuide');
define('DB_USER', 'root');
define('DB_PASS', '');

define('FOODS_JSON_PATH', '../VietnamFoodGuide/Data/foods.json');
define('API_BASE_URL', 'http://localhost/vfg-api/api.php');

// ============================================================
// FUNCTION 1: CẬP NHẬT FOODS.JSON
// ============================================================

function updateFoodsJson() {
    echo "📝 Đang cập nhật foods.json...\n\n";
    
    if (!file_exists(FOODS_JSON_PATH)) {
        echo "❌ Không tìm thấy file: " . FOODS_JSON_PATH . "\n";
        return false;
    }
    
    $foods = json_decode(file_get_contents(FOODS_JSON_PATH), true);
    
    if (!$foods) {
        echo "❌ Không thể đọc file JSON\n";
        return false;
    }
    
    echo "📊 Tìm thấy " . count($foods) . " quán ăn\n\n";
    
    foreach ($foods as &$food) {
        // Thêm Radius
        if (!isset($food['Radius'])) {
            if ($food['Rating'] >= 4.8) {
                $food['Radius'] = 40.0;
            } elseif ($food['Rating'] >= 4.6) {
                $food['Radius'] = 35.0;
            } elseif ($food['Category'] === 'Thức uống') {
                $food['Radius'] = 25.0;
            } else {
                $food['Radius'] = 30.0;
            }
        }
        
        // Thêm Priority
        if (!isset($food['Priority'])) {
            if ($food['Rating'] >= 4.8) {
                $food['Priority'] = 9;
            } elseif ($food['Rating'] >= 4.6) {
                $food['Priority'] = 8;
            } elseif ($food['Rating'] >= 4.5) {
                $food['Priority'] = 7;
            } elseif ($food['Rating'] >= 4.4) {
                $food['Priority'] = 6;
            } else {
                $food['Priority'] = 5;
            }
        }
        
        // Thêm AudioUrl
        if (!isset($food['AudioUrl'])) {
            $food['AudioUrl'] = null;
        }
        
        // Thêm NarrationScript
        if (!isset($food['NarrationScript'])) {
            $descVI = $food['DescriptionVI'] ?? '';
            $food['NarrationScript'] = "Chào mừng bạn đến với " . $food['Name'] . ". " . 
                                        substr($descVI, 0, 150) . "...";
        }
        
        // Thêm CooldownMinutes
        if (!isset($food['CooldownMinutes'])) {
            $food['CooldownMinutes'] = 5;
        }
        
        echo "✅ " . $food['Name'] . " - Radius: " . $food['Radius'] . "m, Priority: " . $food['Priority'] . "\n";
    }
    
    // Lưu lại file
    $json = json_encode($foods, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE);
    file_put_contents(FOODS_JSON_PATH, $json);
    
    echo "\n✅ Hoàn tất! Đã cập nhật file foods.json\n";
    echo "📁 File: " . FOODS_JSON_PATH . "\n";
    
    return true;
}

// ============================================================
// FUNCTION 2: KIỂM TRA DATABASE
// ============================================================

function checkDatabase() {
    echo "🔍 Đang kiểm tra database...\n\n";
    
    try {
        $pdo = new PDO(
            "mysql:host=" . DB_HOST . ";dbname=" . DB_NAME . ";charset=utf8mb4",
            DB_USER,
            DB_PASS,
            [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
        );
        
        echo "✅ Kết nối database thành công!\n\n";
        
        // Kiểm tra bảng Foods
        echo "📊 KIỂM TRA BẢNG FOODS:\n";
        echo str_repeat("-", 50) . "\n";
        
        $stmt = $pdo->query("DESCRIBE Foods");
        $columns = $stmt->fetchAll(PDO::FETCH_COLUMN);
        
        $requiredColumns = ['Radius', 'Priority', 'AudioUrl', 'NarrationScript', 'CooldownMinutes'];
        
        foreach ($requiredColumns as $col) {
            if (in_array($col, $columns)) {
                echo "✅ Cột '$col' tồn tại\n";
            } else {
                echo "❌ Cột '$col' KHÔNG tồn tại\n";
            }
        }
        
        echo "\n📊 THỐNG KÊ DỮ LIỆU:\n";
        echo str_repeat("-", 50) . "\n";
        
        $stats = [
            'Tổng quán ăn' => "SELECT COUNT(*) FROM Foods",
            'Quán có Radius' => "SELECT COUNT(*) FROM Foods WHERE Radius IS NOT NULL AND Radius > 0",
            'Quán có Priority' => "SELECT COUNT(*) FROM Foods WHERE Priority IS NOT NULL AND Priority > 0",
            'Quán có NarrationScript' => "SELECT COUNT(*) FROM Foods WHERE NarrationScript IS NOT NULL AND NarrationScript != ''",
        ];
        
        foreach ($stats as $label => $query) {
            $count = $pdo->query($query)->fetchColumn();
            echo "$label: $count\n";
        }
        
        echo "\n📋 5 QUÁN ĐẦU TIÊN:\n";
        echo str_repeat("-", 50) . "\n";
        
        $stmt = $pdo->query("
            SELECT Name, Radius, Priority, CooldownMinutes 
            FROM Foods 
            ORDER BY Priority DESC, Rating DESC 
            LIMIT 5
        ");
        
        while ($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
            echo sprintf(
                "%-30s | R: %4.1fm | P: %d | C: %dm\n",
                substr($row['Name'], 0, 30),
                $row['Radius'],
                $row['Priority'],
                $row['CooldownMinutes']
            );
        }
        
        echo "\n✅ Kiểm tra hoàn tất!\n";
        
        return true;
        
    } catch (PDOException $e) {
        echo "❌ Lỗi kết nối database: " . $e->getMessage() . "\n";
        return false;
    }
}

// ============================================================
// FUNCTION 3: BACKUP DATABASE
// ============================================================

function backupDatabase() {
    echo "💾 Đang backup database...\n\n";
    
    try {
        $pdo = new PDO(
            "mysql:host=" . DB_HOST . ";dbname=" . DB_NAME . ";charset=utf8mb4",
            DB_USER,
            DB_PASS,
            [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
        );
        
        $backupFile = 'backup_' . date('Y-m-d_H-i-s') . '.sql';
        $backupPath = __DIR__ . '/' . $backupFile;
        
        // Backup bảng Foods
        $stmt = $pdo->query("SELECT * FROM Foods");
        $foods = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        $sql = "-- Backup Foods table - " . date('Y-m-d H:i:s') . "\n\n";
        $sql .= "CREATE TABLE IF NOT EXISTS Foods_Backup AS SELECT * FROM Foods;\n\n";
        
        foreach ($foods as $food) {
            $values = array_map(function($v) use ($pdo) {
                return $v === null ? 'NULL' : $pdo->quote($v);
            }, $food);
            
            $sql .= "INSERT INTO Foods VALUES (" . implode(', ', $values) . ");\n";
        }
        
        file_put_contents($backupPath, $sql);
        
        echo "✅ Backup thành công!\n";
        echo "📁 File: $backupPath\n";
        echo "📊 Số quán ăn: " . count($foods) . "\n";
        
        return true;
        
    } catch (PDOException $e) {
        echo "❌ Lỗi backup: " . $e->getMessage() . "\n";
        return false;
    }
}

// ============================================================
// FUNCTION 4: TEST API
// ============================================================

function testAPI() {
    echo "🧪 Đang test API...\n\n";
    
    // Test GET foods
    echo "📡 Test GET /api.php?action=foods\n";
    echo str_repeat("-", 50) . "\n";
    
    $url = API_BASE_URL . '?action=foods';
    $response = @file_get_contents($url);
    
    if ($response === false) {
        echo "❌ Không thể kết nối API\n";
        echo "   URL: $url\n";
        echo "   Kiểm tra:\n";
        echo "   1. XAMPP Apache đang chạy?\n";
        echo "   2. File api.php tồn tại?\n";
        echo "   3. URL đúng?\n";
        return false;
    }
    
    $data = json_decode($response, true);
    
    if (!$data) {
        echo "❌ Response không phải JSON\n";
        echo "   Response: " . substr($response, 0, 200) . "\n";
        return false;
    }
    
    echo "✅ API hoạt động!\n";
    echo "📊 Số quán ăn: " . count($data) . "\n\n";
    
    // Kiểm tra trường mới
    if (count($data) > 0) {
        $firstFood = $data[0];
        
        echo "📋 Kiểm tra trường mới trong response:\n";
        echo str_repeat("-", 50) . "\n";
        
        $requiredFields = ['Radius', 'Priority', 'AudioUrl', 'NarrationScript', 'CooldownMinutes'];
        
        foreach ($requiredFields as $field) {
            if (isset($firstFood[$field])) {
                $value = $firstFood[$field] ?? 'NULL';
                echo "✅ $field: $value\n";
            } else {
                echo "❌ $field: KHÔNG TỒN TẠI\n";
            }
        }
        
        echo "\n📋 Quán đầu tiên:\n";
        echo str_repeat("-", 50) . "\n";
        echo "Tên: " . ($firstFood['Name'] ?? 'N/A') . "\n";
        echo "Radius: " . ($firstFood['Radius'] ?? 'N/A') . "m\n";
        echo "Priority: " . ($firstFood['Priority'] ?? 'N/A') . "\n";
        echo "Cooldown: " . ($firstFood['CooldownMinutes'] ?? 'N/A') . " phút\n";
    }
    
    echo "\n✅ Test API hoàn tất!\n";
    
    return true;
}

// ============================================================
// MAIN - XỬ LÝ COMMAND LINE
// ============================================================

function showHelp() {
    echo "\n";
    echo "╔════════════════════════════════════════════════════════╗\n";
    echo "║   VIETNAM FOOD GUIDE - UTILITIES                       ║\n";
    echo "╚════════════════════════════════════════════════════════╝\n";
    echo "\n";
    echo "Cách sử dụng:\n";
    echo "  php utilities.php [command]\n";
    echo "\n";
    echo "Commands:\n";
    echo "  update-json    Cập nhật foods.json với các trường mới\n";
    echo "  check-db       Kiểm tra database và dữ liệu\n";
    echo "  backup-db      Backup database Foods table\n";
    echo "  test-api       Test API endpoints\n";
    echo "  all            Chạy tất cả (check-db + test-api)\n";
    echo "\n";
    echo "Ví dụ:\n";
    echo "  php utilities.php update-json\n";
    echo "  php utilities.php check-db\n";
    echo "  php utilities.php all\n";
    echo "\n";
}

// Chạy command
if (php_sapi_name() === 'cli') {
    $command = $argv[1] ?? 'help';
    
    switch ($command) {
        case 'update-json':
            updateFoodsJson();
            break;
            
        case 'check-db':
            checkDatabase();
            break;
            
        case 'backup-db':
            backupDatabase();
            break;
            
        case 'test-api':
            testAPI();
            break;
            
        case 'all':
            echo "🚀 Chạy tất cả kiểm tra...\n\n";
            checkDatabase();
            echo "\n" . str_repeat("=", 60) . "\n\n";
            testAPI();
            break;
            
        case 'help':
        default:
            showHelp();
            break;
    }
} else {
    // Nếu chạy từ browser
    header('Content-Type: text/html; charset=utf-8');
    echo "<pre>";
    echo "⚠️ File này nên chạy từ command line\n\n";
    showHelp();
    echo "</pre>";
}
