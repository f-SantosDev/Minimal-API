using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minimal_API_Project.Domain.DTOs;
using Minimal_API_Project.Domain.Entities;
using Minimal_API_Project.Domain.Interfaces;
using Minimal_API_Project.Domain.ModelViews;
using Minimal_API_Project.Domain.Services;
using Minimal_API_Project.Infrastructure.DataBase;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Minimal_API_Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region Configuration Builder, Services and Swagger
            var builder = WebApplication.CreateBuilder(args);

            #region JWT Authentication
            /*builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });*/

            // configure JWT authentication
            builder.Services.AddAuthentication(option => { 
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true, //*
                        ValidateIssuerSigningKey = true,
                        //ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        //ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) //*
                    };
                });

            builder.Services.AddAuthorization();
            #endregion
            // --**--

            builder.Services.AddScoped<IAdministratorService, AdministratorService>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();

            builder.Services.AddEndpointsApiExplorer();

            #region Configure Swagger and JWT Bearer authentication
            // configure Swagger and add JWT Bearer authentication to Swagger UI
            builder.Services.AddSwaggerGen(options => {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \n\r\n\r Enter 'Bearer' [space] and then your token in the text input below.\n\r\n\r Example: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });
            #endregion

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(builder.Configuration.GetConnectionString("MySQL"), new MySqlServerVersion(new Version(8, 0, 29)));
            });

            var app = builder.Build();
            #endregion

            #region Home
            app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
            #endregion

            #region Administrator

            #region Configure Generate JWT Token
            // Generate JWT Token
            string GenerateJwtToken(Administrator administrator, IConfiguration configuration)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);

                if (key != null)
                {
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new System.Security.Claims.ClaimsIdentity(new[]
                        {
                            new System.Security.Claims.Claim("Id", administrator.Id.ToString()),
                            new System.Security.Claims.Claim("Email", administrator.Email),
                            new System.Security.Claims.Claim("Profile", administrator.Profile.ToString()),
                            new System.Security.Claims.Claim(ClaimTypes.Role, administrator.Profile.ToString()) // Select the role from user
                        }),
                        Expires = DateTime.UtcNow.AddHours(1),
                        //Expires = DateTime.UtcNow.AddDays(1), - to expire in 1 day
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    return tokenHandler.WriteToken(token);
                }
                else
                {
                    throw new Exception("Key not found in configuration.");
                }
            }
            #endregion

            #region Login Administrator
            // Login Administrator
            app.MapPost("/administrator/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
            {
                var adm = administratorService.Login(loginDTO);
                if (adm != null)
                {
                    // Generate JWT Token for the administrator
                    string token = GenerateJwtToken(adm, builder.Configuration);

                    // Create a response object with the token and administrator details
                    return Results.Ok(new AdministratorLoggedIn
                    { 
                        Email = adm.Email,
                        Profile = adm.Profile,
                        Token = token 
                    });
                    //return Results.Ok(new { Message = "Login successful!" });
                }
                else
                    return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Administrator");
            #endregion

            #region Add Administrator
            // Add Administrator
            app.MapPost("/administrator", ([FromBody] AdministratorDTO administrator, IAdministratorService administratorService) =>
            {
                // Validate the Administrator object
                var validation = new ErrorMessage()
                {
                    Message = new List<string>()
                };

                if(string.IsNullOrEmpty(administrator.Email))
                {
                    validation.Message.Add("Email is required!");
                }
                if(string.IsNullOrEmpty(administrator.Password))
                {
                    validation.Message.Add("Password is required!");
                }
                if (administrator.Password.Length < 6)
                {
                    validation.Message.Add("Password must be at least 6 characters long!");
                }
                if (string.IsNullOrEmpty(administrator.Profile.ToString()))
                {
                    validation.Message.Add("Profile is required!");
                }
                /*if (administrator.Profile.ToString() != "Admin" && administrator.Profile.ToString() != "User")
                {
                    validation.Message.Add("Profile must be either 'Admin' or 'User'!");
                }*/

                if (validation.Message.Count > 0)
                {
                    return Results.BadRequest(validation);
                }

                var adm = new Administrator
                {
                    Email = administrator.Email,
                    Password = administrator.Password,
                    Profile = administrator.Profile.ToString()
                };

                var createdAdm = administratorService.AddAdministrator(adm);
                return Results.Created($"/administrator/{createdAdm.Id}", createdAdm);

            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Administrator");
            #endregion

            #region Get Administrator By Id
            // Get Administrator By Id
            app.MapGet("/administrator/{id}", ([FromRoute] int id, IAdministratorService administratorService) =>
            {
                var adm = new AdministratorModelView();
                var administrator = administratorService.GetAdministratorById(id);
                if (administrator != null)
                {
                    adm.Id = administrator.Id;
                    adm.Email = administrator.Email;
                    adm.Profile = administrator.Profile.ToString();
                    return Results.Ok(adm);
                }
                else
                    return Results.NotFound(new { Message = "Administrator not found!" });
                
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Administrator");
            #endregion

            #region Get All Administrators
            // Get All Administrators
            app.MapGet("/administrators", ([FromQuery] int? page, IAdministratorService administratorService) =>
            {
                var adms = new List<AdministratorModelView>();
                var administrators = administratorService.GetAllAdministrators(page);

                foreach (var administrator in administrators)
                {
                    adms.Add(new AdministratorModelView
                    {
                        Id = administrator.Id,
                        Email = administrator.Email,
                        Profile = administrator.Profile.ToString()
                    });
                }

                return Results.Ok(adms);
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Admin"}).WithTags("Administrator");
            #endregion

            #endregion

            #region Validate VehicleDTO
            ErrorMessage ValidateVehicleDTO(VehicleDTO vehicleDTO)
            {
                var validation = new ErrorMessage {
                    Message = new List<string>()
                };

                if (string.IsNullOrEmpty(vehicleDTO.Plate))
                {
                    validation.Message.Add("Plate is required!");
                }

                if (string.IsNullOrEmpty(vehicleDTO.Model))
                {
                    validation.Message.Add("Model is required!");
                }

                if (string.IsNullOrEmpty(vehicleDTO.Brand))
                {
                    validation.Message.Add("Brand is required!");
                }

                if (vehicleDTO.Year == 0 || vehicleDTO.Year < 1950)
                {
                    validation.Message.Add("Year is required and must be greater than 1950!");
                }

                if (string.IsNullOrEmpty(vehicleDTO.Color))
                {
                    validation.Message.Add("Color is required!");
                }

                return validation;
            }
            #endregion

            #region Vehicle

            #region Add Vehicle
            app.MapPost("/vehicle",([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
            {
                // Validate the VehicleDTO object
                var validation = ValidateVehicleDTO(vehicleDTO);

                if (validation.Message.Count > 0) 
                {
                    return Results.BadRequest(validation);
                }

                var vehicle = new Vehicle
                {
                    Plate = vehicleDTO.Plate,
                    Model = vehicleDTO.Model,
                    Brand = vehicleDTO.Brand,
                    Year = vehicleDTO.Year,
                    Color = vehicleDTO.Color
                };

                vehicleService.AddVehicle(vehicle);
                return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
            }).RequireAuthorization()
              .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin, Publisher" })
              .WithTags("Vehicle");
            #endregion

            #region Get All Vehicles
            app.MapGet("/vehicle", ([FromQuery] int? page, [FromQuery] string? model, [FromQuery] string? brand, IVehicleService vehicleService) =>
            {
                var vehicles = vehicleService.GetAllVehicles(page, model, brand);
                return Results.Ok(vehicles);
            }).RequireAuthorization()
              .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin, Publisher" })
              .WithTags("Vehicle");
            #endregion

            #region Get Vehicle By Id
            app.MapGet("/vehicle/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
            {
                var vehicle = vehicleService.GetVehicleById(id);
                if (vehicle != null)
                    return Results.Ok(vehicle);
                else
                    return Results.NotFound(new { Message = "Vehicle not found!" });
            }).RequireAuthorization()
              .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin, Publisher" })
              .WithTags("Vehicle");
            #endregion

            #region Update Vehicle
            app.MapPut("/vehicle/{id}", ([FromRoute] int id, [FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
            {
                // Check if the vehicle exists
                var existingVehicle = vehicleService.GetVehicleById(id);

                if (existingVehicle == null) 
                    return Results.NotFound(new { Message = "Vehicle not found!" });

                // Validate the VehicleDTO object
                var validation = ValidateVehicleDTO(vehicleDTO);

                if (validation.Message.Count > 0)
                    return Results.BadRequest(validation);

                var vehicle = new Vehicle
                {
                    Id = id,
                    Plate = vehicleDTO.Plate,
                    Model = vehicleDTO.Model,
                    Brand = vehicleDTO.Brand,
                    Year = vehicleDTO.Year,
                    Color = vehicleDTO.Color
                };
                var updatedVehicle = vehicleService.UpdateVehicle(id, vehicle);
                if (updatedVehicle != null)
                    return Results.Ok(updatedVehicle);
                else
                    return Results.NotFound(new { Message = "Vehicle not found!" });
            }).RequireAuthorization()
              .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
              .WithTags("Vehicle");
            #endregion

            #region Delete Vehicle
            app.MapDelete("/vehicle/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
            {
                var deleted = vehicleService.DeleteVehicle(id);
                if (deleted)
                    return Results.NoContent();
                else
                    return Results.NotFound(new { Message = "Vehicle not found!" });
            }).RequireAuthorization()
              .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Vehicle");
            #endregion

            #endregion

            #region App
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Run();
            #endregion
        }
    }
}
