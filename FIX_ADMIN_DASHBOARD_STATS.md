# ✅ FIX: ADMIN DASHBOARD STATS

## 🎯 YÊU CẦU:

- ✅ Giữ nguyên "Tổng Người Dùng" (tất cả users)
- ✅ Thêm card mới "Đang Online" (chỉ users đang đăng nhập)

---

## ✅ ĐÃ FIX:

### 1. Thêm card "Đang Online"
```html
<div class="stat-card">
    <div class="stat-icon success">
        <i class="fas fa-user-check"></i>
    </div>
    <div class="stat-info">
        <h3 id="onlineUsers">0</h3>
        <p>🟢 Đang Online</p>
    </div>
</div>
```

### 2. Giữ nguyên card "Tổng Người Dùng"
```html
<div class="stat-card">
    <div class="stat-icon info">
        <i class="fas fa-users"></i>
    </div>
    <div class="stat-info">
        <h3 id="totalUsers">0</h3>
        <p>Tổng Người Dùng</p>
    </div>
</div>
```

### 3. Cập nhật loadStats()
```javascript
async function loadStats() {
    // Load tổng users từ API stats
    const res = await fetch(`${API_BASE}?action=stats`);
    const data = await res.json();
    document.getElementById('totalUsers').textContent = data.totalUsers || 0;
    
    // Load online users từ API getOnlineCount
    const onlineRes = await fetch(`${API_BASE}?action=getOnlineCount`);
    const onlineData = await onlineRes.json();
    document.getElementById('onlineUsers').textContent = onlineData.count || 0;
}
```

---

## 📊 KẾT QUẢ:

Bây giờ admin dashboard có **5 cards**:

1. **Tổng Quán Ăn** (primary - tím)
2. **Tổng Người Dùng** (info - xanh dương) ← Tất cả users
3. **🟢 Đang Online** (success - xanh lá) ← Chỉ users đang đăng nhập
4. **Đánh Giá TB** (warning - vàng)
5. **Yêu Thích** (danger - đỏ)

---

## 🧪 TEST:

```
1. Mở admin dashboard
2. Xem stats:
   - Tổng Người Dùng: 8 (tất cả users trong DB)
   - 🟢 Đang Online: 0 (chưa ai đăng nhập)
3. Đăng nhập app (admin/admin123)
4. Refresh admin dashboard
5. Xem stats:
   - Tổng Người Dùng: 8 (không đổi)
   - 🟢 Đang Online: 1 (admin vừa đăng nhập)
6. Đợi 6 phút (không dùng app)
7. Refresh admin dashboard
8. Xem stats:
   - Tổng Người Dùng: 8 (không đổi)
   - 🟢 Đang Online: 0 (admin không active)
```

---

## 📋 FILES ĐÃ SỬA:

1. ✅ `admin_dashboard.html`
   - Thêm card "Đang Online" với id="onlineUsers"
   - Đổi card "Tổng Người Dùng" về id="totalUsers"
   - Cập nhật loadStats() load cả 2 giá trị
   - Thêm CSS class "info" cho icon xanh dương

---

## 🎯 LOGIC:

### Tổng Người Dùng:
```sql
SELECT COUNT(*) FROM Users
```
→ Tất cả users trong database (8 users)

### Đang Online:
```sql
SELECT COUNT(*) FROM Users 
WHERE LastActiveTime > DATE_SUB(NOW(), INTERVAL 5 MINUTE)
```
→ Chỉ users có LastActiveTime trong 5 phút gần đây

---

**Thời gian fix:** 2 phút  
**Độ khó:** ⭐☆☆☆☆ (Rất dễ)

✅ **Hoàn tất! Bây giờ admin dashboard hiển thị đúng cả 2 số liệu!**
