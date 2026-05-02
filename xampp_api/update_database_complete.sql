-- ============================================================
-- FILE SQL TỔNG HỢP - CẬP NHẬT HOÀN CHỈNH DATABASE
-- Chạy file này để cập nhật database với tất cả tính năng mới
-- ============================================================

-- QUAN TRỌNG: Chọn database trước khi chạy
USE VietnamFoodGuide;

-- BƯỚC 1: Thêm các cột mới vào bảng Foods
-- ============================================================

ALTER TABLE Foods 
ADD COLUMN IF NOT EXISTS Radius DOUBLE DEFAULT 30.0 COMMENT 'Bán kính kích hoạt geofence (mét)';

ALTER TABLE Foods 
ADD COLUMN IF NOT EXISTS Priority INT DEFAULT 5 COMMENT 'Mức ưu tiên phát thuyết minh (1-10)';

ALTER TABLE Foods 
ADD COLUMN IF NOT EXISTS AudioUrl VARCHAR(500) DEFAULT NULL COMMENT 'URL hoặc đường dẫn file audio';

ALTER TABLE Foods 
ADD COLUMN IF NOT EXISTS NarrationScript TEXT DEFAULT NULL COMMENT 'Script thuyết minh riêng cho TTS';

ALTER TABLE Foods 
ADD COLUMN IF NOT EXISTS CooldownMinutes INT DEFAULT 5 COMMENT 'Thời gian chờ trước khi phát lại (phút)';

-- Thêm index cho Priority
ALTER TABLE Foods 
ADD INDEX IF NOT EXISTS idx_priority (Priority);

-- BƯỚC 2: Cập nhật dữ liệu cho các quán ăn hiện có
-- ============================================================

-- Cập nhật Priority dựa trên Rating
UPDATE Foods SET 
    Priority = CASE 
        WHEN Rating >= 4.8 THEN 9
        WHEN Rating >= 4.6 THEN 8
        WHEN Rating >= 4.5 THEN 7
        WHEN Rating >= 4.4 THEN 6
        ELSE 5
    END
WHERE Priority IS NULL OR Priority = 0 OR Priority = 5;

-- Cập nhật Radius dựa trên Rating và Category
UPDATE Foods SET 
    Radius = CASE 
        WHEN Rating >= 4.8 THEN 40.0
        WHEN Rating >= 4.6 THEN 35.0
        WHEN Category = 'Thức uống' THEN 25.0
        ELSE 30.0
    END
WHERE Radius IS NULL OR Radius = 0 OR Radius = 30.0;

-- Cập nhật NarrationScript từ Name và Description
UPDATE Foods SET 
    NarrationScript = CONCAT('Chào mừng bạn đến với ', Name, '. ', 
                             SUBSTRING(Description_VI, 1, 100), '...')
WHERE NarrationScript IS NULL OR NarrationScript = '';

-- Cập nhật CooldownMinutes mặc định
UPDATE Foods SET 
    CooldownMinutes = 5
WHERE CooldownMinutes IS NULL OR CooldownMinutes = 0;

-- BƯỚC 3: Cập nhật chi tiết cho từng quán ăn
-- ============================================================

-- Bánh Mì Trần Văn Hành
UPDATE Foods SET 
    Radius = 30.0,
    Priority = 7,
    NarrationScript = 'Chào mừng bạn đến với Bánh Mì Trần Văn Hành, nơi có bánh mì truyền thống với vỏ bánh giòn rụm và nhân đa dạng.',
    CooldownMinutes = 5
WHERE Name = 'Bánh Mì Trần Văn Hành';

-- Bún Chả Hàng Cót
UPDATE Foods SET 
    Radius = 35.0,
    Priority = 8,
    NarrationScript = 'Bún Chả Hàng Cót nổi tiếng với thịt nướng thơm lừng và nước mắm chua ngọt đậm đà.',
    CooldownMinutes = 5
WHERE Name = 'Bún Chả Hàng Cót';

-- Cơm Tấm Quân Đội
UPDATE Foods SET 
    Radius = 30.0,
    Priority = 6,
    NarrationScript = 'Cơm Tấm Quân Đội với cơm tấm chất lượng cao, thịt nướng và trứng ốp la thơm ngon.',
    CooldownMinutes = 5
WHERE Name = 'Cơm Tấm Quân Đội';

-- Bánh Canh Cua Nha Trang
UPDATE Foods SET 
    Radius = 30.0,
    Priority = 5,
    NarrationScript = 'Bánh Canh Cua Nha Trang với nước dùng cua thanh mát và bánh mềm dẻo thơm ngon.',
    CooldownMinutes = 5
WHERE Name = 'Bánh Canh Cua Nha Trang';

-- Phở Đặc Biệt Trần Hưng Đạo
UPDATE Foods SET 
    Radius = 40.0,
    Priority = 9,
    NarrationScript = 'Phở Đặc Biệt Trần Hưng Đạo với nước dùng được ninh từ xương bò suốt 12 giờ, thơm ngon đậm đà.',
    CooldownMinutes = 5
WHERE Name LIKE 'Phở Đ%c Biệt Trần Hưng Đạo' OR Name LIKE 'Phở Đắc Biệt%';

-- Cà Phê Truyền Thống Sài Gòn
UPDATE Foods SET 
    Radius = 25.0,
    Priority = 6,
    NarrationScript = 'Cà Phê Truyền Thống Sài Gòn với hương vị đậm đà từ cà phê Tây Nguyên rang chậm lửa.',
    CooldownMinutes = 5
WHERE Name = 'Cà Phê Truyền Thống Sài Gòn';

-- Bánh Mì Thục
UPDATE Foods SET 
    Radius = 30.0,
    Priority = 7,
    NarrationScript = 'Bánh Mì Thục với vỏ bánh giòn vàng và nhân đa dạng từ chả lụa, thịt xá xíu.',
    CooldownMinutes = 5
WHERE Name = 'Bánh Mì Thục';

-- Mì Quảng Minh Châu
UPDATE Foods SET 
    Radius = 30.0,
    Priority = 5,
    NarrationScript = 'Mì Quảng Minh Châu với nước xốt mực đậm đà và topping đa dạng.',
    CooldownMinutes = 5
WHERE Name = 'Mì Quảng Minh Châu';

-- Bún Bò Huế Xứ Quảng
UPDATE Foods SET 
    Radius = 35.0,
    Priority = 8,
    NarrationScript = 'Bún Bò Huế Xứ Quảng với nước dùng cay nồng và thịt bò mềm tan.',
    CooldownMinutes = 5
WHERE Name = 'Bún Bò Huế Xứ Quảng';

-- Bánh Chưng Mỳ Tươi Homeboy
UPDATE Foods SET 
    Radius = 30.0,
    Priority = 5,
    NarrationScript = 'Bánh Chưng Mỳ Tươi Homeboy với bánh siêu giòn từ bột mỳ tươi mỗi ngày.',
    CooldownMinutes = 5
WHERE Name = 'Bánh Chưng Mỳ Tươi Homeboy';

-- Tiramisu Café Vĩnh Khánh
UPDATE Foods SET 
    Radius = 25.0,
    Priority = 6,
    NarrationScript = 'Tiramisu Café Vĩnh Khánh với không gian sang trọng và cà phê chuyên nghiệp.',
    CooldownMinutes = 5
WHERE Name = 'Tiramisu Café Vĩnh Khánh';

-- BƯỚC 4: Kiểm tra kết quả
-- ============================================================

SELECT '✅ Cập nhật hoàn tất!' AS Status;

SELECT CONCAT('📊 Tổng số quán ăn: ', COUNT(*)) AS Info FROM Foods;

SELECT CONCAT('✅ Quán có Radius: ', COUNT(*)) AS Info 
FROM Foods WHERE Radius IS NOT NULL AND Radius > 0;

SELECT CONCAT('✅ Quán có Priority: ', COUNT(*)) AS Info 
FROM Foods WHERE Priority IS NOT NULL AND Priority > 0;

SELECT CONCAT('✅ Quán có NarrationScript: ', COUNT(*)) AS Info 
FROM Foods WHERE NarrationScript IS NOT NULL AND NarrationScript != '';

-- Hiển thị 5 quán đầu tiên để kiểm tra
SELECT 
    Name AS 'Tên quán',
    Radius AS 'Bán kính (m)',
    Priority AS 'Ưu tiên',
    CooldownMinutes AS 'Cooldown (phút)',
    LEFT(NarrationScript, 50) AS 'Script (50 ký tự đầu)'
FROM Foods 
ORDER BY Priority DESC, Rating DESC
LIMIT 5;

-- ============================================================
-- HOÀN TẤT!
-- Nếu thấy kết quả ở trên → Thành công!
-- Nếu có lỗi → Kiểm tra lại database name và table name
-- ============================================================
