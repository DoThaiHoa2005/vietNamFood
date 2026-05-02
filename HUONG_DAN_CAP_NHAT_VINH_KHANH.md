# 📍 HƯỚNG DẪN CẬP NHẬT DỮ LIỆU PHỐ VĨNH KHÁNH

## 🎯 MỤC ĐÍCH

Thay thế dữ liệu quán ăn cũ bằng **12 quán ăn thực tế** trên phố ẩm thực Vĩnh Khánh, Quận 4 (cũ), TP.HCM.

---

## 📋 DANH SÁCH 12 QUÁN ĂN

### Hải sản (3 quán):
1. **Alo Quán – Seafood & Beer** ⭐ 4.5
2. **Lãng Quán** ⭐ 4.7
3. **Ớt Xiêm Quán** ⭐ 4.6

### Ốc (4 quán):
4. **Ốc Đào 2** ⭐ 4.6
5. **Ốc Vũ** ⭐ 4.6
6. **Thảo ốc quận 4 cũ** ⭐ 4.7
7. **Ốc Oanh** ⭐ 4.8

### Bún (2 quán):
8. **Bún cá Châu Đốc Dì Tư** ⭐ 4.7
9. **Bún thịt nướng Cô Nga** ⭐ 4.5

### Nướng (2 quán):
10. **Sườn Muối Ớt** ⭐ 4.7
11. **Chilli Quán** ⭐ 4.6

### Lẩu & Nướng (1 quán):
12. **Lẩu nướng HongKong A FAT** ⭐ 4.8

---

## 🔧 THAY ĐỔI

### 1. Dữ liệu quán ăn

**Trước:**
- Dữ liệu giả lập
- Tọa độ random
- Mô tả ngắn gọn

**Sau:**
- 12 quán ăn thực tế trên phố Vĩnh Khánh
- Tọa độ chính xác (khu vực Vĩnh Khánh, Quận 4)
- Mô tả chi tiết bằng 3 ngôn ngữ (Việt, Anh, Trung)

### 2. Categories mới

**Trước:**
- Bánh Mì 🥖
- Cơm 🍚
- Bún 🍜
- Phở 🍲
- Bánh Khác 🧁

**Sau (ưu tiên):**
- **Hải sản 🦐** (mới)
- **Ốc 🐚** (mới)
- **Bún 🍜**
- **Nướng 🍢** (mới)
- **Lẩu & Nướng 🍲** (mới)
- Bánh Mì 🥖
- Cơm 🍚
- Phở 🍲
- Bánh Khác 🧁

---

## 📍 TỌA ĐỘ

Tất cả 12 quán đều nằm trong khu vực:
- **Latitude:** 10.759012 - 10.761444
- **Longitude:** 106.702156 - 106.704619
- **Khu vực:** Phố Vĩnh Khánh, Quận 4 (cũ), TP.HCM

---

## 🌍 DỊCH THUẬT

Mỗi quán có mô tả bằng 3 ngôn ngữ:

### Tiếng Việt (Description_VI):
```
Địa chỉ đầu tiên trên phố ẩm thực Vĩnh Khánh mà bạn không nên bỏ qua 
chính là Alo Quán – Seafood & Beer. Quán nổi tiếng với việc sử dụng 
các nguyên liệu tươi ngon, chế biến khéo léo để mang đến hương vị 
ngon nhất cho thực khách khi thưởng thức...
```

### Tiếng Anh (Description_EN):
```
The first address on Vinh Khanh food street that you should not miss 
is Alo Quan – Seafood & Beer. The restaurant is famous for using 
fresh ingredients and skillful preparation to bring the best flavor 
to diners...
```

### Tiếng Trung (Description_CN):
```
Vinh Khanh美食街上第一个不容错过的地址就是Alo Quán – Seafood & Beer。
这家餐厅以使用新鲜食材和精湛的烹饪技术而闻名，为食客带来最佳风味...
```

---

## 🚀 CÁCH CẬP NHẬT

### Bước 1: Backup database cũ (tùy chọn)

```bash
# Vào phpMyAdmin
# Export database VietnamFoodGuide
# Lưu file backup
```

### Bước 2: Chạy file SQL cập nhật

```bash
# Mở phpMyAdmin
# Chọn database VietnamFoodGuide
# Vào tab SQL
# Copy nội dung file UPDATE_VINH_KHANH_FOODS.sql
# Paste vào và click "Go"
```

**Hoặc dùng command line:**

```bash
mysql -u root -p VietnamFoodGuide < UPDATE_VINH_KHANH_FOODS.sql
```

### Bước 3: Build lại app

```bash
cd VietnamFoodGuide
dotnet clean
dotnet build
```

### Bước 4: Chạy app và kiểm tra

```bash
# Run app (F5)
# Kiểm tra bản đồ
# Xem 12 quán mới
# Click vào từng quán để xem mô tả
```

---

## ✅ KIỂM TRA

### 1. Kiểm tra database

```sql
-- Xem tổng số quán
SELECT COUNT(*) FROM Foods;
-- Kết quả: 12

-- Xem danh sách quán
SELECT Id, Name, Category, Rating FROM Foods ORDER BY Id;

-- Xem categories
SELECT DISTINCT Category FROM Foods;
-- Kết quả: Hải sản, Ốc, Bún, Nướng, Lẩu & Nướng
```

### 2. Kiểm tra app

**Bản đồ:**
- ✅ Hiển thị 12 markers trong khu vực Vĩnh Khánh
- ✅ Mỗi marker có icon phù hợp với category
- ✅ Click vào marker → Hiển thị popup với tên quán

**Chi tiết quán:**
- ✅ Tên quán đầy đủ
- ✅ Category đúng
- ✅ Rating hiển thị
- ✅ Mô tả bằng ngôn ngữ hiện tại (VI/EN/CN)

**Chuyển ngôn ngữ:**
- ✅ Tiếng Việt → Mô tả tiếng Việt
- ✅ English → English description
- ✅ 中文 → 中文描述

---

## 📊 SO SÁNH

### Trước khi cập nhật:

| Đặc điểm | Giá trị |
|----------|---------|
| Số quán | ~20 quán |
| Dữ liệu | Giả lập |
| Tọa độ | Random |
| Mô tả | Ngắn gọn |
| Ngôn ngữ | 1-2 ngôn ngữ |
| Categories | Bánh Mì, Cơm, Bún, Phở |

### Sau khi cập nhật:

| Đặc điểm | Giá trị |
|----------|---------|
| Số quán | 12 quán |
| Dữ liệu | Thực tế |
| Tọa độ | Chính xác (Vĩnh Khánh) |
| Mô tả | Chi tiết, đầy đủ |
| Ngôn ngữ | 3 ngôn ngữ (VI/EN/CN) |
| Categories | Hải sản, Ốc, Bún, Nướng, Lẩu & Nướng |

---

## 🎨 ICON CATEGORIES

| Category | Icon | Số quán |
|----------|------|---------|
| Hải sản | 🦐 | 3 |
| Ốc | 🐚 | 4 |
| Bún | 🍜 | 2 |
| Nướng | 🍢 | 2 |
| Lẩu & Nướng | 🍲 | 1 |

---

## 📂 FILES LIÊN QUAN

### 1. `UPDATE_VINH_KHANH_FOODS.sql`
- File SQL cập nhật dữ liệu
- Xóa dữ liệu cũ
- Thêm 12 quán mới
- Có mô tả 3 ngôn ngữ

### 2. `VietnamFoodGuide/Views/MapWindow.xaml.cs`
- Cập nhật icon mapping
- Thêm categories mới: Hải sản, Ốc, Nướng, Lẩu & Nướng
- Đặt ưu tiên hiển thị

---

## 🎯 KẾT QUẢ MONG ĐỢI

### Bản đồ:
- ✅ 12 markers tập trung ở khu vực Vĩnh Khánh
- ✅ Icon phù hợp với từng loại quán
- ✅ Tên quán thực tế, dễ nhận biết

### Mô tả:
- ✅ Chi tiết, đầy đủ thông tin
- ✅ Dịch chuẩn sang 3 ngôn ngữ
- ✅ Giống với mô tả gốc bạn cung cấp

### Trải nghiệm:
- ✅ User có thể tìm quán ăn thực tế
- ✅ Mô tả giúp user hiểu rõ về quán
- ✅ Hỗ trợ đa ngôn ngữ cho du khách

---

## 🔄 ROLLBACK (NẾU CẦN)

Nếu muốn quay lại dữ liệu cũ:

```sql
-- Restore từ file backup
-- Hoặc chạy lại file setup_complete.sql
```

---

## 📞 LƯU Ý

1. **Tọa độ:** Tất cả quán đều nằm trong khu vực Vĩnh Khánh thực tế
2. **Mô tả:** Giữ nguyên nội dung bạn cung cấp, chỉ format lại
3. **Dịch thuật:** Dịch chính xác, giữ nguyên ý nghĩa
4. **Rating:** Đặt từ 4.5 - 4.8 (quán ngon)
5. **Priority:** Đặt từ 7-9 (ưu tiên cao)

---

**🎉 HOÀN THÀNH!**

Bây giờ app sẽ hiển thị 12 quán ăn thực tế trên phố Vĩnh Khánh với mô tả đầy đủ bằng 3 ngôn ngữ!

---

**Ngày tạo:** 2026-04-30  
**Trạng thái:** ✅ SẴN SÀNG CẬP NHẬT  
**File SQL:** UPDATE_VINH_KHANH_FOODS.sql
