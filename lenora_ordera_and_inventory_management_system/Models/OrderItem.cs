namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class OrderItem
{
    public int OrderItemId { get; init; }

    public int OrderId { get; init; }

    public int ProductId { get; init; }

    public string ProductName { get; init; } = "";

    public int Quantity { get; init; }

    public decimal SellingPrice { get; init; }

    public decimal Subtotal => Quantity * SellingPrice;
}
