using System.Net;
using System.Text;
using System.Text.Json;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.ModelViews;
using Test.Helpers;

namespace Test.Requests;

[TestClass]
public class AdminRequestTest
{
    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        Setup.ClassInit(testContext);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Setup.ClassCleanup();
    }

    [TestMethod]
    public async Task TestGetSetProperties()
    {
        var loginDTO = new LoginDTO
        {
            Email = "adm@teste.com",
            Password = "123456"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginDTO),
            Encoding.UTF8,
            "Application/json"
        );

        var response = await Setup.client.PostAsync("/admin/login", content);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var loggedAdmin = JsonSerializer.Deserialize<LoggedAdmin>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(loggedAdmin.Email);
        Assert.IsNotNull(loggedAdmin.Token);
        Assert.IsNotNull(loggedAdmin.Profile);
    }
}
