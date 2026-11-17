using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;

namespace lenora_ordera_and_inventory_management_system.Services;

internal static class AccountFactory
{
    public static NewUser Create(AccountRequest request, UserRole role)
    {
        return new NewUser
        {
            FullName = request.FullName.Trim(),
            Username = request.Username.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = role,
            IsActive = true
        };
    }
}
