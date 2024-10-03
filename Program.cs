using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enums;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Services;
using minimal_api.Infrastructure.Database;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion

# region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Admin
app.MapPost("/admin/login", ([FromBody] LoginDTO loginDTO, IAdminService adminService) =>
{
    if (adminService.Login(loginDTO) is not null)
        return Results.Ok("Login com sucesso");

    return Results.Unauthorized();
}).WithTags("Admin");

app.MapPost("/admin", ([FromBody] AdminDTO adminDTO, IAdminService adminService) =>
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
}).WithTags("Admin");

app.MapGet("/admin", ([FromQuery] int? page, IAdminService adminService) =>
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
}).WithTags("Admin");

app.MapGet("/admin/{id}", ([FromRoute] int id, IAdminService adminService) =>
{
    var admin = adminService.GetById(id);

    if (admin == null) return Results.NotFound();

    return Results.Ok(admin);
}).WithTags("Admin");
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

app.MapGet("/vehicles", ([FromQuery] int? page, IVehicleService vehicleService) =>
{
    var vehicles = vehicleService.GetAll(page);

    return Results.Ok(vehicles);
}).WithTags("Vehicles");

app.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);

    if (vehicle == null) return Results.NotFound();

    return Results.Ok(vehicle);
}).WithTags("Vehicles");

app.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
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
}).WithTags("Vehicles");

app.MapPut("/vehicles/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
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
}).WithTags("Vehicles");

app.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);
    if (vehicle == null) return Results.NotFound();

    vehicleService.Delete(vehicle);

    return Results.NoContent();
}).WithTags("Vehicles");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion
