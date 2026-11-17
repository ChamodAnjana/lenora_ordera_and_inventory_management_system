namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class OrderPlacementRequest
{
    public string CustomerName { get; init; } = "";

    public string CustomerPhone { get; init; } = "";

    public string CustomerAddress { get; init; } = "";

    public IReadOnlyList<OrderItemRequest> Items { get; init; } = [];
}
