namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class CodPaymentRequest
{
    public int OrderId { get; init; }

    public decimal AmountReceived { get; init; }

    public DateTime PaymentDate { get; init; }
}
