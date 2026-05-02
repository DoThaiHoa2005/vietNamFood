# 🔧 FIX LỖI "KHÔNG CÓ DỮ LIỆU QUÁN ĂN"

## ❌ LỖI

App hiển thị: **"Không có dữ liệu quán ăn"**

---

## 🔍 NGUYÊN NHÂN

Có 3 nguyên nhân chính:

1. **XAMPP không chạy** (Apache hoặc MySQL tắt)
2. **Database chưa có dữ liệu** (chưa chạy setup_complete.sql)
3. **File api.php không tồn tại** (chưa copy vào XAMPP)

---

## ✅ CÁCH FIX (THEO THỨ TỰ)

### BƯỚC 1: Kiểm tra XAMPP

```
1. Mở XAMPP Control Panel
2. Kiểm tra:
   ✅ Apache: Running (màu xanh)
   ✅ MySQL: Running (màu xanh)
3. Nếu chưa chạy → Click "Start"
```

**Quan trọng:** CẢ HAI phải chạy!

---

### BƯỚC 2: Copy file API vào XAMPP

```
1. Mở thư mục project: xampp_api/
2. Copy TOÀN BỘ thư mục xampp_api
3. Paste vào: C:\xampp\htdocs\
4. Đổi tên thành: vfg-api
```

**Kết quả:**
```
C:\xampp\htdocs\vfg-api\
├── api.php
├── setup_complete.sql
├── utilities.php
└── ...
```

---

### BƯỚC 3: Setup Database

```
1. Mở phpMyAdmin: http://localhost/phpmyadmin
2. Click "New" ở menu trái
3. Tên database: VietnamFoodGuide
4. Collation: utf8mb4_unicode_ci
5. Click "Create"
6. Click vào database "VietnamFoodGuide" vừa tạo
7. Click tab "SQL"
8. Copy TOÀN BỘ file: xampp_api/setup_complete.sql
9. Paste vào và click "Go"
10. ✅ Chờ chạy xong!
```

**Kết quả:**
- ✅ 6 bảng được tạo
- ✅ 11 quán ăn được thêm
- ✅ 8 users được tạo

---

### BƯỚC 4: Test API

```
Mở browser và test:
http://localhost/vfg-api/api.php?action=foods
```

**Kết quả mong đợi:**
```json
[
  {
    "Id": 1,
    "Name": "Bánh Mì Trần Văn Hành",
    "City": "TP.HCM - Vĩnh Khánh",
    "Category": "Bánh Mì",
    ...
  },
  ...
]
```

**Nếu thấy JSON với danh sách quán ăn → API OK!**

---

### BƯỚC 5: Chạy lại App

```
1. Đóng app (nếu đang chạy)
2. Trong Visual Studio: F5
3. ✅ App sẽ load dữ liệu từ API
4. ✅ Dữ liệu tự động sync vào SQLite
```

---

## 🧪 KIỂM TRA NHANH

Chạy script PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File check_issue.ps1
```

Script sẽ tự động kiểm tra:
- ✅ Apache có chạy không
- ✅ MySQL có chạy không
- ✅ File api.php có tồn tại không
- ✅ API có trả về dữ liệu không
- ✅ SQLite có dữ liệu không

---

## 🆘 NẾU VẪN LỖI

### Lỗi 1: API trả về 0 quán ăn

**Nguyên nhân:** Database chưa có dữ liệu

**Fix:**
```
Chạy lại BƯỚC 3 (Setup Database)
Đảm bảo chạy file setup_complete.sql
```

---

### Lỗi 2: Không kết nối được API

**Nguyên nhân:** XAMPP chưa chạy hoặc file api.php không đúng vị trí

**Fix:**
```
1. Kiểm tra XAMPP (BƯỚC 1)
2. Kiểm tra file api.php (BƯỚC 2)
3. Test API (BƯỚC 4)
```

---

### Lỗi 3: Database connection failed

**Nguyên nhân:** MySQL không chạy hoặc database chưa tạo

**Fix:**
```
1. Start MySQL trong XAMPP
2. Tạo database "VietnamFoodGuide" (BƯỚC 3)
```

---

### Lỗi 4: SQLite không có dữ liệu

**Nguyên nhân:** App chưa kết nối được API lần nào

**Fix:**
```
1. Fix API trước (BƯỚC 1-4)
2. Chạy lại app để sync dữ liệu
3. SQLite sẽ tự động được tạo và sync
```

---

## 📊 DEBUG TRONG VISUAL STUDIO

### Xem log:

```
1. Chạy app (F5)
2. View > Output
3. Chọn "Debug" trong dropdown
4. Tìm dòng:
   [ApiFoodService] Loading foods from API...
   [ApiFoodService] Successfully loaded X foods from API
   [SQLiteFoodService] Synced X foods from API to SQLite
```

### Nếu thấy lỗi:

```
[ApiFoodService] HTTP Error: ...
→ XAMPP không chạy hoặc API không tồn tại

[ApiFoodService] JSON Parse Error: ...
→ API trả về HTML thay vì JSON (database lỗi)

[ApiFoodService] API returned empty list
→ Database chưa có dữ liệu
```

---

## ✅ CHECKLIST

- [ ] XAMPP Apache đang chạy
- [ ] XAMPP MySQL đang chạy
- [ ] File api.php tồn tại: C:\xampp\htdocs\vfg-api\api.php
- [ ] Database "VietnamFoodGuide" đã tạo
- [ ] Đã chạy setup_complete.sql
- [ ] API test OK: http://localhost/vfg-api/api.php?action=foods
- [ ] App hiển thị dữ liệu

---

## 🎯 TÓM TẮT NHANH

```
1. Start XAMPP (Apache + MySQL)
2. Copy xampp_api → C:\xampp\htdocs\vfg-api\
3. Chạy setup_complete.sql trong phpMyAdmin
4. Test API: http://localhost/vfg-api/api.php?action=foods
5. Chạy lại app (F5)
6. ✅ XONG!
```

---

**Thời gian fix:** 5-10 phút  
**Độ khó:** ⭐⭐☆☆☆ (Dễ)

✅ **Sau khi fix, app sẽ hiển thị 11 quán ăn!**
