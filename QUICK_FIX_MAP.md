# ⚡ FIX NHANH: MAP KHÔNG CÓ DỮ LIỆU

## ❌ VẤN ĐỀ
MainWindow có dữ liệu, nhưng Map trống

## ✅ ĐÃ FIX
Thay đổi MapWindow load từ SQLite thay vì foods.json

## 🔧 CÁCH TEST

```
1. Build > Rebuild Solution
2. F5 (chạy app)
3. Click vào một quán ăn
4. Click "Xem bản đồ"
5. ✅ Phải thấy 11 markers trên bản đồ!
```

## 📊 KIỂM TRA OUTPUT

```
View > Output > Debug

Tìm dòng:
[MapWindow] Loaded 11 foods from SQLite
```

## 🎯 KẾT QUẢ

✅ Bản đồ hiển thị đầy đủ 11 quán ăn  
✅ Mỗi marker có tên, rating, category  
✅ Click marker hiển thị popup  
✅ Hoạt động offline

---

**File đã sửa:** `VietnamFoodGuide/Views/MapWindow.xaml.cs`  
**Thời gian:** 1 phút (rebuild)

📖 **Chi tiết:** Xem `FIX_MAP_KHONG_CO_DU_LIEU.md`
