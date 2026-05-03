# 🎵 Hướng Dẫn Tạo File Audio Tự Động

## 📋 Tổng Quan

Hệ thống tự động tạo file audio MP3 từ Description cho 3 ngôn ngữ (VI, EN, CN) và lưu vào `Assets/Audio/`.

## 🔧 Công Nghệ

### Stack:
1. **System.Speech.Synthesis** → Text to WAV
2. **NAudio** → Audio processing
3. **LAME** → WAV to MP3 (128kbps)

### NuGet Packages cần cài:
```bash
Install-Package NAudio
Install-Package NAudio.Lame
```

## 📁 Cấu Trúc File

```
VietnamFoodGuide/
├── Assets/
│   └── Audio/
│       ├── 1_VI.mp3    ← Alo Quán (Tiếng Việt)
│       ├── 1_EN.mp3    ← Alo Quán (English)
│       ├── 1_CN.mp3    ← Alo Quán (中文)
│       ├── 2_VI.mp3    ← Ốc Đào 2 (Tiếng Việt)
│       └── ...
├── Services/
│   └── AudioGeneratorService.cs  ← Service tạo audio
└── Views/
    ├── AudioGeneratorWindow.xaml     ← UI tạo audio
    └── AudioGeneratorWindow.xaml.cs
```

## 🚀 Cách Sử Dụng

### Bước 1: Cài đặt NuGet Packages

```powershell
# Trong Package Manager Console
Install-Package NAudio
Install-Package NAudio.Lame
```

### Bước 2: Tạo Audio

#### Cách 1: Qua UI (Khuyến nghị)
1. Mở app
2. Đăng nhập với tài khoản Admin
3. Vào Account → Click "🎵 Tạo Audio"
4. Click "🎵 Tạo Audio" → Đợi hoàn thành

#### Cách 2: Code
```csharp
var audioService = new AudioGeneratorService();
var result = await audioService.GenerateAllAudioAsync((current, total, message) =>
{
    Console.WriteLine($"{current}/{total}: {message}");
});

if (result.IsSuccess)
{
    Console.WriteLine($"✅ Thành công: {result.SuccessCount}");
    Console.WriteLine($"❌ Thất bại: {result.FailedCount}");
}
```

### Bước 3: Kiểm Tra Kết Quả

```
Assets/Audio/
├── 1_VI.mp3  ✅ (Alo Quán - Tiếng Việt)
├── 1_EN.mp3  ✅ (Alo Quán - English)
├── 1_CN.mp3  ✅ (Alo Quán - 中文)
├── 2_VI.mp3  ✅ (Ốc Đào 2 - Tiếng Việt)
└── ...
```

## 🎯 Sử Dụng Trong App

### 1. MapWindow (Geofence Auto-Play)

Khi user vào vùng bán kính quán ăn:

```csharp
// Trong MapWindow.xaml.cs
private void CheckNearbyFoods()
{
    foreach (var food in _allFoods)
    {
        double distance = CalculateDistance(_userLat, _userLng, food.Latitude, food.Longitude);
        
        if (distance <= food.Radius)
        {
            // Phát audio theo ngôn ngữ
            string language = LanguageService.Instance.CurrentLanguage; // "vi", "en", "zh"
            string audioPath = GetAudioPath(food.Id, language);
            
            if (!string.IsNullOrEmpty(audioPath))
            {
                // Phát file audio local (offline)
                PlayAudioFile(audioPath);
            }
            else
            {
                // Fallback: Dùng TTS
                _speechService.Speak(food.GetDescription(language));
            }
        }
    }
}

private string GetAudioPath(int foodId, string language)
{
    string langCode = language == "vi" ? "VI" : (language == "en" ? "EN" : "CN");
    string fileName = $"{foodId}_{langCode}.mp3";
    string fullPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Audio", fileName);
    
    return File.Exists(fullPath) ? fullPath : null;
}
```

### 2. FoodDetailWindow (Manual Play)

Khi user click nút "🔊 Nghe":

```csharp
private void SpeakVI(object sender, RoutedEventArgs e)
{
    string audioPath = GetAudioPath(_currentFood.Id, "VI");
    
    if (!string.IsNullOrEmpty(audioPath))
    {
        // Phát file audio
        PlayAudioFile(audioPath);
    }
    else
    {
        // Fallback: TTS
        _speechService.Speak(_currentFood.Description_VI, "vi-VN");
    }
}
```

## 📊 Database Schema

```sql
-- Bảng Foods đã có 3 cột AudioUrl
ALTER TABLE Foods ADD COLUMN AudioUrl_VI VARCHAR(500) DEFAULT NULL;
ALTER TABLE Foods ADD COLUMN AudioUrl_EN VARCHAR(500) DEFAULT NULL;
ALTER TABLE Foods ADD COLUMN AudioUrl_CN VARCHAR(500) DEFAULT NULL;
```

Ví dụ data:
```
Id | Name      | AudioUrl_VI              | AudioUrl_EN              | AudioUrl_CN
---|-----------|--------------------------|--------------------------|---------------------------
1  | Alo Quán  | /Assets/Audio/1_VI.mp3   | /Assets/Audio/1_EN.mp3   | /Assets/Audio/1_CN.mp3
2  | Ốc Đào 2  | /Assets/Audio/2_VI.mp3   | /Assets/Audio/2_EN.mp3   | /Assets/Audio/2_CN.mp3
```

## 🔄 Workflow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. User click "🎵 Tạo Audio"                                │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. AudioGeneratorService.GenerateAllAudioAsync()            │
│    - Load tất cả foods từ database                          │
│    - Foreach food:                                           │
│      + Tạo 1_VI.mp3 từ Description_VI                       │
│      + Tạo 1_EN.mp3 từ Description_EN                       │
│      + Tạo 1_CN.mp3 từ Description_CN                       │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Text → WAV (System.Speech.Synthesis)                     │
│    - Chọn giọng đọc theo locale (vi-VN, en-US, zh-CN)      │
│    - Tạo file WAV tạm                                        │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. WAV → MP3 (NAudio + LAME)                                │
│    - Convert WAV → MP3 (128kbps ABR)                        │
│    - Lưu vào Assets/Audio/                                  │
│    - Xóa file WAV tạm                                        │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Cập nhật AudioUrl vào database (qua API)                 │
│    POST /api.php?action=updateAudioUrls                     │
│    {                                                         │
│      "foodId": 1,                                            │
│      "audioUrl_VI": "/Assets/Audio/1_VI.mp3",               │
│      "audioUrl_EN": "/Assets/Audio/1_EN.mp3",               │
│      "audioUrl_CN": "/Assets/Audio/1_CN.mp3"                │
│    }                                                         │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Hoàn thành! File MP3 sẵn sàng sử dụng                    │
│    ✅ Offline-first: Dùng file local                        │
│    ✅ Geofence: Tự động phát khi vào vùng quán              │
│    ✅ Manual: Click "🔊 Nghe" để phát                       │
└─────────────────────────────────────────────────────────────┘
```

## 🐛 Troubleshooting

### Lỗi: "Không tìm thấy giọng đọc"
**Nguyên nhân:** Windows chưa cài giọng đọc cho ngôn ngữ đó

**Giải pháp:**
1. Mở Settings → Time & Language → Language
2. Add language: Vietnamese, English, Chinese
3. Download speech pack

### Lỗi: "LAME encoder not found"
**Nguyên nhân:** Thiếu NAudio.Lame package

**Giải pháp:**
```powershell
Install-Package NAudio.Lame
```

### File audio không phát
**Nguyên nhân:** File path không đúng

**Giải pháp:**
```csharp
// Kiểm tra file tồn tại
string audioPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Audio", "1_VI.mp3");
if (File.Exists(audioPath))
{
    Console.WriteLine($"✅ File tồn tại: {audioPath}");
}
else
{
    Console.WriteLine($"❌ File không tồn tại: {audioPath}");
}
```

## 📦 Deploy

### Bước 1: Copy file audio lên server
```bash
# Upload toàn bộ thư mục Assets/Audio/ lên server
scp -r Assets/Audio/ user@server:/path/to/app/Assets/
```

### Bước 2: Cập nhật AudioUrl trong database
```sql
-- Nếu deploy lên server, cập nhật URL
UPDATE Foods SET
    AudioUrl_VI = 'https://yourserver.com/Assets/Audio/1_VI.mp3',
    AudioUrl_EN = 'https://yourserver.com/Assets/Audio/1_EN.mp3',
    AudioUrl_CN = 'https://yourserver.com/Assets/Audio/1_CN.mp3'
WHERE Id = 1;
```

## ✅ Checklist

- [ ] Cài đặt NAudio và NAudio.Lame
- [ ] Tạo thư mục Assets/Audio/
- [ ] Chạy AudioGeneratorService
- [ ] Kiểm tra file MP3 đã được tạo
- [ ] Test phát audio trong MapWindow
- [ ] Test phát audio trong FoodDetailWindow
- [ ] Cập nhật database với AudioUrl
- [ ] Deploy file audio lên server (nếu cần)

## 🎉 Kết Quả

- ✅ 12 quán ăn × 3 ngôn ngữ = **36 file MP3**
- ✅ Offline-first: Không cần internet để phát audio
- ✅ Geofence: Tự động phát khi vào vùng quán
- ✅ Multi-language: Hỗ trợ 3 ngôn ngữ
- ✅ Fallback: Dùng TTS nếu không có file audio
