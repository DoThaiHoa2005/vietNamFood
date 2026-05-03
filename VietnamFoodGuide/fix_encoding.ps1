# Script to fix UTF-8 encoding issues in XAML files
# This will convert all XAML files to UTF-8 with BOM

Write-Host "🔧 Fixing encoding for all XAML files..." -ForegroundColor Cyan

$xamlFiles = Get-ChildItem -Path "VietnamFoodGuide" -Filter "*.xaml" -Recurse

foreach ($file in $xamlFiles) {
    try {
        Write-Host "Processing: $($file.FullName)" -ForegroundColor Yellow
        
        # Read file content with correct encoding detection
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
        
        # Write back with UTF-8 BOM
        $utf8BOM = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText($file.FullName, $content, $utf8BOM)
        
        Write-Host "✅ Fixed: $($file.Name)" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Error fixing $($file.Name): $_" -ForegroundColor Red
    }
}

Write-Host "`n✨ Done! All XAML files have been re-encoded to UTF-8 with BOM." -ForegroundColor Green
Write-Host "Please rebuild the project." -ForegroundColor Cyan
