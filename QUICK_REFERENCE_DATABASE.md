# 🚀 QUICK REFERENCE - DATABASE SETUP

## ⚡ SETUP MỚI (2 PHÚT)

```bash
1. http://localhost/phpmyadmin
2. Tạo database "VietnamFoodGuide"
3. Chạy: xampp_api/setup_complete.sql
4. ✅ XONG!
```

---

## 📋 2 FILE SQL

| File | Dùng khi | Tạo bảng | Thêm cột | Dữ liệu |
|------|----------|----------|----------|---------|
| `setup_complete.sql` | Setup MỚI | ✅ | ✅ Có sẵn | ✅ |
| `update_database_complete.sql` | Cập nhật CŨ | ❌ | ✅ ALTER | ❌ |

---

## 🎯 KHI NÀO DÙNG FILE NÀO?

```
Bạn đã có database chưa?
├─ CHƯA → setup_complete.sql
└─ RỒI  → update_database_complete.sql
```

---

## ❌ LỖI THƯỜNG GẶP

### "Table doesn't exist"
```
Nguyên nhân: Chạy sai file
Fix: Chạy setup_complete.sql
```

### "Database doesn't exist"
```
Fix: Tạo database trước
```

### "Column already exists"
```
Fix: Không cần làm gì
```

---

## ✅ KIỂM TRA

```sql
SHOW TABLES;                    -- 6 tables
DESCRIBE Foods;                 -- 16 columns
SELECT COUNT(*) FROM Foods;     -- 11 rows
```

---

## 🔗 TÀI LIỆU

- **DATABASE_SETUP_SUMMARY.md** - Tổng quan
- **SETUP_DATABASE_1_FILE_DUY_NHAT.md** - Hướng dẫn chi tiết
- **SO_SANH_2_FILE_SQL.md** - So sánh 2 file
- **FIX_DATABASE_ERROR.md** - Fix lỗi

---

## 🎉 TEST

```bash
# Test API
http://localhost/vfg-api/api.php?action=foods

# Test App
F5 trong Visual Studio

# Login
admin / admin123
```

---

**Thời gian:** 2 phút | **File:** `setup_complete.sql` (CHỈ 1 FILE!)
