using Minimal_API_Project.Domain.Entities;

namespace Minimal_API_Project.Domain.Interfaces
{
    public interface IVehicleService
    {
        public IEnumerable<Vehicle> GetAllVehicles(int? page = 1, string? model = null, string? brand = null);
        public Vehicle? GetVehicleById(int id);
        public Vehicle? AddVehicle(Vehicle vehicle);
        public Vehicle? UpdateVehicle(int id, Vehicle vehicle);
        public bool DeleteVehicle(int id);
    }
}
