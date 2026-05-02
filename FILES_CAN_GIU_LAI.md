# 📁 DANH SÁCH FILES CẦN GIỮ LẠI

## ✅ ROOT DIRECTORY

### Files quan trọng:
- ✅ `README.md` - Tổng quan project
- ✅ `HUONG_DAN_SU_DUNG.md` - Hướng dẫn sử dụng
- ✅ `HUONG_DAN_HOAN_CHINH_NOI_BAI.md` - Hướng dẫn nội bài
- ✅ `HUONG_DAN_DEPLOY_ONLINE.md` - Hướng dẫn deploy
- ✅ `CLEANUP_LOG.md` - Nhật ký dọn dẹp
- ✅ `admin_dashboard.html` - Admin panel
- ✅ `deploy.ps1` - Script deploy
- ✅ `setup_api.ps1` - Script setup API
- ✅ `VietnamFoodGuide.slnx` - Solution file

### Files CÓ THỂ XÓA:
- ❌ `ACCOUNT_DIALOG_QR_BANNER_UPDATE.md`
- ❌ `BOTTOM_NAV_QR_SCANNER_UPDATE.md`
- ❌ `FIX_API_CONNECTION_ERROR.md`
- ❌ `FIX_FAVORITES_ISSUE.md`
- ❌ `FIX_MAPWINDOW_LANGUAGE_COMPLETE.md`
- ❌ `HUONG_DAN_QR_SCANNER_COMPLETE.md`
- ❌ `MAP_ROUTING_FIX.md`
- ❌ `QR_BANNER_BOTTOM_POSITION_UPDATE.md`
- ❌ `QR_BANNER_POSITION_FIX.md`
- ❌ `QR_SCANNER_FLOW_UPDATE.md`
- ❌ `QR_SCANNER_SUMMARY.md`
- ❌ `QR_SCANNER_WINDOW_RESIZE_UPDATE.md`
- ❌ `QUICK_FIX_NGROK_URL.md`
- ❌ `REMOVE_HEADER_BUTTONS_UPDATE.md`
- ❌ `CLEANUP_SUMMARY.md`
- ❌ `PROJECT_DOCUMENTATION_FINAL.md`
- ❌ `app.xaml` (duplicate)
- ❌ `Application` (không rõ)
- ❌ `idea.doc`
- ❌ `index.css`
- ❌ `index.html`
- ❌ `run.sh`
- ❌ `setup_api.bat`
- ❌ `test_add_favorite.ps1`
- ❌ `PRD_*.docx` (4 files)
- ❌ `cleanup_files.ps1` (sau khi dùng xong)

---

## ✅ XAMPP_API DIRECTORY

### Files quan trọng:
- ✅ `api.php` - API chính
- ✅ `update_database_complete.sql` - Setup database (file duy nhất cần)
- ✅ `utilities.php` - Tools
- ✅ `check_database.php` - Kiểm tra DB

### Files CÓ THỂ XÓA:
- ❌ `setup_complete.sql` (đã có update_database_complete.sql)
- ❌ `create_qr_scans_table.sql`
- ❌ `setup_simple.sql`
- ❌ `setup_with_tracking.sql`
- ❌ `update_descriptions.sql`
- ❌ `README.md` (duplicate)
- ❌ `SETUP_XAMPP.md` (duplicate)

---

## ✅ VIETNAMFOODGUIDE DIRECTORY

### Files quan trọng:
- ✅ `App.xaml` - WPF App
- ✅ `App.xaml.cs`
- ✅ `appsettings.json`
- ✅ `VietnamFoodGuide.csproj`
- ✅ Tất cả files trong Services/
- ✅ Tất cả files trong Views/
- ✅ Tất cả files trong Models/
- ✅ Tất cả files trong Assets/

### Files CÓ THỂ XÓA:
- ❌ `admin_dashboard.html` (duplicate, đã có ở root)
- ❌ `README.md` (duplicate)
- ❌ Files trong Views/ có đuôi `.txt` hoặc `_Simple`

---

## ✅ DOCS DIRECTORY

### Files quan trọng:
- ✅ `API.md`
- ✅ `ARCHITECTURE.md`
- ✅ `DEPLOYMENT.md`

---

## ✅ THỐNG KÊ

### Tổng files cần giữ: ~50 files
### Tổng files có thể xóa: ~35 files

---

## 🎯 KẾT LUẬN

**Bạn có thể xóa thủ công các files được đánh dấu ❌ để project gọn gàng hơn.**

**Hoặc chạy lệnh:**
```powershell
# Xóa từng file
Remove-Item "ACCOUNT_DIALOG_QR_BANNER_UPDATE.md" -Force
Remove-Item "BOTTOM_NAV_QR_SCANNER_UPDATE.md" -Force
# ... (tiếp tục với các files khác)
```

**Hoặc dùng File Explorer:**
1. Mở thư mục project
2. Tìm và xóa các files được đánh dấu ❌
3. ✅ Xong!
