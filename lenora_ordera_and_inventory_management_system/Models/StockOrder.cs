namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class StockOrder
{
    public int StockOrderId { get; init; }

    public int WholesalerId { get; init; }

    public string WholesalerName { get; init; } = "";

    public DateTime OrderDate { get; init; }

    public decimal TotalAmount { get; init; }

    public int RecordedByUserId { get; init; }

    public string RecordedByUserName { get; init; } = "";
}
