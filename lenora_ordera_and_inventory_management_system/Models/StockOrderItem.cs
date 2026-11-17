namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class StockOrderItem
{
    public int StockOrderItemId { get; init; }

    public int StockOrderId { get; init; }

    public int ProductId { get; init; }

    public string ProductName { get; init; } = "";

    public int Quantity { get; init; }

    public decimal UnitCost { get; init; }
}
