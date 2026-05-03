# Hướng Dẫn Cấu Hình Mạng LAN

## Tổng Quan

Hướng dẫn này giúp bạn cấu hình để các thiết bị khác trong mạng LAN có thể truy cập XAMPP server qua IP `192.168.1.112` thay vì `localhost`.

---

## Bước 1: Cấu Hình XAMPP

### 1.1. Cho Phép Truy Cập Từ Mạng LAN

**File:** `C:\xampp\apache\conf\extra\httpd-xampp.conf`

Tìm đoạn:
```apache
<Directory "C:/xampp/phpMyAdmin">
    AllowOverride AuthConfig
    Require local
</Directory>
```

Thay đổi thành:
```apache
<Directory "C:/xampp/phpMyAdmin">
    AllowOverride AuthConfig
    Require all granted
</Directory>
```

### 1.2. Cấu Hình Virtual Host (Tùy Chọn)

**File:** `C:\xampp\apache\conf\extra\httpd-vhosts.conf`

Thêm vào cuối file:
```apache
<VirtualHost *:80>
    DocumentRoot "C:/xampp/htdocs"
    ServerName 192.168.1.112
    <Directory "C:/xampp/htdocs">
        Options Indexes FollowSymLinks
        AllowOverride All
        Require all granted
    </Directory>
</VirtualHost>
```

### 1.3. Restart Apache

1. Mở XAMPP Control Panel
2. Click "Stop" trên Apache
3. Click "Start" lại

---

## Bước 2: Cấu Hình Windows Firewall

### 2.1. Cho Phép Apache Qua Firewall

1. Mở **Windows Defender Firewall**
2. Click "Allow an app or feature through Windows Defender Firewall"
3. Click "Change settings"
4. Tìm "Apache HTTP Server" hoặc "httpd.exe"
5. Tick cả "Private" và "Public"
6. Click "OK"

### 2.2. Tạo Inbound Rule (Nếu Cần)

1. Mở **Windows Defender Firewall with Advanced Security**
2. Click "Inbound Rules" → "New Rule"
3. Chọn "Port" → Next
4. Chọn "TCP" → Specific local ports: `80, 443` → Next
5. Chọn "Allow the connection" → Next
6. Tick tất cả (Domain, Private, Public) → Next
7. Name: "XAMPP Apache" → Finish

---

## Bước 3: Cấu Hình App

### 3.1. Cập Nhật AppConfig.cs

**File:** `VietnamFoodGuide/Services/AppConfig.cs`

```csharp
// OPTION 2: Dùng IP LAN (cho local network - các thiết bị trong mạng)
public static readonly string Domain = "http://192.168.1.112/vfg-api";
```

✅ **Đã cập nhật!**

### 3.2. Rebuild App

```bash
dotnet build VietnamFoodGuide/VietnamFoodGuide.csproj --configuration Release
```

---

## Bước 4: Kiểm Tra Kết Nối

### 4.1. Kiểm Tra Từ Máy Chủ (192.168.1.112)

Mở trình duyệt và truy cập:
- ✅ `http://192.168.1.112/vfg-api/api.php?action=stats`
- ✅ `http://192.168.1.112/vfg-api/admin_dashboard.html`
- ✅ `http://192.168.1.112/phpmyadmin`

### 4.2. Kiểm Tra Từ Thiết Bị Khác

Trên điện thoại hoặc máy tính khác trong cùng mạng WiFi:
- ✅ `http://192.168.1.112/vfg-api/api.php?action=stats`
- ✅ `http://192.168.1.112/vfg-api/admin_dashboard.html`

**Kết quả mong đợi:**
```json
{
  "totalFoods": 12,
  "totalUsers": 2,
  "totalFavorites": 0,
  "avgRating": 4.7
}
```

---

## Bước 5: Cấu Hình Admin Dashboard

Admin Dashboard đã sử dụng **relative path** (`./api.php`) nên không cần sửa gì!

Chỉ cần truy cập:
```
http://192.168.1.112/vfg-api/admin_dashboard.html
```

---

## Cấu Hình Nâng Cao

### Sử Dụng Hostname Thay Vì IP

#### Bước 1: Cấu Hình Hosts File

**Trên máy chủ (192.168.1.112):**

File: `C:\Windows\System32\drivers\etc\hosts`

Thêm dòng:
```
192.168.1.112   vfg.local
```

**Trên các thiết bị khác:**

- **Windows:** `C:\Windows\System32\drivers\etc\hosts`
- **Mac/Linux:** `/etc/hosts`
- **Android:** Cần root, file `/system/etc/hosts`
- **iOS:** Không hỗ trợ trực tiếp

Thêm dòng:
```
192.168.1.112   vfg.local
```

#### Bước 2: Cập Nhật AppConfig.cs

```csharp
public static readonly string Domain = "http://vfg.local/vfg-api";
```

#### Bước 3: Truy Cập

```
http://vfg.local/vfg-api/admin_dashboard.html
```

---

## Troubleshooting

### Lỗi: "This site can't be reached"

**Nguyên nhân:** Firewall chặn hoặc Apache không lắng nghe trên tất cả interfaces

**Giải pháp:**

1. Kiểm tra Apache đang chạy:
   ```bash
   netstat -an | findstr :80
   ```
   Phải thấy: `0.0.0.0:80` hoặc `192.168.1.112:80`

2. Kiểm tra Firewall:
   ```bash
   netsh advfirewall firewall show rule name=all | findstr Apache
   ```

3. Tắt Firewall tạm thời để test:
   ```bash
   netsh advfirewall set allprofiles state off
   ```

### Lỗi: "Access forbidden"

**Nguyên nhân:** Apache không cho phép truy cập từ mạng LAN

**Giải pháp:**

File: `C:\xampp\apache\conf\extra\httpd-xampp.conf`

Thay đổi tất cả `Require local` thành `Require all granted`

### Lỗi: IP thay đổi sau khi restart router

**Nguyên nhân:** DHCP cấp IP động

**Giải pháp:**

#### Option 1: Đặt IP Tĩnh

1. Mở **Network Connections**
2. Right-click adapter → Properties
3. Chọn "Internet Protocol Version 4 (TCP/IPv4)" → Properties
4. Chọn "Use the following IP address":
   - IP address: `192.168.1.112`
   - Subnet mask: `255.255.255.0`
   - Default gateway: `192.168.1.1` (IP router)
   - Preferred DNS: `8.8.8.8`
5. Click OK

#### Option 2: DHCP Reservation (Khuyến nghị)

1. Đăng nhập vào router (thường là `192.168.1.1`)
2. Tìm "DHCP Reservation" hoặc "Static IP"
3. Thêm MAC address của máy → IP `192.168.1.112`
4. Save và restart router

---

## Kiểm Tra IP Máy

### Windows

```bash
ipconfig
```

Tìm dòng:
```
IPv4 Address. . . . . . . . . . . : 192.168.1.112
```

### Kiểm Tra Từ Thiết Bị Khác

```bash
ping 192.168.1.112
```

Kết quả:
```
Reply from 192.168.1.112: bytes=32 time<1ms TTL=128
```

---

## Cấu Hình Cho Các Trường Hợp Khác

### 1. Chỉ Localhost (Mặc Định)

```csharp
public static readonly string Domain = "http://localhost/vfg-api";
```

**Sử dụng khi:**
- Chỉ test trên máy local
- Không cần thiết bị khác truy cập

### 2. IP LAN (Hiện Tại)

```csharp
public static readonly string Domain = "http://192.168.1.112/vfg-api";
```

**Sử dụng khi:**
- Các thiết bị trong cùng mạng WiFi cần truy cập
- Test trên điện thoại, tablet trong nhà

### 3. Ngrok (Remote Access)

```csharp
public static readonly string Domain = "https://abcd-1234.ngrok-free.app/vfg-api";
```

**Sử dụng khi:**
- Cần truy cập từ internet (ngoài mạng LAN)
- Demo cho khách hàng ở xa
- Test trên thiết bị không cùng mạng

---

## Tóm Tắt Các File Đã Thay Đổi

| File | Thay Đổi | Status |
|------|----------|--------|
| `VietnamFoodGuide/Services/AppConfig.cs` | Domain = `http://192.168.1.112/vfg-api` | ✅ Đã cập nhật |
| `admin_dashboard.html` | Dùng relative path `./api.php` | ✅ Không cần sửa |
| `C:\xampp\apache\conf\extra\httpd-xampp.conf` | `Require all granted` | ⚠️ Cần sửa thủ công |
| Windows Firewall | Allow Apache port 80 | ⚠️ Cần cấu hình |

---

## Checklist

- [ ] Cập nhật `AppConfig.cs` với IP `192.168.1.112`
- [ ] Rebuild app
- [ ] Cấu hình `httpd-xampp.conf` (Require all granted)
- [ ] Restart Apache
- [ ] Cấu hình Windows Firewall (Allow port 80)
- [ ] Test từ máy chủ: `http://192.168.1.112/vfg-api/api.php?action=stats`
- [ ] Test từ thiết bị khác trong mạng
- [ ] (Tùy chọn) Đặt IP tĩnh hoặc DHCP reservation

---

## Lưu Ý Quan Trọng

### 1. Bảo Mật

⚠️ **Cảnh báo:** Khi cho phép truy cập từ mạng LAN, bất kỳ ai trong mạng đều có thể truy cập!

**Khuyến nghị:**
- Đổi mật khẩu admin mạnh
- Không expose ra internet (chỉ dùng trong LAN)
- Sử dụng HTTPS nếu có thể

### 2. Performance

- IP LAN nhanh hơn Ngrok (không qua internet)
- Phù hợp cho development và testing
- Không phù hợp cho production

### 3. Khi Nào Dùng Gì?

| Trường Hợp | Cấu Hình |
|------------|----------|
| Dev trên máy local | `localhost` |
| Test trên điện thoại cùng WiFi | `192.168.1.112` |
| Demo cho khách hàng xa | `ngrok` |
| Production | Domain thật + HTTPS |

---

## Kết Luận

✅ **Đã cấu hình xong!**

Bây giờ bạn có thể:
- Truy cập app từ bất kỳ thiết bị nào trong mạng LAN
- Test trên điện thoại, tablet
- Chia sẻ với người khác trong cùng mạng WiFi

**URL truy cập:**
- API: `http://192.168.1.112/vfg-api/api.php`
- Admin: `http://192.168.1.112/vfg-api/admin_dashboard.html`
- phpMyAdmin: `http://192.168.1.112/phpmyadmin`
