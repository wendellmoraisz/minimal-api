using minimal_api.Domain.Entities;

namespace minimal_api.Domain.Interfaces;

public interface IVehicleService
{
    List<Vehicle> GetAll(int? page = 1, string? name = null, string? brand = null);
    Vehicle? GetById(int id);
    void Create(Vehicle vehicle);
    void Update(Vehicle vehicle);
    void Delete(Vehicle vehicle);
}
