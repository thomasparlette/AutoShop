namespace AutoShop.Tests.Unit.Services;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_And_VerifyPassword_Works()
    {
        var hash = PasswordHasher.HashPassword("Admin123!");

        PasswordHasher.VerifyPassword("Admin123!", hash).Should().BeTrue();
        PasswordHasher.VerifyPassword("wrong-password", hash).Should().BeFalse();
    }
}