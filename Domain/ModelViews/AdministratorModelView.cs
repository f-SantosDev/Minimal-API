
namespace Minimal_API_Project.Domain.ModelViews;

public record AdministratorModelView
{
    public int Id { get; set; } = default;
    public string Email { get; set; } = default!;
    //public string Password { get; set; } = default!; - Model View should not contain sensitive data like password
    public string Profile { get; set; } = default!;
}