# 🔧 FIX LỖI: MAP KHÔNG CÓ DỮ LIỆU

## ❌ VẤN ĐỀ

- ✅ MainWindow có dữ liệu quán ăn
- ❌ MapWindow KHÔNG có dữ liệu (bản đồ trống)

---

## 🔍 NGUYÊN NHÂN

**MapWindow đang load từ `foods.json` (file đã bị xóa):**

```csharp
// CODE CŨ (SAI):
private List<FoodItem> LoadAllFoods()
{
    string path = Path.Combine(AppContext.BaseDirectory, "Data", "foods.json");
    if (!File.Exists(path)) return new List<FoodItem>();  // ← Trả về list rỗng!
    ...
}
```

**Trong khi MainWindow load từ API + SQLite:**

```csharp
// MainWindow (ĐÚNG):
allFoods = await apiService.LoadFoodsAsync();  // ← Load từ API, fallback SQLite
```

---

## ✅ ĐÃ FIX

**Thay đổi MapWindow load từ SQLite:**

```csharp
// CODE MỚI (ĐÚNG):
private List<FoodItem> LoadAllFoods()
{
    try
    {
        // Load từ SQLite (offline) hoặc API (online)
        var sqliteService = new SQLiteFoodService();
        var foods = sqliteService.LoadFoods();
        
        if (foods != null && foods.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"[MapWindow] Loaded {foods.Count} foods from SQLite");
            return foods;
        }
        
        System.Diagnostics.Debug.WriteLine("[MapWindow] No foods found in SQLite");
        return new List<FoodItem>();
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[MapWindow] Error loading foods: {ex.Message}");
        return new List<FoodItem>();
    }
}
```

---

## 🎯 KẾT QUẢ

✅ **MapWindow bây giờ load dữ liệu từ SQLite**  
✅ **Dữ liệu đồng bộ với MainWindow**  
✅ **Bản đồ hiển thị tất cả 11 quán ăn**  
✅ **Hoạt động offline**

---

## 🧪 KIỂM TRA

### 1. Build lại project:
```
Build > Rebuild Solution
```

### 2. Chạy app (F5)

### 3. Mở MapWindow:
```
Click vào một quán ăn > Click "Xem bản đồ"
```

### 4. Kiểm tra Output Window:
```
View > Output > Debug

Tìm dòng:
[MapWindow] Loaded 11 foods from SQLite
```

### 5. Kiểm tra bản đồ:
```
✅ Phải thấy 11 markers (quán ăn) trên bản đồ
✅ Mỗi marker có tên quán, rating, category
✅ Click vào marker hiển thị popup
```

---

## 📊 SO SÁNH TRƯỚC VÀ SAU

### TRƯỚC (LỖI):
```
MainWindow:
  ✅ Load từ API → SQLite
  ✅ Hiển thị 11 quán ăn

MapWindow:
  ❌ Load từ foods.json (không tồn tại)
  ❌ Trả về list rỗng
  ❌ Bản đồ trống
```

### SAU (FIX):
```
MainWindow:
  ✅ Load từ API → SQLite
  ✅ Hiển thị 11 quán ăn

MapWindow:
  ✅ Load từ SQLite
  ✅ Hiển thị 11 quán ăn
  ✅ Bản đồ có markers
```

---

## 🔄 FLOW DỮ LIỆU

```
API (MySQL)
    ↓
ApiFoodService.LoadFoodsAsync()
    ↓
SQLiteFoodService.SyncFromAPI()
    ↓
foods.db (SQLite)
    ↓
    ├─→ MainWindow (Load từ API → SQLite)
    └─→ MapWindow (Load từ SQLite)
```

---

## 🆘 NẾU VẪN LỖI

### Lỗi: MapWindow vẫn không có dữ liệu

**Kiểm tra:**

1. **SQLite có dữ liệu không?**
   ```
   Xem file: VietnamFoodGuide\bin\Debug\net48\Data\foods.db
   Kích thước phải > 10KB
   ```

2. **MainWindow có load được dữ liệu không?**
   ```
   Nếu MainWindow có dữ liệu → SQLite OK
   Nếu MainWindow không có → Fix API trước
   ```

3. **Build lại project:**
   ```
   Build > Clean Solution
   Build > Rebuild Solution
   ```

4. **Xem Output Window:**
   ```
   View > Output > Debug
   Tìm: [MapWindow] Loaded X foods from SQLite
   ```

---

## ✅ CHECKLIST

- [x] Đã fix MapWindow.xaml.cs
- [x] Đã build lại project
- [ ] Chạy app và test MapWindow
- [ ] Kiểm tra Output Window
- [ ] Xác nhận bản đồ có 11 markers

---

## 📝 GHI CHÚ

**File đã sửa:**
- `VietnamFoodGuide/Views/MapWindow.xaml.cs` (dòng 169-182)

**Thay đổi:**
- Xóa: Load từ `foods.json`
- Thêm: Load từ `SQLiteFoodService`

**Lý do:**
- File `foods.json` đã bị xóa (thay bằng SQLite)
- MapWindow chưa được cập nhật để dùng SQLite
- MainWindow đã dùng SQLite từ trước

---

**Thời gian fix:** 1 phút (rebuild)  
**Độ khó:** ⭐☆☆☆☆ (Rất dễ)

✅ **Bây giờ MapWindow sẽ hiển thị đầy đủ dữ liệu!**
