# Hướng Dẫn Xuất File APK

## Vấn Đề Hiện Tại

Project của bạn là **WPF (.NET Framework 4.8)** - chỉ chạy trên **Windows Desktop**.

**Không thể** xuất APK trực tiếp từ WPF vì:
- ❌ WPF không hỗ trợ Android
- ❌ .NET Framework 4.8 chỉ chạy trên Windows
- ❌ XAML của WPF khác với XAML của MAUI/Xamarin

---

## Các Phương Án

### 🎯 Phương Án 1: Chuyển Sang .NET MAUI (Khuyến Nghị)

**.NET MAUI** (Multi-platform App UI) là framework mới nhất của Microsoft, hỗ trợ:
- ✅ Android (APK)
- ✅ iOS (IPA)
- ✅ Windows (EXE)
- ✅ macOS

**Ưu điểm:**
- Dùng C# và XAML (tương tự WPF)
- Một codebase cho tất cả platform
- Hỗ trợ chính thức từ Microsoft
- Performance tốt

**Nhược điểm:**
- Cần viết lại UI (XAML khác WPF)
- Cần học MAUI (nhưng dễ nếu đã biết WPF)
- Thời gian: 2-4 tuần

**Chi phí:** Miễn phí

---

### 🔧 Phương Án 2: Xamarin.Forms (Đang Deprecated)

**Xamarin.Forms** là framework cũ, đã bị thay thế bởi .NET MAUI.

**Ưu điểm:**
- Tương tự MAUI
- Nhiều tài liệu

**Nhược điểm:**
- ⚠️ Microsoft không còn hỗ trợ (deprecated)
- Nên dùng MAUI thay vì Xamarin

**Khuyến nghị:** Không nên dùng, chuyển sang MAUI

---

### 📱 Phương Án 3: Progressive Web App (PWA)

Chuyển app thành **Web App** có thể cài đặt như native app.

**Ưu điểm:**
- Không cần viết lại code nhiều
- Chạy trên mọi platform (Android, iOS, Windows)
- Dễ deploy và update
- Không cần Google Play Store

**Nhược điểm:**
- Cần internet (hoặc cache offline)
- Không truy cập được một số tính năng native
- Performance không bằng native app

**Thời gian:** 1-2 tuần

**Chi phí:** Miễn phí

---

### 🌐 Phương Án 4: Hybrid App (Cordova/Capacitor)

Wrap web app thành native app.

**Ưu điểm:**
- Dùng HTML/CSS/JavaScript
- Xuất APK, IPA
- Truy cập được native features qua plugins

**Nhược điểm:**
- Cần viết lại UI bằng web technologies
- Performance không tốt bằng native
- Cần học JavaScript framework (React, Vue, Angular)

**Thời gian:** 2-3 tuần

**Chi phí:** Miễn phí

---

### 🔄 Phương Án 5: Flutter (Dart)

Framework của Google, rất phổ biến.

**Ưu điểm:**
- Performance tốt
- UI đẹp
- Hỗ trợ Android, iOS, Web, Desktop

**Nhược điểm:**
- Phải học Dart (ngôn ngữ mới)
- Viết lại toàn bộ app
- Không dùng được code C# hiện tại

**Thời gian:** 3-4 tuần

**Chi phí:** Miễn phí

---

### 🚀 Phương Án 6: React Native (JavaScript)

Framework của Facebook/Meta.

**Ưu điểm:**
- Rất phổ biến
- Nhiều thư viện
- Hỗ trợ Android, iOS

**Nhược điểm:**
- Phải học JavaScript/TypeScript
- Viết lại toàn bộ app
- Không dùng được code C# hiện tại

**Thời gian:** 3-4 tuần

**Chi phí:** Miễn phí

---

## So Sánh Các Phương Án

| Phương Án | Thời Gian | Độ Khó | Dùng Lại Code C# | APK | iOS | Web | Desktop |
|-----------|-----------|--------|------------------|-----|-----|-----|---------|
| **.NET MAUI** | 2-4 tuần | Trung bình | ✅ 70-80% | ✅ | ✅ | ❌ | ✅ |
| **Xamarin** | 2-4 tuần | Trung bình | ✅ 70-80% | ✅ | ✅ | ❌ | ❌ |
| **PWA** | 1-2 tuần | Dễ | ❌ 0% | ✅ | ✅ | ✅ | ✅ |
| **Cordova** | 2-3 tuần | Trung bình | ❌ 0% | ✅ | ✅ | ✅ | ❌ |
| **Flutter** | 3-4 tuần | Khó | ❌ 0% | ✅ | ✅ | ✅ | ✅ |
| **React Native** | 3-4 tuần | Khó | ❌ 0% | ✅ | ✅ | ❌ | ❌ |

---

## Khuyến Nghị: .NET MAUI

### Tại Sao Chọn MAUI?

1. **Dùng lại được nhiều code:**
   - ✅ Services (API, Database, Storage)
   - ✅ Models (FoodItem, User, etc.)
   - ✅ Business Logic
   - ❌ UI (cần viết lại XAML)

2. **Quen thuộc:**
   - Vẫn dùng C#
   - Vẫn dùng XAML (tương tự WPF)
   - Vẫn dùng Visual Studio

3. **Hỗ trợ chính thức:**
   - Microsoft hỗ trợ lâu dài
   - Cộng đồng lớn
   - Tài liệu đầy đủ

4. **Cross-platform:**
   - Android (APK)
   - iOS (IPA)
   - Windows (EXE)
   - macOS

---

## Hướng Dẫn Chuyển Sang .NET MAUI

### Bước 1: Cài Đặt .NET MAUI

#### 1.1. Cài .NET 8 SDK

Download: https://dotnet.microsoft.com/download/dotnet/8.0

```bash
# Kiểm tra version
dotnet --version
# Phải >= 8.0
```

#### 1.2. Cài MAUI Workload

```bash
dotnet workload install maui
```

#### 1.3. Cài Visual Studio 2022

Chọn workload:
- ✅ .NET Multi-platform App UI development
- ✅ Mobile development with .NET

---

### Bước 2: Tạo Project MAUI Mới

```bash
# Tạo project MAUI
dotnet new maui -n VietnamFoodGuideMaui

# Hoặc dùng Visual Studio:
# File → New → Project → .NET MAUI App
```

---

### Bước 3: Di Chuyển Code

#### 3.1. Copy Services (Dùng Lại 100%)

```
VietnamFoodGuide/Services/
├── ApiService.cs          ✅ Copy nguyên
├── StorageService.cs      ✅ Copy nguyên
├── NetworkService.cs      ✅ Copy nguyên
├── LanguageService.cs     ✅ Copy nguyên
├── AudioCacheService.cs   ⚠️ Cần sửa MediaPlayer
└── ...
```

#### 3.2. Copy Models (Dùng Lại 100%)

```
VietnamFoodGuide/Models/
├── FoodItem.cs           ✅ Copy nguyên
├── User.cs               ✅ Copy nguyên
├── Favorite.cs           ✅ Copy nguyên
└── ...
```

#### 3.3. Viết Lại UI (XAML MAUI)

**WPF XAML:**
```xml
<Window x:Class="VietnamFoodGuide.Views.MainWindow">
    <Grid>
        <Button Content="Click Me" Click="Button_Click"/>
    </Grid>
</Window>
```

**MAUI XAML:**
```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             x:Class="VietnamFoodGuideMaui.Views.MainPage">
    <Grid>
        <Button Text="Click Me" Clicked="Button_Click"/>
    </Grid>
</ContentPage>
```

**Khác biệt chính:**
- `Window` → `ContentPage`
- `Content` → `Text`
- `Click` → `Clicked`
- Không có `x:Name`, dùng `x:Name` hoặc binding

---

### Bước 4: Build APK

#### 4.1. Cấu Hình Android

**File:** `Platforms/Android/AndroidManifest.xml`

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <application 
        android:allowBackup="true" 
        android:icon="@mipmap/appicon" 
        android:label="Vietnam Food Guide"
        android:roundIcon="@mipmap/appicon_round"
        android:supportsRtl="true">
    </application>
    
    <!-- Permissions -->
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
</manifest>
```

#### 4.2. Build APK Debug

```bash
# Build APK debug (không cần sign)
dotnet build -f net8.0-android -c Debug

# APK output:
# bin/Debug/net8.0-android/com.companyname.vietnamfoodguidemaui-Signed.apk
```

#### 4.3. Build APK Release (Signed)

**Bước 1: Tạo Keystore**

```bash
keytool -genkey -v -keystore vietnamfoodguide.keystore -alias vietnamfoodguide -keyalg RSA -keysize 2048 -validity 10000
```

**Bước 2: Cấu Hình Signing**

File: `VietnamFoodGuideMaui.csproj`

```xml
<PropertyGroup Condition="'$(Configuration)|$(TargetFramework)|$(Platform)'=='Release|net8.0-android|AnyCPU'">
    <AndroidKeyStore>True</AndroidKeyStore>
    <AndroidSigningKeyStore>vietnamfoodguide.keystore</AndroidSigningKeyStore>
    <AndroidSigningKeyAlias>vietnamfoodguide</AndroidSigningKeyAlias>
    <AndroidSigningKeyPass>your_password</AndroidSigningKeyPass>
    <AndroidSigningStorePass>your_password</AndroidSigningStorePass>
</PropertyGroup>
```

**Bước 3: Build Release**

```bash
dotnet publish -f net8.0-android -c Release

# APK output:
# bin/Release/net8.0-android/publish/com.companyname.vietnamfoodguidemaui-Signed.apk
```

---

### Bước 5: Test APK

#### 5.1. Cài Trên Emulator

```bash
# List emulators
emulator -list-avds

# Start emulator
emulator -avd Pixel_5_API_33

# Install APK
adb install bin/Release/net8.0-android/publish/com.companyname.vietnamfoodguidemaui-Signed.apk
```

#### 5.2. Cài Trên Thiết Bị Thật

1. Bật **Developer Options** trên điện thoại
2. Bật **USB Debugging**
3. Kết nối USB
4. Chạy:
```bash
adb devices
adb install path/to/your.apk
```

---

## Phương Án Nhanh: PWA (Progressive Web App)

Nếu bạn muốn nhanh hơn, tôi khuyến nghị làm PWA:

### Ưu Điểm PWA:
- ✅ Không cần viết lại nhiều code
- ✅ Chạy trên mọi platform
- ✅ Không cần Google Play Store
- ✅ Dễ update (chỉ cần update server)
- ✅ Có thể cài đặt như native app

### Cách Làm PWA:

1. **Tạo Web App** (HTML/CSS/JavaScript)
2. **Thêm Service Worker** (cache offline)
3. **Thêm Manifest** (icon, name, theme)
4. **Deploy lên server**
5. **User mở trình duyệt → "Add to Home Screen"**

**Thời gian:** 1-2 tuần

---

## Roadmap Khuyến Nghị

### Giai Đoạn 1: PWA (1-2 tuần)
- Tạo web version
- Thêm PWA features
- Deploy lên server
- User có thể dùng ngay

### Giai Đoạn 2: .NET MAUI (2-4 tuần)
- Chuyển sang MAUI
- Build APK
- Submit lên Google Play Store
- Có native app chính thức

---

## Chi Phí

### Miễn Phí:
- ✅ .NET MAUI (miễn phí)
- ✅ Visual Studio Community (miễn phí)
- ✅ Android SDK (miễn phí)

### Có Phí:
- 💰 Google Play Developer Account: **$25** (một lần, trọn đời)
- 💰 Apple Developer Account: **$99/năm** (nếu muốn iOS)

---

## Tài Nguyên Học Tập

### .NET MAUI:
- 📚 Microsoft Docs: https://learn.microsoft.com/dotnet/maui/
- 🎥 YouTube: "James Montemagno" channel
- 📖 Book: ".NET MAUI in Action"

### PWA:
- 📚 Google PWA Guide: https://web.dev/progressive-web-apps/
- 🎥 YouTube: "Fireship PWA Tutorial"

---

## Kết Luận

### Khuyến Nghị Của Tôi:

**Nếu bạn muốn nhanh (1-2 tuần):**
→ Làm **PWA** trước

**Nếu bạn muốn native app chính thức (2-4 tuần):**
→ Chuyển sang **.NET MAUI**

**Nếu bạn muốn cả hai:**
→ Làm PWA trước → Sau đó chuyển MAUI

---

## Tôi Có Thể Giúp Gì?

Tôi có thể giúp bạn:
1. ✅ Tạo project .NET MAUI mới
2. ✅ Di chuyển code từ WPF sang MAUI
3. ✅ Viết lại UI bằng MAUI XAML
4. ✅ Cấu hình build APK
5. ✅ Hoặc tạo PWA version

**Bạn muốn làm phương án nào?**
- Option 1: .NET MAUI (native app, APK)
- Option 2: PWA (web app, có thể cài đặt)
- Option 3: Cả hai (PWA trước, MAUI sau)

Hãy cho tôi biết bạn chọn phương án nào! 🚀
