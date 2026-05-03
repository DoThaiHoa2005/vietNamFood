# Script to fix Vietnamese text encoding issues in XAML files

Write-Host "🔧 Fixing Vietnamese text in XAML files..." -ForegroundColor Cyan

$replacements = @{
    # Common Vietnamese character fixes
    'Ä' = 'Đ'
    'Ã¡' = 'á'
    'Ã ' = 'à'
    'áº£' = 'ả'
    'Ã£' = 'ã'
    'áº¡' = 'ạ'
    'Ã©' = 'é'
    'Ã¨' = 'è'
    'áº»' = 'ẻ'
    'áº½' = 'ẽ'
    'áº¹' = 'ẹ'
    'Ã­' = 'í'
    'Ã¬' = 'ì'
    'áº£' = 'ỉ'
    'Ä©' = 'ĩ'
    'á»‹' = 'ị'
    'Ã³' = 'ó'
    'Ã²' = 'ò'
    'á»Ÿ' = 'ỏ'
    'Ãµ' = 'õ'
    'á»' = 'ọ'
    'Ãº' = 'ú'
    'Ã¹' = 'ù'
    'á»§' = 'ủ'
    'Å©' = 'ũ'
    'á»¥' = 'ụ'
    'Ã½' = 'ý'
    'á»³' = 'ỳ'
    'á»·' = 'ỷ'
    'á»¹' = 'ỹ'
    'á»µ' = 'ỵ'
    'Ä'Æ°' = 'Đư'
    'Ä'á»ƒ' = 'để'
    'Ä'áº¿n' = 'đến'
    'Ä'á»™ng' = 'động'
    'Ä'ang' = 'đang'
    'Ä'Ã£' = 'đã'
    'ÄÄƒng' = 'Đăng'
    'nháº­p' = 'nhập'
    'kháº©u' = 'khẩu'
    'khoáº£n' = 'khoản'
    'Máº­t' = 'Mật'
    'TÃ i' = 'Tài'
    'QuÃ©t' = 'Quét'
    'mÃ£' = 'mã'
    'thÃ nh' = 'thành'
    'cÃ´ng' = 'công'
    'khá»Ÿi' = 'khởi'
    'chuyá»ƒn' = 'chuyển'
    'trang' = 'trang'
    'chá»§' = 'chủ'
    'HÆ°á»›ng' = 'Hướng'
    'dáº«n' = 'dẫn'
    'ÄÆ°a' = 'Đưa'
    'vÃ o' = 'vào'
    'khung' = 'khung'
    'hÃ¬nh' = 'hình'
    'Hoáº·c' = 'Hoặc'
    'cháº¡y' = 'chạy'
    'Há»‡' = 'Hệ'
    'thá»'ng' = 'thống'
    'sáº½' = 'sẽ'
    'tá»±' = 'tự'
    'quÃ©t' = 'quét'
    'lÆ°u' = 'lưu'
    'Cháº¯c' = 'Chắc'
    'cháº¯n' = 'chắn'
    'cáº§n' = 'cần'
    'láº§n' = 'lần'
    'duy' = 'duy'
    'nháº¥t' = 'nhất'
    'Cháº¡y' = 'Chạy'
    'láº¡i' = 'lại'
    'Báº¯t' = 'Bắt'
    'Ä'áº§u' = 'đầu'
    'Báº¡n' = 'Bạn'
    'cÃ³' = 'có'
    'muá»'n' = 'muốn'
    'tiáº¿p' = 'tiếp'
    'tá»¥c' = 'tục'
    'khÃ´ng' = 'không'
    'Ghi' = 'Ghi'
    'nhá»›' = 'nhớ'
    'ChÆ°a' = 'Chưa'
    'kÃ½' = 'ký'
    'ngay' = 'ngay'
    'hoáº·c' = 'hoặc'
    'Táº¡o' = 'Tạo'
    'má»›i' = 'mới'
    'XÃ¡c' = 'Xác'
    'nháº­n' = 'nhận'
    
    # Emoji fixes
    'ðŸ²' = '🍲'
    'ðŸ'¤' = '👤'
    'ðŸ"'' = '🔒'
    'ðŸ''' = '👁'
    'âš ï¸' = '⚠️'
    'ðŸ'¡' = '💡'
    'ðŸ"·' = '📷'
    'âœ…' = '✅'
    'ðŸ"‹' = '📋'
    'âœ‰ï¸' = '✉️'
    'ðŸ"±' = '📱'
}

$xamlFiles = Get-ChildItem -Path "VietnamFoodGuide/Views" -Filter "*.xaml" -File

foreach ($file in $xamlFiles) {
    if ($file.Name -eq "LoginWindow.xaml") {
        Write-Host "⏭️  Skipping LoginWindow.xaml (already fixed)" -ForegroundColor Yellow
        continue
    }
    
    try {
        Write-Host "Processing: $($file.Name)" -ForegroundColor Yellow
        
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
        $originalContent = $content
        
        foreach ($key in $replacements.Keys) {
            $content = $content.Replace($key, $replacements[$key])
        }
        
        if ($content -ne $originalContent) {
            $utf8BOM = New-Object System.Text.UTF8Encoding $true
            [System.IO.File]::WriteAllText($file.FullName, $content, $utf8BOM)
            Write-Host "✅ Fixed: $($file.Name)" -ForegroundColor Green
        } else {
            Write-Host "✓ No changes needed: $($file.Name)" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "❌ Error fixing $($file.Name): $_" -ForegroundColor Red
    }
}

Write-Host "`n✨ Done! Vietnamese text has been fixed." -ForegroundColor Green
