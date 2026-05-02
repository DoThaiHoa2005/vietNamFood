# Script xóa các files không cần thiết

Write-Host "🗑️ Bắt đầu dọn dẹp files..." -ForegroundColor Yellow

# Danh sách files cần xóa
$filesToDelete = @(
    # Files markdown update cũ
    "ACCOUNT_DIALOG_QR_BANNER_UPDATE.md",
    "BOTTOM_NAV_QR_SCANNER_UPDATE.md",
    "FIX_API_CONNECTION_ERROR.md",
    "FIX_FAVORITES_ISSUE.md",
    "FIX_MAPWINDOW_LANGUAGE_COMPLETE.md",
    "HUONG_DAN_QR_SCANNER_COMPLETE.md",
    "MAP_ROUTING_FIX.md",
    "QR_BANNER_BOTTOM_POSITION_UPDATE.md",
    "QR_BANNER_POSITION_FIX.md",
    "QR_SCANNER_FLOW_UPDATE.md",
    "QR_SCANNER_SUMMARY.md",
    "QR_SCANNER_WINDOW_RESIZE_UPDATE.md",
    "QUICK_FIX_NGROK_URL.md",
    "REMOVE_HEADER_BUTTONS_UPDATE.md",
    "CLEANUP_SUMMARY.md",
    "PROJECT_DOCUMENTATION_FINAL.md",
    
    # Files không dùng khác
    "app.xaml",
    "Application",
    "idea.doc",
    "index.css",
    "index.html",
    "run.sh",
    "setup_api.bat",
    "test_add_favorite.ps1",
    
    # Files PRD cũ
    "PRD_VietnamFoodGuide_Detailed_v2.0.docx",
    "PRD_VietnamFoodGuide_Master_v3.0.docx",
    "PRD_VietnamFoodGuide_v2.0.docx",
    "PRD_Vinh_Khanh_Food_Street_Audio_Guide.docx",
    
    # Files SQL cũ trong xampp_api
    "xampp_api/create_qr_scans_table.sql",
    "xampp_api/setup_simple.sql",
    "xampp_api/setup_with_tracking.sql",
    "xampp_api/update_descriptions.sql",
    "xampp_api/README.md",
    "xampp_api/SETUP_XAMPP.md",
    
    # Files duplicate
    "VietnamFoodGuide/admin_dashboard.html",
    "VietnamFoodGuide/README.md"
)

$deletedCount = 0
$notFoundCount = 0

foreach ($file in $filesToDelete) {
    if (Test-Path $file) {
        try {
            Remove-Item $file -Force
            Write-Host "✅ Đã xóa: $file" -ForegroundColor Green
            $deletedCount++
        }
        catch {
            Write-Host "❌ Lỗi xóa: $file - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "⚠️  Không tìm thấy: $file" -ForegroundColor Gray
        $notFoundCount++
    }
}

# Xóa thư mục trống
if (Test-Path "mkdir") {
    Remove-Item "mkdir" -Force -Recurse -ErrorAction SilentlyContinue
    Write-Host "✅ Đã xóa thư mục: mkdir" -ForegroundColor Green
}

Write-Host "`n📊 THỐNG KÊ:" -ForegroundColor Cyan
Write-Host "   Đã xóa: $deletedCount files" -ForegroundColor Green
Write-Host "   Không tìm thấy: $notFoundCount files" -ForegroundColor Gray
Write-Host "`n✅ Hoàn tất dọn dẹp!" -ForegroundColor Yellow
