using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Services;

internal sealed class StockService
{
    private readonly StockOrderRepository _stockOrders;
    private readonly ProductRepository _products;

    public StockService(StockOrderRepository stockOrders, ProductRepository products)
    {
        _stockOrders = stockOrders;
        _products = products;
    }

    public Result RecordStockOrder(StockOrderRequest request)
    {
        try
        {
            Authorizer.RequireOwner();
            var validation = Validate(request);
            if (!validation.Success)
            {
                return validation;
            }

            return SaveStockOrder(request);
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

    public Result ListStockOrders(
        List<StockOrder> orders,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? wholesalerId = null)
    {
        try
        {
            Authorizer.RequireOwner();
            using var connection = Db.OpenConnection();
            orders.AddRange(_stockOrders.ListOrders(connection, fromDate, toDate, wholesalerId));
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

    public Result ListOrderItems(int stockOrderId, List<StockOrderItem> items)
    {
        try
        {
            Authorizer.RequireOwner();
            using var connection = Db.OpenConnection();
            items.AddRange(_stockOrders.ListItems(connection, stockOrderId));
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

    public Result GetSpendingSummary(
        List<WholesalerSpendingSummary> summary,
        DateTime fromDate,
        DateTime toDate,
        int? wholesalerId = null)
    {
        try
        {
            Authorizer.RequireOwner();
            using var connection = Db.OpenConnection();
            summary.AddRange(_stockOrders.GetSpendingByWholesaler(connection, fromDate, toDate, wholesalerId));
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

    private Result SaveStockOrder(StockOrderRequest request)
    {
        var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitCost);
        var recordedByUserId = CurrentSession.Current?.UserId ?? 0;

        using var connection = Db.OpenConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            var orderId = _stockOrders.InsertOrder(
                connection,
                transaction,
                request.WholesalerId,
                request.OrderDate,
                totalAmount,
                recordedByUserId);

            foreach (var item in request.Items)
            {
                _stockOrders.InsertItem(connection, transaction, orderId, item.ProductId, item.Quantity, item.UnitCost);
                _products.IncreaseStock(connection, transaction, item.ProductId, item.Quantity);
            }

            transaction.Commit();
            return Result.Ok(Messages.StockOrderSaved);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static Result Validate(StockOrderRequest request)
    {
        if (request.WholesalerId <= 0)
        {
            return Result.Fail(Messages.SelectWholesaler);
        }

        if (request.Items.Count == 0)
        {
            return Result.Fail(Messages.StockOrderRequiresItems);
        }

        foreach (var item in request.Items)
        {
            if (item.ProductId <= 0)
            {
                return Result.Fail(Messages.SelectProduct);
            }

            if (item.Quantity <= 0)
            {
                return Result.Fail(Messages.QuantityInvalid);
            }

            if (item.UnitCost < 0)
            {
                return Result.Fail(Messages.UnitCostInvalid);
            }
        }

        return Result.Ok(string.Empty);
    }
}
