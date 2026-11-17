using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Models;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Services;

internal sealed class DeliveryService
{
    private readonly OrderRepository _orders;
    private readonly ProductRepository _products;

    public DeliveryService(OrderRepository orders, ProductRepository products)
    {
        _orders = orders;
        _products = products;
    }

    public Result RecordPosting(PostingRequest request)
    {
        try
        {
            var validation = ValidatePosting(request);
            if (!validation.Success)
            {
                return validation;
            }

            return ExecutePosting(request);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result UpdateDeliveryStatus(DeliveryStatusRequest request)
    {
        try
        {
            var validation = ValidateStatusUpdate(request);
            if (!validation.Success)
            {
                return validation;
            }

            return ExecuteStatusUpdate(request);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result GetFollowUpList(List<FollowUpOrder> target)
    {
        try
        {
            using var connection = Db.OpenConnection();
            var threshold = DateTime.Today.AddDays(-Rules.FollowUpDays);
            target.AddRange(_orders.ListFollowUpOrders(connection, threshold));
            return Result.Ok(string.Empty);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    private Result ExecutePosting(PostingRequest request)
    {
        using var connection = Db.OpenConnection();
        using var transaction = connection.BeginTransaction();

        var currentStatus = _orders.GetOrderStatusForUpdate(connection, transaction, request.OrderId);
        if (currentStatus != OrderStatus.Placed)
        {
            transaction.Rollback();
            return Result.Fail(Messages.OnlyPlacedCanBePosted);
        }

        _orders.RecordPosting(connection, transaction, request.OrderId, request.CourierName.Trim(), request.PostingDate);
        transaction.Commit();
        return Result.Ok(Messages.OrderPosted);
    }

    private Result ExecuteStatusUpdate(DeliveryStatusRequest request)
    {
        using var connection = Db.OpenConnection();
        using var transaction = connection.BeginTransaction();

        var currentStatus = _orders.GetOrderStatusForUpdate(connection, transaction, request.OrderId);
        if (currentStatus != OrderStatus.Posted)
        {
            transaction.Rollback();
            return Result.Fail(Messages.OnlyPostedCanBeUpdated);
        }

        if (request.Status == OrderStatus.Returned)
        {
            RestoreReturnedItemsStock(connection, transaction, request.OrderId);
        }

        _orders.UpdateOrderStatus(connection, transaction, request.OrderId, request.Status, request.StatusDate);
        transaction.Commit();
        return Result.Ok(Messages.DeliveryStatusUpdated);
    }

    private void RestoreReturnedItemsStock(MySqlConnection connection, MySqlTransaction transaction, int orderId)
    {
        var items = _orders.ListOrderItems(connection, orderId, transaction);
        foreach (var item in items)
        {
            _products.IncreaseStock(connection, transaction, item.ProductId, item.Quantity);
        }
    }

    private static Result ValidatePosting(PostingRequest request)
    {
        if (request.OrderId <= 0)
        {
            return Result.Fail(Messages.OrderNotFound);
        }
        if (string.IsNullOrWhiteSpace(request.CourierName))
        {
            return Result.Fail(Messages.CourierRequired);
        }
        return Result.Ok(string.Empty);
    }

    private static Result ValidateStatusUpdate(DeliveryStatusRequest request)
    {
        if (request.OrderId <= 0)
        {
            return Result.Fail(Messages.OrderNotFound);
        }
        if (request.Status != OrderStatus.Delivered && request.Status != OrderStatus.Returned)
        {
            return Result.Fail(Messages.OnlyPostedCanBeUpdated);
        }
        return Result.Ok(string.Empty);
    }
}
