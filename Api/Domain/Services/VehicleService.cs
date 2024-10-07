using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infrastructure.Database;

namespace minimal_api.Domain.Services;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _context;

    public VehicleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Vehicle> GetAll(int? page = 1, string? name = null, string? brand = null)
    {
        var query = _context.Vehicles.AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(v => v.Name.ToLower().Contains(name));
        }

        var itemsPerPage = 10;

        if (page != null)
            query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);

        return [.. query];
    }

    public Vehicle? GetById(int id)
    {
        return _context.Vehicles.FirstOrDefault(v => v.Id == id);
    }

    public void Create(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
    }

    public void Update(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        _context.SaveChanges();
    }

    public void Delete(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
    }

}
