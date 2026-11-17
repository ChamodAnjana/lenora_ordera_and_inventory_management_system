namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class DeliveryStatusRequest
{
    public int OrderId { get; init; }

    public OrderStatus Status { get; init; }

    public DateTime StatusDate { get; init; }
}
