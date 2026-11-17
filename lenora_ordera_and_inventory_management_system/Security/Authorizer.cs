using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;

namespace lenora_ordera_and_inventory_management_system.Security;

internal static class Authorizer
{
    public static void RequireOwner()
    {
        if (CurrentSession.Current?.Role == UserRole.Owner)
        {
            return;
        }

        throw new UnauthorizedAccessException(Messages.OwnerOnly);
    }
}
