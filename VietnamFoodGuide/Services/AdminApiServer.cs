using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VietnamFoodGuide.Data;
using VietnamFoodGuide.Models.Entities;

namespace VietnamFoodGuide.Services
{
    /// <summary>
    /// HTTP API Server for Admin Dashboard
    /// Runs on localhost:5555 and provides REST endpoints for dashboard operations
    /// </summary>
    public class AdminApiServer
    {
        private readonly ApplicationDbContext _dbContext;
        private HttpListener _httpListener;
        private bool _isRunning = false;
        private Task _listenerTask;

        public AdminApiServer(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Start the HTTP API server
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add("http://localhost:5555/");
                _httpListener.Start();
                _isRunning = true;

                // Listen for requests on a background thread
                _listenerTask = Task.Run(() => ListenForRequests());
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to start Admin API Server: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop the HTTP API server
        /// </summary>
        public void Stop()
        {
            if (_isRunning)
            {
                _isRunning = false;
                try
                {
                    _httpListener?.Stop();
                    _httpListener?.Close();
                }
                catch { }
            }
        }

        private void ListenForRequests()
        {
            while (_isRunning)
            {
                try
                {
                    var context = _httpListener.GetContext();
                    ProcessRequest(context);
                }
                catch (Exception)
                {
                    // Ignore listener closed exceptions
                    if (_isRunning)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                string method = context.Request.HttpMethod;
                string path = context.Request.Url.AbsolutePath;
                string responseText = "";
                int statusCode = 404;

                // Parse API routes
                if (path == "/api/dashboard/stats" && method == "GET")
                {
                    responseText = GetDashboardStats();
                    statusCode = 200;
                }
                else if (path == "/api/users" && method == "GET")
                {
                    responseText = GetUsers();
                    statusCode = 200;
                }
                else if (path == "/api/users" && method == "POST")
                {
                    responseText = CreateUser(context.Request);
                    statusCode = 201;
                }
                else if (path.StartsWith("/api/users/") && method == "PUT")
                {
                    int userId = int.Parse(path.Split('/')[3]);
                    responseText = UpdateUser(userId, context.Request);
                    statusCode = 200;
                }
                else if (path.StartsWith("/api/users/") && method == "DELETE")
                {
                    int userId = int.Parse(path.Split('/')[3]);
                    responseText = DeleteUser(userId);
                    statusCode = 200;
                }
                else if (path == "/api/foods" && method == "GET")
                {
                    responseText = GetFoods();
                    statusCode = 200;
                }
                else if (path == "/api/foods" && method == "POST")
                {
                    responseText = CreateFood(context.Request);
                    statusCode = 201;
                }
                else if (path.StartsWith("/api/foods/") && method == "PUT")
                {
                    int foodId = int.Parse(path.Split('/')[3]);
                    responseText = UpdateFood(foodId, context.Request);
                    statusCode = 200;
                }
                else if (path.StartsWith("/api/foods/") && method == "DELETE")
                {
                    int foodId = int.Parse(path.Split('/')[3]);
                    responseText = DeleteFood(foodId);
                    statusCode = 200;
                }
                else if (path == "/api/favorites" && method == "GET")
                {
                    responseText = GetFavorites();
                    statusCode = 200;
                }
                else
                {
                    responseText = "{\"error\": \"Route not found\"}";
                    statusCode = 404;
                }

                // Send response
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

                byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                SendErrorResponse(context, ex.Message);
            }
        }

        private string GetDashboardStats()
        {
            var stats = new
            {
                totalUsers = _dbContext.Users.Count(),
                totalFoods = _dbContext.Foods.Count(),
                totalFavorites = _dbContext.Favorites.Count(),
                activityRate = "100%"
            };
            return JsonSerializer.Serialize(stats);
        }

        private string GetUsers()
        {
            var users = _dbContext.Users.Select(u => new
            {
                id = u.Id,
                username = u.Username,
                role = u.Role.ToString(),
                createdDate = u.CreatedDate
            }).ToList();
            return JsonSerializer.Serialize(users);
        }

        private string CreateUser(HttpListenerRequest request)
        {
            using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
            {
                string body = reader.ReadToEnd();
                var data = JsonSerializer.Deserialize<JsonElement>(body);

                string username = data.GetProperty("username").GetString();
                string password = data.GetProperty("password").GetString();
                string roleStr = data.GetProperty("role").GetString();

                // Validate
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    return "{\"error\": \"Username and password required\"}";

                // Check if user exists
                if (_dbContext.Users.Any(u => u.Username == username))
                    return "{\"error\": \"Username already exists\"}";

                // Create user
                var role = roleStr == "0" ? UserRole.Admin : UserRole.User;
                var newUser = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = role,
                    CreatedDate = DateTime.Now
                };

                _dbContext.Users.Add(newUser);
                _dbContext.SaveChanges();

                return JsonSerializer.Serialize(new { success = true, userId = newUser.Id });
            }
        }

        private string UpdateUser(int userId, HttpListenerRequest request)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return "{\"error\": \"User not found\"}";

            using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
            {
                string body = reader.ReadToEnd();
                var data = JsonSerializer.Deserialize<JsonElement>(body);

                if (data.TryGetProperty("username", out var usernameProp))
                    user.Username = usernameProp.GetString();

                if (data.TryGetProperty("password", out var passwordProp))
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordProp.GetString());

                if (data.TryGetProperty("role", out var roleProp))
                    user.Role = roleProp.GetString() == "0" ? UserRole.Admin : UserRole.User;

                _dbContext.SaveChanges();
                return "{\"success\": true}";
            }
        }

        private string DeleteUser(int userId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return "{\"error\": \"User not found\"}";

            // Don't allow deleting the only admin
            if (user.Role == UserRole.Admin && _dbContext.Users.Count(u => u.Role == UserRole.Admin) == 1)
                return "{\"error\": \"Cannot delete the only admin user\"}";

            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
            return "{\"success\": true}";
        }

        private string GetFoods()
        {
            var foods = _dbContext.Foods.Select(f => new
            {
                id = f.Id,
                name = f.Name,
                city = f.City,
                category = f.Category,
                rating = f.Rating,
                description = f.Description_VI,
                image = f.ImagePath
            }).ToList();
            return JsonSerializer.Serialize(foods);
        }

        private string CreateFood(HttpListenerRequest request)
        {
            using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
            {
                string body = reader.ReadToEnd();
                var data = JsonSerializer.Deserialize<JsonElement>(body);

                var newFood = new Food
                {
                    Name = data.GetProperty("name").GetString(),
                    City = data.GetProperty("city").GetString(),
                    Category = data.GetProperty("category").GetString(),
                    Rating = float.Parse(data.GetProperty("rating").GetString() ?? "4.0"),
                    Description_VI = data.GetProperty("description").GetString(),
                    Latitude = 0,
                    Longitude = 0
                };

                _dbContext.Foods.Add(newFood);
                _dbContext.SaveChanges();

                return JsonSerializer.Serialize(new { success = true, foodId = newFood.Id });
            }
        }

        private string UpdateFood(int foodId, HttpListenerRequest request)
        {
            var food = _dbContext.Foods.FirstOrDefault(f => f.Id == foodId);
            if (food == null)
                return "{\"error\": \"Food not found\"}";

            using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
            {
                string body = reader.ReadToEnd();
                var data = JsonSerializer.Deserialize<JsonElement>(body);

                if (data.TryGetProperty("name", out var nameProp))
                    food.Name = nameProp.GetString();
                if (data.TryGetProperty("city", out var cityProp))
                    food.City = cityProp.GetString();
                if (data.TryGetProperty("category", out var categoryProp))
                    food.Category = categoryProp.GetString();
                if (data.TryGetProperty("rating", out var ratingProp))
                    food.Rating = float.Parse(ratingProp.GetString() ?? "4.0");
                if (data.TryGetProperty("description", out var descProp))
                    food.Description_VI = descProp.GetString();

                _dbContext.SaveChanges();
                return "{\"success\": true}";
            }
        }

        private string DeleteFood(int foodId)
        {
            var food = _dbContext.Foods.FirstOrDefault(f => f.Id == foodId);
            if (food == null)
                return "{\"error\": \"Food not found\"}";

            _dbContext.Foods.Remove(food);
            _dbContext.SaveChanges();
            return "{\"success\": true}";
        }

        private string GetFavorites()
        {
            var favorites = _dbContext.Favorites.Select(f => new
            {
                id = f.Id,
                userId = f.UserId,
                foodId = f.FoodId,
                addedDate = f.AddedDate
            }).ToList();
            return JsonSerializer.Serialize(favorites);
        }

        private void SendErrorResponse(HttpListenerContext context, string message)
        {
            try
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                string responseText = JsonSerializer.Serialize(new { error = message });
                byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
            catch { }
        }
    }
}
