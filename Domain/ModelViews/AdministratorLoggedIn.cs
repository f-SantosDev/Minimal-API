namespace Minimal_API_Project.Domain.ModelViews
{
    public record AdministratorLoggedIn
    {
        public string Email { get; set; } = default!;
        public string Profile { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}
