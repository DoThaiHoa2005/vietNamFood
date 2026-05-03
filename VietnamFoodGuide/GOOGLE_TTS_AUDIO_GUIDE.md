# 🎵 Tạo Audio MP3 Bằng Google Translate TTS

## ✅ Ưu Điểm

- ✅ **KHÔNG CẦN** cài giọng đọc Windows
- ✅ **KHÔNG CẦN** restart máy
- ✅ **KHÔNG CẦN** download speech pack
- ✅ Giọng đọc tự nhiên từ Google
- ✅ Hỗ trợ 3 ngôn ngữ: VI, EN, CN
- ✅ Hoàn toàn miễn phí

## 🚀 Cách Sử Dụng (3 Bước)

### Bước 1: Chạy App

```powershell
# Từ Visual Studio: Nhấn F5
# HOẶC
dotnet run --project VietnamFoodGuide/VietnamFoodGuide.csproj
```

### Bước 2: Click Nút 🎵

1. Mở app
2. Nhìn lên **status bar** (thanh màu xanh ở trên cùng)
3. Click nút **🎵** (bên cạnh language selector)

### Bước 3: Tạo Audio

1. Window "Tạo File Audio" sẽ mở
2. Đọc thông tin: "🌐 Sử dụng: Google Translate TTS (không cần cài giọng đọc!)"
3. Click **"🎵 Tạo Audio"**
4. Đợi ~3-10 phút (tùy số lượng quán ăn)
5. Khi xong, click **"Đóng"**

## 📊 Quá Trình Tạo Audio

```
🔄 Đang tải danh sách quán ăn...
📝 Tìm thấy 12 quán ăn

[1/12] Đang tạo audio cho: Alo Quán
🇻🇳 Tạo VI cho Alo Quán...
📥 Downloading chunk: Quán ăn ngon...
✅ Đã tạo MP3: 1_Alo_Quán_VI.mp3 (45 KB)

🇺🇸 Tạo EN cho Alo Quán...
📥 Downloading chunk: Delicious restaurant...
✅ Đã tạo MP3: 1_Alo_Quán_EN.mp3 (38 KB)

🇨🇳 Tạo CN cho Alo Quán...
📥 Downloading chunk: 美味的餐厅...
✅ Đã tạo MP3: 1_Alo_Quán_CN.mp3 (42 KB)

[2/12] Đang tạo audio cho: Ốc Đào 2
...

✅ Hoàn thành! Thành công: 12, Thất bại: 0
```

## 📁 Kết Quả

File audio sẽ được lưu vào:
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

## ⏱️ Thời Gian Ước Tính

| Số quán ăn | Thời gian |
|------------|-----------|
| 10 quán    | ~2 phút   |
| 50 quán    | ~10 phút  |
| 100 quán   | ~20 phút  |
| 200 quán   | ~40 phút  |

**Lưu ý:** Thời gian lâu hơn System.Speech vì phải download từ Google, nhưng không cần cài đặt gì!

## 🔧 Cách Hoạt Động

1. **Chia text thành chunks:** Google giới hạn 200 ký tự/request
2. **Download audio từ Google:** Mỗi chunk tải về dưới dạng MP3
3. **Gộp chunks lại:** Tạo thành 1 file MP3 hoàn chỉnh
4. **Lưu vào Assets/Audio:** File có tên quán để dễ nhận biết
5. **Cập nhật database:** AudioUrl_VI, AudioUrl_EN, AudioUrl_CN

## ⚠️ Lưu Ý

### 1. Cần Kết Nối Internet

- ✅ Phải có internet để download audio từ Google
- ❌ Không thể tạo audio offline

### 2. Delay Giữa Các Request

- App tự động delay 300ms giữa các chunk
- Delay 500ms giữa các quán ăn
- Tránh bị Google block do request quá nhanh

### 3. XAMPP Phải Chạy

- MySQL service phải active
- Database phải có dữ liệu quán ăn

## 🎉 Sau Khi Hoàn Thành

### 1. Kiểm Tra File

```powershell
# Xem danh sách file
Get-ChildItem "VietnamFoodGuide/Assets/Audio" -Filter "*.mp3"

# Đếm số file
(Get-ChildItem "VietnamFoodGuide/Assets/Audio" -Filter "*.mp3").Count
# Kết quả: 36 (12 quán × 3 ngôn ngữ)

# Xem kích thước
Get-ChildItem "VietnamFoodGuide/Assets/Audio" -Filter "*.mp3" | 
    ForEach-Object { "$($_.Name): $([math]::Round($_.Length/1KB, 2)) KB" }
```

### 2. Test Phát Audio

1. Mở app
2. Click vào quán ăn bất kỳ
3. Click **"🗺 Xem bản đồ"**
4. Di chuyển vào vùng quán (trong bán kính)
5. Audio sẽ tự động phát! 🔊

### 3. Database Đã Cập Nhật

```sql
SELECT Id, Name, AudioUrl_VI, AudioUrl_EN, AudioUrl_CN 
FROM Foods 
LIMIT 3;

-- Kết quả:
-- 1 | Alo Quán | /Assets/Audio/1_Alo_Quán_VI.mp3 | /Assets/Audio/1_Alo_Quán_EN.mp3 | /Assets/Audio/1_Alo_Quán_CN.mp3
```

## 🐛 Troubleshooting

### Lỗi: "Không kết nối được database"
→ Kiểm tra XAMPP đang chạy

### Lỗi: "Failed to download audio"
→ Kiểm tra kết nối internet

### Lỗi: "Access denied"
→ Kiểm tra quyền ghi vào thư mục Assets/Audio

### File audio bị lỗi/không phát được
→ Có thể do text quá dài, Google không xử lý được. Kiểm tra Description trong database.

## 💡 So Sánh Với System.Speech

| Tiêu chí | Google TTS | System.Speech |
|----------|------------|---------------|
| Cài đặt | ✅ Không cần | ❌ Cần cài speech pack |
| Internet | ❌ Cần | ✅ Không cần |
| Giọng đọc | ✅ Tự nhiên | ⚠️ Máy móc |
| Tốc độ | ⚠️ Chậm hơn | ✅ Nhanh |
| Miễn phí | ✅ Có | ✅ Có |

## 🎯 Khuyến Nghị

- ✅ **Dùng Google TTS** nếu không muốn cài đặt gì
- ✅ **Dùng System.Speech** nếu muốn tạo nhanh và offline

## 📖 Tài Liệu Liên Quan

- `HOW_TO_GENERATE_AUDIO.md` - Hướng dẫn chung
- `DEBUG_AUDIO_GENERATION.md` - Debug guide
- `AUDIO_GENERATION_GUIDE.md` - Technical guide

---

**Chúc bạn thành công! 🚀**
