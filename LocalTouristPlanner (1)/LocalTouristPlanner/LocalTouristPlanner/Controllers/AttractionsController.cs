using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using LocalTouristPlanner.Data;
using LocalTouristPlanner.Models;
using System;
using System.Collections.Generic;

namespace LocalTouristPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttractionsController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;

        public AttractionsController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Search & Filter API
        [HttpGet("search")]
        public IActionResult Search([FromQuery] string? keyword, [FromQuery] int? categoryId)
        {
            List<Attraction> list = new List<Attraction>();
            using (var conn = _dbContext.GetConnection())
            {
                conn.Open();
                string query = @"SELECT a.*, c.category_name 
                                FROM attractions a 
                                JOIN categories c ON a.category_id = c.category_id 
                                WHERE (@keyword IS NULL OR a.name LIKE CONCAT('%', @keyword, '%'))
                                AND (@categoryId IS NULL OR a.category_id = @categoryId)
                                ORDER BY a.distance_km ASC";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@keyword", string.IsNullOrEmpty(keyword) ? DBNull.Value : keyword);
                    cmd.Parameters.AddWithValue("@categoryId", categoryId.HasValue ? categoryId.Value : DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Attraction
                            {
                                AttractionId = reader.GetInt32("attraction_id"),
                                Name = reader.GetString("name"),
                                CategoryId = reader.GetInt32("category_id"),
                                CategoryName = reader.GetString("category_name"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                                DistanceKm = reader.GetDecimal("distance_km"),
                                LocationMapLink = reader.IsDBNull(reader.GetOrdinal("location_map_link")) ? "" : reader.GetString("location_map_link"),
                                OpeningHours = reader.IsDBNull(reader.GetOrdinal("opening_hours")) ? "" : reader.GetString("opening_hours"),
                                Image1 = reader.IsDBNull(reader.GetOrdinal("image1")) ? "" : reader.GetString("image1"),
                                Image2 = reader.IsDBNull(reader.GetOrdinal("image2")) ? "" : reader.GetString("image2"),
                                Image3 = reader.IsDBNull(reader.GetOrdinal("image3")) ? "" : reader.GetString("image3"),
                                Image4 = reader.IsDBNull(reader.GetOrdinal("image4")) ? "" : reader.GetString("image4"),
                                Image5 = reader.IsDBNull(reader.GetOrdinal("image5")) ? "" : reader.GetString("image5")
                            });
                        }
                    }
                }
            }
            return Ok(list);
        }

        // Update Attraction API
        [HttpPut("update")]
        public IActionResult Update([FromBody] Attraction attraction)
        {
            if (attraction == null || attraction.AttractionId <= 0)
                return BadRequest("Invalid attraction data.");

            using (var conn = _dbContext.GetConnection())
            {
                conn.Open();
                string query = @"UPDATE attractions 
                        SET name = @name, 
                            category_id = @categoryId, 
                            description = @description, 
                            distance_km = @distanceKm, 
                            location_map_link = @locationMapLink, 
                            opening_hours = @openingHours, 
                            image1 = @image1, 
                            image2 = @image2, 
                            image3 = @image3, 
                            image4 = @image4, 
                            image5 = @image5 
                        WHERE attraction_id = @attractionId";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@attractionId", attraction.AttractionId);
                    cmd.Parameters.AddWithValue("@name", attraction.Name);
                    cmd.Parameters.AddWithValue("@categoryId", attraction.CategoryId);
                    cmd.Parameters.AddWithValue("@description", attraction.Description ?? "");
                    cmd.Parameters.AddWithValue("@distanceKm", attraction.DistanceKm);
                    cmd.Parameters.AddWithValue("@locationMapLink", attraction.LocationMapLink ?? "");
                    cmd.Parameters.AddWithValue("@openingHours", attraction.OpeningHours ?? "");
                    cmd.Parameters.AddWithValue("@image1", attraction.Image1 ?? "");
                    cmd.Parameters.AddWithValue("@image2", attraction.Image2 ?? "");
                    cmd.Parameters.AddWithValue("@image3", attraction.Image3 ?? "");
                    cmd.Parameters.AddWithValue("@image4", attraction.Image4 ?? "");
                    cmd.Parameters.AddWithValue("@image5", attraction.Image5 ?? "");

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        return Ok(new { message = "Attraction updated successfully!" });
                    else
                        return NotFound("Attraction not found.");
                }
            }
        }

        // 1-Day Itinerary Planner Generator API
        [HttpPost("generate-plan")]
        public IActionResult GeneratePlan([FromBody] List<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
                return BadRequest("Select at least one place.");

            List<Attraction> itinerary = new List<Attraction>();
            using (var conn = _dbContext.GetConnection())
            {
                conn.Open();

                List<string> paramNames = new List<string>();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    for (int i = 0; i < selectedIds.Count; i++)
                    {
                        string paramName = $"@id{i}";
                        paramNames.Add(paramName);
                        cmd.Parameters.AddWithValue(paramName, selectedIds[i]);
                    }

                    cmd.CommandText = $@"SELECT a.*, c.category_name 
                                       FROM attractions a 
                                       JOIN categories c ON a.category_id = c.category_id 
                                       WHERE a.attraction_id IN ({string.Join(",", paramNames)}) 
                                       ORDER BY a.distance_km ASC";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            itinerary.Add(new Attraction
                            {
                                AttractionId = reader.GetInt32("attraction_id"),
                                Name = reader.GetString("name"),
                                CategoryId = reader.GetInt32("category_id"),
                                CategoryName = reader.GetString("category_name"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                                DistanceKm = reader.GetDecimal("distance_km"),
                                LocationMapLink = reader.IsDBNull(reader.GetOrdinal("location_map_link")) ? "" : reader.GetString("location_map_link"),
                                OpeningHours = reader.IsDBNull(reader.GetOrdinal("opening_hours")) ? "" : reader.GetString("opening_hours"),
                                Image1 = reader.IsDBNull(reader.GetOrdinal("image1")) ? "" : reader.GetString("image1"),
                                Image2 = reader.IsDBNull(reader.GetOrdinal("image2")) ? "" : reader.GetString("image2"),
                                Image3 = reader.IsDBNull(reader.GetOrdinal("image3")) ? "" : reader.GetString("image3"),
                                Image4 = reader.IsDBNull(reader.GetOrdinal("image4")) ? "" : reader.GetString("image4"),
                                Image5 = reader.IsDBNull(reader.GetOrdinal("image5")) ? "" : reader.GetString("image5")
                            });
                        }
                    }
                }
            }
            return Ok(itinerary);
        }
    }
}