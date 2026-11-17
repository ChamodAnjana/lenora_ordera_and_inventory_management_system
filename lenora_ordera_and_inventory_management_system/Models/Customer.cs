namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class Customer
{
    public int CustomerId { get; init; }

    public string Name { get; init; } = "";

    public string Phone { get; init; } = "";

    public string Address { get; init; } = "";

    public bool IsRepeat { get; init; }

    public int OrderCount { get; init; }
}
