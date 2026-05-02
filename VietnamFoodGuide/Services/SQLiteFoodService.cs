using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using VietnamFoodGuide.Models;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// Service quản lý dữ liệu Foods bằng SQLite (thay thế foods.json)
    /// </summary>
    public class SQLiteFoodService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public SQLiteFoodService()
        {
            // Đường dẫn database trong thư mục Data
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(appDir, "Data");
            
            // Tạo thư mục Data nếu chưa có
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            _dbPath = Path.Combine(dataDir, "foods.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";

            System.Diagnostics.Debug.WriteLine($"[SQLiteFoodService] Database path: {_dbPath}");

            // Khởi tạo database nếu chưa có
            InitializeDatabase();
        }

        /// <summary>
        /// Khởi tạo database và bảng Foods
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                // Nếu database chưa tồn tại, tạo mới
                if (!File.Exists(_dbPath))
                {
                    System.Diagnostics.Debug.WriteLine("[SQLiteFoodService] Creating new database...");
                    SQLiteConnection.CreateFile(_dbPath);
                    CreateTables();
                    InsertSampleData();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SQLiteFoodService] Database already exists");
                    // Kiểm tra và thêm cột mới nếu cần (migration)
                    MigrateDatabase();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFoodService] Error initializing database: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo bảng Foods
        /// </summary>
        private void CreateTables()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS Foods (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        City TEXT,
                        Category TEXT,
                        Image TEXT,
                        DescriptionVI TEXT,
                        DescriptionEN TEXT,
                        DescriptionCN TEXT,
                        Latitude REAL DEFAULT 0,
                        Longitude REAL DEFAULT 0,
                        Rating REAL DEFAULT 0,
                        Radius REAL DEFAULT 30.0,
                        Priority INTEGER DEFAULT 5,
                        AudioUrl TEXT,
                        NarrationScript TEXT,
                        CooldownMinutes INTEGER DEFAULT 5,
                        CreatedDate TEXT DEFAULT CURRENT_TIMESTAMP
                    )";

                using (var cmd = new SQLiteCommand(createTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                System.Diagnostics.Debug.WriteLine("[SQLiteFoodService] Table 'Foods' created successfully");
            }
        }

        /// <summary>
        /// Migration: Thêm cột mới nếu database cũ chưa có
        /// </summary>
        private void MigrateDatabase()
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    // Kiểm tra xem cột Radius đã tồn tại chưa
                    string checkColumnSql = "PRAGMA table_info(Foods)";
                    bool hasRadius = false;

                    using (var cmd = new SQLiteCommand(checkColumnSql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["name"].ToString() == "Radius")
                            {
                                hasRadius = true;
                                break;
                            }
                        }
                    }

                    // Nếu chưa có cột mới, thêm vào
                    if (!hasRadius)
                    {
                        System.Diagnostics.Debug.WriteLine("[SQLiteFoodService] Migrating database - adding new columns...");

                        string[] alterTableSqls = new[]
                        {
                            "ALTER TABLE Foods ADD COLUMN Radius REAL DEFAULT 30.0",
                            "ALTER TABLE Foods ADD COLUMN Priority INTEGER DEFAULT 5",
                            "ALTER TABLE Foods ADD COLUMN AudioUrl TEXT",
                            "ALTER TABLE Foods ADD COLUMN NarrationScript TEXT",
                            "ALTER TABLE Foods ADD COLUMN CooldownMinutes INTEGER DEFAULT 5"
                        };

                        foreach (var sql in alterTableSqls)
                        {
                            try
                            {
                                using (var cmd = new SQLiteCommand(sql, conn))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[SQLiteFoodService] Migration warning: {ex.Message}");
                            }
                        }

                        System.Diagnostics.Debug.WriteLine("[SQLiteFoodService] Migration completed");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFoodService] Migration error: {ex.Message}");
            }
        }

        /// <summary>
        /// Thêm dữ liệu mẫu (12 quán ăn thực tế ở Vĩnh Khánh)
        /// </summary>
        private void InsertSampleData()
        {
            System.Diagnostics.Debug.WriteLine("[SQLiteFoodService] Inserting sample data...");

            var sampleFoods = new List<FoodItem>
            {
                // Q1 - Alo Quán
                new FoodItem
                {
                    Name = "Alo Quán – Seafood & Beer",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Hải sản",
                    Image = "Assets/Images/Q1.jpg",
                    DescriptionVI = "Địa chỉ đầu tiên trên phố ẩm thực Vĩnh Khánh mà bạn không nên bỏ qua chính là Alo Quán – Seafood & Beer. Quán nổi tiếng với việc sử dụng các nguyên liệu tươi ngon, chế biến khéo léo để mang đến hương vị ngon nhất cho thực khách khi thưởng thức. Đặc biệt, nếu bạn đang tìm một địa chỉ nhậu ngon – bổ – rẻ thì Alo Quán – Seafood & Beer là lựa chọn lý tưởng.",
                    DescriptionEN = "The first address on Vinh Khanh food street that you should not miss is Alo Quan – Seafood & Beer. The restaurant is famous for using fresh ingredients, skillfully prepared to bring the best flavor to diners. Especially, if you are looking for a delicious, nutritious and cheap drinking place, Alo Quan – Seafood & Beer is the ideal choice.",
                    DescriptionCN = "永庆美食街上第一个不容错过的地址是Alo Quán – Seafood & Beer。餐厅以使用新鲜食材、精心烹制而闻名，为食客带来最佳风味。特别是，如果您正在寻找美味、营养且便宜的饮酒场所，Alo Quán – Seafood & Beer是理想的选择。",
                    Latitude = 10.78024,
                    Longitude = 106.70532,
                    Rating = 4.8,
                    Radius = 30.0,
                    Priority = 9,
                    NarrationScript = "Chào mừng bạn đến với Alo Quán – Seafood & Beer, địa chỉ nhậu ngon – bổ – rẻ với hải sản tươi ngon.",
                    CooldownMinutes = 5
                },
                // Q2 - Ốc Đào 2
                new FoodItem
                {
                    Name = "Ốc Đào 2",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Ốc",
                    Image = "Assets/Images/Q2.jpg",
                    DescriptionVI = "Quán Ốc Đào 2 là địa điểm ẩm thực tuyệt vời với hơn 30 loài ốc khác nhau được quán chế biến thành những món ăn độc đáo và ngon miệng. Thực khách có thể thưởng thức các món ốc hấp, xào… được nêm nếm vừa miệng và hương vị thơm ngon nhất.",
                    DescriptionEN = "Oc Dao 2 restaurant is a great culinary destination with more than 30 different types of snails that the restaurant prepares into unique and delicious dishes. Diners can enjoy steamed and stir-fried snails... seasoned to taste and with the most delicious flavor.",
                    DescriptionCN = "Ốc Đào 2餐厅是一个很棒的美食目的地，拥有30多种不同类型的蜗牛，餐厅将其制作成独特而美味的菜肴。食客可以享受蒸和炒蜗牛...调味适口，味道最美味。",
                    Latitude = 10.78156,
                    Longitude = 106.70614,
                    Rating = 4.7,
                    Radius = 35.0,
                    Priority = 8,
                    NarrationScript = "Chào mừng bạn đến với Ốc Đào 2, nơi có hơn 30 loài ốc được chế biến thành những món ăn độc đáo.",
                    CooldownMinutes = 5
                },
                // Q3 - Bún cá Châu Đốc Dì Tư
                new FoodItem
                {
                    Name = "Bún cá Châu Đốc Dì Tư",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Bún",
                    Image = "Assets/Images/Q3.jpg",
                    DescriptionVI = "Mỗi tô bún cá tại quán bún cá Châu Đốc Dì Tư là sự kết hợp hài hòa giữa thịt cá tươi ngon, bún mềm mịn và các loại rau sống, nước chấm đặc trưng. Ngoài ra nước dùng của tô bún nóng hổi, thơm ngon và rất chuẩn vị. Đây là một trong những địa chỉ nhất định bạn phải ghé khi đến phố ẩm thực Vĩnh Khánh.",
                    DescriptionEN = "Each bowl of fish noodle soup at Di Tu Chau Doc fish noodle restaurant is a harmonious combination of fresh fish, soft and smooth noodles and fresh vegetables, characteristic dipping sauce. In addition, the broth of the hot bowl of noodles is delicious and very authentic. This is one of the addresses you must visit when coming to Vinh Khanh food street.",
                    DescriptionCN = "在Dì Tư Châu Đốc鱼粉餐厅，每碗鱼粉都是新鲜鱼肉、柔软光滑的面条和新鲜蔬菜、特色蘸酱的和谐组合。此外，热面碗的汤汁美味且非常正宗。这是您来到永庆美食街时必须参观的地址之一。",
                    Latitude = 10.78289,
                    Longitude = 106.70478,
                    Rating = 4.6,
                    Radius = 30.0,
                    Priority = 7,
                    NarrationScript = "Chào mừng bạn đến với Bún cá Châu Đốc Dì Tư, nơi có bún cá với nước dùng thơm ngon chuẩn vị.",
                    CooldownMinutes = 5
                },
                // Q4 - Bún thịt nướng Cô Nga
                new FoodItem
                {
                    Name = "Bún thịt nướng Cô Nga",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Bún",
                    Image = "Assets/Images/Q4.jpg",
                    DescriptionVI = "Phố ẩm thực Vĩnh Khánh ngoài nổi tiếng với các món ốc còn gây ấn tượng với thực khách ở món bún thịt nướng Sài Gòn. Trong các quán bún thịt nướng ở đây thì quán Cô Nga thu hút rất đông thực khách ghé đến thưởng thức. Một tô bún đầy đủ gồm bún, thịt nướng, chả giò… ăn kèm cùng rau sống, đồ chua.",
                    DescriptionEN = "Vinh Khanh food street, in addition to being famous for snail dishes, also impresses diners with Saigon grilled pork vermicelli. Among the grilled pork vermicelli restaurants here, Co Nga restaurant attracts a large number of diners to enjoy. A full bowl of vermicelli includes vermicelli, grilled pork, spring rolls... served with fresh vegetables and pickles.",
                    DescriptionCN = "永庆美食街除了以蜗牛菜闻名外，还以西贡烤猪肉米粉给食客留下深刻印象。在这里的烤猪肉米粉餐厅中，Cô Nga餐厅吸引了大量食客前来享用。一碗完整的米粉包括米粉、烤猪肉、春卷...配以新鲜蔬菜和泡菜。",
                    Latitude = 10.78412,
                    Longitude = 106.70321,
                    Rating = 4.5,
                    Radius = 30.0,
                    Priority = 7,
                    NarrationScript = "Chào mừng bạn đến với Bún thịt nướng Cô Nga, nơi có bún thịt nướng Sài Gòn đầy đủ và ngon miệng.",
                    CooldownMinutes = 5
                },
                // Q5 - Ốc Vũ
                new FoodItem
                {
                    Name = "Ốc Vũ",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Ốc",
                    Image = "Assets/Images/Q5.jpg",
                    DescriptionVI = "Ốc Vũ là địa điểm ăn vặt nổi tiếng ở phố ẩm thực Vĩnh Khánh với đa dạng món ốc ngon, giá phải chăng. Đồ ăn tại đây được chế biến tinh tế và nước chấm chua cay độc đáo chính là một trong những điểm cộng khiến quán lúc nào cũng đông khách.",
                    DescriptionEN = "Oc Vu is a famous snack spot on Vinh Khanh food street with a variety of delicious snail dishes at affordable prices. The food here is delicately prepared and the unique sweet and sour dipping sauce is one of the plus points that makes the restaurant always crowded.",
                    DescriptionCN = "Ốc Vũ是永庆美食街上著名的小吃店，拥有各种美味的蜗牛菜肴，价格实惠。这里的食物精心准备，独特的酸甜蘸酱是使餐厅总是人满为患的加分点之一。",
                    Latitude = 10.78567,
                    Longitude = 106.70189,
                    Rating = 4.6,
                    Radius = 35.0,
                    Priority = 8,
                    NarrationScript = "Chào mừng bạn đến với Ốc Vũ, địa điểm ăn vặt nổi tiếng với món ốc ngon và nước chấm chua cay độc đáo.",
                    CooldownMinutes = 5
                },
                // Q6 - Lãng Quán
                new FoodItem
                {
                    Name = "Lãng Quán",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Hải sản",
                    Image = "Assets/Images/Q6.jpg",
                    DescriptionVI = "Lãng Quán là địa chỉ ăn khuya Sài Gòn quen thuộc của rất nhiều người dân quận 4 cũ. Menu của quán đa dạng các món ăn ngon được chế biến từ mực, tôm, bạch tuộc, ếch… như tôm nướng muối ớt, bạch tuộc chiên giòn, mực hấp hành gừng…",
                    DescriptionEN = "Lang Quan is a familiar late-night eating address in Saigon for many people in old District 4. The restaurant menu has a variety of delicious dishes made from squid, shrimp, octopus, frog... such as grilled shrimp with salt and chili, crispy fried octopus, steamed squid with scallions and ginger...",
                    DescriptionCN = "Lãng Quán是许多老第4区人熟悉的西贡深夜用餐地址。餐厅菜单有各种美味的菜肴，由鱿鱼、虾、章鱼、青蛙制成...如盐和辣椒烤虾、脆炸章鱼、葱姜蒸鱿鱼...",
                    Latitude = 10.78741,
                    Longitude = 106.70067,
                    Rating = 4.7,
                    Radius = 30.0,
                    Priority = 8,
                    NarrationScript = "Chào mừng bạn đến với Lãng Quán, địa chỉ ăn khuya quen thuộc với menu đa dạng hải sản.",
                    CooldownMinutes = 5
                },
                // Q7 - Ớt Xiêm Quán
                new FoodItem
                {
                    Name = "Ớt Xiêm Quán",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Hải sản",
                    Image = "Assets/Images/Q7.jpg",
                    DescriptionVI = "Ớt Xiêm Quán là một trong những quán ăn ngon tại phố ẩm thực Vĩnh Khánh được thực khách đánh giá cao. Đến quán bạn có thể lựa chọn các món ăn ngon như cá diêu hồng rang muối Hồng Kông, sò dương mỡ hành… Món nào cũng được quán lựa chọn nguyên liệu kỹ lưỡng và tẩm ướp đậm đà.",
                    DescriptionEN = "Ot Xiem Quan is one of the delicious restaurants on Vinh Khanh food street that is highly rated by diners. When you come to the restaurant, you can choose delicious dishes such as Hong Kong salt-fried red snapper, scallops with scallion oil... Every dish is carefully selected and marinated with rich flavors.",
                    DescriptionCN = "Ớt Xiêm Quán是永庆美食街上美味的餐厅之一，深受食客好评。当您来到餐厅时，您可以选择美味的菜肴，如香港盐炒红鲷鱼、葱油扇贝...每道菜都经过精心挑选和腌制，味道浓郁。",
                    Latitude = 10.78895,
                    Longitude = 106.69945,
                    Rating = 4.7,
                    Radius = 30.0,
                    Priority = 8,
                    NarrationScript = "Chào mừng bạn đến với Ớt Xiêm Quán, nơi có các món hải sản được chế biến tinh tế và đậm đà.",
                    CooldownMinutes = 5
                },
                // Q8 - Lẩu nướng HongKong A FAT
                new FoodItem
                {
                    Name = "Lẩu nướng HongKong A FAT",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Lẩu & Nướng",
                    Image = "Assets/Images/Q8.jpg",
                    DescriptionVI = "Lần đầu đặt chân đến Sài Gòn và bạn chưa biết ăn gì ở Sài Gòn vừa ngon vừa rẻ thì hãy thử ghé ngay Lẩu nướng HongKong A FAT. Quán ăn này nằm trong phố ẩm thực Vĩnh Khánh với đa dạng các món lẩu nướng, miến xào, cơm chiên, Pad Thái… Đặc biệt, không gian quán rộng rãi, sang trọng phù hợp với những buổi gặp gỡ bạn bè, liên hoan.",
                    DescriptionEN = "If you are visiting Saigon for the first time and do not know what to eat in Saigon that is both delicious and cheap, try visiting HongKong A FAT hot pot and grill. This restaurant is located on Vinh Khanh food street with a variety of hot pot and grilled dishes, stir-fried vermicelli, fried rice, Pad Thai... Especially, the spacious and luxurious restaurant space is suitable for meetings with friends and parties.",
                    DescriptionCN = "如果您第一次访问西贡，不知道在西贡吃什么既美味又便宜，请尝试访问HongKong A FAT火锅和烧烤。这家餐厅位于永庆美食街，拥有各种火锅和烧烤菜肴、炒粉丝、炒饭、泰式炒河粉...特别是，宽敞豪华的餐厅空间适合与朋友聚会和派对。",
                    Latitude = 10.79024,
                    Longitude = 106.69823,
                    Rating = 4.8,
                    Radius = 35.0,
                    Priority = 9,
                    NarrationScript = "Chào mừng bạn đến với Lẩu nướng HongKong A FAT, nơi có không gian rộng rãi và đa dạng món lẩu nướng.",
                    CooldownMinutes = 5
                },
                // Q9 - Sườn Muối Ớt
                new FoodItem
                {
                    Name = "Sườn Muối Ớt",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Nướng",
                    Image = "Assets/Images/Q9.jpg",
                    DescriptionVI = "Không gian quán đẹp, đồ ăn ngon, nhân viên phục vụ nhanh nhẹn chính là những đánh giá của thực khách khi ghé quán Sườn Muối Ớt tại quận 4 cũ. Quán mở cửa từ 5h chiều đến 5h sáng hôm nay nên nếu bạn đang tìm địa chỉ ăn tối Sài Gòn đa dạng món, giá hợp lý thì nhất định đừng bỏ qua địa chỉ này.",
                    DescriptionEN = "Beautiful restaurant space, delicious food, and quick service are the reviews of diners when visiting Suon Muoi Ot restaurant in old District 4. The restaurant is open from 5pm to 5am today, so if you are looking for a dinner address in Saigon with a variety of dishes at reasonable prices, do not miss this address.",
                    DescriptionCN = "美丽的餐厅空间、美味的食物和快速的服务是食客访问老第4区Sườn Muối Ớt餐厅时的评价。餐厅今天从下午5点开放到凌晨5点，所以如果您正在寻找西贡的晚餐地址，拥有各种价格合理的菜肴，请不要错过这个地址。",
                    Latitude = 10.79156,
                    Longitude = 106.69701,
                    Rating = 4.6,
                    Radius = 30.0,
                    Priority = 7,
                    NarrationScript = "Chào mừng bạn đến với Sườn Muối Ớt, nơi có không gian đẹp và đồ ăn ngon với giá hợp lý.",
                    CooldownMinutes = 5
                },
                // Q10 - Chilli Quán
                new FoodItem
                {
                    Name = "Chilli Quán",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Nướng",
                    Image = "Assets/Images/Q10.jpg",
                    DescriptionVI = "Chilli Quán là một trong những quán nướng ngon ở Sài Gòn nằm trong phố ẩm thực Vĩnh Khánh. Đến quán, bạn sẽ được thưởng thức đa dạng các món ăn được chế biến từ nguyên liệu tươi ngon, tẩm ướp với những loại sốt đậm đà mà giá cả lại vô cùng phải chăng.",
                    DescriptionEN = "Chilli Quan is one of the delicious grilled restaurants in Saigon located on Vinh Khanh food street. When you come to the restaurant, you will enjoy a variety of dishes made from fresh ingredients, marinated with rich sauces at very affordable prices.",
                    DescriptionCN = "Chilli Quán是西贡美味的烧烤餐厅之一，位于永庆美食街。当您来到餐厅时，您将享受各种由新鲜食材制成的菜肴，用浓郁的酱汁腌制，价格非常实惠。",
                    Latitude = 10.79289,
                    Longitude = 106.69579,
                    Rating = 4.7,
                    Radius = 30.0,
                    Priority = 8,
                    NarrationScript = "Chào mừng bạn đến với Chilli Quán, nơi có các món nướng ngon với giá phải chăng.",
                    CooldownMinutes = 5
                },
                // Q11 - Thảo ốc quận 4 cũ
                new FoodItem
                {
                    Name = "Thảo ốc quận 4 cũ",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Ốc",
                    Image = "Assets/Images/Q11.jpg",
                    DescriptionVI = "Ốc tỏi nướng phô mai, bào ngư xào, ốc hương hoàng kim… đây là những món ăn best – seller tại quán Thảo ốc. Ngoài ra, quán còn rất nhiều món ngon khác có hương vị đậm đà, riêng biệt mà chắc chắn bạn thử 1 lần sẽ ghiền ngay.",
                    DescriptionEN = "Grilled garlic snails with cheese, stir-fried abalone, golden scented snails... these are the best-selling dishes at Thao Oc restaurant. In addition, the restaurant has many other delicious dishes with rich and unique flavors that you will definitely be addicted to after trying once.",
                    DescriptionCN = "烤大蒜蜗牛配奶酪、炒鲍鱼、金色香螺...这些是Thảo ốc餐厅的畅销菜肴。此外，餐厅还有许多其他美味的菜肴，味道浓郁独特，您尝试一次后肯定会上瘾。",
                    Latitude = 10.79412,
                    Longitude = 106.69457,
                    Rating = 4.8,
                    Radius = 35.0,
                    Priority = 9,
                    NarrationScript = "Chào mừng bạn đến với Thảo ốc quận 4 cũ, nơi có các món ốc best-seller với hương vị đậm đà.",
                    CooldownMinutes = 5
                },
                // Q12 - Ốc Oanh
                new FoodItem
                {
                    Name = "Ốc Oanh",
                    City = "TP.HCM - Vĩnh Khánh",
                    Category = "Ốc",
                    Image = "Assets/Images/Q12.jpg",
                    DescriptionVI = "Quán Ốc Oanh trong phố ẩm thực Vĩnh Khánh là quán ốc ngon Sài Gòn nổi tiếng 20 năm. Ngoài các món được chế biến từ ốc thơm ngon, quán còn nổi tiếng với sò điệp nướng muối ớt, ghẹ rang muối ớt, sò dương nướng… Giá cả tại quán được đánh giá hợp lý, vừa túi tiền với người dân và thực khách bốn phương.",
                    DescriptionEN = "Oc Oanh restaurant on Vinh Khanh food street is a famous delicious snail restaurant in Saigon for 20 years. In addition to delicious dishes made from snails, the restaurant is also famous for grilled scallops with salt and chili, stir-fried crab with salt and chili, grilled scallops... The prices at the restaurant are considered reasonable and affordable for locals and visitors from all over.",
                    DescriptionCN = "永庆美食街上的Ốc Oanh餐厅是西贡著名的美味蜗牛餐厅，已有20年历史。除了由蜗牛制成的美味菜肴外，餐厅还以盐和辣椒烤扇贝、盐和辣椒炒蟹、烤扇贝而闻名...餐厅的价格被认为是合理的，对当地人和来自各地的游客来说都是负担得起的。",
                    Latitude = 10.79567,
                    Longitude = 106.69335,
                    Rating = 4.9,
                    Radius = 35.0,
                    Priority = 9,
                    NarrationScript = "Chào mừng bạn đến với Ốc Oanh, quán ốc ngon nổi tiếng 20 năm với giá cả hợp lý.",
                    CooldownMinutes = 5
                }
            };

            foreach (var food in sampleFoods)
            {
                InsertFood(food);
            }

            System.Diagnostics.Debug.WriteLine($"[SQLiteFoodService] Inserted {sampleFoods.Count} sample foods");
        }

        /// <summary>
        /// Thêm một quán ăn vào database
        /// </summary>
        public void InsertFood(FoodItem food)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                string insertSql = @"
                    INSERT INTO Foods (Name, City, Category, Image, DescriptionVI, DescriptionEN, DescriptionCN, 
                                       Latitude, Longitude, Rating, Radius, Priority, AudioUrl, NarrationScript, CooldownMinutes)
                    VALUES (@Name, @City, @Category, @Image, @DescriptionVI, @DescriptionEN, @DescriptionCN,
                            @Latitude, @Longitude, @Rating, @Radius, @Priority, @AudioUrl, @NarrationScript, @CooldownMinutes)";

                using (var cmd = new SQLiteCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", food.Name);
                    cmd.Parameters.AddWithValue("@City", food.City ?? "");
                    cmd.Parameters.AddWithValue("@Category", food.Category ?? "");
                    cmd.Parameters.AddWithValue("@Image", food.Image ?? "");
                    cmd.Parameters.AddWithValue("@DescriptionVI", food.DescriptionVI ?? "");
                    cmd.Parameters.AddWithValue("@DescriptionEN", food.DescriptionEN ?? "");
                    cmd.Parameters.AddWithValue("@DescriptionCN", food.DescriptionCN ?? "");
                    cmd.Parameters.AddWithValue("@Latitude", food.Latitude);
                    cmd.Parameters.AddWithValue("@Longitude", food.Longitude);
                    cmd.Parameters.AddWithValue("@Rating", food.Rating);
                    cmd.Parameters.AddWithValue("@Radius", food.Radius);
                    cmd.Parameters.AddWithValue("@Priority", food.Priority);
                    cmd.Parameters.AddWithValue("@AudioUrl", food.AudioUrl ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NarrationScript", food.NarrationScript ?? "");
                    cmd.Parameters.AddWithValue("@CooldownMinutes", food.CooldownMinutes);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Lấy tất cả quán ăn từ SQLite
        /// </summary>
        public List<FoodItem> LoadFoods()
        {
            var foods = new List<FoodItem>();

            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string selectSql = "SELECT * FROM Foods ORDER BY Rating DESC";

                    using (var cmd = new SQLiteCommand(selectSql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foods.Add(new FoodItem
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                City = reader["City"].ToString(),
                                Category = reader["Category"].ToString(),
                                Image = reader["Image"].ToString(),
                                DescriptionVI = reader["DescriptionVI"].ToString(),
                                DescriptionEN = reader["DescriptionEN"].ToString(),
                                DescriptionCN = reader["DescriptionCN"].ToString(),
                                Latitude = Convert.ToDouble(reader["Latitude"]),
                                Longitude = Convert.ToDouble(reader["Longitude"]),
                                Rating = Convert.ToDouble(reader["Rating"]),
                                Radius = reader["Radius"] != DBNull.Value ? Convert.ToDouble(reader["Radius"]) : 30.0,
                                Priority = reader["Priority"] != DBNull.Value ? Convert.ToInt32(reader["Priority"]) : 5,
                                AudioUrl = reader["AudioUrl"] != DBNull.Value ? reader["AudioUrl"].ToString() : null,
                                NarrationScript = reader["NarrationScript"] != DBNull.Value ? reader["NarrationScript"].ToString() : null,
                                CooldownMinutes = reader["CooldownMinutes"] != DBNull.Value ? Convert.ToInt32(reader["CooldownMinutes"]) : 5
                            });
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[SQLiteFoodService] Loaded {foods.Count} foods from SQLite");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFoodService] Error loading foods: {ex.Message}");
            }

            return foods;
        }

        /// <summary>
        /// Sync dữ liệu từ API về SQLite
        /// </summary>
        public void SyncFromAPI(List<FoodItem> apiFoods)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    // Xóa dữ liệu cũ
                    using (var cmd = new SQLiteCommand("DELETE FROM Foods", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Thêm dữ liệu mới từ API
                    foreach (var food in apiFoods)
                    {
                        InsertFood(food);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[SQLiteFoodService] Synced {apiFoods.Count} foods from API to SQLite");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteFoodService] Error syncing from API: {ex.Message}");
            }
        }
    }
}
