# Cập Nhật Audio Playback Logic

## Tóm Tắt Thay Đổi

### 1. FoodDetailWindow (Trang Chi Tiết)
**Logic phát audio:**
- ✅ **Có mạng** → Dùng Google TTS (giọng nói tự nhiên)
- ❌ **Không có mạng** → Phát file audio local + hiển thị thời gian phát (giây)

**Tính năng mới:**
- Hiển thị thời gian phát audio khi offline (VD: "5s", "12s")
- Nút dừng luôn hiển thị khi đang phát
- Tự động chọn file audio theo ngôn ngữ app (vi/en/zh)

**Files thay đổi:**
- `VietnamFoodGuide/Views/FoodDetailWindow.xaml` - Thêm TextBlock hiển thị thời gian
- `VietnamFoodGuide/Views/FoodDetailWindow.xaml.cs` - Logic phát audio theo network status

### 2. MapWindow (Trang Bản Đồ)
**Logic phát audio:**
- 🗺️ **Luôn luôn dùng file audio** (cả online và offline)
- 🎯 Tự động phát khi vào vòng geofence (radius từ database)
- 🌍 Tự động chọn file audio theo ngôn ngữ app (vi/en/zh)

**Thay đổi:**
- Loại bỏ fallback sang Google TTS
- Chỉ phát audio file, không phát TTS
- Nếu không có file audio → không phát gì (silent)

**Files thay đổi:**
- `VietnamFoodGuide/Views/MapWindow.xaml.cs` - Method `AutoNarrate()`

### 3. AudioCacheService (Service Phát Audio)
**Tính năng mới:**
- Event `OnPlaybackProgress` - Cập nhật thời gian phát mỗi 100ms
- Hiển thị thời gian hiện tại (current) và tổng thời gian (total)
- DispatcherTimer để track progress

**Files thay đổi:**
- `VietnamFoodGuide/Services/AudioCacheService.cs`

### 4. AudioDownloadService
**Sửa lỗi:**
- Thay `File.WriteAllBytesAsync()` → `File.WriteAllBytes()` (tương thích .NET Framework 4.8)

**Files thay đổi:**
- `VietnamFoodGuide/Services/AudioDownloadService.cs`

---

## Chi Tiết Kỹ Thuật

### AudioCacheService - Playback Progress
```csharp
// Event mới
public event Action<TimeSpan, TimeSpan> OnPlaybackProgress; // (current, total)

// Timer cập nhật progress mỗi 100ms
_progressTimer = new DispatcherTimer();
_progressTimer.Interval = TimeSpan.FromMilliseconds(100);
_progressTimer.Tick += (s, e) =>
{
    if (_mediaPlayer.NaturalDuration.HasTimeSpan)
    {
        OnPlaybackProgress?.Invoke(_mediaPlayer.Position, _mediaPlayer.NaturalDuration.TimeSpan);
    }
};
```

### FoodDetailWindow - Hiển Thị Thời Gian
```xml
<!-- UI hiển thị thời gian phát -->
<TextBlock x:Name="PlaybackTime" Text="0s" FontSize="13"
           Foreground="{DynamicResource SecondaryBrush}" FontWeight="Bold"
           VerticalAlignment="Center" Visibility="Collapsed"/>
```

```csharp
// Event handler cập nhật UI
private void OnAudioPlaybackProgress(TimeSpan current, TimeSpan total)
{
    Dispatcher.Invoke(() => {
        var playbackTime = SpeechStatusBar?.FindName("PlaybackTime") as TextBlock;
        if (playbackTime != null)
        {
            playbackTime.Text = $"{(int)current.TotalSeconds}s";
        }
    });
}
```

### MapWindow - Chỉ Dùng Audio File
```csharp
private async void AutoNarrate(FoodItem food)
{
    string currentLang = LanguageService.Instance.CurrentLanguage; // vi, en, zh
    
    // ✅ MapWindow LUÔN LUÔN dùng file audio (cả online và offline)
    var audioCacheService = AudioCacheService.Instance;
    if (audioCacheService.HasAudioForLanguage(food, currentLang))
    {
        bool played = await audioCacheService.PlayAudioAsync(food, currentLang);
        if (played)
        {
            // Success
            return;
        }
    }
    
    // ⚠️ Không có file audio → không phát gì
    NarrationBanner.Visibility = Visibility.Collapsed;
}
```

---

## Luồng Hoạt Động

### FoodDetailWindow
```
User bấm "🔊 Nghe"
    ↓
Kiểm tra network
    ↓
┌─────────────────────────────────────────┐
│ Có mạng?                                │
├─────────────────────────────────────────┤
│ ✅ YES → Google TTS                     │
│          (không hiển thị thời gian)     │
│                                         │
│ ❌ NO  → Audio File                     │
│          (hiển thị thời gian: "5s")     │
└─────────────────────────────────────────┘
```

### MapWindow
```
User vào vòng geofence (radius)
    ↓
Kiểm tra ngôn ngữ app (vi/en/zh)
    ↓
Tìm file audio tương ứng
    ↓
┌─────────────────────────────────────────┐
│ Có file audio?                          │
├─────────────────────────────────────────┤
│ ✅ YES → Phát audio file                │
│                                         │
│ ❌ NO  → Không phát gì (silent)         │
└─────────────────────────────────────────┘
```

---

## Cách Test

### Test FoodDetailWindow
1. **Test Online (có mạng):**
   - Mở app với kết nối mạng
   - Vào trang chi tiết món ăn
   - Bấm "🔊 Nghe"
   - ✅ Phải nghe giọng Google TTS
   - ✅ Không hiển thị thời gian phát

2. **Test Offline (không có mạng):**
   - Tắt WiFi/mạng
   - Vào trang chi tiết món ăn
   - Bấm "🔊 Nghe"
   - ✅ Phải phát file audio local
   - ✅ Hiển thị thời gian phát (VD: "5s", "12s")
   - ✅ Nút "⏹ Dừng" hiển thị

3. **Test Đổi Ngôn Ngữ:**
   - Đổi ngôn ngữ app (VI → EN → CN)
   - Bấm "🔊 Nghe"
   - ✅ Phải phát audio đúng ngôn ngữ

### Test MapWindow
1. **Test Auto-Play:**
   - Mở bản đồ
   - Di chuyển vào vòng geofence của quán ăn
   - ✅ Tự động phát audio file
   - ✅ Banner "🔊 Đang thuyết minh" hiển thị

2. **Test Online/Offline:**
   - Test cả online và offline
   - ✅ Cả 2 trường hợp đều phát audio file (không dùng TTS)

3. **Test Đổi Ngôn Ngữ:**
   - Đổi ngôn ngữ app
   - Vào vòng geofence
   - ✅ Phát audio đúng ngôn ngữ

---

## Lưu Ý

1. **File Audio Format:**
   - Tên file: `{FoodId}_{FoodName}_{Language}.mp3`
   - VD: `1_Pho_Thin_VI.mp3`, `1_Pho_Thin_EN.mp3`

2. **Thư Mục Cache:**
   - `%AppData%/VietnamFoodGuide/AudioCache/`
   - File được download khi đăng nhập lần đầu

3. **Database Columns:**
   - `AudioUrl_VI` - URL file audio tiếng Việt
   - `AudioUrl_EN` - URL file audio tiếng Anh
   - `AudioUrl_CN` - URL file audio tiếng Trung

4. **Network Check:**
   - Dùng `NetworkService.IsInternetAvailableAsync()`
   - Chỉ FoodDetailWindow check network
   - MapWindow không check network (luôn dùng audio file)

---

## Build Status
✅ Build thành công
✅ Không có lỗi compile
✅ Tương thích .NET Framework 4.8
