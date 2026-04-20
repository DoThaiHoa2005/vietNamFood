@echo off
chcp 65001 >nul
echo ==================================
echo SETUP API CHO VIETNAM FOOD GUIDE
echo ==================================
echo.

REM Kiểm tra XAMPP
if not exist "C:\xampp" (
    echo ❌ Không tìm thấy XAMPP tại C:\xampp
    echo Vui lòng cài đặt XAMPP trước!
    pause
    exit /b 1
)

echo ✅ Tìm thấy XAMPP

REM Kiểm tra htdocs
if not exist "C:\xampp\htdocs" (
    echo ❌ Không tìm thấy folder htdocs
    pause
    exit /b 1
)

echo ✅ Tìm thấy htdocs

REM Kiểm tra source folder
if not exist "xampp_api" (
    echo ❌ Không tìm thấy folder xampp_api trong project
    pause
    exit /b 1
)

echo.
echo 📁 Đang copy API folder...

REM Xóa folder cũ nếu có
if exist "C:\xampp\htdocs\vfg-api" (
    echo ⚠️  Folder vfg-api đã tồn tại, đang xóa...
    rmdir /s /q "C:\xampp\htdocs\vfg-api"
)

REM Copy folder mới
xcopy /E /I /Y "xampp_api" "C:\xampp\htdocs\vfg-api" >nul
echo ✅ Đã copy API folder vào C:\xampp\htdocs\vfg-api

REM Tạo uploads folder
if not exist "C:\xampp\htdocs\vfg-api\uploads" (
    mkdir "C:\xampp\htdocs\vfg-api\uploads"
    echo ✅ Đã tạo folder uploads
)

echo.
echo ==================================
echo SETUP HOÀN TẤT!
echo ==================================
echo.
echo 📋 CÁC BƯỚC TIẾP THEO:
echo.
echo 1. Mở XAMPP Control Panel
echo 2. Start Apache và MySQL
echo 3. Mở trình duyệt: http://localhost/phpmyadmin
echo 4. Chọn database 'VietnamFoodGuide'
echo 5. Click tab 'SQL'
echo 6. Copy nội dung file 'xampp_api\setup_complete.sql' và paste vào
echo 7. Click 'Go' để chạy
echo.
echo 🧪 TEST API:
echo Mở trình duyệt: http://localhost/vfg-api/api.php?action=stats
echo.
echo 🎯 SAU ĐÓ:
echo Build lại project WPF và test đăng ký!
echo.
pause
