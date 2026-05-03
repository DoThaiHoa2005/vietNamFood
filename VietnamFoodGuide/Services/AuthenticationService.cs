using System;
using System.Linq;
using VietnamFoodGuide.Data;
using VietnamFoodGuide.Models.Entities;
using BCrypt.Net;

namespace VietnamFoodGuide.Services
{
    public class AuthenticationService
    {
        private readonly ApplicationDbContext _dbContext;

        public AuthenticationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Xác thực tài khoản người dùng
        /// </summary>
        public (bool Success, User User) Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, null);

            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    return (true, user);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auth error: {ex.Message}");
            }

            return (false, null);
        }

        /// <summary>
        /// Tạo tài khoản mới
        /// </summary>
        public bool CreateUser(string username, string password, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            if (_dbContext.Users.Any(u => u.Username == username))
                return false;

            try
            {
                var user = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = role,
                    CreatedDate = DateTime.Now
                };

                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lưu session (remember login)
        /// </summary>
        public void SaveSession(int userId, string token, DateTime expiry)
        {
            try
            {
                var session = new Session
                {
                    UserId = userId,
                    Token = token,
                    ExpiryDate = expiry
                };
                _dbContext.Sessions.Add(session);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Session save error: {ex.Message}");
            }
        }
    }
}
