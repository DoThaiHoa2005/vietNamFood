# 🚀 HƯỚNG DẪN CẬP NHẬT NHANH

## ✅ ĐÃ HOÀN THÀNH

### 1. File SQL duy nhất: `xampp_api/SETUP_VINH_KHANH_COMPLETE.sql`
- ✅ Tạo tất cả tables
- ✅ Thêm 2 users (admin, user123)
- ✅ Thêm 12 quán ăn thực tế phố Vĩnh Khánh
- ✅ Mô tả đầy đủ 3 ngôn ngữ (VI/EN/CN)

### 2. Icon marker nhỏ gọn hơn
- ✅ Giảm từ 44x44px → 32x32px
- ✅ Font size từ 22px → 16px
- ✅ Border từ 3px → 2px
- ✅ Dễ nhìn, gọn gàng hơn

---

## 🔧 CÁCH CẬP NHẬT

### Bước 1: Chạy file SQL

```bash
# Mở phpMyAdmin
# Chọn database VietnamFoodGuide
# Vào tab SQL
# Copy toàn bộ nội dung file: xampp_api/SETUP_VINH_KHANH_COMPLETE.sql
# Paste vào và click "Go"
```

**Hoặc dùng command line:**
```bash
mysql -u root -p VietnamFoodGuide < xampp_api/SETUP_VINH_KHANH_COMPLETE.sql
```

### Bước 2: Build lại app

```bash
cd VietnamFoodGuide
dotnet clean
dotnet build
```

### Bước 3: Chạy app

```bash
# Run (F5)
# Xem bản đồ → 12 quán mới với icon nhỏ gọn
```

---

## 📋 12 QUÁN ĂN PHỐ VĨNH KHÁNH

| # | Tên quán | Category | Rating |
|---|----------|----------|--------|
| 1 | Alo Quán – Seafood & Beer | Hải sản 🦐 | 4.5 |
| 2 | Ốc Đào 2 | Ốc 🐚 | 4.6 |
| 3 | Bún cá Châu Đốc Dì Tư | Bún 🍜 | 4.7 |
| 4 | Bún thịt nướng Cô Nga | Bún 🍜 | 4.5 |
| 5 | Ốc Vũ | Ốc 🐚 | 4.6 |
| 6 | Lãng Quán | Hải sản 🦐 | 4.7 |
| 7 | Ớt Xiêm Quán | Hải sản 🦐 | 4.6 |
| 8 | Lẩu nướng HongKong A FAT | Lẩu & Nướng 🍲 | 4.8 |
| 9 | Sườn Muối Ớt | Nướng 🍢 | 4.7 |
| 10 | Chilli Quán | Nướng 🍢 | 4.6 |
| 11 | Thảo ốc quận 4 cũ | Ốc 🐚 | 4.7 |
| 12 | Ốc Oanh | Ốc 🐚 | 4.8 |

---

## ✅ KẾT QUẢ

### Icon marker:
- **Trước:** 44x44px, font 22px, border 3px
- **Sau:** 32x32px, font 16px, border 2px ✅

### Dữ liệu:
- **Trước:** 11 quán giả lập
- **Sau:** 12 quán thực tế phố Vĩnh Khánh ✅

### Categories:
- **Mới:** Hải sản 🦐, Ốc 🐚, Nướng 🍢, Lẩu & Nướng 🍲

---

**🎉 HOÀN THÀNH!**

Chỉ cần chạy 1 file SQL duy nhất và build lại app!

---

**File SQL:** `xampp_api/SETUP_VINH_KHANH_COMPLETE.sql`  
**Ngày:** 2026-04-30
