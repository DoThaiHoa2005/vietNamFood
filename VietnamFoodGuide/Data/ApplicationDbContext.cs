using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using VietnamFoodGuide.Models.Entities;
using BCrypt.Net;

namespace VietnamFoodGuide.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<QRScan> QRScans { get; set; }

        private string _connectionString;

        public ApplicationDbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Parameterless constructor for design-time tools
        public ApplicationDbContext()
        {
            _connectionString = "Server=localhost;Port=3306;Database=VietnamFoodGuide;Uid=root;Pwd=;CharSet=utf8mb4;";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseMySql(_connectionString, mySqlOptions => mySqlOptions.ServerVersion("5.7"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed admin and user accounts
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
            var userPasswordHash = BCrypt.Net.BCrypt.HashPassword("user123");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = adminPasswordHash,
                    Role = UserRole.Admin,
                    CreatedDate = DateTime.Now
                },
                new User
                {
                    Id = 2,
                    Username = "user123",
                    PasswordHash = userPasswordHash,
                    Role = UserRole.User,
                    CreatedDate = DateTime.Now
                }
            );

            // Unique constraint for username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Foreign keys
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Food)
                .WithMany(f => f.Favorites)
                .HasForeignKey(f => f.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
