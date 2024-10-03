using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infrastructure.Database;

namespace minimal_api.Domain.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Admin Create(Admin admin)
    {
        _context.Admins.Add(admin);
        _context.SaveChanges();

        return admin;
    }

    public List<Admin> GetAll(int? page)
    {
        var query = _context.Admins.AsQueryable();

        var itemsPerPage = 10;

        if (page != null)
            query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);

        return [.. query];
    }

    public Admin? GetById(int id)
    {
        return _context.Admins.FirstOrDefault(a => a.Id == id);
    }

    public Admin? Login(LoginDTO loginDTO)
    {
        var admin = _context.Admins.Where(
            a => a.Email == loginDTO.Email &&
            a.Password == loginDTO.Password
        )
        .FirstOrDefault();
        return admin;
    }
}
