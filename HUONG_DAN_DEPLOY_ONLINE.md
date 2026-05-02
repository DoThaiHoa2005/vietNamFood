# Hướng Dẫn Deploy Backend API + Database Online

## 🎯 Mục Tiêu
Chuyển từ XAMPP local sang Backend API + Database online để:
- ✅ App hoạt động mọi lúc mọi nơi (không cần XAMPP)
- ✅ Web Admin và App dùng chung database
- ✅ Update realtime (Admin thêm → App thấy ngay)
- ✅ Không cần ngrok

## 🏗️ Kiến Trúc

```
📱 App (WPF Desktop)
         ↓ HTTP Request
🌐 API Backend (PHP/Node.js) - Deployed on Hosting
         ↓ Query
💾 Database (MySQL/MongoDB) - Online
         ↑ Query
🖥️ Web Admin (HTML/JS) - Deployed on Hosting
```

## 📋 Các Bước Thực Hiện

### Bước 1: Chọn Hosting FREE

#### Option A: InfinityFree (PHP + MySQL) - RECOMMENDED
- ✅ FREE vĩnh viễn
- ✅ Hỗ trợ PHP + MySQL
- ✅ Không cần credit card
- ✅ Có phpMyAdmin
- 🔗 Link: https://www.infinityfree.net/

#### Option B: Render (Node.js + MongoDB)
- ✅ FREE 750 giờ/tháng
- ✅ Hỗ trợ Node.js
- ✅ Tự động deploy từ GitHub
- 🔗 Link: https://render.com/

#### Option C: Railway (PHP/Node.js + MySQL/MongoDB)
- ✅ FREE $5 credit/tháng
- ✅ Hỗ trợ nhiều ngôn ngữ
- 🔗 Link: https://railway.app/

---

## 🚀 Phương Án 1: InfinityFree (PHP + MySQL) - DỄ NHẤT

### Bước 1.1: Đăng Ký InfinityFree

1. Truy cập: https://www.infinityfree.net/
2. Click **"Sign Up"**
3. Điền email và password
4. Xác nhận email

### Bước 1.2: Tạo Website

1. Đăng nhập vào **Control Panel**
2. Click **"Create Account"**
3. Điền thông tin:
   - **Domain:** Chọn subdomain miễn phí (VD: `vietnamfoodguide.rf.gd`)
   - **Username:** Tự động tạo
4. Click **"Create Account"**

### Bước 1.3: Tạo Database MySQL

1. Trong Control Panel, click **"MySQL Databases"**
2. Click **"Create Database"**
3. Điền:
   - **Database Name:** `vietnamfoodguide`
4. Lưu lại thông tin:
   ```
   Database Name: if0_XXXXXXX_vietnamfoodguide
   Database User: if0_XXXXXXX
   Database Password: [password bạn đặt]
   Database Host: sqlXXX.infinityfree.com
   ```

### Bước 1.4: Import Database

1. Click **"phpMyAdmin"** trong Control Panel
2. Đăng nhập bằng thông tin database ở trên
3. Chọn database `if0_XXXXXXX_vietnamfoodguide`
4. Click tab **"Import"**
5. Chọn file `xampp_api/setup_complete.sql`
6. Click **"Go"**

### Bước 1.5: Upload Files

#### Cách 1: Dùng File Manager (Dễ)

1. Trong Control Panel, click **"Online File Manager"**
2. Vào folder `htdocs`
3. Tạo folder `vfg-api`
4. Upload các file:
   - `api.php`
   - `admin_dashboard.html`
   - `index.html`
   - `check_database.php`

#### Cách 2: Dùng FTP (Pro)

1. Download **FileZilla**: https://filezilla-project.org/
2. Kết nối FTP:
   - **Host:** `ftpupload.net` hoặc `ftp.vietnamfoodguide.rf.gd`
   - **Username:** `if0_XXXXXXX`
   - **Password:** [password hosting]
   - **Port:** 21
3. Upload folder `xampp_api` vào `htdocs/vfg-api`

### Bước 1.6: Cập Nhật api.php

Sửa file `api.php` trên hosting:

```php
// --- CẤU HÌNH DATABASE ---
$host = 'sqlXXX.infinityfree.com'; // Lấy từ Control Panel
$db   = 'if0_XXXXXXX_vietnamfoodguide'; // Database name đầy đủ
$user = 'if0_XXXXXXX'; // Database username
$pass = 'YOUR_PASSWORD'; // Database password
$charset = 'utf8mb4';
```

### Bước 1.7: Test API

Mở browser và test:

```
https://vietnamfoodguide.rf.gd/vfg-api/api.php?action=foods
```

**Kỳ vọng:** Trả về JSON array của foods

### Bước 1.8: Cập Nhật AppConfig.cs

```csharp
public static readonly string Domain = "https://vietnamfoodguide.rf.gd/vfg-api";
```

### Bước 1.9: Rebuild và Test

1. Rebuild app
2. Chạy app
3. Thử thêm yêu thích → Sẽ hoạt động!

---

## 🚀 Phương Án 2: Render + MongoDB Atlas (Node.js)

### Bước 2.1: Tạo MongoDB Atlas (Database)

1. Truy cập: https://www.mongodb.com/cloud/atlas/register
2. Đăng ký tài khoản FREE
3. Tạo Cluster:
   - Chọn **FREE Tier (M0)**
   - Region: **Singapore** (gần VN nhất)
   - Cluster Name: `VietnamFoodGuide`
4. Tạo Database User:
   - Username: `admin`
   - Password: `[password mạnh]`
5. Whitelist IP: Chọn **"Allow Access from Anywhere"** (0.0.0.0/0)
6. Copy **Connection String**:
   ```
   mongodb+srv://admin:<password>@vietnamfoodguide.xxxxx.mongodb.net/
   ```

### Bước 2.2: Tạo Node.js API

Tạo file `server.js`:

```javascript
const express = require('express');
const mongoose = require('mongoose');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

// Connect to MongoDB
mongoose.connect('mongodb+srv://admin:<password>@vietnamfoodguide.xxxxx.mongodb.net/vietnamfoodguide', {
  useNewUrlParser: true,
  useUnifiedTopology: true
});

// Food Schema
const FoodSchema = new mongoose.Schema({
  Name: String,
  City: String,
  Category: String,
  Description_VI: String,
  Description_EN: String,
  Description_CN: String,
  Latitude: Number,
  Longitude: Number,
  Rating: Number,
  ImagePath: String
});

const Food = mongoose.model('Food', FoodSchema);

// API Endpoints
app.get('/api/foods', async (req, res) => {
  const foods = await Food.find().sort({ Rating: -1 });
  res.json(foods);
});

app.post('/api/foods', async (req, res) => {
  const food = new Food(req.body);
  await food.save();
  res.json({ success: true, id: food._id });
});

// Favorites endpoints...
// (Tương tự như PHP)

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
});
```

### Bước 2.3: Deploy lên Render

1. Push code lên GitHub
2. Truy cập: https://render.com/
3. Click **"New +"** → **"Web Service"**
4. Connect GitHub repository
5. Cấu hình:
   - **Name:** `vietnamfoodguide-api`
   - **Environment:** `Node`
   - **Build Command:** `npm install`
   - **Start Command:** `node server.js`
6. Click **"Create Web Service"**
7. Copy URL: `https://vietnamfoodguide-api.onrender.com`

### Bước 2.4: Cập Nhật AppConfig.cs

```csharp
public static readonly string Domain = "https://vietnamfoodguide-api.onrender.com";
public static string ApiBaseUrl => $"{Domain}/api";
```

---

## 📊 So Sánh Các Phương Án

| Tiêu Chí | InfinityFree (PHP) | Render (Node.js) | Railway |
|----------|-------------------|------------------|---------|
| **Giá** | FREE vĩnh viễn | FREE 750h/tháng | FREE $5/tháng |
| **Database** | MySQL | MongoDB Atlas | MySQL/MongoDB |
| **Dễ setup** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Tốc độ** | Trung bình | Nhanh | Nhanh |
| **Uptime** | 99% | 99.9% | 99.9% |
| **Phù hợp** | Sinh viên | Developer | Cả hai |

## 🎯 Khuyến Nghị

### Cho Sinh Viên (Dễ + Nhanh):
👉 **InfinityFree (PHP + MySQL)**
- Không cần học Node.js
- Dùng lại code PHP hiện có
- Setup trong 30 phút

### Cho Developer (Pro + Scalable):
👉 **Render (Node.js + MongoDB)**
- Hiện đại hơn
- Dễ scale
- Tự động deploy từ GitHub

---

## 🔧 Checklist Sau Khi Deploy

- [ ] API hoạt động: Test `GET /api/foods`
- [ ] Database có dữ liệu: Kiểm tra phpMyAdmin/MongoDB Atlas
- [ ] App kết nối được: Cập nhật `AppConfig.cs`
- [ ] Web Admin hoạt động: Test thêm/sửa/xóa
- [ ] Favorites sync: Test thêm yêu thích từ app
- [ ] Realtime update: Admin thêm → App refresh → Thấy ngay

---

## 🐛 Troubleshooting

### Lỗi: "CORS blocked"
**Giải pháp:** Thêm vào `api.php`:
```php
header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS");
header("Access-Control-Allow-Headers: Content-Type");
```

### Lỗi: "Database connection failed"
**Giải pháp:** Kiểm tra:
1. Database host, username, password đúng chưa
2. IP đã được whitelist chưa (MongoDB)
3. Database đã tạo chưa

### Lỗi: "API returns HTML instead of JSON"
**Giải pháp:**
1. Kiểm tra file `api.php` có syntax error không
2. Test trực tiếp trên browser
3. Xem error logs trong hosting control panel

---

## 📚 Tài Liệu Tham Khảo

- InfinityFree Docs: https://forum.infinityfree.net/docs
- MongoDB Atlas: https://docs.atlas.mongodb.com/
- Render Docs: https://render.com/docs
- Express.js: https://expressjs.com/

---

## 🎓 Kết Luận

Sau khi deploy online:
- ✅ App hoạt động mọi lúc mọi nơi
- ✅ Không cần XAMPP, không cần ngrok
- ✅ Web Admin và App sync realtime
- ✅ Dễ demo cho giáo viên/khách hàng
- ✅ Có thể share link cho bạn bè test

**Thời gian setup:** 30-60 phút  
**Chi phí:** $0 (FREE)  
**Độ khó:** ⭐⭐⭐ (Trung bình)

Chúc bạn deploy thành công! 🚀
