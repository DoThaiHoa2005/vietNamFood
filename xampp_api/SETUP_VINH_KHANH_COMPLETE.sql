-- ============================================================
-- SETUP HOÀN CHỈNH - 12 QUÁN ĂN PHỐ VĨNH KHÁNH
-- Copy toàn bộ → Paste vào phpMyAdmin → Click Go
-- ============================================================

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
    IsLocked BOOLEAN DEFAULT FALSE COMMENT 'TRUE = Tài khoản bị khóa',
    INDEX idx_username (Username),
    INDEX idx_qr_scanned (QRScanned),
    INDEX idx_app_installed (AppInstalled),
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
    Radius DOUBLE DEFAULT 30.0 COMMENT 'Bán kính kích hoạt geofence (mét)',
    Priority INT DEFAULT 5 COMMENT 'Mức ưu tiên phát thuyết minh (1-10)',
    AudioUrl VARCHAR(500) DEFAULT NULL COMMENT 'URL hoặc đường dẫn file audio',
    NarrationScript TEXT DEFAULT NULL COMMENT 'Script thuyết minh riêng cho TTS',
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

-- Tạo bảng Sessions
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

-- Tạo bảng UserTracking
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

-- Tạo bảng qr_scans
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

-- ============================================================
-- INSERT USERS (2 tài khoản: admin và user123)
-- Password: admin123 cho cả 2
-- ============================================================
INSERT IGNORE INTO Users (Username, PasswordHash, Role, QRScanned, AppInstalled, LastActiveTime, IsLocked, CreatedDate) VALUES
('admin', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', FALSE, FALSE, NULL, FALSE, NOW()),
('user123', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', FALSE, FALSE, NULL, FALSE, NOW());

-- ============================================================
-- INSERT 12 QUÁN ĂN PHỐ VĨNH KHÁNH
-- ============================================================
INSERT IGNORE INTO Foods (Name, City, Category, Description_VI, Description_EN, Description_CN, Latitude, Longitude, Rating, ImagePath, Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes) VALUES
-- 1. Alo Quán – Seafood & Beer
('Alo Quán – Seafood & Beer', 'TP.HCM', 'Hải sản', 
'Địa chỉ đầu tiên trên phố ẩm thực Vĩnh Khánh mà bạn không nên bỏ qua chính là Alo Quán – Seafood & Beer. Quán nổi tiếng với việc sử dụng các nguyên liệu tươi ngon, chế biến khéo léo để mang đến hương vị ngon nhất cho thực khách khi thưởng thức. Đặc biệt, nếu bạn đang tìm một địa chỉ nhậu ngon – bổ – rẻ thì Alo Quán – Seafood & Beer là lựa chọn lý tưởng.',
'The first address on Vinh Khanh food street that you should not miss is Alo Quan – Seafood & Beer. The restaurant is famous for using fresh ingredients and skillful preparation to bring the best flavor to diners. Especially, if you are looking for a delicious, nutritious and cheap drinking place, Alo Quan – Seafood & Beer is an ideal choice.',
'Vinh Khanh美食街上第一个不容错过的地址就是Alo Quán – Seafood & Beer。这家餐厅以使用新鲜食材和精湛的烹饪技术而闻名，为食客带来最佳风味。特别是，如果您正在寻找一个美味、营养又便宜的饮酒场所，Alo Quán – Seafood & Beer是理想的选择。',
10.759012, 106.702156, 4.5, NULL, 30.0, 8, NULL, 'Chào mừng bạn đến với Alo Quán – Seafood & Beer, nơi có hải sản tươi ngon và bia mát lạnh.', 5),

-- 2. Ốc Đào 2
('Ốc Đào 2', 'TP.HCM', 'Ốc', 
'Quán Ốc Đào 2 là địa điểm ẩm thực tuyệt vời với hơn 30 loài ốc khác nhau được quán chế biến thành những món ăn độc đáo và ngon miệng. Thực khách có thể thưởng thức các món ốc hấp, xào… được nêm nếm vừa miệng và hương vị thơm ngon nhất.',
'Oc Dao 2 is a great culinary destination with more than 30 different types of snails prepared into unique and delicious dishes. Diners can enjoy steamed and stir-fried snails... seasoned to taste and with the most delicious flavor.',
'Ốc Đào 2是一个很棒的美食目的地，拥有30多种不同类型的蜗牛，制作成独特美味的菜肴。食客可以享用蒸蜗牛、炒蜗牛等，调味适口，风味最佳。',
10.759234, 106.702389, 4.6, NULL, 30.0, 8, NULL, 'Ốc Đào 2 với hơn 30 loài ốc khác nhau, món nào cũng ngon.', 5),

-- 3. Bún cá Châu Đốc Dì Tư
('Bún cá Châu Đốc Dì Tư', 'TP.HCM', 'Bún', 
'Mỗi tô bún cá tại quán bún cá Châu Đốc Dì Tư là sự kết hợp hài hòa giữa thịt cá tươi ngon, bún mềm mịn và các loại rau sống, nước chấm đặc trưng. Ngoài ra nước dùng của tô bún nóng hổi, thơm ngon và rất chuẩn vị. Đây là một trong những địa chỉ nhất định bạn phải ghé khi đến phố ẩm thực Vĩnh Khánh.',
'Each bowl of fish noodle soup at Bun Ca Chau Doc Di Tu is a harmonious combination of fresh fish, soft noodles and fresh vegetables, with a distinctive dipping sauce. In addition, the broth of the hot bowl of noodles is fragrant and very authentic. This is one of the addresses you must visit when coming to Vinh Khanh food street.',
'在Bún cá Châu Đốc Dì Tư，每碗鱼粉都是新鲜鱼肉、柔软米粉和新鲜蔬菜的和谐组合，配有独特的蘸酱。此外，热腾腾的粉汤香气扑鼻，味道非常正宗。这是您来到Vinh Khanh美食街时必须光顾的地址之一。',
10.759456, 106.702612, 4.7, NULL, 30.0, 7, NULL, 'Bún cá Châu Đốc Dì Tư với nước dùng thơm ngon, chuẩn vị miền Tây.', 5),

-- 4. Bún thịt nướng Cô Nga
('Bún thịt nướng Cô Nga', 'TP.HCM', 'Bún', 
'Phố ẩm thực Vĩnh Khánh ngoài nổi tiếng với các món ốc còn gây ấn tượng với thực khách ở món bún thịt nướng Sài Gòn. Trong các quán bún thịt nướng ở đây thì quán Cô Nga thu hút rất đông thực khách ghé đến thưởng thức. Một tô bún đầy đủ gồm bún, thịt nướng, chả giò… ăn kèm cùng rau sống, đồ chua.',
'Vinh Khanh food street, in addition to being famous for snail dishes, also impresses diners with Saigon grilled pork vermicelli. Among the grilled pork vermicelli restaurants here, Co Nga restaurant attracts a large number of diners to enjoy. A full bowl of vermicelli includes vermicelli, grilled pork, spring rolls... served with fresh vegetables and pickles.',
'Vinh Khanh美食街除了以蜗牛菜闻名外，还以西贡烤猪肉米粉给食客留下深刻印象。在这里的烤猪肉米粉餐厅中，Cô Nga餐厅吸引了大量食客前来享用。一碗完整的米粉包括米粉、烤猪肉、春卷等，配以新鲜蔬菜和泡菜。',
10.759678, 106.702835, 4.5, NULL, 30.0, 7, NULL, 'Bún thịt nướng Cô Nga với thịt nướng thơm lừng, chả giò giòn rụm.', 5),

-- 5. Ốc Vũ
('Ốc Vũ', 'TP.HCM', 'Ốc', 
'Ốc Vũ là địa điểm ăn vặt nổi tiếng ở phố ẩm thực Vĩnh Khánh với đa dạng món ốc ngon, giá phải chăng. Đồ ăn tại đây được chế biến tinh tế và nước chấm chua cay độc đáo chính là một trong những điểm cộng khiến quán lúc nào cũng đông khách.',
'Oc Vu is a famous snack spot on Vinh Khanh food street with a variety of delicious snail dishes at affordable prices. The food here is delicately prepared and the unique sweet and sour dipping sauce is one of the plus points that makes the restaurant always crowded.',
'Ốc Vũ是Vinh Khanh美食街上著名的小吃店，提供各种美味的蜗牛菜肴，价格实惠。这里的食物制作精致，独特的酸辣蘸酱是使餐厅总是人满为患的加分点之一。',
10.759890, 106.703058, 4.6, NULL, 30.0, 8, NULL, 'Ốc Vũ với món ốc đa dạng và nước chấm chua cay độc đáo.', 5),

-- 6. Lãng Quán
('Lãng Quán', 'TP.HCM', 'Hải sản', 
'Lãng Quán là địa chỉ ăn khuya Sài Gòn quen thuộc của rất nhiều người dân quận 4 cũ. Menu của quán đa dạng các món ăn ngon được chế biến từ mực, tôm, bạch tuộc, ếch… như tôm nướng muối ớt, bạch tuộc chiên giòn, mực hấp hành gừng…',
'Lang Quan is a familiar late-night dining address in Saigon for many people in old District 4. The restaurant menu has a variety of delicious dishes made from squid, shrimp, octopus, frog... such as grilled shrimp with salt and chili, crispy fried octopus, steamed squid with scallions and ginger...',
'Lãng Quán是许多老第4区居民熟悉的西贡深夜用餐地址。餐厅菜单有各种美味菜肴，由鱿鱼、虾、章鱼、青蛙等制成，如盐椒烤虾、香脆炸章鱼、葱姜蒸鱿鱼等。',
10.760112, 106.703281, 4.7, NULL, 30.0, 8, NULL, 'Lãng Quán - địa chỉ ăn khuya quen thuộc với menu hải sản đa dạng.', 5),

-- 7. Ớt Xiêm Quán
('Ớt Xiêm Quán', 'TP.HCM', 'Hải sản', 
'Quán Ớt Xiêm Quán là một trong những quán ăn ngon tại phố ẩm thực Vĩnh Khánh được thực khách đánh giá cao. Đến quán bạn có thể lựa chọn các món ăn ngon như cá diêu hồng rang muối Hồng Kông, sò dương mỡ hành… Món nào cũng được quán lựa chọn nguyên liệu kỹ lưỡng và tẩm ướp đậm đà.',
'Ot Xiem Quan is one of the delicious restaurants on Vinh Khanh food street that is highly rated by diners. At the restaurant, you can choose delicious dishes such as Hong Kong salt-fried red snapper, scallops with scallion oil... Every dish is carefully selected and marinated with rich flavors.',
'Ớt Xiêm Quán是Vinh Khanh美食街上备受食客好评的美味餐厅之一。在餐厅，您可以选择美味的菜肴，如香港盐炒红鲷鱼、葱油扇贝等。每道菜都经过精心挑选，腌制味道浓郁。',
10.760334, 106.703504, 4.6, NULL, 30.0, 7, NULL, 'Ớt Xiêm Quán với cá diêu hồng rang muối Hồng Kông đặc sắc.', 5),

-- 8. Lẩu nướng HongKong A FAT
('Lẩu nướng HongKong A FAT', 'TP.HCM', 'Lẩu & Nướng', 
'Lần đầu đặt chân đến Sài Gòn và bạn chưa biết ăn gì ở Sài Gòn vừa ngon vừa rẻ thì hãy thử ghé ngay Lẩu nướng HongKong A FAT. Quán ăn này nằm trong phố ẩm thực Vĩnh Khánh với đa dạng các món lẩu nướng, miến xào, cơm chiên, Pad Thái… Đặc biệt, không gian quán rộng rãi, sang trọng phù hợp với những buổi gặp gỡ bạn bè, liên hoan.',
'If you are visiting Saigon for the first time and do not know what to eat in Saigon that is both delicious and cheap, try visiting HongKong A FAT hotpot and BBQ. This restaurant is located on Vinh Khanh food street with a variety of hotpot and BBQ dishes, stir-fried vermicelli, fried rice, Pad Thai... Especially, the spacious and luxurious restaurant space is suitable for meetings with friends and parties.',
'如果您第一次来西贡，不知道在西贡吃什么既美味又便宜，请尝试光顾HongKong A FAT火锅烧烤。这家餐厅位于Vinh Khanh美食街，提供各种火锅和烧烤菜肴、炒粉、炒饭、泰式炒河粉等。特别是，宽敞豪华的餐厅空间适合与朋友聚会和派对。',
10.760556, 106.703727, 4.8, NULL, 30.0, 9, NULL, 'Lẩu nướng HongKong A FAT với không gian rộng rãi, menu đa dạng.', 5),

-- 9. Sườn Muối Ớt
('Sườn Muối Ớt', 'TP.HCM', 'Nướng', 
'Không gian quán đẹp, đồ ăn ngon, nhân viên phục vụ nhanh nhẹn chính là những đánh giá của thực khách khi ghé quán Sườn Muối Ớt tại quận 4 cũ. Quán mở cửa từ 5h chiều đến 5h sáng hôm nay nên nếu bạn đang tìm địa chỉ ăn tối Sài Gòn đa dạng món, giá hợp lý thì nhất định đừng bỏ qua địa chỉ này.',
'Beautiful restaurant space, delicious food, and quick service are the reviews of diners when visiting Suon Muoi Ot restaurant in old District 4. The restaurant is open from 5pm to 5am today, so if you are looking for a Saigon dinner address with a variety of dishes at reasonable prices, do not miss this address.',
'美丽的餐厅空间、美味的食物和快速的服务是食客光顾老第4区Sườn Muối Ớt餐厅时的评价。餐厅从下午5点营业至次日凌晨5点，因此如果您正在寻找一个价格合理、菜品多样的西贡晚餐地址，请不要错过这个地址。',
10.760778, 106.703950, 4.7, NULL, 30.0, 8, NULL, 'Sườn Muối Ớt mở cửa từ 5h chiều đến 5h sáng, phục vụ tận tình.', 5),

-- 10. Chilli Quán
('Chilli Quán', 'TP.HCM', 'Nướng', 
'Chilli Quán là một trong những quán nướng ngon ở Sài Gòn nằm trong phố ẩm thực Vĩnh Khánh. Đến quán, bạn sẽ được thưởng thức đa dạng các món ăn được chế biến từ nguyên liệu tươi ngon, tẩm ướp với những loại sốt đậm đà mà giá cả lại vô cùng phải chăng.',
'Chilli Quan is one of the delicious BBQ restaurants in Saigon located on Vinh Khanh food street. At the restaurant, you will enjoy a variety of dishes made from fresh ingredients, marinated with rich sauces at very affordable prices.',
'Chilli Quán是西贡美味烧烤餐厅之一，位于Vinh Khanh美食街。在餐厅，您将享用各种由新鲜食材制成的菜肴，用浓郁的酱汁腌制，价格非常实惠。',
10.761000, 106.704173, 4.6, NULL, 30.0, 7, NULL, 'Chilli Quán với món nướng đa dạng, sốt đậm đà, giá phải chăng.', 5),

-- 11. Thảo ốc quận 4 cũ
('Thảo ốc quận 4 cũ', 'TP.HCM', 'Ốc', 
'Ốc tỏi nướng phô mai, bào ngư xào, ốc hương hoàng kim… đây là những món ăn best-seller tại quán Thảo ốc. Ngoài ra, quán còn rất nhiều món ngon khác có hương vị đậm đà, riêng biệt mà chắc chắn bạn thử 1 lần sẽ ghiền ngay.',
'Grilled snails with cheese, stir-fried abalone, golden snails... these are the best-selling dishes at Thao Oc restaurant. In addition, the restaurant has many other delicious dishes with rich and unique flavors that you will definitely be addicted to after trying once.',
'奶酪烤蜗牛、炒鲍鱼、金色蜗牛等是Thảo ốc餐厅的畅销菜肴。此外，餐厅还有许多其他美味菜肴，味道浓郁独特，您尝试一次后肯定会上瘾。',
10.761222, 106.704396, 4.7, NULL, 30.0, 8, NULL, 'Thảo ốc với ốc tỏi nướng phô mai và bào ngư xào đặc sắc.', 5),

-- 12. Ốc Oanh
('Ốc Oanh', 'TP.HCM', 'Ốc', 
'Quán Ốc Oanh trong phố ẩm thực Vĩnh Khánh là quán ốc ngon Sài Gòn nổi tiếng 20 năm. Ngoài các món được chế biến từ ốc thơm ngon, quán còn nổi tiếng với sò điệp nướng muối ớt, ghẹ rang muối ớt, sò dương nướng… Giá cả tại quán được đánh giá hợp lý, vừa túi tiền với người dân và thực khách bốn phương.',
'Oc Oanh restaurant on Vinh Khanh food street is a famous Saigon snail restaurant for 20 years. In addition to delicious snail dishes, the restaurant is also famous for grilled scallops with salt and chili, stir-fried crab with salt and chili, grilled razor clams... The prices at the restaurant are considered reasonable and affordable for locals and visitors from all over.',
'Vinh Khanh美食街上的Ốc Oanh餐厅是西贡著名的蜗牛餐厅，已有20年历史。除了美味的蜗牛菜肴外，餐厅还以盐椒烤扇贝、盐椒炒蟹、烤竹蛏等而闻名。餐厅的价格被认为是合理的，当地人和来自各地的游客都能负担得起。',
10.761444, 106.704619, 4.8, NULL, 30.0, 9, NULL, 'Ốc Oanh - quán ốc nổi tiếng 20 năm với sò điệp nướng muối ớt.', 5);

-- ============================================================
-- KIỂM TRA KẾT QUẢ
-- ============================================================
SELECT '✅ Setup hoàn tất - 12 quán ăn phố Vĩnh Khánh!' AS Status;
SELECT CONCAT('👥 Users: ', COUNT(*), ' tài khoản') AS Info FROM Users;
SELECT CONCAT('🍜 Foods: ', COUNT(*), ' quán ăn') AS Info FROM Foods;
SELECT CONCAT('❤️ Favorites: ', COUNT(*), ' yêu thích') AS Info FROM Favorites;
SELECT CONCAT('📍 Tracking: ', COUNT(*), ' phiên') AS Info FROM UserTracking;
