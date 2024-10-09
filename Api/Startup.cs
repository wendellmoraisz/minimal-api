using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enums;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Services;
using minimal_api.Infrastructure.Database;

namespace minimal_api;

public class Startup
{
    public IConfiguration Configuration { get; set; }
    private readonly string JwtKey;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        JwtKey = Configuration.GetSection("Jwt").ToString() ?? "123456";
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IVehicleService, VehicleService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insert your JWT token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
        }
            });
        });

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(
                Configuration.GetConnectionString("mysql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("mysql"))
            );
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
            #endregion

            #region Admin
            string GenerateJwtToken(Admin admin)
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new(ClaimTypes.Email, admin.Email),
                    new("Profile", admin.Profile),
                    new(ClaimTypes.Role, admin.Profile)
                };

                var token = new JwtSecurityToken
                (
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endpoints.MapPost("/admin/login", ([FromBody] LoginDTO loginDTO, IAdminService adminService) =>
            {
                var admin = adminService.Login(loginDTO);
                if (admin is not null)
                {
                    var token = GenerateJwtToken(admin);
                    return Results.Ok(new LoggedAdmin
                    {
                        Email = admin.Email,
                        Profile = admin.Profile,
                        Token = token
                    });
                }

                return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Admin");

            endpoints.MapPost("/admin", ([FromBody] AdminDTO adminDTO, IAdminService adminService) =>
            {
                var validation = new ValidationErrors
                {
                    Messages = []
                };

                if (string.IsNullOrEmpty(adminDTO.Email))
                    validation.Messages.Add("Email is required");

                if (string.IsNullOrEmpty(adminDTO.Password))
                    validation.Messages.Add("Password is required");

                if (adminDTO.Profile is null)
                    validation.Messages.Add("Profile is required");

                if (validation.Messages.Count > 0)
                    return Results.BadRequest(validation);

                var admin = new Admin
                {
                    Email = adminDTO.Email,
                    Password = adminDTO.Password,
                    Profile = adminDTO.Profile.ToString() ?? Profile.Editor.ToString()
                };
                adminService.Create(admin);

                return Results.Created($"/admin/{admin.Id}", admin);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Admin");

            endpoints.MapGet("/admin", ([FromQuery] int? page, IAdminService adminService) =>
            {
                var modelView = new List<AdminModelView>();
                var admins = adminService.GetAll(page);

                foreach (var admin in admins)
                {
                    modelView.Add(new AdminModelView
                    {
                        Id = admin.Id,
                        Email = admin.Email,
                        Profile = admin.Profile
                    });
                }
                return Results.Ok(modelView);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Admin");

            endpoints.MapGet("/admin/{id}", ([FromRoute] int id, IAdminService adminService) =>
            {
                var admin = adminService.GetById(id);

                if (admin == null) return Results.NotFound();

                return Results.Ok(admin);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Admin");
            #endregion

            #region Vehicles
            ValidationErrors validateDTO(VehicleDTO vehicleDTO)
            {
                var validation = new ValidationErrors
                {
                    Messages = new List<string>()
                };

                if (string.IsNullOrEmpty(vehicleDTO.Name))
                    validation.Messages.Add("Name is required");

                if (string.IsNullOrEmpty(vehicleDTO.Brand))
                    validation.Messages.Add("Brand is required");

                if (vehicleDTO.Year < 1800)
                    validation.Messages.Add("Year not allowed");

                return validation;
            }

            endpoints.MapGet("/vehicles", ([FromQuery] int? page, IVehicleService vehicleService) =>
            {
                var vehicles = vehicleService.GetAll(page);

                return Results.Ok(vehicles);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Vehicles");

            endpoints.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
            {
                var vehicle = vehicleService.GetById(id);

                if (vehicle == null) return Results.NotFound();

                return Results.Ok(vehicle);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Vehicles");

            endpoints.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
            {
                var validationErrors = validateDTO(vehicleDTO);
                if (validationErrors.Messages.Count > 0)
                    return Results.BadRequest(validationErrors);

                var vehicle = new Vehicle
                {
                    Name = vehicleDTO.Name,
                    Brand = vehicleDTO.Brand,
                    Year = vehicleDTO.Year
                };
                vehicleService.Create(vehicle);

                return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Vehicles");

            endpoints.MapPut("/vehicles/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
            {
                var validationErrors = validateDTO(vehicleDTO);
                if (validationErrors.Messages.Count > 0)
                    return Results.BadRequest(validationErrors);

                var vehicle = vehicleService.GetById(id);
                if (vehicle == null) return Results.NotFound();

                vehicle.Name = vehicleDTO.Name;
                vehicle.Brand = vehicleDTO.Brand;
                vehicle.Year = vehicleDTO.Year;

                vehicleService.Update(vehicle);

                return Results.Ok(vehicle);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Vehicles");

            endpoints.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
            {
                var vehicle = vehicleService.GetById(id);
                if (vehicle == null) return Results.NotFound();

                vehicleService.Delete(vehicle);

                return Results.NoContent();
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Vehicles");
            #endregion
        });
    }
}
