# Test script để kiểm tra giọng đọc Windows

Write-Host "🔊 Kiểm tra giọng đọc Windows..." -ForegroundColor Cyan
Write-Host ""

# Tạo file C# tạm để test
$testCode = @"
using System;
using System.Speech.Synthesis;

class VoiceTest
{
    static void Main()
    {
        using (var synth = new SpeechSynthesizer())
        {
            var voices = synth.GetInstalledVoices();
            Console.WriteLine("Tìm thấy " + voices.Count + " giọng đọc:");
            Console.WriteLine("");
            
            foreach (var voice in voices)
            {
                Console.WriteLine("✅ " + voice.VoiceInfo.Name);
                Console.WriteLine("   Ngôn ngữ: " + voice.VoiceInfo.Culture.Name);
                Console.WriteLine("   Giới tính: " + voice.VoiceInfo.Gender);
                Console.WriteLine("");
            }
            
            if (voices.Count == 0)
            {
                Console.WriteLine("❌ KHÔNG CÓ GIỌNG ĐỌC NÀO!");
                Console.WriteLine("");
                Console.WriteLine("Cách cài đặt:");
                Console.WriteLine("1. Mở Settings → Time & Language → Language");
                Console.WriteLine("2. Click 'Add a language'");
                Console.WriteLine("3. Thêm: Vietnamese, English (US), Chinese (Simplified)");
                Console.WriteLine("4. Click vào từng ngôn ngữ → Options → Download speech pack");
                Console.WriteLine("5. Restart máy");
            }
        }
    }
}
"@

# Lưu file tạm
$testFile = "VoiceTest.cs"
$testCode | Out-File -FilePath $testFile -Encoding UTF8

# Compile và chạy
Write-Host "Đang compile..." -ForegroundColor Yellow
csc /reference:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Speech.dll" $testFile 2>&1 | Out-Null

if (Test-Path "VoiceTest.exe") {
    Write-Host "Đang kiểm tra..." -ForegroundColor Yellow
    Write-Host ""
    .\VoiceTest.exe
    
    # Cleanup
    Remove-Item "VoiceTest.exe" -Force
    Remove-Item "VoiceTest.cs" -Force
} else {
    Write-Host "❌ Lỗi compile. Thử cách khác..." -ForegroundColor Red
    Write-Host ""
    Write-Host "Chạy lệnh PowerShell sau để kiểm tra:" -ForegroundColor Cyan
    Write-Host 'Add-Type -AssemblyName System.Speech; $synth = New-Object System.Speech.Synthesis.SpeechSynthesizer; $synth.GetInstalledVoices() | ForEach-Object { $_.VoiceInfo.Name + " (" + $_.VoiceInfo.Culture.Name + ")" }'
}

Write-Host ""
Write-Host "Nhấn Enter để thoát..." -ForegroundColor Gray
Read-Host
