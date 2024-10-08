using minimal_api.Domain.Entities;

namespace Test.Domain.Entities;

[TestClass]
public class AdminTest
{
    [TestMethod]
    public void TestGetSetProperties()
    {
        // Arrange
        var adm = new Admin();

        // Act
        adm.Id = 1;
        adm.Email = "test@email.com";
        adm.Password = "test123";
        adm.Profile = "Adm";

        // Assert
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("test@email.com", adm.Email);
        Assert.AreEqual("test123", adm.Password);
        Assert.AreEqual("Adm", adm.Profile);
    }
}
