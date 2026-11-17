using lenora_ordera_and_inventory_management_system.Models;

namespace lenora_ordera_and_inventory_management_system.Security;

internal static class CurrentSession
{
    public static User? Current { get; private set; }

    public static void SignIn(User user)
    {
        Current = user;
    }

    public static void SignOut()
    {
        Current = null;
    }
}
