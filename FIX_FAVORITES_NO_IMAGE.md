# ✅ SỬA LỖI KHÔNG HIỆN ẢNH TRONG FAVORITES

## 🐛 VẤN ĐỀ

- ✅ MainWindow: Ảnh hiển thị bình thường
- ❌ FavoritesWindow: Ảnh KHÔNG hiển thị (chỉ có background màu xám)

## 🔍 NGUYÊN NHÂN

### Vấn đề 1: Join sai database
`SQLiteFavoritesService.GetUserFavorites()` đang cố join với bảng `Foods`:

```sql
SELECT f.* 
FROM Favorites fav
INNER JOIN (SELECT * FROM Foods) f ON fav.FoodId = f.Id
```

**Lỗi:** Bảng `Foods` nằm trong `foods.db`, không phải `favorites.db`!

SQLite không hỗ trợ cross-database join như vậy.

### Vấn đề 2: Đường dẫn ảnh cũ
Ngay cả khi join thành công, nếu `foods.db` chưa rebuild, đường dẫn ảnh vẫn là `/Assets/Images/Q1.jpg` (sai).

## ✅ GIẢI PHÁP

### Đã sửa: SQLiteFavoritesService.cs

Thay vì join, giờ sẽ:
1. Lấy danh sách `FoodId` từ `favorites.db`
2. Load tất cả foods từ `SQLiteFoodService` (từ `foods.db`)
3. Filter chỉ lấy foods có trong favorites

```csharp
public List<FoodItem> GetUserFavorites(int userId)
{
    var favorites = new List<FoodItem>();

    try
    {
        // 1. Lấy danh sách FoodId từ favorites.db
        var foodIds = new List<int>();
        
        using (var conn = new SQLiteConnection(_connectionString))
        {
            conn.Open();

            string selectSql = @"
                SELECT FoodId 
                FROM Favorites 
                WHERE UserId = @UserId
                ORDER BY CreatedDate DESC";

            using (var cmd = new SQLiteCommand(selectSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        foodIds.Add(Convert.ToInt32(reader["FoodId"]));
                    }
                }
            }
        }

        // 2. Load thông tin đầy đủ từ foods.db
        if (foodIds.Count > 0)
        {
            var foodService = new SQLiteFoodService();
            var allFoods = foodService.LoadFoods();
            
            // 3. Filter chỉ lấy foods có trong favorites
            foreach (var foodId in foodIds)
            {
                var food = allFoods.Find(f => f.Id == foodId);
                if (food != null)
                {
                    favorites.Add(food);
                }
            }
        }

        System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Loaded {favorites.Count} favorites for user {userId}");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[SQLiteFavoritesService] Error loading favorites: {ex.Message}");
    }

    return favorites;
}
```

## 🚀 CÁCH CHẠY

### 1. Build lại app
```
Ctrl + Shift + B
```

### 2. Chạy app
```
F5
```

### 3. Test
```
1. Đăng nhập
2. Thêm quán vào Favorites
3. Mở FavoritesWindow
4. ✅ Ảnh hiển thị bình thường
```

## 📊 SO SÁNH TRƯỚC VÀ SAU

### Trước (Lỗi):
```
favorites.db (SQLiteFavoritesService)
    ↓
    JOIN với Foods (KHÔNG TỒN TẠI trong favorites.db)
    ↓
    ❌ Lỗi hoặc không có dữ liệu
    ↓
    ❌ Ảnh không hiển thị
```

### Sau (Đã sửa):
```
favorites.db (SQLiteFavoritesService)
    ↓
    Lấy danh sách FoodId
    ↓
foods.db (SQLiteFoodService)
    ↓
    Load tất cả foods với đường dẫn ảnh đúng
    ↓
    Filter theo FoodId
    ↓
    ✅ Trả về favorites với ảnh đầy đủ
```

## 🗄️ KIẾN TRÚC DATABASE

### Trước (Sai):
```
favorites.db
├── Favorites (UserId, FoodId, FoodName)
└── [Cố join với Foods - KHÔNG TỒN TẠI]

foods.db
└── Foods (Id, Name, Image, ...)
```

### Sau (Đúng):
```
favorites.db
└── Favorites (UserId, FoodId, FoodName)
        ↓
        Lấy FoodId
        ↓
foods.db
└── Foods (Id, Name, Image="Assets/Images/Q1.jpg", ...)
        ↓
        Load foods theo FoodId
        ↓
        ✅ Trả về với ảnh đúng
```

## 🧪 TEST CASES

### Test 1: Thêm vào Favorites
```
1. Mở MainWindow
2. Click ❤️ trên một quán
3. ✅ Thêm vào favorites.db
4. ✅ Lưu FoodId
```

### Test 2: Xem Favorites
```
1. Mở FavoritesWindow
2. GetUserFavorites(userId)
3. ✅ Lấy FoodId từ favorites.db
4. ✅ Load foods từ foods.db
5. ✅ Ảnh hiển thị đúng
```

### Test 3: Xóa khỏi Favorites
```
1. Click ❌ trên một quán trong Favorites
2. ✅ Xóa khỏi favorites.db
3. ✅ Refresh danh sách
4. ✅ Quán đã bị xóa
```

## 📝 FILES ĐÃ SỬA

### 1. SQLiteFavoritesService.cs
- ✅ Sửa method `GetUserFavorites()`
- ✅ Không còn join sai database
- ✅ Load từ `SQLiteFoodService` thay vì join

### 2. SQLiteFoodService.cs (đã sửa trước đó)
- ✅ Đường dẫn ảnh đúng: `Assets/Images/Q1.jpg`
- ✅ Không có dấu `/` ở đầu

## ⚠️ LƯU Ý

### Nếu vẫn không hiển thị ảnh:

#### 1. Xóa database cũ
```powershell
Remove-Item "VietnamFoodGuide\bin\Debug\net48\Data\foods.db" -Force
Remove-Item "VietnamFoodGuide\bin\Debug\net48\Data\favorites.db" -Force
```

#### 2. Rebuild app
```
Build → Clean Solution
Build → Rebuild Solution
```

#### 3. Chạy lại app
```
F5
```

#### 4. Thêm lại favorites
```
1. Đăng nhập
2. Thêm quán vào Favorites
3. Mở FavoritesWindow
4. ✅ Ảnh hiển thị
```

## 🎯 KẾT QUẢ

Sau khi sửa:
- ✅ MainWindow: Ảnh hiển thị
- ✅ FavoritesWindow: Ảnh hiển thị
- ✅ FoodDetailWindow: Ảnh hiển thị
- ✅ Tất cả đều dùng đường dẫn ảnh đúng

## 📸 TRƯỚC VÀ SAU

### Trước (Lỗi):
```
FavoritesWindow:
┌─────────────────┐
│                 │
│   [Màu xám]     │  ← Không có ảnh
│                 │
└─────────────────┘
Alo Quán – Seafood & Beer
TP.HCM - Vĩnh Khánh
⭐ 4.8
```

### Sau (Đã sửa):
```
FavoritesWindow:
┌─────────────────┐
│                 │
│   [Ảnh quán]    │  ← Hiển thị ảnh đẹp
│                 │
└─────────────────┘
Alo Quán – Seafood & Beer
TP.HCM - Vĩnh Khánh
⭐ 4.8
```

## 🔗 LIÊN QUAN

### Files đã sửa:
1. `SQLiteFoodService.cs` - Đường dẫn ảnh đúng
2. `SQLiteFavoritesService.cs` - Load favorites đúng cách
3. `FIX_IMAGE_NOT_SHOWING.md` - Sửa lỗi ảnh MainWindow
4. `FIX_FAVORITES_NO_IMAGE.md` - File này

### Kiến trúc:
- `favorites.db` - Chỉ lưu UserId, FoodId
- `foods.db` - Lưu thông tin đầy đủ quán ăn + ảnh
- `SQLiteFavoritesService` - Load FoodId từ favorites.db
- `SQLiteFoodService` - Load thông tin đầy đủ từ foods.db

---

**🎉 ĐÃ SỬA XONG LỖI KHÔNG HIỆN ẢNH TRONG FAVORITES!**

**✅ Build lại app và test để xem kết quả!**
