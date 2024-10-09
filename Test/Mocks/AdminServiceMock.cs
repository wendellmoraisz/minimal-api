using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;

namespace Test.Mocks;

public class AdminServiceMock : IAdminService
{
    private static List<Admin> admins =
    [
        new() {
            Id = 1,
            Email = "adm@teste.com",
            Password = "123456",
            Profile = "Adm"
        },
        new() {
            Id = 2,
            Email = "editor@teste.com",
            Password = "123456",
            Profile = "Editor"
        },
    ];

    public Admin Create(Admin admin)
    {
        admin.Id = admins.Count + 1;
        admins.Add(admin);
        return admin;
    }

    public List<Admin> GetAll(int? page)
    {
        return admins;
    }

    public Admin? GetById(int id)
    {
        return admins.Find(a => a.Id == id);
    }

    public Admin? Login(LoginDTO loginDTO)
    {
        return admins.Find(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password);
    }
}
