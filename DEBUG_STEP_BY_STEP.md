# 🔍 DEBUG TỪNG BƯỚC - TẠI SAO SỐ VẪN = 0?

## ❓ VẤN ĐỀ
Bấm "Bắt đầu" nhưng "Đang Chỉ Đường" vẫn = 0

## 🔍 KIỂM TRA TỪNG BƯỚC

### **BƯỚC 1: Kiểm Tra App Có Gửi Message Không?**

**Mở Output Window trong Visual Studio:**
```
View → Output → Chọn "Debug"
```

**Tìm các dòng log sau khi bấm "Bắt đầu":**
```
✅ Phải thấy: 🧭 [C#] Navigation state changed: True
✅ Phải thấy: 🚀 [C#] GỬI NGAY LẬP TỨC lên server (forceImmediate = true)
✅ Phải thấy: 📡 [Tracking] Đã gửi vị trí User user123 (...) lên server - Navigate: True (Force: True)
```

**Nếu KHÔNG thấy 3 dòng trên:**
→ App chưa build lại với code mới
→ Giải pháp: Rebuild app

**Nếu THẤY 3 dòng trên:**
→ App đã gửi thành công
→ Chuyển sang Bước 2

---

### **BƯỚC 2: Kiểm Tra Database Có Nhận Data Không?**

**Mở browser:**
```
http://localhost/vfg-api/check_tracking.php
```

**Xem bảng UserTracking:**
```
✅ Phải thấy: Username = user123
✅ Phải thấy: IsNavigating = 🧭 TRUE
✅ Phải thấy: IsActive = ✅ TRUE
✅ Phải thấy: User Online? = 🟢 Online
✅ Phải thấy: Kết Quả Đếm: 🧭 Đang Chỉ Đường: 1
```

**Nếu IsNavigating = ❌ FALSE:**
→ API không nhận được data hoặc không lưu đúng
→ Chuyển sang Bước 3

**Nếu User Online? = ⚪ Offline:**
→ LastActiveTime quá cũ (> 5 phút)
→ Chuyển sang Bước 4

**Nếu TẤT CẢ ĐÚNG nhưng Admin Dashboard vẫn = 0:**
→ Vấn đề ở Admin Dashboard
→ Chuyển sang Bước 5

---

### **BƯỚC 3: Kiểm Tra API Có Hoạt Động Không?**

**Test API trực tiếp:**
```
http://localhost/vfg-api/api.php?action=getTracking
```

**Xem response JSON:**
```json
{
  "success": true,
  "data": [
    {
      "UserId": 2,
      "IsNavigating": 1,  ← Phải là 1
      "IsActive": 1,      ← Phải là 1
      "LastUpdate": "2026-04-30 ..."
    }
  ]
}
```

**Nếu data = [] (rỗng):**
→ Không có tracking record
→ App chưa gửi hoặc API không lưu

**Nếu IsNavigating = 0:**
→ API nhận được nhưng lưu sai
→ Kiểm tra code API

---

### **BƯỚC 4: Kiểm Tra User Có Online Không?**

**Mở check_tracking.php, xem bảng Users:**
```
✅ Username: user123
✅ LastActiveTime: (phải là thời gian gần đây, < 5 phút)
✅ Online?: 🟢 Online
```

**Nếu LastActiveTime = NULL hoặc quá cũ:**
→ User chưa đăng nhập hoặc activity timer không chạy
→ Giải pháp: Đăng nhập lại

**Nếu IsLocked = 🔒 Locked:**
→ User bị khóa
→ Giải pháp: Mở khóa trong Admin Dashboard

---

### **BƯỚC 5: Kiểm Tra Admin Dashboard**

**Mở Console (F12):**
```
Console → Tìm log: 📊 [Tracking Stats]
```

**Xem output:**
```javascript
{
  totalMerged: 1,     // Có bao nhiêu tracking records
  online: 1,          // Có bao nhiêu users online
  navigating: 0,      // ← Số này phải > 0
  mergedData: [
    {
      UserId: 2,
      Username: "user123",
      IsNavigating: true,   // ← Phải là true
      IsActive: true,       // ← Phải là true
      isOnline: true        // ← Phải là true
    }
  ]
}
```

**Nếu navigating = 0 nhưng mergedData có IsNavigating = true:**
→ Logic đếm sai
→ Kiểm tra hàm updateTrackingStats()

**Nếu mergedData rỗng []:**
→ API không trả về data
→ Quay lại Bước 3

---

## 🔧 GIẢI PHÁP NHANH

### **Giải Pháp 1: Rebuild App**
```bash
cd VietnamFoodGuide
dotnet clean
dotnet build
```
Sau đó Run lại (F5)

### **Giải Pháp 2: Test Gửi Data Thủ Công**
```
http://localhost/vfg-api/test_tracking.php
```
→ Tạo test data
→ Refresh Admin Dashboard
→ Nếu thấy số tăng → App chưa gửi đúng
→ Nếu vẫn = 0 → Admin Dashboard có vấn đề

### **Giải Pháp 3: Đăng Nhập Lại**
```
1. Đóng app
2. Mở lại
3. Đăng nhập user123 / user123
4. Thử lại
```

### **Giải Pháp 4: Refresh Admin Dashboard**
```
1. Click nút "🔄 Làm Mới"
2. Hoặc F5 refresh trang
3. Xem Console log (F12)
```

---

## 📋 CHECKLIST DEBUG

Làm theo thứ tự:

- [ ] **Bước 1:** Xem Output window → Có 3 dòng log?
  - ❌ Không → Rebuild app
  - ✅ Có → Sang Bước 2

- [ ] **Bước 2:** Mở check_tracking.php → IsNavigating = TRUE?
  - ❌ FALSE → Sang Bước 3
  - ✅ TRUE → Sang Bước 4

- [ ] **Bước 3:** Mở API getTracking → IsNavigating = 1?
  - ❌ 0 hoặc rỗng → API có vấn đề
  - ✅ 1 → Sang Bước 4

- [ ] **Bước 4:** Xem LastActiveTime → < 5 phút?
  - ❌ Quá cũ → Đăng nhập lại
  - ✅ Gần đây → Sang Bước 5

- [ ] **Bước 5:** Xem Console log → navigating > 0?
  - ❌ = 0 → Admin Dashboard có vấn đề
  - ✅ > 0 → Refresh trang

---

## 🎯 CÂU HỎI QUAN TRỌNG

**Hãy trả lời các câu hỏi sau để tôi biết vấn đề ở đâu:**

1. **Output window có hiển thị 3 dòng log không?**
   - [ ] Có
   - [ ] Không
   - [ ] Không biết cách xem

2. **check_tracking.php hiển thị gì?**
   - [ ] IsNavigating = TRUE
   - [ ] IsNavigating = FALSE
   - [ ] Không có data
   - [ ] Lỗi kết nối

3. **User có online không? (LastActiveTime < 5 phút)**
   - [ ] Online (🟢)
   - [ ] Offline (⚪)
   - [ ] Không biết

4. **Console log (F12) hiển thị gì?**
   - [ ] navigating = 0
   - [ ] navigating > 0
   - [ ] Không có log
   - [ ] Lỗi

**Trả lời 4 câu hỏi trên để tôi biết chính xác vấn đề ở đâu!**

---

## 📞 GỬI CHO TÔI

Nếu vẫn không được, chụp màn hình:
1. Output window (sau khi bấm "Bắt đầu")
2. check_tracking.php (toàn bộ trang)
3. Console log (F12) trong Admin Dashboard
4. Admin Dashboard (phần Tracking)

Gửi 4 ảnh này cho tôi!
