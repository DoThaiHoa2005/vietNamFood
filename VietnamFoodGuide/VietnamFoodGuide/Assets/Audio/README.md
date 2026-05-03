# 🎵 Audio Files - Thuyết Minh Tự Động

## 📁 Cấu trúc thư mục

```
Assets/Audio/
├── 1_VI.mp3    ← Alo Quán (Tiếng Việt)
├── 1_EN.mp3    ← Alo Quán (English)
├── 1_CN.mp3    ← Alo Quán (中文)
├── 2_VI.mp3    ← Ốc Đào 2 (Tiếng Việt)
├── 2_EN.mp3    ← Ốc Đào 2 (English)
├── 2_CN.mp3    ← Ốc Đào 2 (中文)
└── ...
```

## 🔧 Cách tạo file audio

### Tự động (Khuyến nghị):
1. Mở app → Vào Account → Click "🎵 Tạo Audio"
2. Hệ thống tự động tạo MP3 cho tất cả quán ăn
3. File được lưu vào thư mục này

### Thủ công (Nếu cần):
```csharp
var audioService = new AudioGeneratorService();
await audioService.GenerateAllAudioAsync();
```

## 📊 Công nghệ sử dụng

- **System.Speech.Synthesis**: Text → WAV
- **NAudio + LAME**: WAV → MP3 (128kbps)
- **Format**: MP3, 128kbps ABR

## 🌍 Ngôn ngữ hỗ trợ

- 🇻🇳 **VI**: Tiếng Việt (vi-VN)
- 🇺🇸 **EN**: English (en-US)
- 🇨🇳 **CN**: 中文 (zh-CN)

## 🎯 Sử dụng

### Trong MapWindow (Geofence):
- Khi user vào vùng bán kính quán ăn
- Tự động phát file audio theo ngôn ngữ đã chọn
- Offline-first: Dùng file local, fallback sang TTS nếu không có

### Trong FoodDetailWindow:
- Click nút "🔊 Nghe"
- Phát file audio nếu có, fallback sang TTS

## 📝 Lưu ý

- File audio được tạo 1 lần duy nhất
- Nếu Description thay đổi, cần tạo lại audio
- File MP3 có thể deploy lên server để download về
