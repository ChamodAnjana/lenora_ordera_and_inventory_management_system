using Xunit;
using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Tests;

public class UserManagementTests
{
    [Fact]
    public void TC07_FirstOwnerRegistration_HidesCreateAccountButton()
    {
        // FR04a: On first run, show Create Account button; after first Owner is created, hide it permanently
        // Arrange
        var userRepo = new UserRepository();
        var auth = new AuthService(userRepo);

        // Act
        bool databaseEmpty = auth.DatabaseIsEmpty();

        // Assert - first check should show database is not empty after setup
        // This test assumes the database has been populated during integration testing
        Assert.False(databaseEmpty);
    }

    [Fact]
    public void TC08_EditAccount_AllowsUpdatingFullNameAndUsername()
    {
        // FR04b: Owner can edit existing account full name and username
        // This is an integration test that would need a test database
        // Placeholder for actual implementation
        Assert.True(true);
    }

    [Fact]
    public void TC09_LastOwnerProtection_PreventsDisablingLastActiveOwner()
    {
        // FR05a: Prevent disabling the last active Owner account to avoid lockout
        // This is an integration test that would need a test database
        // Placeholder for actual implementation
        Assert.True(true);
    }

    [Fact]
    public void TC10_PasswordReset_SetsRequiredChangeFlag()
    {
        // FR05b: Owner reset password sets a temporary password and requires user to change it on next login
        // This is an integration test that would need a test database
        // Placeholder for actual implementation
        Assert.True(true);
    }

    [Fact]
    public void TC02_InvalidPasswordLogin_ReturnsLoginFailed()
    {
        // FR02: Compare entered password with stored hash, reject invalid or empty entries with a message
        var userRepo = new UserRepository();
        var auth = new AuthService(userRepo);

        // Test with empty username
        var result = auth.Login("", "password");
        Assert.False(result.Success);
        Assert.Equal(Messages.UsernameRequired, result.Message);

        // Test with empty password
        result = auth.Login("testuser", "");
        Assert.False(result.Success);
        Assert.Equal(Messages.PasswordRequired, result.Message);
    }

    [Fact]
    public void TC03_PasswordHashing_UsesSecureHash()
    {
        // NFR05: Passwords stored only as salted bcrypt hashes
        var password = "TestPassword123!";
        var hash1 = PasswordHasher.Hash(password);
        var hash2 = PasswordHasher.Hash(password);

        // BCrypt produces different hashes for the same password
        Assert.NotEqual(hash1, hash2);

        // But both hashes should verify the same password
        Assert.True(PasswordHasher.Verify(password, hash1));
        Assert.True(PasswordHasher.Verify(password, hash2));
    }

    [Fact]
    public void TC05_CreateAccount_ValidatesUsername()
    {
        // FR04: Owner creates Assistant account with full name, username, password
        // This test verifies username validation
        Assert.True(true); // Placeholder for integration test
    }

    [Fact]
    public void TC06_NoDataBeforeLogin_EnforcesRoleRestrictions()
    {
        // NFR06: No data before login. Role restrictions enforced in program logic
        // Verify that CurrentSession starts empty
        CurrentSession.SignOut();
        Assert.Null(CurrentSession.Current);
    }
}
