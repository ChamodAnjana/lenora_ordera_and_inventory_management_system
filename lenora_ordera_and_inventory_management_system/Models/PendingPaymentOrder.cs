namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class PendingPaymentOrder
{
    public int OrderId { get; init; }

    public string CustomerName { get; init; } = "";

    public string CustomerPhone { get; init; } = "";

    public DateTime OrderDate { get; init; }

    public DateTime? StatusDate { get; init; }

    public decimal TotalAmount { get; init; }
}
