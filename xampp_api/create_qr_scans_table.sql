-- ============================================================
-- Tạo bảng qr_scans để lưu thông tin quét QR
-- ============================================================

CREATE TABLE IF NOT EXISTS `qr_scans` (
  `id` INT(11) NOT NULL AUTO_INCREMENT,
  `device_id` VARCHAR(255) NOT NULL,
  `qr_code` VARCHAR(500) NOT NULL,
  `scan_date` DATETIME NOT NULL,
  `device_name` VARCHAR(255) DEFAULT NULL,
  `os_version` VARCHAR(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `unique_device` (`device_id`),
  KEY `idx_scan_date` (`scan_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- Hướng dẫn sử dụng:
-- 1. Mở phpMyAdmin: http://localhost/phpmyadmin
-- 2. Chọn database "VietnamFoodGuide"
-- 3. Click tab "SQL"
-- 4. Copy và paste nội dung file này
-- 5. Click "Go" để thực thi
-- ============================================================
