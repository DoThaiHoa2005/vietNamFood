-- ============================================================
-- SETUP HOÀN CHỈNH VỚI USER TRACKING
-- Copy toàn bộ → Paste vào phpMyAdmin → Click Go
-- CHỈ CẦN CHẠY 1 LẦN - Tự động xử lý cả database mới và cũ
-- ============================================================

-- Lưu ý: Trên Hosting, bạn không được dùng lệnh DROP/CREATE DATABASE. 
-- Hãy chọn database của bạn trước khi chạy script này.

-- ============================================================
-- BƯỚC 1: UPDATE SCHEMA (Nếu database đã tồn tại)
-- Bỏ các cột không cần thiết nếu có
-- ============================================================

-- Bỏ cột QRScanned và index (nếu có)
SET @exist := (SELECT COUNT(*) FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'QRScanned');
SET @sqlstmt := IF(@exist > 0, 'ALTER TABLE Users DROP COLUMN QRScanned', 'SELECT "Column QRScanned does not exist" AS Info');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @exist := (SELECT COUNT(*) FROM information_schema.STATISTICS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND INDEX_NAME = 'idx_qr_scanned');
SET @sqlstmt := IF(@exist > 0, 'ALTER TABLE Users DROP INDEX idx_qr_scanned', 'SELECT "Index idx_qr_scanned does not exist" AS Info');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Bỏ cột AppInstalled và index (nếu có)
SET @exist := (SELECT COUNT(*) FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'AppInstalled');
SET @sqlstmt := IF(@exist > 0, 'ALTER TABLE Users DROP COLUMN AppInstalled', 'SELECT "Column AppInstalled does not exist" AS Info');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @exist := (SELECT COUNT(*) FROM information_schema.STATISTICS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Users' AND INDEX_NAME = 'idx_app_installed');
SET @sqlstmt := IF(@exist > 0, 'ALTER TABLE Users DROP INDEX idx_app_installed', 'SELECT "Index idx_app_installed does not exist" AS Info');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Bỏ cột NarrationScript (nếu có)
SET @exist := (SELECT COUNT(*) FROM information_schema.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Foods' AND COLUMN_NAME = 'NarrationScript');
SET @sqlstmt := IF(@exist > 0, 'ALTER TABLE Foods DROP COLUMN NarrationScript', 'SELECT "Column NarrationScript does not exist" AS Info');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT '✅ Schema cleanup completed!' AS Status;

-- ============================================================
-- BƯỚC 2: TẠO BẢNG (Nếu chưa tồn tại)
-- ============================================================

-- Tạo bảng Users
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'User',
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastActiveTime DATETIME NULL,
    IsLocked BOOLEAN DEFAULT FALSE COMMENT 'TRUE = Tài khoản bị khóa, không đăng nhập được',
    INDEX idx_username (Username),
    INDEX idx_last_active (LastActiveTime),
    INDEX idx_locked (IsLocked)
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
    -- ===== GIAI ĐOẠN 1: Thêm các trường mới cho Geofence & Narration =====
    Radius DOUBLE DEFAULT 30.0 COMMENT 'Bán kính kích hoạt geofence (mét)',
    Priority INT DEFAULT 5 COMMENT 'Mức ưu tiên phát thuyết minh (1-10)',
    AudioUrl VARCHAR(500) DEFAULT NULL COMMENT 'URL hoặc đường dẫn file audio',
    CooldownMinutes INT DEFAULT 5 COMMENT 'Thời gian chờ trước khi phát lại (phút)',
    INDEX idx_category (Category),
    INDEX idx_rating (Rating),
    INDEX idx_priority (Priority)
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
-- CHỈ 2 USERS: admin (Admin) và user123 (User)
-- BẮT ĐẦU TỪ 0: Chưa đăng nhập
-- IsLocked = FALSE: Tài khoản chưa bị khóa
INSERT IGNORE INTO Users (Username, PasswordHash, Role, LastActiveTime, IsLocked, CreatedDate) VALUES
('admin', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', NULL, FALSE, NOW()),
('user123', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', NULL, FALSE, NOW());

-- Insert Foods (12 quán ăn thực tế ở Vĩnh Khánh) với các trường mới
INSERT IGNORE INTO Foods (Name, City, Category, Description_VI, Description_EN, Description_CN, Latitude, Longitude, Rating, ImagePath, Radius, Priority, AudioUrl, CooldownMinutes) VALUES
('Alo Quán – Seafood & Beer', 'TP.HCM - Vĩnh Khánh', 'Hải sản', 
'Địa chỉ đầu tiên trên phố ẩm thực Vĩnh Khánh mà bạn không nên bỏ qua chính là Alo Quán – Seafood & Beer. Quán nổi tiếng với việc sử dụng các nguyên liệu tươi ngon, chế biến khéo léo để mang đến hương vị ngon nhất cho thực khách khi thưởng thức. Đặc biệt, nếu bạn đang tìm một địa chỉ nhậu ngon – bổ – rẻ thì Alo Quán – Seafood & Beer là lựa chọn lý tưởng.',
'The first address on Vinh Khanh food street that you should not miss is Alo Quan – Seafood & Beer. The restaurant is famous for using fresh ingredients, skillfully prepared to bring the best flavor to diners. Especially, if you are looking for a delicious, nutritious and cheap drinking place, Alo Quan – Seafood & Beer is the ideal choice.',
'永庆美食街上第一个不容错过的地址是Alo Quán – Seafood & Beer。餐厅以使用新鲜食材、精心烹制而闻名，为食客带来最佳风味。特别是，如果您正在寻找美味、营养且便宜的饮酒场所，Alo Quán – Seafood & Beer是理想的选择。',
10.78024, 106.70532, 4.8, '/Assets/Images/Q1.jpg', 30.0, 9, NULL, 5),

('Ốc Đào 2', 'TP.HCM - Vĩnh Khánh', 'Ốc',
'Quán Ốc Đào 2 là địa điểm ẩm thực tuyệt vời với hơn 30 loài ốc khác nhau được quán chế biến thành những món ăn độc đáo và ngon miệng. Thực khách có thể thưởng thức các món ốc hấp, xào… được nêm nếm vừa miệng và hương vị thơm ngon nhất.',
'Oc Dao 2 restaurant is a great culinary destination with more than 30 different types of snails that the restaurant prepares into unique and delicious dishes. Diners can enjoy steamed and stir-fried snails... seasoned to taste and with the most delicious flavor.',
'Ốc Đào 2餐厅是一个很棒的美食目的地，拥有30多种不同类型的蜗牛，餐厅将其制作成独特而美味的菜肴。食客可以享受蒸和炒蜗牛...调味适口，味道最美味。',
10.78156, 106.70614, 4.7, '/Assets/Images/Q2.jpg', 35.0, 8, NULL, 5),

('Bún cá Châu Đốc Dì Tư', 'TP.HCM - Vĩnh Khánh', 'Bún',
'Mỗi tô bún cá tại quán bún cá Châu Đốc Dì Tư là sự kết hợp hài hòa giữa thịt cá tươi ngon, bún mềm mịn và các loại rau sống, nước chấm đặc trưng. Ngoài ra nước dùng của tô bún nóng hổi, thơm ngon và rất chuẩn vị. Đây là một trong những địa chỉ nhất định bạn phải ghé khi đến phố ẩm thực Vĩnh Khánh.',
'Each bowl of fish noodle soup at Di Tu Chau Doc fish noodle restaurant is a harmonious combination of fresh fish, soft and smooth noodles and fresh vegetables, characteristic dipping sauce. In addition, the broth of the hot bowl of noodles is delicious and very authentic. This is one of the addresses you must visit when coming to Vinh Khanh food street.',
'在Dì Tư Châu Đốc鱼粉餐厅，每碗鱼粉都是新鲜鱼肉、柔软光滑的面条和新鲜蔬菜、特色蘸酱的和谐组合。此外，热面碗的汤汁美味且非常正宗。这是您来到永庆美食街时必须参观的地址之一。',
10.78289, 106.70478, 4.6, '/Assets/Images/Q3.jpg', 30.0, 7, NULL, 5),

('Bún thịt nướng Cô Nga', 'TP.HCM - Vĩnh Khánh', 'Bún',
'Phố ẩm thực Vĩnh Khánh ngoài nổi tiếng với các món ốc còn gây ấn tượng với thực khách ở món bún thịt nướng Sài Gòn. Trong các quán bún thịt nướng ở đây thì quán Cô Nga thu hút rất đông thực khách ghé đến thưởng thức. Một tô bún đầy đủ gồm bún, thịt nướng, chả giò… ăn kèm cùng rau sống, đồ chua.',
'Vinh Khanh food street, in addition to being famous for snail dishes, also impresses diners with Saigon grilled pork vermicelli. Among the grilled pork vermicelli restaurants here, Co Nga restaurant attracts a large number of diners to enjoy. A full bowl of vermicelli includes vermicelli, grilled pork, spring rolls... served with fresh vegetables and pickles.',
'永庆美食街除了以蜗牛菜闻名外，还以西贡烤猪肉米粉给食客留下深刻印象。在这里的烤猪肉米粉餐厅中，Cô Nga餐厅吸引了大量食客前来享用。一碗完整的米粉包括米粉、烤猪肉、春卷...配以新鲜蔬菜和泡菜。',
10.78412, 106.70321, 4.5, '/Assets/Images/Q4.jpg', 30.0, 7, NULL, 5),

('Ốc Vũ', 'TP.HCM - Vĩnh Khánh', 'Ốc',
'Ốc Vũ là địa điểm ăn vặt nổi tiếng ở phố ẩm thực Vĩnh Khánh với đa dạng món ốc ngon, giá phải chăng. Đồ ăn tại đây được chế biến tinh tế và nước chấm chua cay độc đáo chính là một trong những điểm cộng khiến quán lúc nào cũng đông khách.',
'Oc Vu is a famous snack spot on Vinh Khanh food street with a variety of delicious snail dishes at affordable prices. The food here is delicately prepared and the unique sweet and sour dipping sauce is one of the plus points that makes the restaurant always crowded.',
'Ốc Vũ是永庆美食街上著名的小吃店，拥有各种美味的蜗牛菜肴，价格实惠。这里的食物精心准备，独特的酸甜蘸酱是使餐厅总是人满为患的加分点之一。',
10.78567, 106.70189, 4.6, '/Assets/Images/Q5.jpg', 35.0, 8, NULL, 5),

('Lãng Quán', 'TP.HCM - Vĩnh Khánh', 'Hải sản',
'Lãng Quán là địa chỉ ăn khuya Sài Gòn quen thuộc của rất nhiều người dân quận 4 cũ. Menu của quán đa dạng các món ăn ngon được chế biến từ mực, tôm, bạch tuộc, ếch… như tôm nướng muối ớt, bạch tuộc chiên giòn, mực hấp hành gừng…',
'Lang Quan is a familiar late-night eating address in Saigon for many people in old District 4. The restaurant menu has a variety of delicious dishes made from squid, shrimp, octopus, frog... such as grilled shrimp with salt and chili, crispy fried octopus, steamed squid with scallions and ginger...',
'Lãng Quán是许多老第4区人熟悉的西贡深夜用餐地址。餐厅菜单有各种美味的菜肴，由鱿鱼、虾、章鱼、青蛙制成...如盐和辣椒烤虾、脆炸章鱼、葱姜蒸鱿鱼...',
10.78741, 106.70067, 4.7, '/Assets/Images/Q6.jpg', 30.0, 8, NULL, 5),

('Ớt Xiêm Quán', 'TP.HCM - Vĩnh Khánh', 'Hải sản',
'Ớt Xiêm Quán là một trong những quán ăn ngon tại phố ẩm thực Vĩnh Khánh được thực khách đánh giá cao. Đến quán bạn có thể lựa chọn các món ăn ngon như cá diêu hồng rang muối Hồng Kông, sò dương mỡ hành… Món nào cũng được quán lựa chọn nguyên liệu kỹ lưỡng và tẩm ướp đậm đà.',
'Ot Xiem Quan is one of the delicious restaurants on Vinh Khanh food street that is highly rated by diners. When you come to the restaurant, you can choose delicious dishes such as Hong Kong salt-fried red snapper, scallops with scallion oil... Every dish is carefully selected and marinated with rich flavors.',
'Ớt Xiêm Quán是永庆美食街上美味的餐厅之一，深受食客好评。当您来到餐厅时，您可以选择美味的菜肴，如香港盐炒红鲷鱼、葱油扇贝...每道菜都经过精心挑选和腌制，味道浓郁。',
10.78895, 106.69945, 4.7, '/Assets/Images/Q7.jpg', 30.0, 8, NULL, 5),

('Lẩu nướng HongKong A FAT', 'TP.HCM - Vĩnh Khánh', 'Lẩu & Nướng',
'Lần đầu đặt chân đến Sài Gòn và bạn chưa biết ăn gì ở Sài Gòn vừa ngon vừa rẻ thì hãy thử ghé ngay Lẩu nướng HongKong A FAT. Quán ăn này nằm trong phố ẩm thực Vĩnh Khánh với đa dạng các món lẩu nướng, miến xào, cơm chiên, Pad Thái… Đặc biệt, không gian quán rộng rãi, sang trọng phù hợp với những buổi gặp gỡ bạn bè, liên hoan.',
'If you are visiting Saigon for the first time and do not know what to eat in Saigon that is both delicious and cheap, try visiting HongKong A FAT hot pot and grill. This restaurant is located on Vinh Khanh food street with a variety of hot pot and grilled dishes, stir-fried vermicelli, fried rice, Pad Thai... Especially, the spacious and luxurious restaurant space is suitable for meetings with friends and parties.',
'如果您第一次访问西贡，不知道在西贡吃什么既美味又便宜，请尝试访问HongKong A FAT火锅和烧烤。这家餐厅位于永庆美食街，拥有各种火锅和烧烤菜肴、炒粉丝、炒饭、泰式炒河粉...特别是，宽敞豪华的餐厅空间适合与朋友聚会和派对。',
10.79024, 106.69823, 4.8, '/Assets/Images/Q8.jpg', 35.0, 9, NULL, 5),

('Sườn Muối Ớt', 'TP.HCM - Vĩnh Khánh', 'Nướng',
'Không gian quán đẹp, đồ ăn ngon, nhân viên phục vụ nhanh nhẹn chính là những đánh giá của thực khách khi ghé quán Sườn Muối Ớt tại quận 4 cũ. Quán mở cửa từ 5h chiều đến 5h sáng hôm nay nên nếu bạn đang tìm địa chỉ ăn tối Sài Gòn đa dạng món, giá hợp lý thì nhất định đừng bỏ qua địa chỉ này.',
'Beautiful restaurant space, delicious food, and quick service are the reviews of diners when visiting Suon Muoi Ot restaurant in old District 4. The restaurant is open from 5pm to 5am today, so if you are looking for a dinner address in Saigon with a variety of dishes at reasonable prices, do not miss this address.',
'美丽的餐厅空间、美味的食物和快速的服务是食客访问老第4区Sườn Muối Ớt餐厅时的评价。餐厅今天从下午5点开放到凌晨5点，所以如果您正在寻找西贡的晚餐地址，拥有各种价格合理的菜肴，请不要错过这个地址。',
10.79156, 106.69701, 4.6, '/Assets/Images/Q9.jpg', 30.0, 7, NULL, 5),

('Chilli Quán', 'TP.HCM - Vĩnh Khánh', 'Nướng',
'Chilli Quán là một trong những quán nướng ngon ở Sài Gòn nằm trong phố ẩm thực Vĩnh Khánh. Đến quán, bạn sẽ được thưởng thức đa dạng các món ăn được chế biến từ nguyên liệu tươi ngon, tẩm ướp với những loại sốt đậm đà mà giá cả lại vô cùng phải chăng.',
'Chilli Quan is one of the delicious grilled restaurants in Saigon located on Vinh Khanh food street. When you come to the restaurant, you will enjoy a variety of dishes made from fresh ingredients, marinated with rich sauces at very affordable prices.',
'Chilli Quán是西贡美味的烧烤餐厅之一，位于永庆美食街。当您来到餐厅时，您将享受各种由新鲜食材制成的菜肴，用浓郁的酱汁腌制，价格非常实惠。',
10.79289, 106.69579, 4.7, '/Assets/Images/Q10.jpg', 30.0, 8, NULL, 5),

('Thảo ốc quận 4 cũ', 'TP.HCM - Vĩnh Khánh', 'Ốc',
'Ốc tỏi nướng phô mai, bào ngư xào, ốc hương hoàng kim… đây là những món ăn best – seller tại quán Thảo ốc. Ngoài ra, quán còn rất nhiều món ngon khác có hương vị đậm đà, riêng biệt mà chắc chắn bạn thử 1 lần sẽ ghiền ngay.',
'Grilled garlic snails with cheese, stir-fried abalone, golden scented snails... these are the best-selling dishes at Thao Oc restaurant. In addition, the restaurant has many other delicious dishes with rich and unique flavors that you will definitely be addicted to after trying once.',
'烤大蒜蜗牛配奶酪、炒鲍鱼、金色香螺...这些是Thảo ốc餐厅的畅销菜肴。此外，餐厅还有许多其他美味的菜肴，味道浓郁独特，您尝试一次后肯定会上瘾。',
10.79412, 106.69457, 4.8, '/Assets/Images/Q11.jpg', 35.0, 9, NULL, 5),

('Ốc Oanh', 'TP.HCM - Vĩnh Khánh', 'Ốc',
'Quán Ốc Oanh trong phố ẩm thực Vĩnh Khánh là quán ốc ngon Sài Gòn nổi tiếng 20 năm. Ngoài các món được chế biến từ ốc thơm ngon, quán còn nổi tiếng với sò điệp nướng muối ớt, ghẹ rang muối ớt, sò dương nướng… Giá cả tại quán được đánh giá hợp lý, vừa túi tiền với người dân và thực khách bốn phương.',
'Oc Oanh restaurant on Vinh Khanh food street is a famous delicious snail restaurant in Saigon for 20 years. In addition to delicious dishes made from snails, the restaurant is also famous for grilled scallops with salt and chili, stir-fried crab with salt and chili, grilled scallops... The prices at the restaurant are considered reasonable and affordable for locals and visitors from all over.',
'永庆美食街上的Ốc Oanh餐厅是西贡著名的美味蜗牛餐厅，已有20年历史。除了由蜗牛制成的美味菜肴外，餐厅还以盐和辣椒烤扇贝、盐和辣椒炒蟹、烤扇贝而闻名...餐厅的价格被认为是合理的，对当地人和来自各地的游客来说都是负担得起的。',
10.79567, 106.69335, 4.9, '/Assets/Images/Q12.jpg', 35.0, 9, NULL, 5);

-- Insert mock tracking data (để test)
-- BẮT ĐẦU TỪ 0: Chưa có ai tracking
-- Khi user đăng nhập và bắt đầu navigate, data sẽ được thêm tự động từ app
-- INSERT IGNORE INTO UserTracking (UserId, CurrentLat, CurrentLng, DestinationLat, DestinationLng, DestinationName, IsNavigating, IsActive) VALUES
-- (2, 10.7769, 106.7009, 10.78567, 106.70189, 'Phở Đặc Biệt Trần Hưng Đạo', TRUE, TRUE);

-- Insert mock favorites data (để test)
-- BẮT ĐẦU TỪ 0: Chưa có ai yêu thích quán nào
-- Khi user click nút yêu thích trong app, data sẽ được thêm tự động
-- INSERT IGNORE INTO Favorites (UserId, FoodId, CreatedDate) VALUES
-- (1, 1, NOW());

-- Kiểm tra kết quả
SELECT 'Setup hoàn tất - Bắt đầu từ 0!' AS Status;
SELECT CONCAT('👥 Users: ', COUNT(*), ' tài khoản (admin + user123)') AS Info FROM Users;
SELECT CONCAT('🟢 Currently Online: ', COUNT(*), ' người dùng (bắt đầu từ 0)') AS Info FROM Users WHERE LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE);
SELECT CONCAT('🧭 Currently Navigating: ', COUNT(*), ' người dùng (bắt đầu từ 0)') AS Info FROM UserTracking WHERE IsNavigating = TRUE AND IsActive = TRUE;
SELECT CONCAT('🍜 Foods: ', COUNT(*), ' quán ăn') AS Info FROM Foods;
SELECT CONCAT('❤️ Favorites: ', COUNT(*), ' yêu thích (bắt đầu từ 0)') AS Info FROM Favorites;
SELECT CONCAT('📍 Tracking Active: ', COUNT(*), ' phiên (bắt đầu từ 0)') AS Info FROM UserTracking WHERE IsActive = TRUE;
SELECT CONCAT('📱 QR Scans: ', COUNT(*), ' lượt quét (bắt đầu từ 0)') AS Info FROM qr_scans;
