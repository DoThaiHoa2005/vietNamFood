using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.Configuration;
using VietnamFoodGuide.Data;
using VietnamFoodGuide.Models.Entities;
using VietnamFoodGuide.Services;
using VietnamFoodGuide.Views;

namespace VietnamFoodGuide
{
    public partial class App : Application
    {
        /// <summary>
        /// Current logged-in user (shared across application)
        /// </summary>
        public static User CurrentUser { get; set; }

        /// <summary>
        /// Current logged-in user from API (XAMPP)
        /// </summary>
        public static UserInfo CurrentApiUser { get; set; }

        /// <summary>
        /// Database context (shared across application)
        /// </summary>
        public static ApplicationDbContext DbContext { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Load configuration from appsettings.json
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Initialize database with configuration
                DbContext = new ApplicationDbContext(configuration);

                // Create database if not exists and apply migrations
                DbContext.Database.EnsureCreated();

                // Seed foods if empty (on first run)
                SeedFoodsIfEmpty();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo database: {ex.Message}",
                               "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Show Login Window first
            LoginWindow loginWindow = new LoginWindow(DbContext);
            loginWindow.Show();
        }

        private void SeedFoodsIfEmpty()
        {
            // Check if Foods table is empty
            if (DbContext.Foods.Count() == 0)
            {
                // Load foods from JSON and insert into database
                try
                {
                    string jsonPath = System.IO.Path.Combine(
                        System.AppDomain.CurrentDomain.BaseDirectory,
                        "Data",
                        "foods.json"
                    );

                    if (System.IO.File.Exists(jsonPath))
                    {
                        string json = System.IO.File.ReadAllText(jsonPath);
                        var foods = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<Food>>(json);

                        if (foods != null && foods.Count > 0)
                        {
                            DbContext.Foods.AddRange(foods);
                            DbContext.SaveChanges();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Seed error: {ex.Message}");
                }
            }
        }
    }
}
