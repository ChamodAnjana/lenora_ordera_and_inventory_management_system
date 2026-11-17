namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class FollowUpOrder
{
    public int OrderId { get; init; }

    public string CustomerName { get; init; } = "";

    public string CustomerPhone { get; init; } = "";

    public string CourierName { get; init; } = "";

    public DateTime PostingDate { get; init; }

    public int DaysElapsed => (DateTime.Today - PostingDate.Date).Days;
}
