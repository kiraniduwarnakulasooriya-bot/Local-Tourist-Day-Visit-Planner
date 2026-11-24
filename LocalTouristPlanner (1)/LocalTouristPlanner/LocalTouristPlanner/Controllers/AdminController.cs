using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using LocalTouristPlanner.Data;
using LocalTouristPlanner.Models;

namespace LocalTouristPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;

        public AdminController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 1. Admin Login API
        [HttpPost("login")]
        public IActionResult Login([FromBody] AdminLoginModel model)
        {
            using (var conn = _dbContext.GetConnection())
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM admin_users WHERE username = @username AND password_hash = @password";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", model.Username);
                    cmd.Parameters.AddWithValue("@password", model.Password);

                    long count = (long)cmd.ExecuteScalar()!;
                    if (count > 0)
                        return Ok(new { success = true, message = "Login successful" });
                }
            }
            return Unauthorized(new { success = false, message = "Invalid credentials" });
        }

        // 2. Add New Attraction (CREATE with 3 Images)
        [HttpPost("attraction")]
        public IActionResult AddAttraction([FromBody] Attraction model)
        {
            using (var conn = _dbContext.GetConnection())
            {
                conn.Open();
                string query = @"INSERT INTO attractions (name, category_id, description, distance_km, location_map_link, opening_hours, image1, image2, image3) 
                                VALUES (@name, @categoryId, @description, @distance, @mapLink, @openingHours, @img1, @img2, @img3)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", model.Name);
                    cmd.Parameters.AddWithValue("@categoryId", model.CategoryId);
                    cmd.Parameters.AddWithValue("@description", model.Description);
                    cmd.Parameters.AddWithValue("@distance", model.DistanceKm);
                    cmd.Parameters.AddWithValue("@mapLink", model.LocationMapLink);
                    cmd.Parameters.AddWithValue("@openingHours", model.OpeningHours);
                    cmd.Parameters.AddWithValue("@img1", string.IsNullOrEmpty(model.Image1) ? DBNull.Value : model.Image1);
                    cmd.Parameters.AddWithValue("@img2", string.IsNullOrEmpty(model.Image2) ? DBNull.Value : model.Image2);
                    cmd.Parameters.AddWithValue("@img3", string.IsNullOrEmpty(model.Image3) ? DBNull.Value : model.Image3);

                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Attraction added successfully" });
        }

        // 3. Delete Attraction (DELETE)
        [HttpDelete("attraction/{id}")]
        public IActionResult DeleteAttraction(int id)
        {
            using (var conn = _dbContext.GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM attractions WHERE attraction_id = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Attraction deleted successfully" });
        }
    }

    public class AdminLoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}