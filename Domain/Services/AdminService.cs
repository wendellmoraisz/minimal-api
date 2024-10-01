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
