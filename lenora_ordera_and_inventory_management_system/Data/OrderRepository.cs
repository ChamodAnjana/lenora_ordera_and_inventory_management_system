using System.Data;
using lenora_ordera_and_inventory_management_system.Models;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Data;

internal sealed class OrderRepository
{
    public int InsertOrder(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int customerId,
        DateTime orderDate,
        int createdByUserId)
    {
        using var command = new MySqlCommand(
            """
            INSERT INTO CustomerOrder (CustomerID, OrderDate, Status, CreatedByUserID)
            VALUES (@customerId, @orderDate, 'Placed', @createdByUserId);
            """,
            connection,
            transaction);
        command.Parameters.Add("@customerId", MySqlDbType.Int32).Value = customerId;
        command.Parameters.Add("@orderDate", MySqlDbType.Date).Value = orderDate.Date;
        command.Parameters.Add("@createdByUserId", MySqlDbType.Int32).Value = createdByUserId;
        command.ExecuteNonQuery();
        return Convert.ToInt32(command.LastInsertedId);
    }

    public int InsertOrderItem(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int orderId,
        int productId,
        int quantity,
        decimal sellingPrice)
    {
        using var command = new MySqlCommand(
            """
            INSERT INTO OrderItem (OrderID, ProductID, Quantity, SellingPrice)
            VALUES (@orderId, @productId, @quantity, @sellingPrice);
            """,
            connection,
            transaction);
        command.Parameters.Add("@orderId", MySqlDbType.Int32).Value = orderId;
        command.Parameters.Add("@productId", MySqlDbType.Int32).Value = productId;
        command.Parameters.Add("@quantity", MySqlDbType.Int32).Value = quantity;
        command.Parameters.Add("@sellingPrice", MySqlDbType.Decimal).Value = sellingPrice;
        return command.ExecuteNonQuery();
    }

    public OrderStatus? GetOrderStatusForUpdate(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int orderId)
    {
        using var command = new MySqlCommand(
            """
            SELECT Status
            FROM CustomerOrder
            WHERE OrderID = @orderId
            FOR UPDATE;
            """,
            connection,
            transaction);
        command.Parameters.Add("@orderId", MySqlDbType.Int32).Value = orderId;
        var result = command.ExecuteScalar();
        if (result is null || result is DBNull)
        {
            return null;
        }

        return Enum.Parse<OrderStatus>(result.ToString()!);
    }

    public int DeleteOrderItems(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int orderId)
    {
        using var command = new MySqlCommand(
            "DELETE FROM OrderItem WHERE OrderID = @orderId;",
            connection,
            transaction);
        command.Parameters.Add("@orderId", MySqlDbType.Int32).Value = orderId;
        return command.ExecuteNonQuery();
    }

    public int UpdateOrderStatus(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int orderId,
        OrderStatus status,
        DateTime statusDate)
    {
        using var command = new MySqlCommand(
            """
            UPDATE CustomerOrder
            SET Status = @status, StatusDate = @statusDate
            WHERE OrderID = @orderId;
            """,
            connection,
            transaction);
        command.Parameters.Add("@orderId", MySqlDbType.Int32).Value = orderId;
        command.Parameters.Add("@status", MySqlDbType.VarChar).Value = status.ToString();
        command.Parameters.Add("@statusDate", MySqlDbType.Date).Value = statusDate.Date;
        return command.ExecuteNonQuery();
    }

    public int RecordPosting(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int orderId,
        string courierName,
        DateTime postingDate)
    {
        using var command = new MySqlCommand(
            """
            UPDATE CustomerOrder
            SET Status = 'Posted',
                CourierName = @courierName,
                PostingDate = @postingDate,
                StatusDate = @postingDate
            WHERE OrderID = @orderId;
            """,
            connection,
            transaction);
        command.Parameters.Add("@orderId", MySqlDbType.Int32).Value = orderId;
        command.Parameters.Add("@courierName", MySqlDbType.VarChar, 50).Value = courierName;
        command.Parameters.Add("@postingDate", MySqlDbType.Date).Value = postingDate.Date;
        return command.ExecuteNonQuery();
    }

    public IReadOnlyList<FollowUpOrder> ListFollowUpOrders(
        MySqlConnection connection,
        DateTime thresholdDate)
    {
        using var command = new MySqlCommand(
            """
            SELECT o.OrderID, c.Name AS CustomerName, c.Phone AS CustomerPhone,
                   COALESCE(o.CourierName, '') AS CourierName, o.PostingDate
            FROM CustomerOrder o
            JOIN Customer c ON o.CustomerID = c.CustomerID
            WHERE o.Status = 'Posted'
              AND o.PostingDate IS NOT NULL
              AND o.PostingDate <= @thresholdDate
            ORDER BY o.PostingDate ASC, o.OrderID ASC;
            """,
            connection);
        command.Parameters.Add("@thresholdDate", MySqlDbType.Date).Value = thresholdDate.Date;

        using var reader = command.ExecuteReader();
        var list = new List<FollowUpOrder>();
        while (reader.Read())
        {
            list.Add(new FollowUpOrder
            {
                OrderId = reader.GetInt32("OrderID"),
                CustomerName = reader.GetString("CustomerName"),
                CustomerPhone = reader.GetString("CustomerPhone"),
                CourierName = reader.GetString("CourierName"),
                PostingDate = reader.GetDateTime("PostingDate")
            });
        }
        return list;
    }

    public int ConfirmCodPayment(
        MySqlConnection connection,
        int orderId,
        decimal amountReceived,
        DateTime paymentDate)
    {
        using var command = new MySqlCommand(
            """
            UPDATE CustomerOrder
            SET PaymentReceived = TRUE,
                AmountReceived = @amountReceived,
                PaymentDate = @paymentDate
            WHERE OrderID = @orderId;
            """,
            connection);
        command.Parameters.Add("@orderId", MySqlDbType.Int32).Value = orderId;
        command.Parameters.Add("@amountReceived", MySqlDbType.Decimal).Value = amountReceived;
        command.Parameters.Add("@paymentDate", MySqlDbType.Date).Value = paymentDate.Date;
        return command.ExecuteNonQuery();
    }

    public IReadOnlyList<PendingPaymentOrder> ListPendingCodPayments(MySqlConnection connection)
    {
        using var command = new MySqlCommand(
            """
            SELECT o.OrderID, c.Name AS CustomerName, c.Phone AS CustomerPhone,
                   o.OrderDate, o.StatusDate,
                   COALESCE(SUM(oi.Quantity * oi.SellingPrice), 0.00) AS TotalAmount
            FROM CustomerOrder o
            JOIN Customer c ON o.CustomerID = c.CustomerID
            LEFT JOIN OrderItem oi ON o.OrderID = oi.OrderID
            WHERE o.Status = 'Delivered'
              AND o.PaymentReceived = FALSE
            GROUP BY o.OrderID, c.Name, c.Phone, o.OrderDate, o.StatusDate
            ORDER BY o.StatusDate ASC, o.OrderID ASC;
            """,
            connection);

        using var reader = command.ExecuteReader();
        var list = new List<PendingPaymentOrder>();
        while (reader.Read())
        {
            list.Add(new PendingPaymentOrder
            {
                OrderId = reader.GetInt32("OrderID"),
                CustomerName = reader.GetString("CustomerName"),
                CustomerPhone = reader.GetString("CustomerPhone"),
                OrderDate = reader.GetDateTime("OrderDate"),
                StatusDate = reader.IsDBNull("StatusDate") ? null : reader.GetDateTime("StatusDate"),
                TotalAmount = reader.GetDecimal("TotalAmount")
            });
        }
        return list;
    }

    public CustomerOrder? FindById(MySqlConnection connection, int orderId, MySqlTransaction? transaction = null)
    {
        using var command = new MySqlCommand(
            """
            SELECT o.OrderID, o.CustomerID, c.Name AS CustomerName, c.Phone AS CustomerPhone,
                   c.Address AS CustomerAddress, c.IsRepeat AS IsRepeatCustomer,
                   o.OrderDate, o.Status, o.CourierName, o.PostingDate, o.StatusDate,
                   o.PaymentReceived, o.AmountReceived, o.PaymentDate,
                   o.CreatedByUserID, u.FullName AS CreatedByUserName,
                   COALESCE(SUM(oi.Quantity * oi.SellingPrice), 0.00) AS TotalAmount
            FROM CustomerOrder o
            JOIN Customer c ON o.CustomerID = c.CustomerID
            JOIN `User` u ON o.CreatedByUserID = u.UserID
            LEFT JOIN OrderItem oi ON o.OrderID = oi.OrderID
            WHERE o.OrderID = @orderId
            GROUP BY o.OrderID, o.CustomerID, c.Name, c.Phone, c.Address, c.IsRepeat,
                     o.OrderDate, o.Status, o.CourierName, o.PostingDate, o.StatusDate,
                     o.PaymentReceived, o.AmountReceived, o.PaymentDate,
                     o.CreatedByUserID, u.FullName;
            """,
            connection,
            transaction);
        command.Parameters.Add("@orderId", MySqlDbType.Int32).Value = orderId;
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return ReadOrder(reader);
    }

    public IReadOnlyList<CustomerOrder> ListOrders(
        MySqlConnection connection,
        string? search = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        OrderStatus? status = null)
    {
        var sql = """
            SELECT o.OrderID, o.CustomerID, c.Name AS CustomerName, c.Phone AS CustomerPhone,
                   c.Address AS CustomerAddress, c.IsRepeat AS IsRepeatCustomer,
                   o.OrderDate, o.Status, o.CourierName, o.PostingDate, o.StatusDate,
                   o.PaymentReceived, o.AmountReceived, o.PaymentDate,
                   o.CreatedByUserID, u.FullName AS CreatedByUserName,
                   COALESCE(SUM(oi.Quantity * oi.SellingPrice), 0.00) AS TotalAmount
            FROM CustomerOrder o
            JOIN Customer c ON o.CustomerID = c.CustomerID
            JOIN `User` u ON o.CreatedByUserID = u.UserID
            LEFT JOIN OrderItem oi ON o.OrderID = oi.OrderID
            WHERE 1=1
            """;

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (c.Name LIKE @search OR c.Phone LIKE @search)";
        }
        if (fromDate.HasValue)
        {
            sql += " AND o.OrderDate >= @fromDate";
        }
        if (toDate.HasValue)
        {
            sql += " AND o.OrderDate <= @toDate";
        }
        if (status.HasValue)
        {
            sql += " AND o.Status = @status";
        }

        sql += """
             GROUP BY o.OrderID, o.CustomerID, c.Name, c.Phone, c.Address, c.IsRepeat,
                      o.OrderDate, o.Status, o.CourierName, o.PostingDate, o.StatusDate,
                      o.PaymentReceived, o.AmountReceived, o.PaymentDate,
                      o.CreatedByUserID, u.FullName
             ORDER BY o.OrderDate DESC, o.OrderID DESC;
            """;

        using var command = new MySqlCommand(sql, connection);
        AddListParameters(command, search, fromDate, toDate, status);

        using var reader = command.ExecuteReader();
        var list = new List<CustomerOrder>();
        while (reader.Read())
        {
            list.Add(ReadOrder(reader));
        }
        return list;
    }

    public IReadOnlyList<OrderItem> ListOrderItems(
        MySqlConnection connection,
        int orderId,
        MySqlTransaction? transaction = null)
    {
        using var command = new MySqlCommand(
            """
            SELECT oi.OrderItemID, oi.OrderID, oi.ProductID, p.Name AS ProductName,
                   oi.Quantity, oi.SellingPrice
            FROM OrderItem oi
            JOIN Product p ON oi.ProductID = p.ProductID
            WHERE oi.OrderID = @orderId
            ORDER BY p.Name;
            """,
            connection,
            transaction);
        command.Parameters.Add("@orderId", MySqlDbType.Int32).Value = orderId;
        using var reader = command.ExecuteReader();
        var list = new List<OrderItem>();
        while (reader.Read())
        {
            list.Add(new OrderItem
            {
                OrderItemId = reader.GetInt32("OrderItemID"),
                OrderId = reader.GetInt32("OrderID"),
                ProductId = reader.GetInt32("ProductID"),
                ProductName = reader.GetString("ProductName"),
                Quantity = reader.GetInt32("Quantity"),
                SellingPrice = reader.GetDecimal("SellingPrice")
            });
        }
        return list;
    }

    private static void AddListParameters(
        MySqlCommand command,
        string? search,
        DateTime? fromDate,
        DateTime? toDate,
        OrderStatus? status)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            command.Parameters.Add("@search", MySqlDbType.VarChar).Value = $"%{search.Trim()}%";
        }
        if (fromDate.HasValue)
        {
            command.Parameters.Add("@fromDate", MySqlDbType.Date).Value = fromDate.Value.Date;
        }
        if (toDate.HasValue)
        {
            command.Parameters.Add("@toDate", MySqlDbType.Date).Value = toDate.Value.Date;
        }
        if (status.HasValue)
        {
            command.Parameters.Add("@status", MySqlDbType.VarChar).Value = status.Value.ToString();
        }
    }

    private static CustomerOrder ReadOrder(MySqlDataReader reader)
    {
        return new CustomerOrder
        {
            OrderId = reader.GetInt32("OrderID"),
            CustomerId = reader.GetInt32("CustomerID"),
            CustomerName = reader.GetString("CustomerName"),
            CustomerPhone = reader.GetString("CustomerPhone"),
            CustomerAddress = reader.GetString("CustomerAddress"),
            IsRepeatCustomer = reader.GetBoolean("IsRepeatCustomer"),
            OrderDate = reader.GetDateTime("OrderDate"),
            Status = Enum.Parse<OrderStatus>(reader.GetString("Status")),
            CourierName = reader.IsDBNull("CourierName") ? null : reader.GetString("CourierName"),
            PostingDate = reader.IsDBNull("PostingDate") ? null : reader.GetDateTime("PostingDate"),
            StatusDate = reader.IsDBNull("StatusDate") ? null : reader.GetDateTime("StatusDate"),
            PaymentReceived = reader.GetBoolean("PaymentReceived"),
            AmountReceived = reader.IsDBNull("AmountReceived") ? null : reader.GetDecimal("AmountReceived"),
            PaymentDate = reader.IsDBNull("PaymentDate") ? null : reader.GetDateTime("PaymentDate"),
            CreatedByUserId = reader.GetInt32("CreatedByUserID"),
            CreatedByUserName = reader.GetString("CreatedByUserName"),
            TotalAmount = reader.GetDecimal("TotalAmount")
        };
    }
}
