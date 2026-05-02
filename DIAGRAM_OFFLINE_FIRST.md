# 📊 DIAGRAM: KIẾN TRÚC OFFLINE-FIRST

## 🏗️ TỔNG QUAN KIẾN TRÚC

```
┌─────────────────────────────────────────────────────────────────┐
│                     VIETNAM FOOD GUIDE APP                      │
│                    (WPF - Windows Desktop)                      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │
        ┌─────────────────────┴─────────────────────┐
        │                                           │
        ▼                                           ▼
┌───────────────────┐                    ┌──────────────────────┐
│   OFFLINE MODE    │                    │    ONLINE MODE       │
│   (SQLite Local)  │                    │  (XAMPP + MySQL)     │
└───────────────────┘                    └──────────────────────┘
        │                                           │
        │                                           │
        ▼                                           ▼
┌───────────────────┐                    ┌──────────────────────┐
│  foods.db         │◄───────────────────┤  MySQL: Foods        │
│  favorites.db     │      Auto Sync     │  MySQL: Favorites    │
│  users.db         │◄───────────────────┤  MySQL: Users        │
│  qr_scans.db      │                    │  MySQL: Sessions     │
└───────────────────┘                    └──────────────────────┘
```

---

## 🔄 LUỒNG DỮ LIỆU CHI TIẾT

### 1. User Mở App

```
┌──────────┐
│   USER   │
│ Mở App   │
└────┬─────┘
     │
     ▼
┌─────────────────────────────────────────┐
│         ApiFoodService                  │
│  LoadFoodsAsync()                       │
└─────────────────────────────────────────┘
     │
     ├─────────────────────────────────────┐
     │                                     │
     ▼                                     ▼
┌──────────────────┐              ┌──────────────────┐
│ Try XAMPP API    │              │ Load SQLite      │
│ (Background)     │              │ (Instant)        │
└────┬─────────────┘              └────┬─────────────┘
     │                                  │
     │                                  ▼
     │                            ┌──────────────────┐
     │                            │ Display Data     │
     │                            │ (< 50ms)         │
     │                            └──────────────────┘
     │
     ├─── Success ───┐
     │               │
     ▼               ▼
┌──────────┐   ┌──────────────┐
│ Sync to  │   │ Update UI    │
│ SQLite   │   │ (if changed) │
└──────────┘   └──────────────┘
     │
     └─── Fail ─────┐
                    │
                    ▼
              ┌──────────────┐
              │ Use SQLite   │
              │ (Fallback)   │
              └──────────────┘
```

---

## 🎯 AUTO-SWITCHING MECHANISM

### Scenario 1: Online → Offline

```
┌─────────────────────────────────────────────────────────┐
│  User đang dùng app với mạng                            │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  App đang load từ XAMPP API                             │
│  ✅ Kết nối thành công                                  │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  User TẮT MẠNG (giữa chừng)                             │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  Next API call → HttpRequestException                   │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  ApiFoodService catch exception                         │
│  → Tự động fallback: _sqliteService.LoadFoods()        │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  App tiếp tục hoạt động bình thường                     │
│  ✅ Không crash, không lỗi                              │
└─────────────────────────────────────────────────────────┘
```

### Scenario 2: Offline → Online

```
┌─────────────────────────────────────────────────────────┐
│  User đang dùng app không có mạng                       │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  App đang load từ SQLite                                │
│  ✅ Hoạt động bình thường                               │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  User thêm vào Favorites                                │
│  → Lưu vào favorites.db (SyncedToServer = 0)           │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  User BẬT MẠNG trở lại                                  │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  BackgroundSyncService (Timer: 60s)                     │
│  → Phát hiện có mạng                                    │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  GetPendingSync() → Tìm favorites chưa sync             │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  Sync lên XAMPP API                                     │
│  → POST /api.php?action=addFavorite                     │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  MarkAsSynced() → Update SyncedToServer = 1             │
│  ✅ Sync hoàn tất                                       │
└─────────────────────────────────────────────────────────┘
```

---

## 🗄️ DATABASE SCHEMA

### SQLite (Local)

```
┌─────────────────────────────────────────────────────────┐
│                      foods.db                           │
├─────────────────────────────────────────────────────────┤
│  Foods                                                  │
│  ├── Id (INTEGER PRIMARY KEY)                          │
│  ├── Name (TEXT)                                        │
│  ├── City (TEXT)                                        │
│  ├── Category (TEXT)                                    │
│  ├── DescriptionVI (TEXT)                              │
│  ├── DescriptionEN (TEXT)                              │
│  ├── DescriptionCN (TEXT)                              │
│  ├── Latitude (REAL)                                    │
│  ├── Longitude (REAL)                                   │
│  ├── Rating (REAL)                                      │
│  ├── Radius (REAL)                                      │
│  ├── Priority (INTEGER)                                 │
│  ├── AudioUrl (TEXT)                                    │
│  ├── NarrationScript (TEXT)                            │
│  └── CooldownMinutes (INTEGER)                         │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                   favorites.db                          │
├─────────────────────────────────────────────────────────┤
│  Favorites                                              │
│  ├── Id (INTEGER PRIMARY KEY)                          │
│  ├── UserId (INTEGER)                                   │
│  ├── FoodId (INTEGER)                                   │
│  ├── FoodName (TEXT)                                    │
│  ├── CreatedDate (TEXT)                                 │
│  └── SyncedToServer (INTEGER) ← 0: chưa sync, 1: đã sync│
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                     users.db                            │
├─────────────────────────────────────────────────────────┤
│  Users                                                  │
│  ├── Id (INTEGER PRIMARY KEY)                          │
│  ├── Username (TEXT UNIQUE)                            │
│  ├── PasswordHash (TEXT)                               │
│  ├── Role (TEXT)                                        │
│  └── CreatedDate (TEXT)                                 │
└─────────────────────────────────────────────────────────┘
```

### MySQL (Server)

```
┌─────────────────────────────────────────────────────────┐
│              VietnamFoodGuide (MySQL)                   │
├─────────────────────────────────────────────────────────┤
│  Foods                                                  │
│  ├── Id (INT PRIMARY KEY AUTO_INCREMENT)               │
│  ├── Name (VARCHAR)                                     │
│  ├── City (VARCHAR)                                     │
│  ├── Category (VARCHAR)                                 │
│  ├── Description_VI (TEXT)                             │
│  ├── Description_EN (TEXT)                             │
│  ├── Description_CN (TEXT)                             │
│  ├── Latitude (DOUBLE)                                  │
│  ├── Longitude (DOUBLE)                                 │
│  ├── Rating (DOUBLE)                                    │
│  └── ImagePath (VARCHAR)                               │
├─────────────────────────────────────────────────────────┤
│  Favorites                                              │
│  ├── Id (INT PRIMARY KEY AUTO_INCREMENT)               │
│  ├── UserId (INT FOREIGN KEY → Users.Id)               │
│  ├── FoodId (INT FOREIGN KEY → Foods.Id)               │
│  └── CreatedDate (DATETIME)                            │
├─────────────────────────────────────────────────────────┤
│  Users                                                  │
│  ├── Id (INT PRIMARY KEY AUTO_INCREMENT)               │
│  ├── Username (VARCHAR UNIQUE)                         │
│  ├── PasswordHash (VARCHAR)                            │
│  ├── Role (VARCHAR)                                     │
│  ├── LastActiveTime (DATETIME)                         │
│  └── CreatedDate (DATETIME)                            │
└─────────────────────────────────────────────────────────┘
```

---

## 🔄 BACKGROUND SYNC FLOW

```
┌─────────────────────────────────────────────────────────┐
│         BackgroundSyncService (Timer: 60s)              │
└─────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  Check Network: NetworkService.IsNetworkAvailable()    │
└─────────────────────────────────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
        ▼                       ▼
    ❌ No Network          ✅ Has Network
        │                       │
        ▼                       ▼
    Skip Sync          ┌────────────────┐
                       │  Sync Foods    │
                       │  from API      │
                       └────────┬───────┘
                                │
                                ▼
                       ┌────────────────┐
                       │ Sync Favorites │
                       │ to Server      │
                       └────────┬───────┘
                                │
                                ▼
                       ┌────────────────┐
                       │  Sync QR Scans │
                       │  to Server     │
                       └────────┬───────┘
                                │
                                ▼
                       ┌────────────────┐
                       │ Sync Complete  │
                       │ ✅ Success     │
                       └────────────────┘
```

---

## 📱 UI FLOW

```
┌──────────────┐
│ LoginWindow  │
└──────┬───────┘
       │
       ├─── Offline Login ───┐
       │                     │
       │                     ▼
       │            ┌─────────────────┐
       │            │ SQLiteUserService│
       │            │ .Login()        │
       │            └────────┬────────┘
       │                     │
       │                     ▼
       │            ┌─────────────────┐
       │            │ ✅ Success      │
       │            │ → MainWindow    │
       │            └─────────────────┘
       │
       └─── Online Login ────┐
                             │
                             ▼
                    ┌─────────────────┐
                    │ ApiAuthService  │
                    │ .LoginAsync()   │
                    └────────┬────────┘
                             │
                 ┌───────────┴───────────┐
                 │                       │
                 ▼                       ▼
         ✅ Success              ❌ Fail
                 │                       │
                 ▼                       ▼
        ┌─────────────────┐    ┌─────────────────┐
        │ Save Session    │    │ Fallback:       │
        │ → MainWindow    │    │ SQLiteUserService│
        └─────────────────┘    └─────────────────┘
```

---

## 🎯 DECISION TREE

```
                    User Action
                         │
                         ▼
              ┌──────────────────┐
              │ Need Data?       │
              └──────────────────┘
                         │
        ┌────────────────┴────────────────┐
        │                                 │
        ▼                                 ▼
┌───────────────┐              ┌──────────────────┐
│ Load from     │              │ Write Data?      │
│ SQLite        │              └──────────────────┘
│ (Instant)     │                       │
└───────┬───────┘          ┌────────────┴────────────┐
        │                  │                         │
        ▼                  ▼                         ▼
┌───────────────┐  ┌──────────────┐      ┌──────────────────┐
│ Display Data  │  │ Save to      │      │ Check Network    │
└───────┬───────┘  │ SQLite       │      └──────────────────┘
        │          │ (Instant)    │               │
        ▼          └──────┬───────┘    ┌──────────┴──────────┐
┌───────────────┐         │            │                     │
│ Background:   │         ▼            ▼                     ▼
│ Try Sync      │  ┌──────────────┐  ✅ Online          ❌ Offline
│ from API      │  │ Mark as      │   │                     │
└───────────────┘  │ Pending Sync │   ▼                     ▼
                   └──────────────┘  Sync to Server    Wait for Network
```

---

**📊 DIAGRAM HOÀN TẤT - OFFLINE-FIRST ARCHITECTURE**
