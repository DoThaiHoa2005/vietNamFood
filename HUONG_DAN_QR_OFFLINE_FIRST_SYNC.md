# ✅ QR SCANNER - OFFLINE-FIRST VỚI AUTO-SYNC

## 🎯 Tính Năng Mới

### 1. **Offline-First Architecture**
- ✅ Lưu QR scan vào SQLite local ngay lập tức
- ✅ Không cần internet để quét QR
- ✅ App hoạt động hoàn toàn offline

### 2. **Auto-Sync to Server**
- ✅ Tự động sync lên XAMPP server khi có mạng
- ✅ Background sync mỗi 60 giây
- ✅ Retry tự động nếu sync fail

### 3. **Success Notification**
- ✅ Hiển thị "Quét thành công!" khi quét xong
- ✅ Tự động quay về MainWindow sau 2 giây
- ✅ Smooth animation

## 🔄 Luồng Hoạt Động

### Khi Quét QR Code:

```
User quét QR
    ↓
1. Lưu vào SQLite local (offline-first)
    ↓
2. Kiểm tra kết nối mạng
    ↓
3a. Có mạng → Sync ngay lên server
3b. Không mạng → Đánh dấu "chưa sync"
    ↓
4. Hiển thị "Quét thành công!"
    ↓
5. Đợi 2 giây
    ↓
6. Quay về MainWindow
```

### Background Sync (Tự động):

```
App khởi động
    ↓
Start BackgroundSyncService
    ↓
Mỗi 60 giây:
    ↓
1. Kiểm tra kết nối mạng
    ↓
2. Nếu có mạng:
   - Lấy danh sách QR chưa sync
   - Sync từng cái lên server
   - Đánh dấu "đã sync"
    ↓
3. Nếu không mạng:
   - Skip, thử lại sau 60 giây
```

## 📊 Database Schema

### SQLite Local Database: `qr_scans.db`

```sql
CREATE TABLE qr_scans (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    device_id TEXT NOT NULL,
    qr_code TEXT NOT NULL,
    device_name TEXT,
    os_version TEXT,
    scanned_at TEXT NOT NULL,
    synced INTEGER DEFAULT 0,        -- 0 = chưa sync, 1 = đã sync
    synced_at TEXT,                  -- Thời gian sync
    created_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_device_id ON qr_scans(device_id);
CREATE INDEX idx_synced ON qr_scans(synced);
```

## 🧪 Test Scenarios

### Test 1: Quét QR Khi Có Mạng
```
1. Bật WiFi
2. Mở app → Đăng nhập
3. Quét QR code
Expected:
- ✅ Lưu vào SQLite local
- ✅ Sync ngay lên server
- ✅ Hiển thị "Quét thành công!"
- ✅ Quay về MainWindow sau 2 giây
```

### Test 2: Quét QR Khi Không Có Mạng
```
1. Tắt WiFi
2. Mở app → Đăng nhập
3. Quét QR code
Expected:
- ✅ Lưu vào SQLite local
- ⚠️ Không sync được (đánh dấu chưa sync)
- ✅ Hiển thị "Quét thành công!"
- ✅ Quay về MainWindow sau 2 giây
```

### Test 3: Auto-Sync Khi Có Mạng Trở Lại
```
1. Quét QR khi offline (Test 2)
2. Bật WiFi trở lại
3. Đợi 60 giây (hoặc restart app)
Expected:
- ✅ Background sync tự động chạy
- ✅ Sync QR scan lên server
- ✅ Đánh dấu "đã sync"
```

### Test 4: Quét Nhiều QR Khi Offline
```
1. Tắt WiFi
2. Quét 5 QR codes khác nhau
3. Bật WiFi trở lại
4. Đợi 60 giây
Expected:
- ✅ Tất cả 5 QR được sync lên server
- ✅ Đánh dấu tất cả "đã sync"
```

## 📂 Files Mới

### 1. **VietnamFoodGuide/Services/QRScanService.cs**
```csharp
// Service quản lý QR scan với offline-first
public class QRScanService
{
    // Lưu QR scan vào local database
    public bool SaveQRScanLocal(...)
    
    // Kiểm tra device đã quét chưa
    public bool HasScanned(string deviceId)
    
    // Lấy danh sách QR chưa sync
    public List<QRScanRecord> GetUnsyncedScans()
    
    // Sync lên server
    public async Task<int> SyncToServerAsync()
    
    // Đánh dấu đã sync
    public bool MarkAsSynced(int scanId)
}
```

### 2. **VietnamFoodGuide/Services/BackgroundSyncService.cs**
```csharp
// Service tự động sync background
public class BackgroundSyncService
{
    // Singleton instance
    public static BackgroundSyncService Instance { get; }
    
    // Start background sync (mỗi 60 giây)
    public void Start()
    
    // Stop background sync
    public void Stop()
    
    // Sync ngay lập tức
    public async Task SyncNowAsync()
    
    // Lấy sync status
    public SyncStatus GetSyncStatus()
}
```

### 3. **VietnamFoodGuide/Views/QRScannerWindow.xaml.cs** (CẬP NHẬT)
```csharp
// Cập nhật SaveQRScan để dùng QRScanService
private async Task SaveQRScan(string qrCode)
{
    // 1. Save to local SQLite
    var qrScanService = new QRScanService();
    qrScanService.SaveQRScanLocal(...);
    
    // 2. Try to sync if online
    if (isOnline) {
        await qrScanService.SyncToServerAsync();
    }
    
    // 3. Show success and proceed
    await ShowSuccessAndProceed();
}
```

### 4. **VietnamFoodGuide/App.xaml.cs** (CẬP NHẬT)
```csharp
// Start background sync khi app khởi động
protected override void OnStartup(StartupEventArgs e)
{
    BackgroundSyncService.Instance.Start();
    // ...
}

// Stop background sync khi app tắt
protected override void OnExit(ExitEventArgs e)
{
    BackgroundSyncService.Instance.Stop();
    // ...
}
```

## 🔍 Debug Log

### Log Mẫu (Có Mạng):
```
[QR] Scanned: VFG-TEST-2024
✅ [QR] Saved to local database
[QR] Online - Syncing to server...
✅ [QR] Synced 1 scans to server
[BackgroundSync] Online - Starting sync...
[BackgroundSync] Sync complete
```

### Log Mẫu (Không Mạng):
```
[QR] Scanned: VFG-TEST-2024
✅ [QR] Saved to local database
⚠️ [QR] Offline - Will sync later
[BackgroundSync] Offline - Skip sync
```

### Log Mẫu (Có Mạng Trở Lại):
```
[BackgroundSync] Online - Starting sync...
[QRScanService] Found 3 unsynced scans
✅ [QRScanService] Synced scan 1: VFG-TEST-2024
✅ [QRScanService] Synced scan 2: VFG-PROMO-50OFF
✅ [QRScanService] Synced scan 3: VFG-USER-123
✅ [BackgroundSync] Synced 3 QR scans
[BackgroundSync] Sync complete
```

## 💡 Lợi Ích

### 1. **User Experience**
- ✅ Không cần internet để quét QR
- ✅ Quét nhanh, không bị lag
- ✅ Thông báo rõ ràng
- ✅ Tự động quay về trang chủ

### 2. **Data Integrity**
- ✅ Không mất data khi offline
- ✅ Auto-sync khi có mạng
- ✅ Retry tự động nếu fail
- ✅ Track sync status

### 3. **Performance**
- ✅ SQLite nhanh hơn API call
- ✅ Background sync không block UI
- ✅ Batch sync nhiều records cùng lúc

### 4. **Reliability**
- ✅ Hoạt động offline 100%
- ✅ Sync tự động, không cần user action
- ✅ Không bị mất data

## 🚀 Cách Test

### Bước 1: Build & Run
```bash
# Build project
dotnet build VietnamFoodGuide/VietnamFoodGuide.csproj

# Hoặc bấm F5 trong Visual Studio
```

### Bước 2: Test Offline
```
1. Tắt WiFi
2. Mở app → Đăng nhập
3. Quét QR code
4. Xem log: "Saved to local database"
5. Xem log: "Offline - Will sync later"
6. ✅ Thấy "Quét thành công!"
7. ✅ Tự động quay về MainWindow
```

### Bước 3: Test Auto-Sync
```
1. Bật WiFi trở lại
2. Đợi 60 giây (hoặc restart app)
3. Xem log: "Syncing to server..."
4. Xem log: "Synced X scans"
5. ✅ Check database server: có data mới
```

### Bước 4: Kiểm Tra Database

**SQLite Local:**
```bash
# Mở database
sqlite3 VietnamFoodGuide/bin/Debug/net48/Data/qr_scans.db

# Xem tất cả scans
SELECT * FROM qr_scans;

# Xem scans chưa sync
SELECT * FROM qr_scans WHERE synced = 0;

# Xem scans đã sync
SELECT * FROM qr_scans WHERE synced = 1;
```

**MySQL Server:**
```sql
-- Xem tất cả QR scans
SELECT * FROM qr_scans ORDER BY scanned_at DESC;

-- Xem theo device
SELECT * FROM qr_scans WHERE device_id = 'YOUR_DEVICE_ID';
```

## 🎯 Kết Luận

✅ **Đã implement hoàn chỉnh Offline-First với Auto-Sync!**

**Tính năng chính**:
1. ✅ Lưu QR scan vào SQLite local (offline-first)
2. ✅ Auto-sync lên server khi có mạng
3. ✅ Background sync mỗi 60 giây
4. ✅ Hiển thị "Quét thành công!" và quay về MainWindow
5. ✅ Track sync status (synced/unsynced)
6. ✅ Retry tự động nếu sync fail

**User flow**:
- Quét QR → Lưu local → Sync (nếu có mạng) → Thông báo thành công → Quay về trang chủ

**Không cần internet để quét QR!** 🎉

---

**Tác Giả**: Kiro AI Assistant  
**Ngày**: May 2, 2026  
**Version**: 4.0 - QR Offline-First với Auto-Sync
