namespace AutoShop.Tests.Integration.Services;

public class AuthServiceTests
{
    [Fact]
    public void Authenticate_WithSeededAdminCredentials_ReturnsUser()
    {
        using var scope = new TestDatabaseScope();

        var auth = new AuthService();
        var user = auth.Authenticate("admin", "Admin123!");

        user.Should().NotBeNull();
        user!.UserName.Should().Be("admin");
    }
}