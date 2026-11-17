namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class OrderUpdateRequest
{
    public int OrderId { get; init; }

    public string CustomerName { get; init; } = "";

    public string CustomerPhone { get; init; } = "";

    public string CustomerAddress { get; init; } = "";

    public IReadOnlyList<OrderItemRequest> Items { get; init; } = [];
}
