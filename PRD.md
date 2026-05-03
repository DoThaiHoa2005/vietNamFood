# Tài Liệu Đặc Tả Yêu Cầu Sản Phẩm (PRD)
**Tên dự án:** Vietnam Food Guide
**Phiên bản:** 1.0 (Offline-First Edition)
**Mô tả ngắn:** Hệ sinh thái ứng dụng Cầm tay (WPF Desktop/Tablet) kết hợp Web Admin giúp du khách khám phá ẩm thực Việt Nam (đặc biệt khu vực phố ẩm thực Vĩnh Khánh, Quận 4), hỗ trợ bản đồ dẫn đường ngoại tuyến, thuyết minh đa ngôn ngữ tự động và theo dõi vị trí người dùng thời gian thực dành cho Quản trị viên.

---

## 1. Kiến Trúc Tổng Thể (System Architecture)
Hệ thống được thiết kế theo mô hình **Client - Server** kết hợp triệt để cơ chế **Offline-First** (Hoạt động hoàn hảo khi không có mạng).
- **Client (Ứng dụng C# WPF):** Đóng vai trò là thiết bị của người dùng/du khách. Sử dụng WebView2 để kết xuất Bản đồ và Quét mã QR, kết hợp SQLite để lưu trữ ngoại tuyến.
- **Server (XAMPP - PHP/MySQL):** Máy chủ trung tâm lưu trữ dữ liệu nguồn, quản lý tài khoản, đồng bộ hóa và nhận dữ liệu định vị (Tracking).
- **Admin Dashboard (HTML/JS):** Bảng điều khiển Web dành cho Quản trị viên để giám sát người dùng theo thời gian thực.

---

## 2. Các Chức Năng Của Ứng Dụng (Client App - C# WPF)

### 2.1. Xác thực & Tài khoản (Authentication)
- **Đăng nhập / Đăng ký:** Cho phép người dùng tạo tài khoản mới. Mật khẩu được mã hóa an toàn (Bcrypt).
- **Đăng nhập Ngoại tuyến (Tương lai/Có thể nâng cấp):** Duy trì phiên đăng nhập bằng Token cục bộ để không yêu cầu mạng mỗi lần mở app.

### 2.2. Khám phá Ẩm Thực (Food Catalog)
- **Danh sách Quán ăn:** Hiển thị danh sách các quán ăn nổi bật kèm Hình ảnh, Đánh giá (Rating), Phân loại (Hải sản, Ốc, Nướng...).
- **Chi tiết Quán ăn:** Xem chi tiết mô tả quán ăn.
- **Đa ngôn ngữ (Localization):** Hỗ trợ 3 ngôn ngữ (Tiếng Việt, Tiếng Anh, Tiếng Trung) cho giao diện và nội dung miêu tả quán ăn.
- **Cơ chế Offline-First:** Dữ liệu quán ăn được đồng bộ từ Server về lưu tại cơ sở dữ liệu **SQLite** nội bộ. Người dùng có thể lướt xem danh sách mượt mà kể cả khi ngắt kết nối mạng.

### 2.3. Bản đồ & Dẫn đường Thông minh (Smart Map & Navigation)
- **Bản đồ Tương tác:** Sử dụng thư viện Leaflet.js nhúng qua WebView2. Hiển thị vị trí người dùng và các quán ăn.
- **Offline Map (Bộ nhớ đệm Bản đồ):** Tự động tải và lưu trữ (Cache) các khối hình ảnh bản đồ (Map Tiles) của Google Maps khi có mạng. Khi mất mạng, bản đồ vẫn hiển thị rõ nét đường phố khu vực đã lưu.
- **Tìm đường (Routing):** Tính toán đường đi thực tế từ vị trí hiện tại đến quán ăn thông qua API OSRM/OpenRouteService.
- **Offline Route Caching:** Lộ trình đã tìm kiếm khi có mạng sẽ được lưu vào `LocalStorage`. Khi mất mạng, hệ thống dùng lại lộ trình này để vẽ đường thực tế thay vì vẽ đường thẳng (đường chim bay).
- **Chỉ đường bằng giọng nói (Voice Navigation):** Đọc hướng dẫn rẽ trái/phải/đi thẳng bằng Text-to-Speech (TTS) hỗ trợ 3 ngôn ngữ. Hệ thống âm thanh được tinh chỉnh không bị chồng chéo.
- **Geofencing (Thuyết minh tự động):** Khi người dùng di chuyển đến gần một quán ăn (trong bán kính quy định như 30m), ứng dụng tự động phát âm thanh thuyết minh giới thiệu về quán ăn đó.

### 2.4. Danh mục Yêu Thích (Favorites)
- **Lưu trữ Offline:** Người dùng bấm "Trái tim" để lưu quán ăn vào danh sách yêu thích. Dữ liệu được ghi ngay lập tức vào SQLite.
- **Đồng bộ ngầm (Background Sync):** Ứng dụng chạy một tiến trình chạy ngầm (`BackgroundSyncService`). Khi có mạng trở lại, tiến trình sẽ tự động đẩy các quán yêu thích mới thêm/xóa lên MySQL Server mà không làm gián đoạn trải nghiệm người dùng.

### 2.5. Quét Mã QR (QR Scanner)
- **Công nghệ WebView2:** Sử dụng camera thiết bị thông qua luồng HTML5-QRCode. Chạy trên Virtual Host (`https://qr.local`) để qua mặt giới hạn bảo mật SSL của trình duyệt và cho phép quét mã offline (không treo App).
- **Chức năng:** Dùng để điểm danh, check-in, hoặc mở khóa tính năng ẩn.

---

## 3. Các Chức Năng Của Máy Chủ (XAMPP Server - PHP/MySQL)

### 3.1. API Dịch vụ Cơ bản (Core API)
- **Auth API:** Cung cấp các Endpoints đăng nhập (`login`), đăng ký (`register`).
- **Food API:** Trả về danh sách quán ăn trung tâm (`get_foods`), hỗ trợ hình ảnh, tọa độ, script thuyết minh.
- **Favorites API:** Cho phép Client đồng bộ danh sách yêu thích (`sync_favorites`, `add_favorite`, `remove_favorite`).

### 3.2. API Giám sát Lộ trình (Tracking API)
- **Cập nhật Vị trí (`update_location`):** Tiếp nhận dữ liệu Tọa độ (Lat/Lng), trạng thái đang điều hướng (IsNavigating), và điểm đến từ Ứng dụng C#.
- **Quản lý Phiên (Session Management):** Cập nhật thời gian hoạt động cuối (`LastUpdate`). Cung cấp Endpoint `SetOffline` khi người dùng tắt App hoặc Đăng xuất để ngừng theo dõi lập tức.
- **Admin Dashboard API:** Trả về toàn bộ vị trí hiện tại và lịch sử của các người dùng đang trực tuyến cho màn hình Admin.

### 3.3. Cơ sở dữ liệu (MySQL Database Schema)
- **Bảng `Users`:** Quản lý thông tin tài khoản, quyền hạn (Role), trạng thái khóa tài khoản (IsLocked).
- **Bảng `Foods`:** Lưu trữ thông tin quán ăn, bao gồm các cấu hình nâng cao như Bán kính kích hoạt (`Radius`), Mức độ ưu tiên phát giọng nói (`Priority`), Giọng đọc (`NarrationScript`).
- **Bảng `Favorites`:** Quản lý quan hệ Yêu thích (Many-to-Many giữa Users và Foods).
- **Bảng `UserTracking`:** Lưu trữ và cập nhật liên tục tọa độ Live của người dùng.

---

## 4. Các Chức Năng Của Web Admin (Admin Dashboard)

### 4.1. Thống Kê Tổng Quan (Analytics)
- Hiển thị số lượng Tổng người dùng, Người dùng đang Trực tuyến (Active), và Tổng số quán ăn trong hệ thống.

### 4.2. Bản Đồ Giám Sát (Live Tracking Map)
- **Real-time Map:** Hiển thị bản đồ lớn, vẽ chính xác vị trí của toàn bộ người dùng đang mở App bằng các Marker.
- **Trạng thái Di chuyển:** Nếu người dùng đang dùng tính năng "Dẫn đường" trong App, Admin Dashboard sẽ hiển thị mũi tên đường đi (Polyline) nối từ vị trí người dùng đến Quán ăn mục tiêu (Destination).
- **Tự động làm mới:** Tọa độ được làm mới liên tục mỗi vài giây mà không cần tải lại trang. Chấm dứt theo dõi ngay khi người dùng tắt App/Đăng xuất.

### 4.3. Quản Lý Người Dùng (User Management)
- Xem danh sách người dùng trong hệ thống.
- Chức năng Khóa/Mở khóa tài khoản (Ban/Unban).

---

## 5. Tóm Tắt Đặc Điểm Nổi Bật (USPs)
1. **Kiến trúc Offline-First toàn diện:** Từ Dữ liệu quán ăn, Danh sách Yêu thích, Bản đồ đường phố (Tiles Cache), đến Chỉ đường (Route Cache) đều có thể hoạt động không cần Wi-Fi/4G.
2. **Đồng bộ ngầm không gián đoạn:** Giữ nguyên trải nghiệm mượt mà cho người dùng, xử lý vấn đề chập chờn mạng bằng cơ chế Sync ẩn.
3. **Tracking Sinh động:** Tạo sự liên kết chặt chẽ giữa Ứng dụng di động của khách và Bảng điều khiển giám sát của Quản lý, phù hợp mô hình Tour Du lịch Công nghệ.
4. **Hướng dẫn viên Ảo:** Hệ thống âm thanh độc lập, đa ngôn ngữ biến ứng dụng thành một Hướng dẫn viên tự động phát giọng nói khi du khách đến gần địa danh.
