# 🧪 HƯỚNG DẪN TEST HỆ THỐNG OFFLINE-FIRST

## 📋 CHUẨN BỊ

### Yêu cầu:
- ✅ Visual Studio đã cài đặt
- ✅ XAMPP đã cài đặt (Apache + MySQL)
- ✅ Database `VietnamFoodGuide` đã được tạo
- ✅ App đã build thành công

---

## 🧪 TEST 1: KIỂM TRA KHÔNG CÒN JSON

### Mục đích:
Xác nhận app không còn sử dụng file JSON nào

### Các bước:
```powershell
# 1. Tìm tất cả file JSON trong Data folder
Get-ChildItem -Path "VietnamFoodGuide/Data" -Filter "*.json" -Recurse

# Kết quả mong đợi: Không tìm thấy file nào (hoặc chỉ có file config)

# 2. Kiểm tra AppData folder
Get-ChildItem -Path "$env:APPDATA\VietnamFoodGuide" -Filter "*.json" -Recurse

# Kết quả mong đợi: Không có favorites.json
```

### ✅ Pass nếu:
- Không tìm thấy `foods.json`
- Không tìm thấy `favorites.json`
- Không tìm thấy cache JSON files

---

## 🧪 TEST 2: APP HOẠT ĐỘNG OFFLINE (KHÔNG CÓ MẠNG)

### Mục đích:
Kiểm tra app hoạt động hoàn toàn offline

### Các bước:

#### Bước 1: Tắt mạng
```
1. Tắt WiFi
2. Hoặc rút dây mạng
3. Hoặc bật Airplane Mode
```

#### Bước 2: Mở app
```
1. Chạy VietnamFoodGuide.exe
2. Quan sát thời gian khởi động
```

#### Bước 3: Đăng nhập
```
Username: user123
Password: user123
```

#### Bước 4: Kiểm tra chức năng
- [ ] Xem danh sách quán ăn (MainWindow)
- [ ] Xem chi tiết quán ăn (FoodDetailWindow)
- [ ] Thêm vào yêu thích
- [ ] Xem danh sách yêu thích (FavoritesWindow)
- [ ] Xóa khỏi yêu thích
- [ ] Xem bản đồ offline (MapWindow)
- [ ] Tìm kiếm quán ăn

### ✅ Pass nếu:
- App mở nhanh (< 2 giây)
- Đăng nhập thành công
- Tất cả chức năng hoạt động
- Không có lỗi "No internet connection"
- Không có crash

### 📊 Kết quả mong đợi:
```
[SQLiteFoodService] Loaded 12 foods from SQLite
[SQLiteUserService] Offline login successful for user: user123
[SQLiteFavoritesService] Loaded X favorites for user 2
```

---

## 🧪 TEST 3: APP HOẠT ĐỘNG ONLINE (CÓ MẠNG + XAMPP)

### Mục đích:
Kiểm tra app kết nối với XAMPP và sync dữ liệu

### Các bước:

#### Bước 1: Chuẩn bị
```
1. Bật WiFi/Mạng
2. Mở XAMPP Control Panel
3. Start Apache
4. Start MySQL
5. Kiểm tra database VietnamFoodGuide đã tồn tại
```

#### Bước 2: Kiểm tra API
```
Mở browser: http://localhost/vfg-api/api.php?action=foods

Kết quả mong đợi: JSON array với 12 quán ăn
```

#### Bước 3: Mở app
```
1. Chạy VietnamFoodGuide.exe
2. Quan sát console output
```

#### Bước 4: Đăng nhập
```
Username: user123
Password: user123
```

#### Bước 5: Kiểm tra sync
- [ ] Xem danh sách quán ăn
- [ ] Thêm vào yêu thích
- [ ] Kiểm tra database MySQL (table Favorites)
- [ ] Xóa khỏi yêu thích
- [ ] Kiểm tra database MySQL (đã xóa)

### ✅ Pass nếu:
- App kết nối XAMPP thành công
- Dữ liệu được sync từ MySQL
- Favorites được sync lên server
- Console log hiển thị "API thành công"

### 📊 Kết quả mong đợi:
```
[ApiFoodService] Loading foods from API: http://localhost/vfg-api/api.php?action=foods
[ApiFoodService] Successfully loaded 12 foods from API
[SQLiteFoodService] Synced 12 foods from API to SQLite
[ApiAuthService] Online login successful for user: user123
```

---

## 🧪 TEST 4: AUTO-SWITCHING (CHUYỂN ĐỔI TỰ ĐỘNG)

### Mục đích:
Kiểm tra app tự động chuyển đổi giữa online và offline

### Các bước:

#### Scenario 1: Online → Offline
```
1. Mở app với mạng (online mode)
2. Đăng nhập
3. Xem danh sách quán ăn
4. TẮT MẠNG (giữa chừng)
5. Thêm vào yêu thích
6. Xem danh sách yêu thích
```

**✅ Pass nếu:**
- App không crash
- Favorites vẫn được lưu vào SQLite
- App tiếp tục hoạt động bình thường

#### Scenario 2: Offline → Online
```
1. Mở app không có mạng (offline mode)
2. Đăng nhập
3. Thêm vào yêu thích (lưu local)
4. BẬT MẠNG
5. Đợi 60 giây (background sync)
6. Kiểm tra database MySQL
```

**✅ Pass nếu:**
- Favorites được sync lên server
- Database MySQL có dữ liệu mới
- Console log hiển thị "Sync thành công"

### 📊 Kết quả mong đợi:
```
[BackgroundSync] Network available, starting sync...
[BackgroundSync] Synced X favorites to server
[BackgroundSync] Sync completed successfully
```

---

## 🧪 TEST 5: XAMPP CHƯA CHẠY (CÓ MẠNG NHƯNG API LỖI)

### Mục đích:
Kiểm tra app fallback khi XAMPP chưa chạy

### Các bước:

#### Bước 1: Chuẩn bị
```
1. Bật WiFi/Mạng
2. ĐÓNG XAMPP (hoặc Stop Apache)
```

#### Bước 2: Mở app
```
1. Chạy VietnamFoodGuide.exe
2. Quan sát console output
```

#### Bước 3: Đăng nhập
```
Username: user123
Password: user123
```

#### Bước 4: Kiểm tra
- [ ] App vẫn mở được
- [ ] Đăng nhập thành công (offline)
- [ ] Xem danh sách quán ăn (từ SQLite)
- [ ] Tất cả chức năng hoạt động

### ✅ Pass nếu:
- App phát hiện XAMPP chưa chạy
- Tự động fallback sang SQLite
- Không có lỗi, không crash
- App hoạt động bình thường

### 📊 Kết quả mong đợi:
```
[ApiFoodService] ERROR: API returned HTML instead of JSON
[ApiFoodService] Possible causes:
  1. XAMPP is not running
  2. Ngrok URL expired
  3. Database not created
[ApiFoodService] Fallback to SQLite
[SQLiteFoodService] Loaded 12 foods from SQLite
```

---

## 🧪 TEST 6: TỐC ĐỘ LOAD

### Mục đích:
Đo thời gian load dữ liệu

### Các bước:

#### Test Offline (SQLite)
```
1. Tắt mạng
2. Mở app
3. Đăng nhập
4. Đo thời gian từ lúc click "Đăng Nhập" đến khi hiển thị MainWindow
```

**✅ Pass nếu:** < 1 giây

#### Test Online (XAMPP)
```
1. Bật mạng + XAMPP
2. Mở app
3. Đăng nhập
4. Đo thời gian từ lúc click "Đăng Nhập" đến khi hiển thị MainWindow
```

**✅ Pass nếu:** < 2 giây

### 📊 Benchmark:
| Mode | Thời gian load | Kết quả |
|------|----------------|---------|
| Offline (SQLite) | < 50ms | ✅ Rất nhanh |
| Online (XAMPP) | < 500ms | ✅ Nhanh |
| Online (API lỗi → Fallback) | < 100ms | ✅ Nhanh |

---

## 🧪 TEST 7: DATABASE INTEGRITY

### Mục đích:
Kiểm tra tính toàn vẹn của database

### Các bước:

#### Kiểm tra foods.db
```powershell
# Mở SQLite database
sqlite3 "VietnamFoodGuide/bin/Debug/net48/Data/foods.db"

# Chạy query
SELECT COUNT(*) FROM Foods;
# Kết quả mong đợi: 12

SELECT Name, Rating FROM Foods ORDER BY Rating DESC LIMIT 5;
# Kết quả: Top 5 quán ăn có rating cao nhất
```

#### Kiểm tra favorites.db
```powershell
sqlite3 "VietnamFoodGuide/bin/Debug/net48/Data/favorites.db"

SELECT * FROM Favorites;
# Kết quả: Danh sách favorites của user
```

#### Kiểm tra users.db
```powershell
sqlite3 "VietnamFoodGuide/bin/Debug/net48/Data/users.db"

SELECT Username, Role FROM Users;
# Kết quả: Danh sách users
```

### ✅ Pass nếu:
- Tất cả database đều tồn tại
- Dữ liệu đầy đủ và chính xác
- Không có lỗi corruption

---

## 🧪 TEST 8: BACKGROUND SYNC

### Mục đích:
Kiểm tra background sync tự động

### Các bước:

#### Bước 1: Offline mode
```
1. Tắt mạng
2. Mở app, đăng nhập
3. Thêm 3 quán ăn vào yêu thích
4. Đóng app
```

#### Bước 2: Online mode
```
1. Bật mạng + XAMPP
2. Mở app, đăng nhập
3. Đợi 60 giây (background sync timer)
4. Kiểm tra console log
```

#### Bước 3: Verify
```
1. Mở phpMyAdmin
2. Vào database VietnamFoodGuide
3. Xem table Favorites
4. Kiểm tra có 3 records mới
```

### ✅ Pass nếu:
- Background sync chạy tự động
- Dữ liệu được sync lên server
- Console log hiển thị "Sync completed"

### 📊 Kết quả mong đợi:
```
[BackgroundSync] Timer started - will sync every 60 seconds
[BackgroundSync] Network available, starting sync...
[BackgroundSync] Found 3 favorites pending sync
[BackgroundSync] Synced favorite: User 2, Food 5
[BackgroundSync] Synced favorite: User 2, Food 8
[BackgroundSync] Synced favorite: User 2, Food 11
[BackgroundSync] Sync completed successfully
```

---

## 📊 BẢNG TỔNG KẾT

| Test | Mục đích | Kết quả |
|------|----------|---------|
| 1. Không còn JSON | Xác nhận đã loại bỏ JSON | ⬜ |
| 2. Offline Mode | App hoạt động không có mạng | ⬜ |
| 3. Online Mode | App kết nối XAMPP | ⬜ |
| 4. Auto-Switching | Chuyển đổi tự động | ⬜ |
| 5. XAMPP chưa chạy | Fallback SQLite | ⬜ |
| 6. Tốc độ Load | < 1 giây offline | ⬜ |
| 7. Database Integrity | Dữ liệu đầy đủ | ⬜ |
| 8. Background Sync | Sync tự động | ⬜ |

---

## 🐛 TROUBLESHOOTING

### Lỗi: "Cannot find foods.db"
**Nguyên nhân:** Database chưa được tạo  
**Giải pháp:** 
```
1. Chạy app lần đầu
2. SQLiteFoodService sẽ tự động tạo database
3. Insert 12 quán ăn mẫu
```

### Lỗi: "API returned HTML"
**Nguyên nhân:** XAMPP chưa chạy  
**Giải pháp:**
```
1. Mở XAMPP Control Panel
2. Start Apache
3. Start MySQL
4. Kiểm tra http://localhost/vfg-api/api.php
```

### Lỗi: "Offline login failed"
**Nguyên nhân:** users.db chưa có dữ liệu  
**Giải pháp:**
```
1. Đăng ký tài khoản mới
2. Hoặc chạy SQL script để tạo user mặc định
```

### App chậm khi load
**Nguyên nhân:** Database quá lớn  
**Giải pháp:**
```
1. Kiểm tra số lượng records
2. Xóa dữ liệu cũ không cần thiết
3. Optimize database: VACUUM
```

---

## ✅ CHECKLIST HOÀN THÀNH

- [ ] Test 1: Không còn JSON ✅
- [ ] Test 2: Offline Mode ✅
- [ ] Test 3: Online Mode ✅
- [ ] Test 4: Auto-Switching ✅
- [ ] Test 5: XAMPP chưa chạy ✅
- [ ] Test 6: Tốc độ Load ✅
- [ ] Test 7: Database Integrity ✅
- [ ] Test 8: Background Sync ✅

---

**🎉 NẾU TẤT CẢ TEST ĐỀU PASS → HỆ THỐNG OFFLINE-FIRST HOÀN HẢO!**
