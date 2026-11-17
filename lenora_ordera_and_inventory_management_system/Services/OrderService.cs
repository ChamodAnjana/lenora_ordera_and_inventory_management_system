using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Services;

internal sealed class OrderService
{
    private readonly OrderRepository _orders;
    private readonly CustomerRepository _customers;
    private readonly ProductRepository _products;

    public OrderService(OrderRepository orders, CustomerRepository customers, ProductRepository products)
    {
        _orders = orders;
        _customers = customers;
        _products = products;
    }

    public Result PlaceOrder(OrderPlacementRequest request)
    {
        try
        {
            var validation = ValidatePlacement(request);
            if (!validation.Success)
            {
                return validation;
            }

            return SaveNewOrder(request);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result UpdateOrder(OrderUpdateRequest request)
    {
        try
        {
            var validation = ValidateUpdate(request);
            if (!validation.Success)
            {
                return validation;
            }

            return SaveUpdatedOrder(request);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result CancelOrder(int orderId)
    {
        try
        {
            Authorizer.RequireOwner();
            return ExecuteCancel(orderId);
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

    public Result ListOrders(
        List<CustomerOrder> target,
        string? search = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        OrderStatus? status = null)
    {
        try
        {
            using var connection = Db.OpenConnection();
            target.AddRange(_orders.ListOrders(connection, search, fromDate, toDate, status));
            return Result.Ok(string.Empty);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result GetOrderDetails(int orderId, out CustomerOrder? order, List<OrderItem> items)
    {
        order = null;
        try
        {
            using var connection = Db.OpenConnection();
            order = _orders.FindById(connection, orderId);
            if (order is null)
            {
                return Result.Fail(Messages.OrderNotFound);
            }

            items.AddRange(_orders.ListOrderItems(connection, orderId));
            return Result.Ok(string.Empty);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result ListRepeatCustomers(List<Customer> target, string? search = null)
    {
        try
        {
            using var connection = Db.OpenConnection();
            target.AddRange(_customers.ListRepeatCustomers(connection, search));
            return Result.Ok(string.Empty);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result FindCustomerByPhone(string phone, out Customer? customer)
    {
        customer = null;
        var check = RequireMobile(phone, out var canonical);
        if (!check.Success)
        {
            return check;
        }

        try
        {
            using var connection = Db.OpenConnection();
            customer = _customers.FindByPhone(connection, canonical);
            return Result.Ok(string.Empty);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result FindCustomersByName(string name, List<Customer> target)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail(Messages.CustomerNameRequired);
        }

        try
        {
            using var connection = Db.OpenConnection();
            target.AddRange(_customers.FindByName(connection, name.Trim()));
            return Result.Ok(string.Empty);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    private Result SaveNewOrder(OrderPlacementRequest request)
    {
        using var connection = Db.OpenConnection();
        using var transaction = connection.BeginTransaction();
        var stockCheck = CheckAndLockStock(connection, transaction, request.Items);
        if (!stockCheck.Success)
        {
            transaction.Rollback();
            return stockCheck;
        }

        var customerId = EnsureCustomer(connection, transaction, request.CustomerName, request.CustomerPhone, request.CustomerAddress);
        var userId = CurrentSession.Current?.UserId ?? 0;
        var orderId = _orders.InsertOrder(connection, transaction, customerId, DateTime.Today, userId);

        InsertItemsAndReduceStock(connection, transaction, orderId, request.Items);
        _customers.RecalculateRepeatFlag(connection, transaction, customerId);
        transaction.Commit();
        return Result.Ok(Messages.OrderSaved);
    }

    private Result SaveUpdatedOrder(OrderUpdateRequest request)
    {
        using var connection = Db.OpenConnection();
        using var transaction = connection.BeginTransaction();

        var currentStatus = _orders.GetOrderStatusForUpdate(connection, transaction, request.OrderId);
        if (currentStatus != OrderStatus.Placed)
        {
            transaction.Rollback();
            return Result.Fail(Messages.OnlyPlacedOrdersCanBeUpdated);
        }

        RevertOldItemsStock(connection, transaction, request.OrderId);
        var stockCheck = CheckAndLockStock(connection, transaction, request.Items);
        if (!stockCheck.Success)
        {
            transaction.Rollback();
            return stockCheck;
        }

        _orders.DeleteOrderItems(connection, transaction, request.OrderId);
        InsertItemsAndReduceStock(connection, transaction, request.OrderId, request.Items);

        var customerId = EnsureCustomer(connection, transaction, request.CustomerName, request.CustomerPhone, request.CustomerAddress);
        _customers.RecalculateRepeatFlag(connection, transaction, customerId);
        transaction.Commit();
        return Result.Ok(Messages.OrderUpdated);
    }

    private Result ExecuteCancel(int orderId)
    {
        using var connection = Db.OpenConnection();
        using var transaction = connection.BeginTransaction();

        var currentStatus = _orders.GetOrderStatusForUpdate(connection, transaction, orderId);
        if (currentStatus != OrderStatus.Placed)
        {
            transaction.Rollback();
            return Result.Fail(Messages.OnlyPlacedOrdersCanBeCancelled);
        }

        var existingOrder = _orders.FindById(connection, orderId, transaction);
        RevertOldItemsStock(connection, transaction, orderId);
        _orders.UpdateOrderStatus(connection, transaction, orderId, OrderStatus.Cancelled, DateTime.Today);

        if (existingOrder != null)
        {
            _customers.RecalculateRepeatFlag(connection, transaction, existingOrder.CustomerId);
        }

        transaction.Commit();
        return Result.Ok(Messages.OrderCancelled);
    }

    private Result CheckAndLockStock(
        MySqlConnection connection,
        MySqlTransaction transaction,
        IReadOnlyList<OrderItemRequest> items)
    {
        foreach (var item in items)
        {
            var product = _products.LockProductForUpdate(connection, transaction, item.ProductId);
            if (product is null || item.Quantity > product.Quantity)
            {
                var available = product?.Quantity ?? 0;
                var name = product?.Name ?? item.ProductName;
                return Result.Fail(Messages.InsufficientStock(name, available));
            }
        }
        return Result.Ok(string.Empty);
    }

    private void RevertOldItemsStock(MySqlConnection connection, MySqlTransaction transaction, int orderId)
    {
        var oldItems = _orders.ListOrderItems(connection, orderId, transaction);
        foreach (var item in oldItems)
        {
            _products.IncreaseStock(connection, transaction, item.ProductId, item.Quantity);
        }
    }

    private void InsertItemsAndReduceStock(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int orderId,
        IReadOnlyList<OrderItemRequest> items)
    {
        foreach (var item in items)
        {
            _orders.InsertOrderItem(connection, transaction, orderId, item.ProductId, item.Quantity, item.SellingPrice);
            _products.ReduceStock(connection, transaction, item.ProductId, item.Quantity);
        }
    }

    private int EnsureCustomer(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string name,
        string phone,
        string address)
    {
        var canonical = PhoneNumber.CanonicalMobile(phone);
        var existing = _customers.FindByPhone(connection, canonical, transaction);
        if (existing is null)
        {
            return _customers.Insert(connection, transaction, name.Trim(), canonical, address.Trim());
        }

        _customers.Update(connection, transaction, existing.CustomerId, name.Trim(), address.Trim());
        return existing.CustomerId;
    }

    private static Result ValidatePlacement(OrderPlacementRequest request)
    {
        return ValidateDetails(request.CustomerName, request.CustomerPhone, request.CustomerAddress, request.Items);
    }

    private static Result ValidateUpdate(OrderUpdateRequest request)
    {
        if (request.OrderId <= 0)
        {
            return Result.Fail(Messages.OrderNotFound);
        }

        return ValidateDetails(request.CustomerName, request.CustomerPhone, request.CustomerAddress, request.Items);
    }

    private static Result ValidateDetails(
        string name,
        string phone,
        string address,
        IReadOnlyList<OrderItemRequest> items)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail(Messages.CustomerNameRequired);
        }
        var mobile = RequireMobile(phone, out _);
        if (!mobile.Success)
        {
            return mobile;
        }
        if (string.IsNullOrWhiteSpace(address))
        {
            return Result.Fail(Messages.CustomerAddressRequired);
        }
        if (items.Count == 0)
        {
            return Result.Fail(Messages.OrderRequiresItems);
        }
        if (items.Any(i => i.Quantity <= 0))
        {
            return Result.Fail(Messages.QuantityInvalid);
        }

        return Result.Ok(string.Empty);
    }

    private static Result RequireMobile(string phone, out string canonical)
    {
        canonical = string.Empty;
        if (string.IsNullOrWhiteSpace(phone))
        {
            return Result.Fail(Messages.CustomerPhoneRequired);
        }

        canonical = PhoneNumber.CanonicalMobile(phone);
        if (canonical.Length == 0)
        {
            return Result.Fail(Messages.CustomerPhoneInvalid);
        }

        return Result.Ok(string.Empty);
    }
}
