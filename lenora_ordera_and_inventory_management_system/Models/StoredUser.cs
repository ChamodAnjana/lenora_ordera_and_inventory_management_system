namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class StoredUser
{
    public int UserId { get; init; }

    public string FullName { get; init; } = "";

    public string Username { get; init; } = "";

    public string PasswordHash { get; init; } = "";

    public UserRole Role { get; init; }

    public bool IsActive { get; init; }

    public bool MustChangePassword { get; init; }
}
