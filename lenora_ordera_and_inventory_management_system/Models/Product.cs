namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class Product
{
    public int ProductId { get; init; }

    public string Name { get; init; } = "";

    public string Category { get; init; } = "";

    public decimal CostPrice { get; init; }

    public decimal SellingPrice { get; init; }

    public int Quantity { get; init; }
}
