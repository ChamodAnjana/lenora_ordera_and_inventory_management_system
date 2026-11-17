namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class Wholesaler
{
    public int WholesalerId { get; init; }

    public string Name { get; init; } = "";

    public string ContactNo { get; init; } = "";

    public string? Address { get; init; }
}
