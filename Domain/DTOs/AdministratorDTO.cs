using Minimal_API_Project.Domain.Enums;

namespace Minimal_API_Project.Domain.DTOs
{
    public class AdministratorDTO
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public Profile Profile { get; set; } = default!;
    }
}
