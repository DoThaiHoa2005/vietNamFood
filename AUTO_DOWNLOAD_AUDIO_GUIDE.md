# 🎵 Tự Động Tải Audio Khi Đăng Nhập

## ✅ Tính Năng Mới

Khi đăng nhập vào app, hệ thống sẽ **TỰ ĐỘNG**:
1. ✅ Kiểm tra file audio trên server
2. ✅ Tải xuống các file audio mới (chưa có local)
3. ✅ Bỏ qua các file đã tồn tại (không tải lại)
4. ✅ Lưu vào `Assets/Audio/` để dùng offline

## 🚀 Cách Hoạt Động

### 1. Đăng Nhập

```
User nhập username/password → Click "Đăng Nhập"
```

### 2. Tự Động Download Audio (Background)

```
🔄 Đang tải danh sách quán ăn...
📝 Tìm thấy 36 file audio

[1/36] Alo Quán (VI)
📥 Downloading: http://localhost/Assets/Audio/1_Alo_Quán_VI.mp3
✅ Downloaded: 1_Alo_Quán_VI.mp3 (45 KB)

[2/36] Alo Quán (EN)
⏭️ Skipped: 1_Alo_Quán_EN.mp3 (already exists)

[3/36] Alo Quán (CN)
📥 Downloading: http://localhost/Assets/Audio/1_Alo_Quán_CN.mp3
✅ Downloaded: 1_Alo_Quán_CN.mp3 (42 KB)

...

✅ Hoàn thành! Downloaded: 24, Skipped: 12
```

### 3. Thông Báo

Nếu có file mới được tải:
```
┌─────────────────────────────────────┐
│  Audio Downloaded                   │
├─────────────────────────────────────┤
│  Đã tải xuống 24 file audio mới!    │
│                                     │
│              [ OK ]                 │
└─────────────────────────────────────┘
```

## 📁 Cấu Trúc File

```
VietnamFoodGuide/Assets/Audio/
├── 1_Alo_Quán_VI.mp3
├── 1_Alo_Quán_EN.mp3
├── 1_Alo_Quán_CN.mp3
├── 2_Ốc_Đào_2_VI.mp3
├── 2_Ốc_Đào_2_EN.mp3
├── 2_Ốc_Đào_2_CN.mp3
└── ...
```

## 🔄 Sync Logic

### Lần Đầu Đăng Nhập
- Tải **TẤT CẢ** file audio từ server
- Thời gian: ~2-5 phút (tùy số lượng)

### Các Lần Sau
- Chỉ tải **FILE MỚI** (chưa có local)
- Bỏ qua file đã tồn tại
- Thời gian: ~10-30 giây (nếu có file mới)

### Khi Có Quán Mới
- Admin thêm quán mới trên server
- Admin upload file audio lên server
- User đăng nhập lại → Tự động tải file audio của quán mới

## 📊 Ví Dụ Thực Tế

### Scenario 1: Lần đầu đăng nhập

```
Database: 12 quán × 3 ngôn ngữ = 36 file
Local: 0 file

→ Download: 36 file
→ Skipped: 0 file
→ Thời gian: ~3 phút
```

### Scenario 2: Đăng nhập lần 2

```
Database: 12 quán × 3 ngôn ngữ = 36 file
Local: 36 file (đã có từ lần trước)

→ Download: 0 file
→ Skipped: 36 file
→ Thời gian: ~5 giây
```

### Scenario 3: Admin thêm 2 quán mới

```
Database: 14 quán × 3 ngôn ngữ = 42 file
Local: 36 file (12 quán cũ)

→ Download: 6 file (2 quán mới × 3 ngôn ngữ)
→ Skipped: 36 file
→ Thời gian: ~30 giây
```

## ⚙️ Cấu Hình Server

### 1. Upload File Audio Lên Server

```
xampp/htdocs/VietnamFoodGuide/Assets/Audio/
├── 1_Alo_Quán_VI.mp3
├── 1_Alo_Quán_EN.mp3
├── 1_Alo_Quán_CN.mp3
└── ...
```

### 2. Cập Nhật Database

```sql
UPDATE Foods SET
    AudioUrl_VI = '/Assets/Audio/1_Alo_Quán_VI.mp3',
    AudioUrl_EN = '/Assets/Audio/1_Alo_Quán_EN.mp3',
    AudioUrl_CN = '/Assets/Audio/1_Alo_Quán_CN.mp3'
WHERE Id = 1;
```

### 3. User Đăng Nhập

App tự động tải file từ:
```
http://localhost/VietnamFoodGuide/Assets/Audio/1_Alo_Quán_VI.mp3
```

## 🐛 Troubleshooting

### Không Tải Được File

**Kiểm tra:**
1. ✅ XAMPP đang chạy
2. ✅ File tồn tại trên server: `http://localhost/VietnamFoodGuide/Assets/Audio/`
3. ✅ Database có AudioUrl: `SELECT AudioUrl_VI FROM Foods WHERE Id = 1;`
4. ✅ Kết nối internet OK

### File Bị Lỗi/Không Phát Được

**Nguyên nhân:** File trên server bị lỗi

**Giải pháp:**
1. Xóa file local: `VietnamFoodGuide/Assets/Audio/`
2. Upload file mới lên server
3. Đăng nhập lại → Tải lại

### Muốn Tải Lại Tất Cả File

```powershell
# Xóa tất cả file local
Remove-Item "VietnamFoodGuide/Assets/Audio/*.mp3" -Force

# Đăng nhập lại → Tải lại tất cả
```

## 💡 Tips

### 1. Tạo File Audio Trên Server

Có 2 cách:

**Cách 1: Dùng Google TTS (Khuyến nghị)**
- Chạy script Python/Node.js trên server
- Tự động tạo MP3 từ Description
- Upload lên `Assets/Audio/`

**Cách 2: Tạo Thủ Công**
- Dùng tool TTS (Google Cloud TTS, Amazon Polly, etc.)
- Tạo file MP3
- Upload lên server

### 2. Optimize Download Speed

- Nén file MP3 (128kbps đủ rồi)
- Dùng CDN nếu có nhiều user
- Enable gzip compression trên Apache

### 3. Monitoring

Xem log trong Output window (Visual Studio):
```
🎵 [Login] Bắt đầu download audio files...
🎵 [AudioDownload] [1/36] Alo Quán (VI)
✅ [AudioDownload] Downloaded: 1_Alo_Quán_VI.mp3 (45 KB)
...
✅ [AudioDownload] Hoàn thành! Downloaded: 24, Skipped: 12
```

## 📖 So Sánh Với Cách Cũ

| Tiêu chí | Cách Cũ (Tạo Local) | Cách Mới (Download) |
|----------|---------------------|---------------------|
| Cài đặt | ❌ Cần giọng đọc Windows | ✅ Không cần |
| Tốc độ | ✅ Nhanh (2-5 phút) | ⚠️ Chậm hơn (3-10 phút lần đầu) |
| Chất lượng | ⚠️ Máy móc | ✅ Tự nhiên (từ server) |
| Sync | ❌ Không tự động | ✅ Tự động khi login |
| Offline | ✅ Có | ✅ Có (sau khi download) |
| Quản lý | ❌ Mỗi user tự tạo | ✅ Tập trung trên server |

## 🎯 Khuyến Nghị

- ✅ **Dùng cách mới** (Auto Download) cho production
- ✅ Admin tạo audio 1 lần trên server
- ✅ Tất cả user tự động download khi login
- ✅ Dễ quản lý, dễ cập nhật

## 📝 Checklist

- [ ] Upload file audio lên server (`xampp/htdocs/VietnamFoodGuide/Assets/Audio/`)
- [ ] Cập nhật AudioUrl trong database
- [ ] Test download: Đăng nhập → Xem Output window
- [ ] Kiểm tra file local: `VietnamFoodGuide/Assets/Audio/`
- [ ] Test phát audio: Vào MapWindow → Di chuyển vào vùng quán

---

**Hoàn thành! Bây giờ audio sẽ tự động download khi đăng nhập! 🎉**
