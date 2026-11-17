namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class CustomerOrder
{
    public int OrderId { get; init; }

    public int CustomerId { get; init; }

    public string CustomerName { get; init; } = "";

    public string CustomerPhone { get; init; } = "";

    public string CustomerAddress { get; init; } = "";

    public bool IsRepeatCustomer { get; init; }

    public DateTime OrderDate { get; init; }

    public OrderStatus Status { get; init; }

    public string? CourierName { get; init; }

    public DateTime? PostingDate { get; init; }

    public DateTime? StatusDate { get; init; }

    public bool PaymentReceived { get; init; }

    public decimal? AmountReceived { get; init; }

    public DateTime? PaymentDate { get; init; }

    public int CreatedByUserId { get; init; }

    public string CreatedByUserName { get; init; } = "";

    public decimal TotalAmount { get; init; }
}
