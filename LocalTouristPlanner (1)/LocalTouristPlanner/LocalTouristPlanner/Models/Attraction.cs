namespace LocalTouristPlanner.Models
{
    public class Attraction
    {
        public int AttractionId { get; set; }

        
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }

    
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public decimal DistanceKm { get; set; }
        public string? LocationMapLink { get; set; }
        public string? OpeningHours { get; set; }

        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public string? Image5 { get; set; }
    }
}