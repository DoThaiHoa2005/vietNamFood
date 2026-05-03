# 🚀 Hướng Dẫn Tạo File Audio Nhanh Nhất

## ✅ Bước 1: Build Project

```powershell
# Đóng app nếu đang chạy
# Sau đó build:
dotnet build VietnamFoodGuide/VietnamFoodGuide.csproj --configuration Debug
```

## ✅ Bước 2: Chạy AudioGeneratorLauncher

### Cách 1: Từ Visual Studio (Khuyến nghị)

1. Mở file `VietnamFoodGuide/Views/AudioGeneratorLauncher.xaml.cs`
2. Nhấn **F5** hoặc click **Start** để chạy app
3. Trong `App.xaml.cs`, tạm thời đổi StartupUri:

```csharp
// Trong App.xaml
StartupUri="Views/AudioGeneratorLauncher.xaml"
```

### Cách 2: Từ Code (Nhanh nhất)

Thêm code này vào `MainWindow.xaml.cs` trong constructor:

```csharp
public MainWindow()
{
    InitializeComponent();
    
    // ===== TẠO AUDIO NGAY KHI KHỞI ĐỘNG =====
    this.Loaded += async (s, e) =>
    {
        // Hiển thị dialog xác nhận
        var result = MessageBox.Show(
            "Bạn có muốn tạo file audio MP3 cho tất cả quán ăn không?\n\n" +
            "✅ 3 ngôn ngữ: VI, EN, CN\n" +
            "✅ Lưu vào: Assets/Audio/\n" +
            "⏱️ Thời gian: ~2-5 phút",
            "🎵 Tạo File Audio",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );
        
        if (result == MessageBoxResult.Yes)
        {
            var audioWindow = new AudioGeneratorWindow();
            audioWindow.ShowDialog();
        }
    };
    
    // ... rest of constructor
}
```

### Cách 3: Tạo Button trong MainWindow

Thêm button vào MainWindow.xaml (trong Grid.Row="0" - status bar):

```xml
<!-- Thêm vào status bar -->
<Button Content="🎵" Width="30" Height="20"
        FontSize="12" Margin="0,0,5,0"
        HorizontalAlignment="Right"
        Background="#6C5CE7" Foreground="White"
        BorderThickness="0" Cursor="Hand"
        Click="OpenAudioGenerator"
        ToolTip="Tạo file audio MP3"/>
```

Thêm handler vào MainWindow.xaml.cs:

```csharp
private void OpenAudioGenerator(object sender, RoutedEventArgs e)
{
    var audioWindow = new AudioGeneratorWindow();
    audioWindow.ShowDialog();
}
```

## ✅ Bước 3: Chạy Tạo Audio

1. Click nút **"🚀 Bắt Đầu Tạo Audio"**
2. Đợi ~2-5 phút (tùy số lượng quán ăn)
3. Xem progress bar và log
4. Khi hoàn thành, click **"Đóng"**

## ✅ Bước 4: Kiểm Tra Kết Quả

```powershell
# Kiểm tra file đã tạo
ls VietnamFoodGuide/Assets/Audio/

# Kết quả mong đợi:
# 1_VI.mp3, 1_EN.mp3, 1_CN.mp3
# 2_VI.mp3, 2_EN.mp3, 2_CN.mp3
# ...
```

## 🎯 Cách Nhanh Nhất (1 Lệnh)

Nếu bạn muốn tạo audio mà không cần UI, tạo file console app:

```csharp
// AudioGeneratorConsole.cs
using System;
using System.Threading.Tasks;
using VietnamFoodGuide.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🎵 Bắt đầu tạo audio...");
        
        var service = new AudioGeneratorService();
        var result = await service.GenerateAllAudioAsync((current, total, message) =>
        {
            Console.WriteLine($"[{current}/{total}] {message}");
        });
        
        if (result.IsSuccess)
        {
            Console.WriteLine($"✅ Hoàn thành! Thành công: {result.SuccessCount}, Thất bại: {result.FailedCount}");
        }
        else
        {
            Console.WriteLine($"❌ Lỗi: {result.ErrorMessage}");
        }
        
        Console.WriteLine("Nhấn Enter để thoát...");
        Console.ReadLine();
    }
}
```

Chạy:
```powershell
dotnet run --project AudioGeneratorConsole.csproj
```

## 📊 Thời Gian Ước Tính

| Số quán ăn | Thời gian |
|------------|-----------|
| 10 quán    | ~30 giây  |
| 50 quán    | ~2 phút   |
| 100 quán   | ~5 phút   |
| 200 quán   | ~10 phút  |

## ⚠️ Lưu Ý

1. **Cần cài đặt giọng đọc Windows:**
   - Settings → Time & Language → Language
   - Add: Vietnamese, English, Chinese
   - Download speech pack

2. **Cần kết nối database:**
   - Đảm bảo XAMPP đang chạy
   - Database có dữ liệu quán ăn

3. **Không đóng app khi đang tạo:**
   - Đợi progress bar chạy xong
   - Không tắt máy

## 🎉 Sau Khi Hoàn Thành

File audio sẽ tự động được sử dụng trong:
- ✅ MapWindow (geofence auto-play)
- ✅ FoodDetailWindow (manual play)
- ✅ Offline mode (không cần internet)

Database cũng được cập nhật với AudioUrl_VI, AudioUrl_EN, AudioUrl_CN.
