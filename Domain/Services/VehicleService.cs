using Microsoft.EntityFrameworkCore;
using Minimal_API_Project.Domain.DTOs;
using Minimal_API_Project.Domain.Entities;
using Minimal_API_Project.Domain.Interfaces;
using Minimal_API_Project.Infrastructure.DataBase;

namespace Minimal_API_Project.Domain.Services;

public class VehicleService : IVehicleService
{
    private readonly AppDbContext _context;
    public VehicleService(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Vehicle> GetAllVehicles(int? page = 1, string? model = null, string? brand = null)
    {
        var vehicle = _context.Vehicle.AsQueryable();

        if (!string.IsNullOrEmpty(model))
        {
            vehicle = vehicle.Where(v => EF.Functions.Like(v.Model.ToLower(), $"%{model}%"));
        }

        if (!string.IsNullOrEmpty(brand))
        {
            vehicle = vehicle.Where(v => EF.Functions.Like(v.Brand.ToLower(), $"%{brand}%"));
        }

        if (page == null || page < 1)
        {
            page = 1;
        }

        int pageSize = 10;
        int skip =(int)(page - 1) * pageSize;
        vehicle = vehicle.Skip(skip).Take(pageSize);

        return vehicle.ToList();

    }

    public Vehicle? GetVehicleById(int id)
    {
        return _context.Vehicle.Where(v => v.Id == id).FirstOrDefault();
    }

    public Vehicle? AddVehicle(Vehicle vehicle)
    {
        _context.Vehicle.Add(vehicle);
        _context.SaveChanges();
        return vehicle;
    }

    public Vehicle? UpdateVehicle(int id, Vehicle vehicle)
    {
        var existingVehicle = _context.Vehicle.Find(vehicle.Id);
        if (existingVehicle != null)
        {
            existingVehicle.Model = vehicle.Model;
            existingVehicle.Brand = vehicle.Brand;
            existingVehicle.Year = vehicle.Year;
            existingVehicle.Color = vehicle.Color;
            //_context.Vehicle.Update(existingVehicle); 
            _context.SaveChanges();
        }
        return existingVehicle;
    }

    public bool DeleteVehicle(int id)
    {
        var vehicle = _context.Vehicle.Find(id);
        if (vehicle != null)
        {
            _context.Vehicle.Remove(vehicle);
            _context.SaveChanges();
            return true;
        }
        return false;
    }
}