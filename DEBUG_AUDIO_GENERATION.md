# 🐛 Debug Audio Generation

## Vấn Đề: Không Tạo Được File MP3

Nếu bạn click nút 🎵 nhưng không thấy file MP3 nào, hãy làm theo các bước sau:

## ✅ Bước 1: Kiểm Tra Giọng Đọc Windows

### Cách 1: Chạy Test Script

```powershell
.\test_voices.ps1
```

### Cách 2: PowerShell Command

```powershell
Add-Type -AssemblyName System.Speech
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
$synth.GetInstalledVoices() | ForEach-Object { 
    Write-Host "✅" $_.VoiceInfo.Name "(" $_.VoiceInfo.Culture.Name ")"
}
```

### Kết Quả Mong Đợi:

```
✅ Microsoft David Desktop (en-US)
✅ Microsoft Zira Desktop (en-US)
✅ Microsoft Huihui Desktop (zh-CN)
✅ Microsoft An Desktop (vi-VN)
```

### Nếu Không Có Giọng Đọc:

1. Mở **Settings** → **Time & Language** → **Language**
2. Click **Add a language**
3. Thêm:
   - **Vietnamese** (Tiếng Việt)
   - **English (United States)**
   - **Chinese (Simplified, China)** (中文)
4. Click vào từng ngôn ngữ → **Options** → **Download** speech pack
5. **Restart máy**

## ✅ Bước 2: Kiểm Tra Database

```powershell
# Kiểm tra XAMPP đang chạy
Get-Process | Where-Object {$_.Name -like "*mysql*" -or $_.Name -like "*apache*"}
```

Nếu không có kết quả → Mở XAMPP Control Panel và Start MySQL

## ✅ Bước 3: Xem Debug Log

1. Chạy app từ Visual Studio (F5)
2. Click nút 🎵
3. Click "🎵 Tạo Audio"
4. Xem **Output** window trong Visual Studio
5. Tìm các dòng log:

```
🚀 [Audio] Bắt đầu tạo audio cho tất cả quán ăn...
📝 [Audio] Tìm thấy 12 quán ăn
🍽️ [1/12] Đang tạo audio cho: Alo Quán
🇻🇳 [Audio] Tạo VI cho Alo Quán...
🎵 [Audio] Tạo file: E:\...\Assets\Audio\1_Alo_Quán_VI.mp3
📝 [Audio] Text: Quán ăn ngon...
🌐 [Audio] Locale: vi-VN
🔊 [Audio] Tìm thấy 2 giọng đọc:
  - Microsoft David Desktop (en-US)
  - Microsoft Zira Desktop (en-US)
⚠️ [Audio] Không tìm thấy giọng vi-VN, dùng giọng mặc định: Microsoft David Desktop
✅ [Audio] Đã tạo WAV: ...
✅ [Audio] Đã tạo MP3: ...
```

## ✅ Bước 4: Kiểm Tra Thư Mục

```powershell
# Kiểm tra thư mục tồn tại
Test-Path "VietnamFoodGuide/Assets/Audio"

# Xem file trong thư mục
Get-ChildItem "VietnamFoodGuide/Assets/Audio"

# Đếm file MP3
(Get-ChildItem "VietnamFoodGuide/Assets/Audio" -Filter "*.mp3").Count
```

## ✅ Bước 5: Test Tạo 1 File Thủ Công

Tạo file test:

```csharp
// TestAudio.cs
using System;
using System.Speech.Synthesis;
using NAudio.Wave;
using NAudio.Lame;

class TestAudio
{
    static void Main()
    {
        Console.WriteLine("🎵 Test tạo audio...");
        
        try
        {
            // Bước 1: Text → WAV
            using (var synth = new SpeechSynthesizer())
            {
                Console.WriteLine("Giọng đọc: " + synth.Voice.Name);
                synth.SetOutputToWaveFile("test.wav");
                synth.Speak("Xin chào, đây là test audio");
            }
            Console.WriteLine("✅ Đã tạo test.wav");
            
            // Bước 2: WAV → MP3
            using (var reader = new AudioFileReader("test.wav"))
            using (var writer = new LameMP3FileWriter("test.mp3", reader.WaveFormat, LAMEPreset.ABR_128))
            {
                reader.CopyTo(writer);
            }
            Console.WriteLine("✅ Đã tạo test.mp3");
            
            // Cleanup
            System.IO.File.Delete("test.wav");
            Console.WriteLine("✅ Hoàn thành!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Lỗi: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        
        Console.WriteLine("\nNhấn Enter để thoát...");
        Console.ReadLine();
    }
}
```

Compile và chạy:

```powershell
# Compile
csc /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Speech.dll" TestAudio.cs

# Chạy
.\TestAudio.exe

# Kiểm tra kết quả
Test-Path "test.mp3"
```

## 🔍 Các Lỗi Thường Gặp

### Lỗi 1: "Không có quán ăn nào trong database"

**Nguyên nhân:** Database trống hoặc không kết nối được

**Giải pháp:**
1. Kiểm tra XAMPP đang chạy
2. Kiểm tra `AppConfig.ApiBaseUrl` đúng
3. Kiểm tra database có dữ liệu:

```sql
SELECT COUNT(*) FROM Foods;
```

### Lỗi 2: "Không tìm thấy giọng đọc"

**Nguyên nhân:** Chưa cài speech pack

**Giải pháp:** Xem Bước 1

### Lỗi 3: "LAME encoder not found"

**Nguyên nhân:** Thiếu NAudio.Lame package

**Giải pháp:**
```powershell
dotnet add VietnamFoodGuide/VietnamFoodGuide.csproj package NAudio.Lame
```

### Lỗi 4: "Access denied" khi tạo file

**Nguyên nhân:** Không có quyền ghi vào thư mục

**Giải pháp:**
```powershell
# Kiểm tra quyền
Get-Acl "VietnamFoodGuide/Assets/Audio"

# Tạo thư mục nếu chưa có
New-Item -ItemType Directory -Path "VietnamFoodGuide/Assets/Audio" -Force
```

## 📊 Kiểm Tra Kết Quả

Sau khi tạo xong, kiểm tra:

```powershell
# 1. Số file MP3
$mp3Files = Get-ChildItem "VietnamFoodGuide/Assets/Audio" -Filter "*.mp3"
Write-Host "Tổng số file MP3: $($mp3Files.Count)"

# 2. Kích thước file
$mp3Files | ForEach-Object {
    Write-Host "$($_.Name): $([math]::Round($_.Length/1KB, 2)) KB"
}

# 3. File theo ngôn ngữ
Write-Host "`nFile VI: $(($mp3Files | Where-Object {$_.Name -like '*_VI.mp3'}).Count)"
Write-Host "File EN: $(($mp3Files | Where-Object {$_.Name -like '*_EN.mp3'}).Count)"
Write-Host "File CN: $(($mp3Files | Where-Object {$_.Name -like '*_CN.mp3'}).Count)"
```

## 🎯 Kết Quả Mong Đợi

```
Tổng số file MP3: 36
1_Alo_Quán_VI.mp3: 45.2 KB
1_Alo_Quán_EN.mp3: 38.7 KB
1_Alo_Quán_CN.mp3: 42.1 KB
...

File VI: 12
File EN: 12
File CN: 12
```

## 💡 Tips

1. **Chạy từ Visual Studio** để xem debug log
2. **Không đóng app** khi đang tạo audio
3. **Kiểm tra giọng đọc** trước khi tạo
4. **Test với 1 quán** trước khi tạo tất cả
5. **Xem Output window** để debug

## 📞 Nếu Vẫn Không Được

Gửi cho tôi:
1. Screenshot Output window
2. Kết quả của `.\test_voices.ps1`
3. Kết quả của `Get-ChildItem "VietnamFoodGuide/Assets/Audio"`
