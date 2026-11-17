namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class PostingRequest
{
    public int OrderId { get; init; }

    public string CourierName { get; init; } = "";

    public DateTime PostingDate { get; init; }
}
