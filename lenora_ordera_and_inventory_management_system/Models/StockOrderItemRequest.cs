namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class StockOrderItemRequest
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = "";

    public int Quantity { get; init; }

    public decimal UnitCost { get; init; }
}
