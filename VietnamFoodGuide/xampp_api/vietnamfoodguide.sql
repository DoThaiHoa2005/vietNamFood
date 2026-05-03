-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Máy chủ: 127.0.0.1
-- Thời gian đã tạo: Th5 03, 2026 lúc 05:41 PM
-- Phiên bản máy phục vụ: 10.4.32-MariaDB
-- Phiên bản PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Cơ sở dữ liệu: `vietnamfoodguide`
--

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `favorites`
--

CREATE TABLE `favorites` (
  `Id` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `FoodId` int(11) NOT NULL,
  `CreatedDate` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `foods`
--

CREATE TABLE `foods` (
  `Id` int(11) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `City` varchar(255) DEFAULT NULL,
  `Category` varchar(100) DEFAULT NULL,
  `Description_VI` text DEFAULT NULL,
  `Description_EN` text DEFAULT NULL,
  `Description_CN` text DEFAULT NULL,
  `Latitude` double DEFAULT 0,
  `Longitude` double DEFAULT 0,
  `Rating` double DEFAULT 0,
  `ImagePath` varchar(500) DEFAULT NULL,
  `Radius` double DEFAULT 30 COMMENT 'Bán kính kích hoạt geofence (mét)',
  `Priority` int(11) DEFAULT 5 COMMENT 'Mức ưu tiên phát thuyết minh (1-10)',
  `AudioUrl_VI` varchar(500) DEFAULT NULL COMMENT 'URL file audio thuyết minh tiếng Việt',
  `AudioUrl_EN` varchar(500) DEFAULT NULL COMMENT 'URL file audio thuyết minh tiếng Anh',
  `AudioUrl_CN` varchar(500) DEFAULT NULL COMMENT 'URL file audio thuyết minh tiếng Trung',
  `CooldownMinutes` int(11) DEFAULT 5 COMMENT 'Thời gian chờ trước khi phát lại (phút)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `foods`
--

INSERT INTO `foods` (`Id`, `Name`, `City`, `Category`, `Description_VI`, `Description_EN`, `Description_CN`, `Latitude`, `Longitude`, `Rating`, `ImagePath`, `Radius`, `Priority`, `AudioUrl_VI`, `AudioUrl_EN`, `AudioUrl_CN`, `CooldownMinutes`) VALUES
(1, 'Alo Quán – Seafood & Beer', 'TP.HCM - Vĩnh Khánh', 'Hải sản', 'Địa chỉ đầu tiên trên phố ẩm thực Vĩnh Khánh mà bạn không nên bỏ qua chính là Alo Quán – Seafood & Beer. Quán nổi tiếng với việc sử dụng các nguyên liệu tươi ngon, chế biến khéo léo để mang đến hương vị ngon nhất cho thực khách khi thưởng thức. Đặc biệt, nếu bạn đang tìm một địa chỉ nhậu ngon – bổ – rẻ thì Alo Quán – Seafood & Beer là lựa chọn lý tưởng.', 'The first address on Vinh Khanh food street that you should not miss is Alo Quan – Seafood & Beer. The restaurant is famous for using fresh ingredients, skillfully prepared to bring the best flavor to diners. Especially, if you are looking for a delicious, nutritious and cheap drinking place, Alo Quan – Seafood & Beer is the ideal choice.', '永庆美食街上第一个不容错过的地址是Alo Quán – Seafood & Beer。餐厅以使用新鲜食材、精心烹制而闻名，为食客带来最佳风味。特别是，如果您正在寻找美味、营养且便宜的饮酒场所，Alo Quán – Seafood & Beer是理想的选择。', 10.78024, 106.70532, 4.8, '/Assets/Images/Q1.jpg', 30, 9, 'uploads/audio/food_1_vi.mp3', 'uploads/audio/food_1_en.mp3', 'uploads/audio/food_1_cn.mp3', 5),
(2, 'Ốc Đào 2', 'TP.HCM - Vĩnh Khánh', 'Ốc', 'Quán Ốc Đào 2 là địa điểm ẩm thực tuyệt vời với hơn 30 loài ốc khác nhau được quán chế biến thành những món ăn độc đáo và ngon miệng. Thực khách có thể thưởng thức các món ốc hấp, xào… được nêm nếm vừa miệng và hương vị thơm ngon nhất.', 'Oc Dao 2 restaurant is a great culinary destination with more than 30 different types of snails that the restaurant prepares into unique and delicious dishes. Diners can enjoy steamed and stir-fried snails... seasoned to taste and with the most delicious flavor.', 'Ốc Đào 2餐厅是一个很棒的美食目的地，拥有30多种不同类型的蜗牛，餐厅将其制作成独特而美味的菜肴。食客可以享受蒸和炒蜗牛...调味适口，味道最美味。', 10.78156, 106.70614, 4.7, '/Assets/Images/Q2.jpg', 35, 8, 'uploads/audio/food_2_vi.mp3', 'uploads/audio/food_2_en.mp3', 'uploads/audio/food_2_cn.mp3', 5),
(3, 'Bún cá Châu Đốc Dì Tư', 'TP.HCM - Vĩnh Khánh', 'Bún', 'Mỗi tô bún cá tại quán bún cá Châu Đốc Dì Tư là sự kết hợp hài hòa giữa thịt cá tươi ngon, bún mềm mịn và các loại rau sống, nước chấm đặc trưng. Ngoài ra nước dùng của tô bún nóng hổi, thơm ngon và rất chuẩn vị. Đây là một trong những địa chỉ nhất định bạn phải ghé khi đến phố ẩm thực Vĩnh Khánh.', 'Each bowl of fish noodle soup at Di Tu Chau Doc fish noodle restaurant is a harmonious combination of fresh fish, soft and smooth noodles and fresh vegetables, characteristic dipping sauce. In addition, the broth of the hot bowl of noodles is delicious and very authentic. This is one of the addresses you must visit when coming to Vinh Khanh food street.', '在Dì Tư Châu Đốc鱼粉餐厅，每碗鱼粉都是新鲜鱼肉、柔软光滑的面条和新鲜蔬菜、特色蘸酱的和谐组合。此外，热面碗的汤汁美味且非常正宗。这是您来到永庆美食街时必须参观的地址之一。', 10.78289, 106.70478, 4.6, '/Assets/Images/Q3.jpg', 30, 7, 'uploads/audio/food_3_vi.mp3', 'uploads/audio/food_3_en.mp3', 'uploads/audio/food_3_cn.mp3', 5),
(4, 'Bún thịt nướng Cô Nga', 'TP.HCM - Vĩnh Khánh', 'Bún', 'Phố ẩm thực Vĩnh Khánh ngoài nổi tiếng với các món ốc còn gây ấn tượng với thực khách ở món bún thịt nướng Sài Gòn. Trong các quán bún thịt nướng ở đây thì quán Cô Nga thu hút rất đông thực khách ghé đến thưởng thức. Một tô bún đầy đủ gồm bún, thịt nướng, chả giò… ăn kèm cùng rau sống, đồ chua.', 'Vinh Khanh food street, in addition to being famous for snail dishes, also impresses diners with Saigon grilled pork vermicelli. Among the grilled pork vermicelli restaurants here, Co Nga restaurant attracts a large number of diners to enjoy. A full bowl of vermicelli includes vermicelli, grilled pork, spring rolls... served with fresh vegetables and pickles.', '永庆美食街除了以蜗牛菜闻名外，还以西贡烤猪肉米粉给食客留下深刻印象。在这里的烤猪肉米粉餐厅中，Cô Nga餐厅吸引了大量食客前来享用。一碗完整的米粉包括米粉、烤猪肉、春卷...配以新鲜蔬菜和泡菜。', 10.78412, 106.70321, 4.5, '/Assets/Images/Q4.jpg', 30, 7, 'uploads/audio/food_4_vi.mp3', 'uploads/audio/food_4_en.mp3', 'uploads/audio/food_4_cn.mp3', 5),
(5, 'Ốc Vũ', 'TP.HCM - Vĩnh Khánh', 'Ốc', 'Ốc Vũ là địa điểm ăn vặt nổi tiếng ở phố ẩm thực Vĩnh Khánh với đa dạng món ốc ngon, giá phải chăng. Đồ ăn tại đây được chế biến tinh tế và nước chấm chua cay độc đáo chính là một trong những điểm cộng khiến quán lúc nào cũng đông khách.', 'Oc Vu is a famous snack spot on Vinh Khanh food street with a variety of delicious snail dishes at affordable prices. The food here is delicately prepared and the unique sweet and sour dipping sauce is one of the plus points that makes the restaurant always crowded.', 'Ốc Vũ是永庆美食街上著名的小吃店，拥有各种美味的蜗牛菜肴，价格实惠。这里的食物精心准备，独特的酸甜蘸酱是使餐厅总是人满为患的加分点之一。', 10.78567, 106.70189, 4.6, '/Assets/Images/Q5.jpg', 35, 8, 'uploads/audio/food_5_vi.mp3', 'uploads/audio/food_5_en.mp3', 'uploads/audio/food_5_cn.mp3', 5),
(6, 'Lãng Quán', 'TP.HCM - Vĩnh Khánh', 'Hải sản', 'Lãng Quán là địa chỉ ăn khuya Sài Gòn quen thuộc của rất nhiều người dân quận 4 cũ. Menu của quán đa dạng các món ăn ngon được chế biến từ mực, tôm, bạch tuộc, ếch… như tôm nướng muối ớt, bạch tuộc chiên giòn, mực hấp hành gừng…', 'Lang Quan is a familiar late-night eating address in Saigon for many people in old District 4. The restaurant menu has a variety of delicious dishes made from squid, shrimp, octopus, frog... such as grilled shrimp with salt and chili, crispy fried octopus, steamed squid with scallions and ginger...', 'Lãng Quán是许多老第4区人熟悉的西贡深夜用餐地址。餐厅菜单有各种美味的菜肴，由鱿鱼、虾、章鱼、青蛙制成...如盐和辣椒烤虾、脆炸章鱼、葱姜蒸鱿鱼...', 10.78741, 106.70067, 4.7, '/Assets/Images/Q6.jpg', 30, 8, 'uploads/audio/food_6_vi.mp3', 'uploads/audio/food_6_en.mp3', 'uploads/audio/food_6_cn.mp3', 5),
(7, 'Ớt Xiêm Quán', 'TP.HCM - Vĩnh Khánh', 'Hải sản', 'Ớt Xiêm Quán là một trong những quán ăn ngon tại phố ẩm thực Vĩnh Khánh được thực khách đánh giá cao. Đến quán bạn có thể lựa chọn các món ăn ngon như cá diêu hồng rang muối Hồng Kông, sò dương mỡ hành… Món nào cũng được quán lựa chọn nguyên liệu kỹ lưỡng và tẩm ướp đậm đà.', 'Ot Xiem Quan is one of the delicious restaurants on Vinh Khanh food street that is highly rated by diners. When you come to the restaurant, you can choose delicious dishes such as Hong Kong salt-fried red snapper, scallops with scallion oil... Every dish is carefully selected and marinated with rich flavors.', 'Ớt Xiêm Quán是永庆美食街上美味的餐厅之一，深受食客好评。当您来到餐厅时，您可以选择美味的菜肴，如香港盐炒红鲷鱼、葱油扇贝...每道菜都经过精心挑选和腌制，味道浓郁。', 10.78895, 106.69945, 4.7, '/Assets/Images/Q7.jpg', 30, 8, 'uploads/audio/food_7_vi.mp3', 'uploads/audio/food_7_en.mp3', 'uploads/audio/food_7_cn.mp3', 5),
(8, 'Lẩu nướng HongKong A FAT', 'TP.HCM - Vĩnh Khánh', 'Lẩu & Nướng', 'Lần đầu đặt chân đến Sài Gòn và bạn chưa biết ăn gì ở Sài Gòn vừa ngon vừa rẻ thì hãy thử ghé ngay Lẩu nướng HongKong A FAT. Quán ăn này nằm trong phố ẩm thực Vĩnh Khánh với đa dạng các món lẩu nướng, miến xào, cơm chiên, Pad Thái… Đặc biệt, không gian quán rộng rãi, sang trọng phù hợp với những buổi gặp gỡ bạn bè, liên hoan.', 'If you are visiting Saigon for the first time and do not know what to eat in Saigon that is both delicious and cheap, try visiting HongKong A FAT hot pot and grill. This restaurant is located on Vinh Khanh food street with a variety of hot pot and grilled dishes, stir-fried vermicelli, fried rice, Pad Thai... Especially, the spacious and luxurious restaurant space is suitable for meetings with friends and parties.', '如果您第一次访问西贡，不知道在西贡吃什么既美味又便宜，请尝试访问HongKong A FAT火锅和烧烤。这家餐厅位于永庆美食街，拥有各种火锅和烧烤菜肴、炒粉丝、炒饭、泰式炒河粉...特别是，宽敞豪华的餐厅空间适合与朋友聚会和派对。', 10.79024, 106.69823, 4.8, '/Assets/Images/Q8.jpg', 35, 9, 'uploads/audio/food_8_vi.mp3', 'uploads/audio/food_8_en.mp3', 'uploads/audio/food_8_cn.mp3', 5),
(9, 'Sườn Muối Ớt', 'TP.HCM - Vĩnh Khánh', 'Nướng', 'Không gian quán đẹp, đồ ăn ngon, nhân viên phục vụ nhanh nhẹn chính là những đánh giá của thực khách khi ghé quán Sườn Muối Ớt tại quận 4 cũ. Quán mở cửa từ 5h chiều đến 5h sáng hôm nay nên nếu bạn đang tìm địa chỉ ăn tối Sài Gòn đa dạng món, giá hợp lý thì nhất định đừng bỏ qua địa chỉ này.', 'Beautiful restaurant space, delicious food, and quick service are the reviews of diners when visiting Suon Muoi Ot restaurant in old District 4. The restaurant is open from 5pm to 5am today, so if you are looking for a dinner address in Saigon with a variety of dishes at reasonable prices, do not miss this address.', '美丽的餐厅空间、美味的食物和快速的服务是食客访问老第4区Sườn Muối Ớt餐厅时的评价。餐厅今天从下午5点开放到凌晨5点，所以如果您正在寻找西贡的晚餐地址，拥有各种价格合理的菜肴，请不要错过这个地址。', 10.79156, 106.69701, 4.6, '/Assets/Images/Q9.jpg', 30, 7, 'uploads/audio/food_9_vi.mp3', 'uploads/audio/food_9_en.mp3', 'uploads/audio/food_9_cn.mp3', 5),
(10, 'Chilli Quán', 'TP.HCM - Vĩnh Khánh', 'Nướng', 'Chilli Quán là một trong những quán nướng ngon ở Sài Gòn nằm trong phố ẩm thực Vĩnh Khánh. Đến quán, bạn sẽ được thưởng thức đa dạng các món ăn được chế biến từ nguyên liệu tươi ngon, tẩm ướp với những loại sốt đậm đà mà giá cả lại vô cùng phải chăng.', 'Chilli Quan is one of the delicious grilled restaurants in Saigon located on Vinh Khanh food street. When you come to the restaurant, you will enjoy a variety of dishes made from fresh ingredients, marinated with rich sauces at very affordable prices.', 'Chilli Quán是西贡美味的烧烤餐厅之一，位于永庆美食街。当您来到餐厅时，您将享受各种由新鲜食材制成的菜肴，用浓郁的酱汁腌制，价格非常实惠。', 10.79289, 106.69579, 4.7, '/Assets/Images/Q10.jpg', 30, 8, 'uploads/audio/food_10_vi.mp3', 'uploads/audio/food_10_en.mp3', 'uploads/audio/food_10_cn.mp3', 5),
(11, 'Thảo ốc quận 4 cũ', 'TP.HCM - Vĩnh Khánh', 'Ốc', 'Ốc tỏi nướng phô mai, bào ngư xào, ốc hương hoàng kim… đây là những món ăn best – seller tại quán Thảo ốc. Ngoài ra, quán còn rất nhiều món ngon khác có hương vị đậm đà, riêng biệt mà chắc chắn bạn thử 1 lần sẽ ghiền ngay.', 'Grilled garlic snails with cheese, stir-fried abalone, golden scented snails... these are the best-selling dishes at Thao Oc restaurant. In addition, the restaurant has many other delicious dishes with rich and unique flavors that you will definitely be addicted to after trying once.', '烤大蒜蜗牛配奶酪、炒鲍鱼、金色香螺...这些是Thảo ốc餐厅的畅销菜肴。此外，餐厅还有许多其他美味的菜肴，味道浓郁独特，您尝试一次后肯定会上瘾。', 10.79412, 106.69457, 4.8, '/Assets/Images/Q11.jpg', 35, 9, 'uploads/audio/food_11_vi.mp3', 'uploads/audio/food_11_en.mp3', 'uploads/audio/food_11_cn.mp3', 5),
(12, 'Ốc Oanh', 'TP.HCM - Vĩnh Khánh', 'Ốc', 'Quán Ốc Oanh trong phố ẩm thực Vĩnh Khánh là quán ốc ngon Sài Gòn nổi tiếng 20 năm. Ngoài các món được chế biến từ ốc thơm ngon, quán còn nổi tiếng với sò điệp nướng muối ớt, ghẹ rang muối ớt, sò dương nướng… Giá cả tại quán được đánh giá hợp lý, vừa túi tiền với người dân và thực khách bốn phương.', 'Oc Oanh restaurant on Vinh Khanh food street is a famous delicious snail restaurant in Saigon for 20 years. In addition to delicious dishes made from snails, the restaurant is also famous for grilled scallops with salt and chili, stir-fried crab with salt and chili, grilled scallops... The prices at the restaurant are considered reasonable and affordable for locals and visitors from all over.', '永庆美食街上的Ốc Oanh餐厅是西贡著名的美味蜗牛餐厅，已有20年历史。除了由蜗牛制成的美味菜肴外，餐厅还以盐和辣椒烤扇贝、盐和辣椒炒蟹、烤扇贝而闻名...餐厅的价格被认为是合理的，对当地人和来自各地的游客来说都是负担得起的。', 10.79567, 106.69335, 4.9, '/Assets/Images/Q12.jpg', 35, 9, 'uploads/audio/food_12_vi.mp3', 'uploads/audio/food_12_en.mp3', 'uploads/audio/food_12_cn.mp3', 5);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `qr_scans`
--

CREATE TABLE `qr_scans` (
  `id` int(11) NOT NULL,
  `device_id` varchar(255) NOT NULL,
  `qr_code` varchar(500) NOT NULL,
  `scan_date` datetime NOT NULL DEFAULT current_timestamp(),
  `device_name` varchar(255) DEFAULT NULL,
  `os_version` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `qr_scans`
--

INSERT INTO `qr_scans` (`id`, `device_id`, `qr_code`, `scan_date`, `device_name`, `os_version`) VALUES
(2, 'C0975651-6B2F-009D-4FC3-0A2DF7712E6E', 'VFG-TEST-2024', '2026-05-03 19:51:37', 'DESKTOP-M8QQCGI', 'Microsoft Windows NT 6.2.9200.0');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `sessions`
--

CREATE TABLE `sessions` (
  `Id` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `Token` varchar(255) NOT NULL,
  `ExpiresAt` datetime NOT NULL,
  `CreatedDate` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `sessions`
--

INSERT INTO `sessions` (`Id`, `UserId`, `Token`, `ExpiresAt`, `CreatedDate`) VALUES
(1, 2, 'a16fa46470f292cd2afab86ba7a963a5cff542bd03212e8520dd5447f9f6ada5', '2026-05-10 14:00:30', '2026-05-03 19:00:30'),
(2, 2, 'b3144d02bf512896d8e3b03c58708ee59fb527fa67b5b98de4fc69a1d6adcfec', '2026-05-10 14:44:43', '2026-05-03 19:44:43'),
(3, 2, '1361c32e7c2968f83fa33e49c1bf9bf3a912c322e621c07c68452cc11592ba00', '2026-05-10 14:50:15', '2026-05-03 19:50:15'),
(4, 2, 'b178d12c6869beaa5cae24d3847f50bc586847ac950c8bd70e12ca6c35a7c1e1', '2026-05-10 15:12:12', '2026-05-03 20:12:12'),
(5, 2, '40121d2565e6b3111d8cae2e7531bb821b27e4c5f547fa4440814ee6c5a52c75', '2026-05-10 15:25:05', '2026-05-03 20:25:05'),
(6, 2, 'b3f8db4cb7ac26679732b4391bcd4ce3d02d168425df73c0e9a0d90237c20fd7', '2026-05-10 15:40:19', '2026-05-03 20:40:19'),
(7, 2, '41f3b2e844dd9730bafc2d060db1aecdb6f28a3eb3190c21343423622d2a8962', '2026-05-10 17:13:55', '2026-05-03 22:13:55'),
(8, 2, 'ccba28c420cceadbb44cce757f66a833279f60e30108a8e13346ee6bc1a7e0be', '2026-05-10 17:32:40', '2026-05-03 22:32:40');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `users`
--

CREATE TABLE `users` (
  `Id` int(11) NOT NULL,
  `Username` varchar(100) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `Role` varchar(20) NOT NULL DEFAULT 'User',
  `CreatedDate` datetime NOT NULL DEFAULT current_timestamp(),
  `LastActiveTime` datetime DEFAULT NULL,
  `IsLocked` tinyint(1) DEFAULT 0 COMMENT 'TRUE = Tài khoản bị khóa, không đăng nhập được',
  `HasInstalledApp` tinyint(1) DEFAULT 0 COMMENT 'TRUE = Đã cài app và đăng nhập lần đầu',
  `FirstLoginDate` datetime DEFAULT NULL COMMENT 'Ngày đăng nhập lần đầu từ app'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `users`
--

INSERT INTO `users` (`Id`, `Username`, `PasswordHash`, `Role`, `CreatedDate`, `LastActiveTime`, `IsLocked`, `HasInstalledApp`, `FirstLoginDate`) VALUES
(1, 'admin', '$2y$10$T3ZbFpMS8HIJgtQZW4vDGu1Pr3hKjwTLOPvmVbZ7RXmIKQSxHrwY6', 'Admin', '2026-05-03 18:59:41', NULL, 0, 0, NULL),
(2, 'user123', '$2y$10$LUGiJfDAcNH36vJwJPH7YemBR4lpPTE6rEfcBxRqeLnR0ag.7Brdm', 'User', '2026-05-03 18:59:41', '2026-05-03 22:41:12', 0, 0, NULL);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `usertracking`
--

CREATE TABLE `usertracking` (
  `Id` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `CurrentLat` double DEFAULT NULL,
  `CurrentLng` double DEFAULT NULL,
  `DestinationLat` double DEFAULT NULL,
  `DestinationLng` double DEFAULT NULL,
  `DestinationName` varchar(255) DEFAULT NULL,
  `IsNavigating` tinyint(1) DEFAULT 0,
  `IsActive` tinyint(1) DEFAULT 1,
  `LastUpdate` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `CreatedDate` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `usertracking`
--

INSERT INTO `usertracking` (`Id`, `UserId`, `CurrentLat`, `CurrentLng`, `DestinationLat`, `DestinationLng`, `DestinationName`, `IsNavigating`, `IsActive`, `LastUpdate`, `CreatedDate`) VALUES
(1, 2, 10.789009711703, 106.65798953995, 10.79567, 106.69335, 'Ốc Oanh', 1, 1, '2026-05-03 22:37:34', '2026-05-03 19:00:30');

--
-- Chỉ mục cho các bảng đã đổ
--

--
-- Chỉ mục cho bảng `favorites`
--
ALTER TABLE `favorites`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `unique_favorite` (`UserId`,`FoodId`),
  ADD KEY `idx_user` (`UserId`),
  ADD KEY `idx_food` (`FoodId`);

--
-- Chỉ mục cho bảng `foods`
--
ALTER TABLE `foods`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `idx_category` (`Category`),
  ADD KEY `idx_rating` (`Rating`),
  ADD KEY `idx_priority` (`Priority`);

--
-- Chỉ mục cho bảng `qr_scans`
--
ALTER TABLE `qr_scans`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_device` (`device_id`),
  ADD KEY `idx_scan_date` (`scan_date`);

--
-- Chỉ mục cho bảng `sessions`
--
ALTER TABLE `sessions`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `idx_token` (`Token`),
  ADD KEY `idx_user` (`UserId`);

--
-- Chỉ mục cho bảng `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Username` (`Username`),
  ADD KEY `idx_username` (`Username`),
  ADD KEY `idx_last_active` (`LastActiveTime`),
  ADD KEY `idx_locked` (`IsLocked`),
  ADD KEY `idx_has_installed_app` (`HasInstalledApp`);

--
-- Chỉ mục cho bảng `usertracking`
--
ALTER TABLE `usertracking`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `unique_user_tracking` (`UserId`),
  ADD KEY `idx_user` (`UserId`),
  ADD KEY `idx_active` (`IsActive`),
  ADD KEY `idx_last_update` (`LastUpdate`);

--
-- AUTO_INCREMENT cho các bảng đã đổ
--

--
-- AUTO_INCREMENT cho bảng `favorites`
--
ALTER TABLE `favorites`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `foods`
--
ALTER TABLE `foods`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=13;

--
-- AUTO_INCREMENT cho bảng `qr_scans`
--
ALTER TABLE `qr_scans`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT cho bảng `sessions`
--
ALTER TABLE `sessions`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT cho bảng `users`
--
ALTER TABLE `users`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT cho bảng `usertracking`
--
ALTER TABLE `usertracking`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=255;

--
-- Các ràng buộc cho các bảng đã đổ
--

--
-- Các ràng buộc cho bảng `favorites`
--
ALTER TABLE `favorites`
  ADD CONSTRAINT `favorites_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `favorites_ibfk_2` FOREIGN KEY (`FoodId`) REFERENCES `foods` (`Id`) ON DELETE CASCADE;

--
-- Các ràng buộc cho bảng `sessions`
--
ALTER TABLE `sessions`
  ADD CONSTRAINT `sessions_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE;

--
-- Các ràng buộc cho bảng `usertracking`
--
ALTER TABLE `usertracking`
  ADD CONSTRAINT `usertracking_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
