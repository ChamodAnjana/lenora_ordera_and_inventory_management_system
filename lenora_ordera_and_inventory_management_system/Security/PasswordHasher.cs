using lenora_ordera_and_inventory_management_system.Common;

namespace lenora_ordera_and_inventory_management_system.Security;

internal static class PasswordHasher
{
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, Rules.BcryptWorkFactor);
    }

    public static bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
