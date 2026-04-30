<?php
// Script kiểm tra database và tables
header("Content-Type: application/json; charset=utf-8");

$host = 'localhost';
$db = 'VietnamFoodGuide';
$user = 'root';
$pass = '';

try {
    $pdo = new PDO("mysql:host=$host;charset=utf8mb4", $user, $pass);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    
    $result = [
        "status" => "checking",
        "checks" => []
    ];
    
    // Check 1: Database exists
    $stmt = $pdo->query("SHOW DATABASES LIKE 'VietnamFoodGuide'");
    $dbExists = $stmt->rowCount() > 0;
    $result["checks"][] = [
        "name" => "Database VietnamFoodGuide",
        "status" => $dbExists ? "OK" : "MISSING",
        "message" => $dbExists ? "Database exists" : "Database not found! Run setup_complete.sql"
    ];
    
    if (!$dbExists) {
        $result["status"] = "error";
        echo json_encode($result, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE);
        exit;
    }
    
    // Switch to database
    $pdo->exec("USE VietnamFoodGuide");
    
    // Check 2: Foods table
    $stmt = $pdo->query("SHOW TABLES LIKE 'Foods'");
    $foodsExists = $stmt->rowCount() > 0;
    $result["checks"][] = [
        "name" => "Table Foods",
        "status" => $foodsExists ? "OK" : "MISSING",
        "message" => $foodsExists ? "Foods table exists" : "Foods table not found!"
    ];
    
    // Check 3: Users table
    $stmt = $pdo->query("SHOW TABLES LIKE 'Users'");
    $usersExists = $stmt->rowCount() > 0;
    $result["checks"][] = [
        "name" => "Table Users",
        "status" => $usersExists ? "OK" : "MISSING",
        "message" => $usersExists ? "Users table exists" : "Users table not found!"
    ];
    
    // Check 4: Favorites table
    $stmt = $pdo->query("SHOW TABLES LIKE 'Favorites'");
    $favoritesExists = $stmt->rowCount() > 0;
    $result["checks"][] = [
        "name" => "Table Favorites",
        "status" => $favoritesExists ? "OK" : "MISSING",
        "message" => $favoritesExists ? "Favorites table exists" : "Favorites table not found!"
    ];
    
    // Check 5: Count records
    if ($foodsExists) {
        $count = $pdo->query("SELECT COUNT(*) FROM Foods")->fetchColumn();
        $result["checks"][] = [
            "name" => "Foods count",
            "status" => $count > 0 ? "OK" : "WARNING",
            "message" => "Found $count foods"
        ];
    }
    
    if ($usersExists) {
        $count = $pdo->query("SELECT COUNT(*) FROM Users")->fetchColumn();
        $result["checks"][] = [
            "name" => "Users count",
            "status" => "INFO",
            "message" => "Found $count users"
        ];
    }
    
    if ($favoritesExists) {
        $count = $pdo->query("SELECT COUNT(*) FROM Favorites")->fetchColumn();
        $result["checks"][] = [
            "name" => "Favorites count",
            "status" => "INFO",
            "message" => "Found $count favorites"
        ];
    }
    
    // Overall status
    $allOk = $dbExists && $foodsExists && $usersExists && $favoritesExists;
    $result["status"] = $allOk ? "success" : "error";
    $result["message"] = $allOk ? "All checks passed!" : "Some checks failed. Please run setup_complete.sql";
    
    echo json_encode($result, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE);
    
} catch (PDOException $e) {
    echo json_encode([
        "status" => "error",
        "message" => "Database connection failed: " . $e->getMessage(),
        "hint" => "Make sure XAMPP MySQL is running"
    ], JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE);
}
?>
