-- ============================================================
-- SETUP HOÀN CHỈNH VỚI USER TRACKING
-- Copy toàn bộ → Paste vào phpMyAdmin → Click Go
-- ============================================================

-- Lưu ý: Trên Hosting, bạn không được dùng lệnh DROP/CREATE DATABASE. 
-- Hãy chọn database của bạn trước khi chạy script này.

-- Tạo bảng Users
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'User',
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    QRScanned BOOLEAN DEFAULT FALSE,
    AppInstalled BOOLEAN DEFAULT FALSE,
    LastActiveTime DATETIME NULL,
    INDEX idx_username (Username),
    INDEX idx_qr_scanned (QRScanned),
    INDEX idx_app_installed (AppInstalled),
    INDEX idx_last_active (LastActiveTime)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tạo bảng Foods
CREATE TABLE IF NOT EXISTS Foods (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    City VARCHAR(255),
    Category VARCHAR(100),
    Description_VI TEXT,
    Description_EN TEXT,
    Description_CN TEXT,
    Latitude DOUBLE DEFAULT 0,
    Longitude DOUBLE DEFAULT 0,
    Rating DOUBLE DEFAULT 0,
    ImagePath VARCHAR(500),
    INDEX idx_category (Category),
    INDEX idx_rating (Rating)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tạo bảng Favorites
CREATE TABLE IF NOT EXISTS Favorites (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    FoodId INT NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (FoodId) REFERENCES Foods(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_favorite (UserId, FoodId),
    INDEX idx_user (UserId),
    INDEX idx_food (FoodId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tạo bảng Sessions (cho authentication)
CREATE TABLE IF NOT EXISTS Sessions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Token VARCHAR(255) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_token (Token),
    INDEX idx_user (UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- BẢNG MỚI: USER TRACKING
-- ============================================================
CREATE TABLE IF NOT EXISTS UserTracking (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    CurrentLat DOUBLE NULL,
    CurrentLng DOUBLE NULL,
    DestinationLat DOUBLE NULL,
    DestinationLng DOUBLE NULL,
    DestinationName VARCHAR(255) NULL,
    IsNavigating BOOLEAN DEFAULT FALSE,
    IsActive BOOLEAN DEFAULT TRUE,
    LastUpdate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user (UserId),
    INDEX idx_active (IsActive),
    INDEX idx_last_update (LastUpdate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- BẢNG MỚI: QR SCANS (Tracking người quét QR)
-- ============================================================
CREATE TABLE IF NOT EXISTS qr_scans (
    id INT AUTO_INCREMENT PRIMARY KEY,
    device_id VARCHAR(255) NOT NULL,
    qr_code VARCHAR(500) NOT NULL,
    scan_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    device_name VARCHAR(255) NULL,
    os_version VARCHAR(100) NULL,
    UNIQUE KEY unique_device (device_id),
    INDEX idx_scan_date (scan_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert Users với password hash BCrypt
-- Password: admin123 cho admin, user123 cho user
INSERT IGNORE INTO Users (Username, PasswordHash, Role, QRScanned, AppInstalled, LastActiveTime, CreatedDate) VALUES
('admin', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', TRUE, TRUE, NOW(), NOW()),
('user123', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', TRUE, TRUE, NOW(), NOW()),
('testuser', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', TRUE, TRUE, DATE_SUB(NOW(), INTERVAL 2 HOUR), NOW()),
('khach001', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', TRUE, FALSE, NULL, NOW()),
('khach002', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', TRUE, TRUE, DATE_SUB(NOW(), INTERVAL 30 MINUTE), NOW()),
('khach003', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', FALSE, FALSE, NULL, NOW()),
('nguoiyeuthich', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', TRUE, TRUE, DATE_SUB(NOW(), INTERVAL 1 HOUR), NOW()),
('dukhach01', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', TRUE, TRUE, DATE_SUB(NOW(), INTERVAL 3 MINUTE), NOW());

-- Insert Foods (11 quán ăn ở Vĩnh Khánh)
INSERT IGNORE INTO Foods (Name, City, Category, Description_VI, Description_EN, Description_CN, Latitude, Longitude, Rating, ImagePath) VALUES
('Bánh Mì Trần Văn Hành', 'TP.HCM - Vĩnh Khánh', 'Bánh Mì', 
'Bánh mì truyền thống Việt Nam với vỏ bánh giòn rụm, nhân đa dạng, chả cua, pâté, dưa chuối chua, ớt tươi và rau thơm.',
'Traditional Vietnamese sandwich with crispy baguette, Vietnamese ham, crab paste, pate, pickled radish, fresh chili and aromatic herbs.',
'传统越南三明治，脆皮法棍、越南火腿、蟹酱、肝酱、腌胡萝卜、新鲜辣椒和香草。',
10.78024, 106.70532, 4.7, '/Assets/Images/vn_banh_mi.png'),

('Bún Chả Hàng Cót', 'TP.HCM - Vĩnh Khánh', 'Bún',
'Bún chả nổi tiếng với thịt nướng thơm lừng, nước mắm chua ngọt đậm đà, kèm theo bún tươi mát, rau sống dồi dào.',
'Famous grilled pork over noodles with aromatic charred meat, tangy-sweet fish sauce, fresh rice noodles, abundant fresh herbs.',
'以香味四溢的烤肉闻名，搭配浓郁的鱼露酸甜汤、清爽的米粉、丰富的新鲜草本植物。',
10.78156, 106.70614, 4.6, '/Assets/Images/vn_bun_cha.png'),

('Cơm Tấm Quân Đội', 'TP.HCM - Vĩnh Khánh', 'Cơm',
'Cơm tấm được nấu từ những hạt gạo tấm chất lượng cao, phục vụ kèm thịt nướng, trứng ốp la, dưa leo ăn kèm.',
'Broken rice cooked from premium quality grains, served with grilled pork, fried egg, pickled cucumber.',
'用优质碎米烹制的破碎米饭，配以烤猪肉、炒蛋、腌黄瓜。',
10.78289, 106.70478, 4.5, '/Assets/Images/vn_com_tam.png'),

('Bánh Canh Cua Nha Trang', 'TP.HCM - Vĩnh Khánh', 'Bánh Khác',
'Bánh canh với nước dùng cua thanh mát, bánh mềm dẻo được tỏa ra hương thơm tự nhiên.',
'Tapioca cake soup with fresh crab broth, soft and chewy cake that releases natural aroma.',
'蟹汤木薯蛋糕，清爽的螃蟹高汤、软糯的蛋糕散发天然香气。',
10.78412, 106.70321, 4.4, '/Assets/Images/vn_banh_canh.png'),

('Phở Đặc Biệt Trần Hưng Đạo', 'TP.HCM - Vĩnh Khánh', 'Phở',
'Phở với nước dùng vàng chanh, được ninh từ xương bò suốt 12 giờ, kết hợp với những sợi phở mỏng, thịt bò tái lăn.',
'Pho with aromatic golden broth simmered from beef bones for 12 hours, combined with thin noodles, rare beef.',
'香喷喷的金色高汤河粉，由骨头炖制12小时，搭配细面条、半熟牛肉。',
10.78567, 106.70189, 4.8, '/Assets/Images/vn_pho.png'),

('Cà Phê Truyền Thống Sài Gòn', 'TP.HCM - Vĩnh Khánh', 'Thức uống',
'Cà phê pha theo kiểu truyền thống Sài Gòn, sử dụng cà phê Tây Nguyên rang chậm lửa.',
'Coffee brewed in traditional Saigon style, using slow-roasted Central Highlands coffee.',
'按照西贡传统方式冲泡的咖啡，采用西原地区慢烤咖啡豆。',
10.78741, 106.70067, 4.9, '/Assets/Images/vn_coffee.png'),

('Bánh Mì Thục', 'TP.HCM - Vĩnh Khánh', 'Bánh Mì',
'Bánh mì với vỏ bánh giòn vàng, nhân đa dạng từ chả lụa, thịt xá xíu, pâté, dưa chuối chua.',
'Sandwich with crispy golden crust and diverse fillings of pork roll, char siu, pate, pickled radish.',
'外脆内馅丰富的三明治，包括越南火腿肠、叉烧、肝酱、腌胡萝卜。',
10.78895, 106.69945, 4.6, '/Assets/Images/vn_banh_mi2.png'),

('Mì Quảng Minh Châu', 'TP.HCM - Vĩnh Khánh', 'Bánh Khác',
'Mì Quảng với nước xốt mực đậm đà, mì béo mềm, topping đa dạng gồm tôm sú to, thịt heo nướng.',
'Quang noodles with rich squid sauce, soft and tender noodles, diverse toppings including large river shrimp, grilled pork.',
'带有浓郁鱿鱼酱的广宁面，软糯的面条配以各种丰富配料。',
10.79024, 106.69823, 4.5, '/Assets/Images/vn_mi_quang.png'),

('Bún Bò Huế Xứ Quảng', 'TP.HCM - Vĩnh Khánh', 'Bún',
'Bún bò Huế với thịt bò kho tàu mềm tan, giò heo, bò ăn béo, chả cua, ớt cay nóng.',
'Hue beef noodles with spicy broth, tender stewed beef, pork knuckle, fatty beef, crab paste.',
'带有海鲜和辣味高汤的顺化牛肉面，配以软嫩的炖牛肉、猪蹄、肥牛肉。',
10.79156, 106.69701, 4.7, '/Assets/Images/vn_bun_bo_hue.png'),

('Bánh Chưng Mỳ Tươi Homeboy', 'TP.HCM - Vĩnh Khánh', 'Bánh Khác',
'Bánh chưng và bánh siêu giòn từ bột mỳ tươi mỗi ngày, nhân pâté, thịt nướng, dưa chuối.',
'Square cake and super crunchy pastry made from fresh flour daily, filled with pate, grilled pork, pickled radish.',
'每天用新鲜面粉制作的四方蛋糕和超脆油炸糕，馅料包括肝酱、烤肉、腌萝卜。',
10.79289, 106.69579, 4.4, '/Assets/Images/vn_banh_chung.png'),

('Tiramisu Café Vĩnh Khánh', 'TP.HCM - Vĩnh Khánh', 'Thức uống',
'Một quán cà phê hiện đại với không gian sang trọng, phục vụ cà phê chuyên nghiệp cùng các loại tiramisu tươi ngon.',
'A modern cafe with elegant ambiance, serving professional coffee and fresh tiramisu daily.',
'一间装饰优雅的现代咖啡馆，提供专业咖啡和每日新鲜提拉米苏。',
10.79412, 106.69457, 4.8, '/Assets/Images/vn_tiramisu_cafe.png');

-- Insert mock tracking data (để test)
INSERT IGNORE INTO UserTracking (UserId, CurrentLat, CurrentLng, DestinationLat, DestinationLng, DestinationName, IsNavigating, IsActive) VALUES
(2, 10.7769, 106.7009, 10.78567, 106.70189, 'Phở Đặc Biệt Trần Hưng Đạo', TRUE, TRUE),
(3, 10.7800, 106.7050, 10.78741, 106.70067, 'Cà Phê Truyền Thống Sài Gòn', TRUE, TRUE);

-- Insert mock favorites data (để test)
INSERT IGNORE INTO Favorites (UserId, FoodId, CreatedDate) VALUES
(1, 1, NOW()),
(1, 5, NOW()),
(2, 1, NOW()),
(2, 6, NOW()),
(2, 2, NOW()),
(3, 1, NOW()),
(3, 3, NOW()),
(4, 4, NOW()),
(5, 1, NOW()),
(5, 7, NOW()),
(7, 1, NOW()),
(7, 5, NOW()),
(7, 6, NOW()),
(8, 2, NOW()),
(8, 8, NOW());

-- Kiểm tra kết quả
SELECT 'Setup hoàn tất với User Tracking & QR Scanner!' AS Status;
SELECT CONCAT('👥 Users: ', COUNT(*), ' tài khoản') AS Info FROM Users;
SELECT CONCAT('📱 QR Scanned: ', COUNT(*), ' người dùng') AS Info FROM Users WHERE QRScanned = TRUE;
SELECT CONCAT('📲 App Installed: ', COUNT(*), ' người dùng') AS Info FROM Users WHERE AppInstalled = TRUE;
SELECT CONCAT('🟢 Currently Active: ', COUNT(*), ' người dùng') AS Info FROM Users WHERE LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE);
SELECT CONCAT('🍜 Foods: ', COUNT(*), ' quán ăn') AS Info FROM Foods;
SELECT CONCAT('❤️ Favorites: ', COUNT(*), ' yêu thích') AS Info FROM Favorites;
SELECT CONCAT('📍 Tracking: ', COUNT(*), ' phiên đang hoạt động') AS Info FROM UserTracking WHERE IsActive = TRUE;
SELECT CONCAT('📱 QR Scans: ', COUNT(*), ' lượt quét') AS Info FROM qr_scans;
