namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class StockOrderRequest
{
    public int WholesalerId { get; init; }

    public DateTime OrderDate { get; init; }

    public IReadOnlyList<StockOrderItemRequest> Items { get; init; } = [];
}
