using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Services;
using minimal_api.Infrastructure.Database;

namespace Test.Domain.Services;

[TestClass]
public class AdminServiceTest
{
    private readonly Admin _adminTest = new()
    {
        Email = "test@email.com",
        Password = "test123",
        Profile = "Adm",
    };

    private ApplicationDbContext CreateTestContext()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

        var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new ApplicationDbContext(configuration);
    }

    [TestMethod]
    public void TestSaveAdmin()
    {
        var context = CreateTestContext();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Admins");

        var adminService = new AdminService(context);

        adminService.Create(_adminTest);

        Assert.AreEqual(1, adminService.GetAll(1).Count);
    }

    [TestMethod]
    public void TestGetById()
    {
        var context = CreateTestContext();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Admins");

        var adminService = new AdminService(context);

        adminService.Create(_adminTest);
        var adm = adminService.GetById(_adminTest.Id);

        // Assert
        Assert.AreEqual(_adminTest.Id, adm.Id);
    }
}
