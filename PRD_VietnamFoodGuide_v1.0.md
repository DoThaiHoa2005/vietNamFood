# VIETNAM FOOD GUIDE — PRD v1.0

**PRODUCT REQUIREMENTS DOCUMENT**

**VIETNAM FOOD GUIDE**  
Hệ thống hướng dẫn ẩm thực Việt Nam với bản đồ tương tác và chỉ đường thông minh

---

| **Phiên bản** | 1.0 — Phiên bản đầu tiên (tháng 4/2026) |
|---------------|------------------------------------------|
| **Ngày cập nhật** | Tháng 4, 2026 |
| **Trạng thái** | Đồ án môn học — Phản ánh codebase thực tế |
| **Công nghệ** | C# WPF / .NET Framework 4.8 / WebView2 |
| **Database** | MySQL (XAMPP) + JSON (local data) |

---

## CHÚ GIẢI TRẠNG THÁI TÍNH NĂNG

| Ký hiệu | Ý nghĩa |
|---------|---------|
| ✔ ĐÃ HOÀN THÀNH | Tính năng đã được hiện thực đầy đủ trong codebase, đã test và hoạt động ổn định |
| ◑ MỘT PHẦN | Tính năng được hiện thực một phần, còn thiếu một số chức năng phụ |
| ⏳ ROADMAP | Tính năng đã thiết kế và lên kế hoạch, chưa hiện thực trong giai đoạn hiện tại |
| 🆕 MỚI THÊM | Tính năng MỚI được bổ sung trong quá trình phát triển |

---

## 1. TỔNG QUAN DỰ ÁN (PROJECT OVERVIEW)

**Vietnam Food Guide** là hệ thống hướng dẫn ẩm thực tập trung vào khu vực Vĩnh Khánh, TP.HCM. Ứng dụng cung cấp trải nghiệm tìm kiếm và khám phá món ăn Việt Nam với:
- Bản đồ tương tác hiển thị 11 quán ăn nổi tiếng
- Chỉ đường thông minh với giọng nói đa ngôn ngữ (Tiếng Việt, English, 中文)
- Thuyết minh tự động dựa trên vị trí và rating quán
- Hỗ trợ offline với dữ liệu JSON local

### Đối tượng sử dụng

| Đối tượng | Giá trị mang lại |
|-----------|------------------|
| **Du khách** | Khám phá ẩm thực địa phương, chỉ đường tự động, nghe thuyết minh đa ngôn ngữ |
| **Người dùng thường** | Tìm kiếm quán ăn ngon, xem đánh giá, lưu yêu thích |
| **Quản trị viên** | Quản lý dữ liệu quán ăn, người dùng qua Web Admin Dashboard |

---

## 2. KIẾN TRÚC HỆ THỐNG (SYSTEM ARCHITECTURE)

Hệ thống theo mô hình Client-Server đơn giản, gồm 3 thành phần chính:

| Project | Công nghệ | Vai trò |
|---------|-----------|---------|
| **VietnamFoodGuide** | C# WPF, .NET 4.8, WebView2 | Desktop application cho người dùng cuối |
| **xampp_api** | PHP 7.4+, MySQL 8.0 | Backend RESTful API xử lý authentication và data |
| **admin_dashboard.html** | HTML5, JavaScript, Bootstrap | Web portal quản trị (single-page application) |

### Sơ đồ kiến trúc

```
┌─────────────────────┐
│  WPF Desktop App    │
│  (VietnamFoodGuide) │
│  - Login/Register   │
│  - Main Window      │
│  - Food Detail      │
│  - Map Window       │
└──────────┬──────────┘
           │ HTTP API
           ▼
┌─────────────────────┐
│   XAMPP API (PHP)   │
│   - Authentication  │
│   - User Management │
│   - Data CRUD       │
└──────────┬──────────┘
           │ MySQL
           ▼
┌─────────────────────┐
│   MySQL Database    │
│   - Users           │
│   - Foods           │
│   - Favorites       │
│   - Sessions        │
└─────────────────────┘

┌─────────────────────┐
│  Admin Dashboard    │
│  (HTML/JS/Bootstrap)│
│  - Manage Foods     │
│  - Manage Users     │
│  - Analytics        │
└─────────────────────┘
```

---

## 3. CƠ SỞ DỮ LIỆU (DATABASE SCHEMA)

Hệ thống sử dụng 4 bảng chính trong MySQL:

| Bảng | Mô tả | Các trường chính |
|------|-------|------------------|
| **Users** | Thông tin người dùng | Id, Username, PasswordHash, Role (Admin/User), CreatedDate |
| **Foods** | Dữ liệu quán ăn | Id, Name, City, Category, Description_VI/EN/CN, Latitude, Longitude, Rating, ImagePath |
| **Favorites** | Món ăn yêu thích | Id, UserId, FoodId, CreatedDate |
| **Sessions** | Phiên đăng nhập | Id, UserId, Token, ExpiresAt, CreatedDate |

### Dữ liệu mẫu
- 2 users: `admin` (Admin), `user123` (User)
- 11 quán ăn tại khu vực Vĩnh Khánh, TP.HCM
- Password mặc định: `admin123` (BCrypt hash)

---

## 4. PHẠM VI CÔNG VIỆC (SCOPE OF WORK)

### 4.1. Trong phạm vi (In-Scope)

**Desktop Application — WPF**
1. ✔ Hệ thống đăng nhập/đăng ký với xác thực qua API
2. ✔ Ghi nhớ tài khoản (Remember Me) với mã hóa XOR
3. ✔ Màn hình chính hiển thị danh sách món ăn với ảnh, rating
4. ✔ Chi tiết món ăn với mô tả đa ngôn ngữ, audio narration (TTS)
5. ✔ Bản đồ tương tác với OpenStreetMap (Leaflet.js)
6. ✔ Chỉ đường thông minh với OSRM routing API
7. ✔ Giọng nói chỉ đường đa ngôn ngữ (Google TTS)
8. ✔ Thuyết minh tự động dựa trên khoảng cách và rating
9. ✔ Tìm kiếm điểm xuất phát (toàn quốc Việt Nam)
10. ✔ GPS fallback: HTML5 Geolocation → IP Geolocation → Default location

**Backend API — PHP/MySQL**
1. ✔ Authentication: Login, Register, Session management
2. ✔ CRUD operations cho Foods, Users, Favorites
3. ✔ Role-based access control (Admin/User)
4. ✔ BCrypt password hashing
5. ✔ CORS support cho cross-origin requests

**Admin Dashboard — Web**
1. ✔ Quản lý quán ăn (CRUD) với modal editor
2. ✔ Quản lý người dùng, thay đổi role
3. ✔ Upload ảnh với drag-and-drop
4. ✔ Đa ngôn ngữ tabs (Vietnamese, English, Chinese)
5. ✔ Analytics và export functionality

### 4.2. Ngoài phạm vi (Out-of-Scope)

| Tính năng | Lý do |
|-----------|-------|
| Mobile app (Android/iOS) | Tập trung vào desktop WPF |
| Offline map tiles | Yêu cầu SDK trả phí, cache tự động sau lần đầu |
| Real-time GPS tracking | Chỉ hỗ trợ GPS khi mở bản đồ |
| Payment gateway | Ứng dụng thông tin, không có thương mại |
| Social features | Không có comment, review, share |

---

## 5. YÊU CẦU CHỨC NĂNG (FUNCTIONAL REQUIREMENTS)

### 5.1. Ứng dụng Desktop (WPF)

| Trạng thái | Tính năng | Chi tiết | Ghi chú |
|------------|-----------|----------|---------|
| ✔ | **Đăng nhập/Đăng ký** | Form login với username/password. Đăng ký tài khoản mới. Ghi nhớ tài khoản với checkbox. | Mã hóa XOR cho credentials |
| ✔ | **Màn hình chính** | Grid hiển thị 11 quán ăn với ảnh, tên, rating. Click để xem chi tiết. | Role-based: Admin → Web admin, User → Main window |
| ✔ | **Chi tiết món ăn** | Hiển thị ảnh lớn, tên, mô tả đa ngôn ngữ, rating, nút "Xem bản đồ". Audio narration với TTS. | Hỗ trợ 3 ngôn ngữ: VI/EN/CN |
| ✔ | **Bản đồ tương tác** | OpenStreetMap với Google Maps tiles. Hiển thị 11 markers với emoji icon theo category. | Leaflet.js trong WebView2 |
| ✔ | **Chỉ đường thông minh** | OSRM routing API vẽ đường đi ngắn nhất. Auto-zoom theo khoảng cách. Hiển thị khoảng cách (m/km) và thời gian. | Route caching cho hiệu suất |
| ✔ | **Giọng nói chỉ đường** | Google TTS đọc hướng dẫn theo ngôn ngữ đã chọn. Lặp lại sau 10s nếu chưa di chuyển. | Tự động theo selected language |
| 🆕 | **Thuyết minh tự động** | Khi đến gần quán (20-40m tùy rating), tự động đọc mô tả quán. | Rating cao → thuyết minh từ xa hơn |
| ✔ | **Tìm kiếm xuất phát** | Nominatim API tìm kiếm địa điểm toàn Việt Nam. Hiển thị 10 kết quả. | Filter countrycodes=vn |
| ✔ | **GPS fallback** | 3 cấp độ: HTML5 Geolocation → IP Geolocation → Default (Bến Thành). | Tự động không hiển thị lỗi |
| ✔ | **Đổi ngôn ngữ** | Selector ở góc phải trên: 🇻🇳 Tiếng Việt, 🇺🇸 English, 🇨🇳 中文 | Realtime update UI |

### 5.2. Web Admin Dashboard

| Trạng thái | Tính năng | Chi tiết | Ghi chú |
|------------|-----------|----------|---------|
| ✔ | **Quản lý quán ăn** | CRUD đầy đủ với modal editor. Multi-language tabs. | Không cần login |
| ✔ | **Upload ảnh** | Drag-and-drop, preview, delete. | Lưu vào server |
| ✔ | **Quản lý user** | Xem danh sách, thay đổi role, xóa user. | Admin/User roles |
| ✔ | **Analytics** | Thống kê số lượng, export CSV. | Basic analytics |

### 5.3. Backend API

| Trạng thái | Endpoint | Method | Mô tả |
|------------|----------|--------|-------|
| ✔ | `/api.php?action=login` | POST | Đăng nhập, trả về token |
| ✔ | `/api.php?action=register` | POST | Đăng ký user mới, auto role=User |
| ✔ | `/api.php?action=foods` | GET | Lấy danh sách quán ăn |
| ✔ | `/api.php?action=food` | POST/PUT/DELETE | CRUD quán ăn |
| ✔ | `/api.php?action=users` | GET | Lấy danh sách users |
| ✔ | `/api.php?action=update_user_role` | POST | Thay đổi role user |
| ✔ | `/api.php?action=stats` | GET | Thống kê hệ thống |

---

## 6. QUY TRÌNH NGHIỆP VỤ (WORKFLOW)

### 6.1. Quy trình đăng nhập

| Bước | Thực hiện bởi | Mô tả |
|------|---------------|-------|
| 1 | User | Nhập username/password, check "Ghi nhớ" nếu muốn |
| 2 | App | Gửi POST request đến `/api.php?action=login` |
| 3 | API | Verify BCrypt hash, tạo session token |
| 4 | App | Lưu credentials (XOR encrypted) nếu "Ghi nhớ" được chọn |
| 5 | App | Điều hướng: Admin → Browser (admin_dashboard.html), User → MainWindow |

### 6.2. Quy trình chỉ đường

| Bước | Thực hiện bởi | Mô tả |
|------|---------------|-------|
| 1 | User | Chọn quán ăn từ MainWindow, click "Xem bản đồ" |
| 2 | MapWindow | Hiển thị popup "Chọn điểm xuất phát": GPS/Search/Default |
| 3 | User | Chọn phương án (VD: GPS) |
| 4 | MapWindow | Thử HTML5 Geolocation → Fallback IP → Fallback Default |
| 5 | MapWindow | Gọi OSRM API tính route ngắn nhất |
| 6 | MapWindow | Vẽ đường xanh, hiển thị khoảng cách và thời gian |
| 7 | User | Click "Bắt đầu" để bắt đầu navigation |
| 8 | MapWindow | Đọc hướng dẫn đầu tiên bằng giọng nói, lặp lại sau 10s |
| 9 | MapWindow | Khi đến gần quán khác (20-40m), tự động thuyết minh |

---

## 7. YÊU CẦU PHI CHỨC NĂNG (NON-FUNCTIONAL REQUIREMENTS)

| Nhóm | Yêu cầu |
|------|---------|
| **Hiệu năng** | Route calculation < 2s. Map rendering mượt mà. TTS latency < 500ms. |
| **Bảo mật** | BCrypt password hashing. XOR encryption cho Remember Me. Role-based access control. |
| **Tương thích** | Windows 10/11. .NET Framework 4.8. WebView2 Runtime. MySQL 8.0+. |
| **Khả năng mở rộng** | RESTful API dễ tích hợp. JSON data format chuẩn. Modular architecture. |
| **Offline** | Dữ liệu quán ăn từ JSON local. Map tiles cache tự động. |

---

## 8. CÔNG NGHỆ SỬ DỤNG (TECHNOLOGY STACK)

### Frontend (Desktop)
- **Framework**: WPF (.NET Framework 4.8)
- **UI**: XAML, Material Design
- **Web Engine**: Microsoft WebView2
- **Map**: Leaflet.js + OpenStreetMap
- **Routing**: OSRM API
- **TTS**: Google Translate TTS API
- **Audio**: NAudio library

### Backend
- **Server**: XAMPP (Apache + PHP 7.4+)
- **Database**: MySQL 8.0
- **API**: RESTful PHP
- **Auth**: BCrypt password hashing

### Admin Dashboard
- **Frontend**: HTML5, CSS3, JavaScript (ES6+)
- **UI Framework**: Bootstrap 5
- **Icons**: Font Awesome

---

## 9. ROADMAP — TÍNH NĂNG TƯƠNG LAI

| Tính năng | Mô tả | Ưu tiên |
|-----------|-------|---------|
| **Mobile App** | Android/iOS app với .NET MAUI | Cao |
| **Real-time GPS** | Background tracking, geofencing | Trung bình |
| **Social Features** | Review, rating, comment, share | Trung bình |
| **Offline Maps** | Cache map tiles offline | Thấp |
| **AR Navigation** | Augmented reality chỉ đường | Thấp |
| **Multi-city** | Mở rộng ra các thành phố khác | Cao |
| **Food Ordering** | Tích hợp đặt món online | Trung bình |

---

## 10. HƯỚNG DẪN CÀI ĐẶT (INSTALLATION GUIDE)

### Yêu cầu hệ thống
- Windows 10/11 (64-bit)
- .NET Framework 4.8
- WebView2 Runtime
- XAMPP (Apache + MySQL + PHP)

### Các bước cài đặt

1. **Clone repository**
   ```bash
   git clone https://github.com/yourusername/VietnamFoodGuide.git
   cd VietnamFoodGuide
   ```

2. **Setup Database**
   - Mở XAMPP Control Panel, start Apache và MySQL
   - Mở phpMyAdmin: `http://localhost/phpmyadmin`
   - Import file `xampp_api/setup_complete.sql`

3. **Setup API**
   - Copy folder `xampp_api` vào `C:\xampp\htdocs\vfg-api`
   - Hoặc chạy script: `setup_api.bat` (Windows)

4. **Build Application**
   ```bash
   dotnet build VietnamFoodGuide/VietnamFoodGuide.csproj
   ```

5. **Run Application**
   - Mở `VietnamFoodGuide.sln` trong Visual Studio
   - Nhấn F5 để chạy
   - Hoặc chạy file `.exe` trong `bin/Debug/net48/`

### Login credentials
- Admin: `admin` / `admin123`
- User: `user123` / `admin123`

---

## 11. TESTING & QUALITY ASSURANCE

### Test Cases

| ID | Tính năng | Test Case | Kết quả mong đợi |
|----|-----------|-----------|------------------|
| TC01 | Login | Đăng nhập với credentials đúng | Chuyển đến MainWindow/Admin Dashboard |
| TC02 | Login | Đăng nhập với credentials sai | Hiển thị lỗi "Invalid credentials" |
| TC03 | Register | Đăng ký với username mới | Tạo account thành công, role=User |
| TC04 | Remember Me | Check "Ghi nhớ", đóng app, mở lại | Auto-fill credentials |
| TC05 | Map | Click "Xem bản đồ" từ FoodDetail | Hiển thị bản đồ với marker quán đã chọn |
| TC06 | Navigation | Click "Bắt đầu" | Đọc hướng dẫn đầu tiên bằng giọng nói |
| TC07 | GPS Fallback | Từ chối GPS permission | Tự động dùng IP Geolocation |
| TC08 | Auto Narration | Di chuyển đến gần quán (< 40m) | Tự động đọc mô tả quán |
| TC09 | Language Switch | Đổi ngôn ngữ sang English | UI và giọng nói chuyển sang English |
| TC10 | Admin CRUD | Thêm quán ăn mới từ Admin Dashboard | Quán mới xuất hiện trong app |

---

## 12. DEPLOYMENT

### Production Checklist
- [ ] Build Release configuration
- [ ] Test trên máy sạch (không có Visual Studio)
- [ ] Đóng gói installer với Inno Setup
- [ ] Chuẩn bị database backup
- [ ] Setup production server (nếu có)
- [ ] Viết user manual
- [ ] Tạo video hướng dẫn

### Deployment Steps
1. Build Release: `dotnet build -c Release`
2. Copy files từ `bin/Release/net48/`
3. Tạo installer với Inno Setup
4. Test installer trên máy sạch
5. Distribute

---

## 13. MAINTENANCE & SUPPORT

### Bug Reporting
- GitHub Issues: https://github.com/yourusername/VietnamFoodGuide/issues
- Email: support@vietnamfoodguide.com

### Update Schedule
- Minor updates: Hàng tháng
- Major updates: Hàng quý
- Security patches: Ngay lập tức

---

## 14. TEAM & CONTRIBUTORS

| Vai trò | Tên | Trách nhiệm |
|---------|-----|-------------|
| **Project Lead** | [Your Name] | Overall architecture, WPF development |
| **Backend Developer** | [Your Name] | PHP API, MySQL database |
| **UI/UX Designer** | [Your Name] | Interface design, user experience |
| **QA Tester** | [Your Name] | Testing, bug reporting |

---

## 15. REFERENCES & RESOURCES

### Documentation
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [Leaflet.js](https://leafletjs.com/)
- [OSRM API](http://project-osrm.org/)
- [Google TTS](https://cloud.google.com/text-to-speech)

### Third-party Services
- OpenStreetMap: Map tiles
- OSRM: Routing engine
- Nominatim: Geocoding
- IP Geolocation API: Fallback location

---

**Tài liệu nội bộ, dùng cho mục đích học thuật. Vietnam Food Guide © 2026**

*Trang 1 / 1 | Tài liệu nội bộ — Đồ án môn học*
