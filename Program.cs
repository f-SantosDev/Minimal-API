using Minimal_API_Project.Domain.DTOs;

namespace Minimal_API_Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.MapPost("/login", (LoginDTO loginDTO) =>
            {
                if (loginDTO.Email == "adm@teste.com" && loginDTO.Password == "123456")
                    return Results.Ok(new { Message = "Login successful!" });
                else
                    return Results.Unauthorized();
            });

            app.Run();
        }
    }
}
