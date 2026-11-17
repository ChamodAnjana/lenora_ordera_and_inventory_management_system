using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Services;

internal sealed class PaymentService
{
    private readonly OrderRepository _orders;

    public PaymentService(OrderRepository orders)
    {
        _orders = orders;
    }

    public Result ConfirmCodPayment(CodPaymentRequest request)
    {
        try
        {
            Authorizer.RequireOwner();
            var validation = ValidatePayment(request);
            if (!validation.Success)
            {
                return validation;
            }

            return ExecutePaymentConfirmation(request);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Fail(Messages.OwnerOnly);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result ListPendingPayments(List<PendingPaymentOrder> target)
    {
        try
        {
            Authorizer.RequireOwner();
            using var connection = Db.OpenConnection();
            target.AddRange(_orders.ListPendingCodPayments(connection));
            return Result.Ok(string.Empty);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Fail(Messages.OwnerOnly);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    private Result ExecutePaymentConfirmation(CodPaymentRequest request)
    {
        using var connection = Db.OpenConnection();
        var order = _orders.FindById(connection, request.OrderId);
        if (order is null)
        {
            return Result.Fail(Messages.OrderNotFound);
        }
        if (order.Status != OrderStatus.Delivered)
        {
            return Result.Fail(Messages.OnlyDeliveredCanBePaid);
        }
        if (order.PaymentReceived)
        {
            return Result.Fail(Messages.PaymentAlreadyConfirmed);
        }

        _orders.ConfirmCodPayment(connection, request.OrderId, request.AmountReceived, request.PaymentDate);
        return Result.Ok(Messages.PaymentConfirmed);
    }

    private static Result ValidatePayment(CodPaymentRequest request)
    {
        if (request.OrderId <= 0)
        {
            return Result.Fail(Messages.OrderNotFound);
        }
        if (request.AmountReceived <= 0)
        {
            return Result.Fail(Messages.PaymentAmountInvalid);
        }
        return Result.Ok(string.Empty);
    }
}
