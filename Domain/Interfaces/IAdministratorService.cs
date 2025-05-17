using Minimal_API_Project.Domain.DTOs;
using Minimal_API_Project.Domain.Entities;

namespace Minimal_API_Project.Domain.Interfaces;

public interface IAdministratorService
{
    Administrator? Login(LoginDTO loginDTO);

    Administrator AddAdministrator(Administrator administrator);
    Administrator? GetAdministratorById(int id);
    List<Administrator> GetAllAdministrators(int? page);
}