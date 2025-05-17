using Microsoft.EntityFrameworkCore;
using Minimal_API_Project.Domain.DTOs;
using Minimal_API_Project.Domain.Entities;
using Minimal_API_Project.Domain.Interfaces;
using Minimal_API_Project.Infrastructure.DataBase;

namespace Minimal_API_Project.Domain.Services;

public class AdministratorService : IAdministratorService
{
    private readonly AppDbContext _context;
    public AdministratorService(AppDbContext context)
    {
        _context = context;
    }
    public Administrator Login(LoginDTO loginDTO)
    {
        Administrator administrator = _context.Administrator.Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password).FirstOrDefault();

        if (administrator != null)
            return administrator;
        else
            return null;
    }

    public Administrator AddAdministrator(Administrator administrator)
    {
        var adm = new Administrator
        {
            Email = administrator.Email,
            Password = administrator.Password,
            Profile = administrator.Profile.ToString()
        };

        _context.Administrator.Add(adm);
        _context.SaveChanges();
        return adm;
    }

    public Administrator? GetAdministratorById(int id)
    {
        return _context.Administrator.Where(a => a.Id == id).FirstOrDefault();
    }

    public List<Administrator> GetAllAdministrators(int? page)
    {
        var administrators = _context.Administrator.AsQueryable();

        if (page == null || page < 1)
        {
            page = 1;
        }

        int pageSize = 10;
        int skip = (int)(page - 1) * pageSize;
        administrators = administrators.Skip(skip).Take(pageSize);

        return administrators.ToList();
    }
}
