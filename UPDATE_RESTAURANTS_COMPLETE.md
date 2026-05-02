# ✅ CẬP NHẬT HOÀN CHỈNH 12 QUÁN ĂN VĨNH KHÁNH

## Tóm tắt
Đã cập nhật file `xampp_api/setup_complete.sql` với 12 quán ăn thực tế ở Vĩnh Khánh, mỗi quán có:
- ✅ Tên quán chính xác
- ✅ Mô tả đầy đủ bằng 3 ngôn ngữ (Tiếng Việt, English, 中文)
- ✅ Hình ảnh tương ứng (Q1.jpg đến Q12.jpg)
- ✅ Phân loại đúng (Hải sản, Ốc, Bún, Nướng, Lẩu & Nướng)

## Danh sách 12 quán ăn

| STT | Tên quán | Loại | Hình ảnh | Rating |
|-----|----------|------|----------|--------|
| 1 | Alo Quán – Seafood & Beer | Hải sản | Q1.jpg | 4.8 ⭐ |
| 2 | Ốc Đào 2 | Ốc | Q2.jpg | 4.7 ⭐ |
| 3 | Bún cá Châu Đốc Dì Tư | Bún | Q3.jpg | 4.6 ⭐ |
| 4 | Bún thịt nướng Cô Nga | Bún | Q4.jpg | 4.5 ⭐ |
| 5 | Ốc Vũ | Ốc | Q5.jpg | 4.6 ⭐ |
| 6 | Lãng Quán | Hải sản | Q6.jpg | 4.7 ⭐ |
| 7 | Ớt Xiêm Quán | Hải sản | Q7.jpg | 4.7 ⭐ |
| 8 | Lẩu nướng HongKong A FAT | Lẩu & Nướng | Q8.jpg | 4.8 ⭐ |
| 9 | Sườn Muối Ớt | Nướng | Q9.jpg | 4.6 ⭐ |
| 10 | Chilli Quán | Nướng | Q10.jpg | 4.7 ⭐ |
| 11 | Thảo ốc quận 4 cũ | Ốc | Q11.jpg | 4.8 ⭐ |
| 12 | Ốc Oanh | Ốc | Q12.jpg | 4.9 ⭐ |

## Phân loại theo thể loại

### 🦐 Hải sản (3 quán)
1. Alo Quán – Seafood & Beer
2. Lãng Quán
3. Ớt Xiêm Quán

### 🐚 Ốc (4 quán)
1. Ốc Đào 2
2. Ốc Vũ
3. Thảo ốc quận 4 cũ
4. Ốc Oanh

### 🍜 Bún (2 quán)
1. Bún cá Châu Đốc Dì Tư
2. Bún thịt nướng Cô Nga

### 🍢 Nướng (2 quán)
1. Sườn Muối Ớt
2. Chilli Quán

### 🍲 Lẩu & Nướng (1 quán)
1. Lẩu nướng HongKong A FAT

## Đường dẫn hình ảnh
Tất cả hình ảnh được lưu tại: `/Assets/Images/Q#.jpg`
- Q1.jpg → Alo Quán
- Q2.jpg → Ốc Đào 2
- Q3.jpg → Bún cá Châu Đốc Dì Tư
- Q4.jpg → Bún thịt nướng Cô Nga
- Q5.jpg → Ốc Vũ
- Q6.jpg → Lãng Quán
- Q7.jpg → Ớt Xiêm Quán
- Q8.jpg → Lẩu nướng HongKong A FAT
- Q9.jpg → Sườn Muối Ớt
- Q10.jpg → Chilli Quán
- Q11.jpg → Thảo ốc quận 4 cũ
- Q12.jpg → Ốc Oanh

## Cách deploy
1. Mở phpMyAdmin trên hosting
2. Chọn database của bạn
3. Vào tab SQL
4. Copy toàn bộ nội dung file `xampp_api/setup_complete.sql`
5. Paste vào và click "Go"
6. Database sẽ được tạo với đầy đủ 12 quán ăn thực tế

## Lưu ý
- File SQL này tạo TẤT CẢ các bảng (Users, Foods, Favorites, Sessions, UserTracking, qr_scans)
- Chỉ cần chạy 1 lần duy nhất
- Nếu bảng đã tồn tại, nó sẽ không bị xóa (dùng CREATE TABLE IF NOT EXISTS)
- Dữ liệu được thêm với INSERT IGNORE (không trùng lặp)
- Bắt đầu với 2 users: admin/admin123 và user123/user123
- Chưa có tracking data và favorites (sẽ được thêm tự động khi user sử dụng app)

## Hoàn thành ✅
- [x] Thay thế 11 quán cũ bằng 12 quán thực tế
- [x] Thêm mô tả đầy đủ 3 ngôn ngữ cho mỗi quán
- [x] Gán hình ảnh Q1.jpg đến Q12.jpg theo đúng thứ tự
- [x] Phân loại đúng: Hải sản, Ốc, Bún, Nướng, Lẩu & Nướng
- [x] Giữ nguyên cấu trúc bảng và các trường (Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes)
