# 🔄 HỆ THỐNG OFFLINE-FIRST HOÀN CHỈNH

## ✅ ĐÃ LOẠI BỎ TẤT CẢ FILE JSON

App không còn sử dụng bất kỳ file JSON nào. Tất cả dữ liệu được lưu trong **SQLite databases**.

---

## 📊 KIẾN TRÚC DỮ LIỆU

### 1. **Foods Data** → `Data/foods.db`
- **Service**: `SQLiteFoodService.cs`
- **Chức năng**: Lưu trữ 12 quán ăn Vĩnh Khánh
- **Offline-First**: 
  - ✅ Có mạng: Load từ SQLite (instant) → Sync từ XAMPP API (background) → Update SQLite
  - ✅ Không mạng: Load từ SQLite (app vẫn hoạt động bình thường)

### 2. **Favorites Data** → `Data/favorites.db`
- **Service**: `SQLiteFavoritesService.cs` (MỚI)
- **Chức năng**: Lưu trữ danh sách yêu thích của user
- **Offline-First**:
  - ✅ Thêm/Xóa favorite → Lưu ngay vào SQLite
  - ✅ Có mạng: Auto sync lên XAMPP API
  - ✅ Không mạng: Lưu local, sync sau khi có mạng trở lại

### 3. **Users Data** → `Data/users.db`
- **Service**: `SQLiteUserService.cs`
- **Chức năng**: Authentication offline
- **Offline-First**:
  - ✅ Đăng nhập offline với SQLite
  - ✅ Có mạng: Đăng nhập online với XAMPP API
  - ✅ Tự động fallback khi mất kết nối

### 4. **QR Scans Data** → `Data/qr_scans.db`
- **Service**: `QRScanService.cs`
- **Chức năng**: Lưu lịch sử quét QR
- **Offline-First**:
  - ✅ Quét QR → Lưu ngay vào SQLite
  - ✅ Có mạng: Auto sync lên server
  - ✅ Không mạng: Lưu local, sync sau

---

## 🔄 CƠ CHẾ AUTO-SWITCHING

### **ApiFoodService.cs** - Tự động chuyển đổi

```csharp
public async Task<List<FoodItem>> LoadFoodsAsync()
{
    try
    {
        // 1. Thử kết nối XAMPP API
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}?action=foods");
        
        if (!response.IsSuccessStatusCode)
        {
            // ❌ API lỗi → Fallback SQLite
            return _sqliteService.LoadFoods();
        }

        var json = await response.Content.ReadAsStringAsync();
        
        // Kiểm tra response có phải HTML không (XAMPP chưa chạy)
        if (json.TrimStart().StartsWith("<"))
        {
            // ❌ XAMPP trả về HTML → Fallback SQLite
            return _sqliteService.LoadFoods();
        }
        
        var foods = JsonSerializer.Deserialize<List<ApiFood>>(json);
        
        if (foods == null || foods.Count == 0)
        {
            // ❌ API trả về rỗng → Fallback SQLite
            return _sqliteService.LoadFoods();
        }

        // ✅ API thành công → Sync về SQLite
        _sqliteService.SyncFromAPI(foodItems);
        return foodItems;
    }
    catch (HttpRequestException)
    {
        // ❌ Không có mạng → Fallback SQLite
        return _sqliteService.LoadFoods();
    }
    catch (JsonException)
    {
        // ❌ JSON parse lỗi → Fallback SQLite
        return _sqliteService.LoadFoods();
    }
    catch (Exception)
    {
        // ❌ Lỗi bất ngờ → Fallback SQLite
        return _sqliteService.LoadFoods();
    }
}
```

### **Kịch bản hoạt động**

#### 🟢 **Khi có mạng + XAMPP đang chạy**
1. App gọi `ApiFoodService.LoadFoodsAsync()`
2. Kết nối thành công với `http://localhost/vfg-api/api.php?action=foods`
3. Nhận dữ liệu từ MySQL database
4. Sync dữ liệu về `foods.db` (SQLite)
5. Hiển thị dữ liệu cho user

#### 🔴 **Khi không có mạng**
1. App gọi `ApiFoodService.LoadFoodsAsync()`
2. `HttpRequestException` → Catch
3. Tự động fallback: `_sqliteService.LoadFoods()`
4. Load dữ liệu từ `foods.db` (SQLite)
5. Hiển thị dữ liệu cho user (app vẫn hoạt động bình thường)

#### 🟡 **Khi có mạng nhưng XAMPP chưa chạy**
1. App gọi `ApiFoodService.LoadFoodsAsync()`
2. Kết nối thành công nhưng nhận HTML error page
3. Phát hiện response bắt đầu bằng `<` → HTML
4. Tự động fallback: `_sqliteService.LoadFoods()`
5. Load dữ liệu từ SQLite

---

## 🔧 BACKGROUND SYNC SERVICE

### **BackgroundSyncService.cs**

```csharp
// Tự động sync mỗi 60 giây khi có mạng
private async void SyncTimer_Tick(object sender, EventArgs e)
{
    if (!NetworkService.IsNetworkAvailable())
    {
        Debug.WriteLine("[BackgroundSync] No network, skip sync");
        return;
    }

    // Sync Foods từ API về SQLite
    var foods = await _apiFoodService.LoadFoodsAsync();
    
    // Sync Favorites lên server
    var pendingFavorites = _favoritesService.GetPendingSync();
    foreach (var (userId, foodId) in pendingFavorites)
    {
        await SyncFavoriteToServer(userId, foodId);
    }
    
    // Sync QR Scans lên server
    await _qrScanService.SyncPendingScansAsync();
}
```

---

## 📱 LUỒNG DỮ LIỆU TRONG APP

### **MainWindow - Trang chủ**

```csharp
private async void LoadFoods()
{
    // Offline-First: Luôn load từ SQLite trước (instant)
    var sqliteFoods = _sqliteFoodService.LoadFoods();
    DisplayFoods(sqliteFoods);
    
    // Background: Thử sync từ API
    try
    {
        var apiFoods = await _apiFoodService.LoadFoodsAsync();
        if (apiFoods.Count > 0)
        {
            DisplayFoods(apiFoods); // Update UI với dữ liệu mới
        }
    }
    catch
    {
        // Không có mạng, giữ nguyên dữ liệu SQLite
    }
}
```

### **FavoritesWindow - Yêu thích**

```csharp
private void LoadFavorites()
{
    if (App.CurrentApiUser == null) return;
    
    // Load từ SQLite (offline-first)
    var favorites = _favoritesService.GetUserFavorites(App.CurrentApiUser.Id);
    DisplayFavorites(favorites);
    
    // Background: Sync từ server nếu có mạng
    _ = SyncFavoritesFromServer();
}

private async Task SyncFavoritesFromServer()
{
    try
    {
        var response = await _httpClient.GetAsync(
            $"{AppConfig.ApiBaseUrl}?action=getUserFavorites&userId={App.CurrentApiUser.Id}"
        );
        
        if (response.IsSuccessStatusCode)
        {
            var serverFavorites = await response.Content.ReadAsAsync<List<FoodItem>>();
            _favoritesService.SyncFromServer(App.CurrentApiUser.Id, serverFavorites);
        }
    }
    catch
    {
        // Không có mạng, dùng dữ liệu SQLite
    }
}
```

---

## 🗄️ CẤU TRÚC DATABASE

### **foods.db**
```sql
CREATE TABLE Foods (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    City TEXT,
    Category TEXT,
    Image TEXT,
    DescriptionVI TEXT,
    DescriptionEN TEXT,
    DescriptionCN TEXT,
    Latitude REAL DEFAULT 0,
    Longitude REAL DEFAULT 0,
    Rating REAL DEFAULT 0,
    Radius REAL DEFAULT 30.0,
    Priority INTEGER DEFAULT 5,
    AudioUrl TEXT,
    NarrationScript TEXT,
    CooldownMinutes INTEGER DEFAULT 5,
    CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP
)
```

### **favorites.db**
```sql
CREATE TABLE Favorites (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    FoodId INTEGER NOT NULL,
    FoodName TEXT NOT NULL,
    CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP,
    SyncedToServer INTEGER DEFAULT 0,
    UNIQUE(UserId, FoodId)
)
```

### **users.db**
```sql
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT DEFAULT 'User',
    CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP
)
```

### **qr_scans.db**
```sql
CREATE TABLE qr_scans (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    device_id TEXT NOT NULL,
    qr_code TEXT NOT NULL,
    device_name TEXT,
    os_version TEXT,
    scan_time TEXT DEFAULT CURRENT_TIMESTAMP,
    synced_to_server INTEGER DEFAULT 0
)
```

---

## ✅ LỢI ÍCH CỦA OFFLINE-FIRST

### 1. **Tốc độ nhanh**
- Load dữ liệu từ SQLite local → Instant (< 50ms)
- Không phải chờ API response

### 2. **Hoạt động mọi lúc**
- Không có mạng → App vẫn chạy bình thường
- XAMPP chưa chạy → App vẫn hoạt động
- API lỗi → App tự động fallback

### 3. **Đồng bộ tự động**
- Có mạng trở lại → Background sync tự động
- Không cần user làm gì
- Dữ liệu luôn được cập nhật

### 4. **Trải nghiệm tốt**
- Không có loading lâu
- Không có lỗi "No internet connection"
- App luôn responsive

---

## 🧪 CÁCH KIỂM TRA

### **Test 1: Offline Mode**
1. Tắt WiFi/Mạng
2. Mở app → Đăng nhập
3. ✅ App vẫn hoạt động bình thường
4. ✅ Xem danh sách quán ăn
5. ✅ Thêm/Xóa favorites
6. ✅ Xem bản đồ offline

### **Test 2: Online Mode**
1. Bật WiFi/Mạng
2. Chạy XAMPP (Apache + MySQL)
3. Mở app → Đăng nhập
4. ✅ Kết nối với API thành công
5. ✅ Dữ liệu được sync từ MySQL
6. ✅ Favorites được sync lên server

### **Test 3: Auto-Switching**
1. Mở app với mạng → Hoạt động online
2. Tắt mạng → App tự động chuyển offline
3. ✅ Không có lỗi
4. ✅ App vẫn hoạt động
5. Bật mạng lại → App tự động sync

### **Test 4: XAMPP Not Running**
1. Bật mạng nhưng không chạy XAMPP
2. Mở app
3. ✅ App phát hiện XAMPP chưa chạy
4. ✅ Tự động fallback SQLite
5. ✅ App vẫn hoạt động bình thường

---

## 📝 NOTES

- ❌ **Không còn file JSON nào** trong app
- ✅ **Tất cả dữ liệu** đều lưu trong SQLite
- ✅ **Auto-switching** giữa online/offline
- ✅ **Background sync** tự động
- ✅ **Offline-first** architecture hoàn chỉnh

---

## 🔗 FILES LIÊN QUAN

### Services
- `ApiFoodService.cs` - API với auto-fallback
- `SQLiteFoodService.cs` - Foods offline storage
- `SQLiteFavoritesService.cs` - Favorites offline storage (MỚI)
- `SQLiteUserService.cs` - Users offline authentication
- `QRScanService.cs` - QR scans offline storage
- `BackgroundSyncService.cs` - Auto sync service
- `StorageService.cs` - Wrapper service (đã loại bỏ JSON)

### Windows
- `LoginWindow.xaml.cs` - Offline/Online login
- `MainWindow.xaml.cs` - Load foods offline-first
- `FavoritesWindow.xaml.cs` - Favorites offline-first
- `MapWindow.xaml.cs` - Map với offline tiles

---

**🎉 HỆ THỐNG OFFLINE-FIRST HOÀN CHỈNH - KHÔNG CÒN JSON!**
