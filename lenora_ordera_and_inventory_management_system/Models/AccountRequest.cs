namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class AccountRequest
{
    public string FullName { get; init; } = "";

    public string Username { get; init; } = "";

    public string Password { get; init; } = "";

    public string ConfirmPassword { get; init; } = "";

    public UserRole? Role { get; init; }
}
