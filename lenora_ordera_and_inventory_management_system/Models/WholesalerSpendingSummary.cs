namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class WholesalerSpendingSummary
{
    public int WholesalerId { get; init; }

    public string WholesalerName { get; init; } = "";

    public int OrderCount { get; init; }

    public decimal TotalSpending { get; init; }
}
