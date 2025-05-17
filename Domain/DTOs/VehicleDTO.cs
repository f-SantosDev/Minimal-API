
namespace Minimal_API_Project.Domain.DTOs
{
    public record VehicleDTO
    {
        public string Plate { get; set; } = default!;
        public string Color { get; set; } = default!;
        public string Brand { get; set; } = default!;
        public string Model { get; set; } = default!;
        public int Year { get; set; } = default;
    }
}