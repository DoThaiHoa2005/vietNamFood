-- Xóa database cũ
DROP DATABASE IF EXISTS VietnamFoodGuide;

-- Tạo database mới
CREATE DATABASE VietnamFoodGuide CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Chọn database
USE VietnamFoodGuide;

-- Tạo bảng Users
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'User',
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_username (Username)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tạo bảng Foods
CREATE TABLE Foods (
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
CREATE TABLE Favorites (
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
CREATE TABLE Sessions (
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
CREATE TABLE UserTracking (
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

-- Insert Users
INSERT INTO Users (Username, PasswordHash, Role, CreatedDate) VALUES
('admin', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', NOW()),
('user123', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', NOW()),
('testuser', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'User', NOW());

-- Insert Foods
INSERT INTO Foods (Name, City, Category, Description_VI, Description_EN, Description_CN, Latitude, Longitude, Rating, ImagePath) VALUES
('Bánh Mì Trần Văn Hành', 'TP.HCM - Vĩnh Khánh', 'Bánh Mì', 'Bánh mì truyền thống Việt Nam với vỏ bánh giòn rụm.', 'Traditional Vietnamese sandwich with crispy baguette.', '传统越南三明治，脆皮法棍。', 10.78024, 106.70532, 4.7, '/Assets/Images/vn_banh_mi.png'),
('Bún Chả Hàng Cót', 'TP.HCM - Vĩnh Khánh', 'Bún', 'Bún chả nổi tiếng với thịt nướng thơm lừng.', 'Famous grilled pork over noodles.', '以香味四溢的烤肉闻名。', 10.78156, 106.70614, 4.6, '/Assets/Images/vn_bun_cha.png'),
('Cơm Tấm Quân Đội', 'TP.HCM - Vĩnh Khánh', 'Cơm', 'Cơm tấm được nấu từ những hạt gạo tấm chất lượng cao.', 'Broken rice cooked from premium quality grains.', '用优质碎米烹制的破碎米饭。', 10.78289, 106.70478, 4.5, '/Assets/Images/vn_com_tam.png'),
('Phở Đắc Biệt Trần Hưng Đạo', 'TP.HCM - Vĩnh Khánh', 'Phở', 'Phở với nước dùng vàng chanh.', 'Pho with aromatic golden broth.', '香喷喷的金色高汤河粉。', 10.78567, 106.70189, 4.8, '/Assets/Images/vn_pho.png'),
('Cà Phê Truyền Thống Sài Gòn', 'TP.HCM - Vĩnh Khánh', 'Thức uống', 'Cà phê pha theo kiểu truyền thống Sài Gòn.', 'Coffee brewed in traditional Saigon style.', '按照西贡传统方式冲泡的咖啡。', 10.78741, 106.70067, 4.9, '/Assets/Images/vn_coffee.png');

-- Insert tracking data
INSERT INTO UserTracking (UserId, CurrentLat, CurrentLng, DestinationLat, DestinationLng, DestinationName, IsNavigating, IsActive) VALUES
(2, 10.7769, 106.7009, 10.78567, 106.70189, 'Phở Đắc Biệt Trần Hưng Đạo', TRUE, TRUE),
(3, 10.7800, 106.7050, 10.78741, 106.70067, 'Cà Phê Truyền Thống Sài Gòn', TRUE, TRUE);
