using lenora_ordera_and_inventory_management_system.Models;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Data;

internal sealed class StockOrderRepository
{
    public int InsertOrder(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int wholesalerId,
        DateTime orderDate,
        decimal totalAmount,
        int recordedByUserId)
    {
        using var command = new MySqlCommand(
            """
            INSERT INTO StockOrder (WholesalerID, OrderDate, TotalAmount, RecordedByUserID)
            VALUES (@wholesalerId, @orderDate, @totalAmount, @recordedByUserId);
            """,
            connection,
            transaction);
        command.Parameters.Add("@wholesalerId", MySqlDbType.Int32).Value = wholesalerId;
        command.Parameters.Add("@orderDate", MySqlDbType.Date).Value = orderDate.Date;
        command.Parameters.Add("@totalAmount", MySqlDbType.Decimal).Value = totalAmount;
        command.Parameters.Add("@recordedByUserId", MySqlDbType.Int32).Value = recordedByUserId;
        command.ExecuteNonQuery();
        return Convert.ToInt32(command.LastInsertedId);
    }

    public int InsertItem(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int stockOrderId,
        int productId,
        int quantity,
        decimal unitCost)
    {
        using var command = new MySqlCommand(
            """
            INSERT INTO StockOrderItem (StockOrderID, ProductID, Quantity, UnitCost)
            VALUES (@stockOrderId, @productId, @quantity, @unitCost)
            """,
            connection,
            transaction);
        command.Parameters.Add("@stockOrderId", MySqlDbType.Int32).Value = stockOrderId;
        command.Parameters.Add("@productId", MySqlDbType.Int32).Value = productId;
        command.Parameters.Add("@quantity", MySqlDbType.Int32).Value = quantity;
        command.Parameters.Add("@unitCost", MySqlDbType.Decimal).Value = unitCost;
        return command.ExecuteNonQuery();
    }

    public IReadOnlyList<StockOrder> ListOrders(
        MySqlConnection connection,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? wholesalerId = null)
    {
        var sql = BuildOrdersSql(fromDate, toDate, wholesalerId);
        using var command = new MySqlCommand(sql, connection);
        AddFilterParameters(command, fromDate, toDate, wholesalerId);
        using var reader = command.ExecuteReader();
        var list = new List<StockOrder>();
        while (reader.Read())
        {
            list.Add(new StockOrder
            {
                StockOrderId = reader.GetInt32("StockOrderID"),
                WholesalerId = reader.GetInt32("WholesalerID"),
                WholesalerName = reader.GetString("WholesalerName"),
                OrderDate = reader.GetDateTime("OrderDate"),
                TotalAmount = reader.GetDecimal("TotalAmount"),
                RecordedByUserId = reader.GetInt32("RecordedByUserID"),
                RecordedByUserName = reader.GetString("RecordedByUserName")
            });
        }
        return list;
    }

    public IReadOnlyList<StockOrderItem> ListItems(MySqlConnection connection, int stockOrderId)
    {
        using var command = new MySqlCommand(
            """
            SELECT soi.StockOrderItemID, soi.StockOrderID, soi.ProductID, p.Name AS ProductName,
                   soi.Quantity, soi.UnitCost
            FROM StockOrderItem soi
            INNER JOIN Product p ON soi.ProductID = p.ProductID
            WHERE soi.StockOrderID = @stockOrderId
            ORDER BY p.Name
            """,
            connection);
        command.Parameters.Add("@stockOrderId", MySqlDbType.Int32).Value = stockOrderId;
        using var reader = command.ExecuteReader();
        var list = new List<StockOrderItem>();
        while (reader.Read())
        {
            list.Add(new StockOrderItem
            {
                StockOrderItemId = reader.GetInt32("StockOrderItemID"),
                StockOrderId = reader.GetInt32("StockOrderID"),
                ProductId = reader.GetInt32("ProductID"),
                ProductName = reader.GetString("ProductName"),
                Quantity = reader.GetInt32("Quantity"),
                UnitCost = reader.GetDecimal("UnitCost")
            });
        }
        return list;
    }

    public IReadOnlyList<WholesalerSpendingSummary> GetSpendingByWholesaler(
        MySqlConnection connection,
        DateTime fromDate,
        DateTime toDate,
        int? wholesalerId = null)
    {
        using var command = new MySqlCommand(
            """
            SELECT w.WholesalerID, w.Name AS WholesalerName,
                   COUNT(so.StockOrderID) AS OrderCount,
                   COALESCE(SUM(so.TotalAmount), 0) AS TotalSpending
            FROM Wholesaler w
            INNER JOIN StockOrder so ON w.WholesalerID = so.WholesalerID
            WHERE so.OrderDate >= @fromDate AND so.OrderDate <= @toDate
              AND (@wholesalerId IS NULL OR w.WholesalerID = @wholesalerId)
            GROUP BY w.WholesalerID, w.Name
            ORDER BY TotalSpending DESC, w.Name
            """,
            connection);
        command.Parameters.Add("@fromDate", MySqlDbType.Date).Value = fromDate.Date;
        command.Parameters.Add("@toDate", MySqlDbType.Date).Value = toDate.Date;
        command.Parameters.Add("@wholesalerId", MySqlDbType.Int32).Value = wholesalerId.HasValue ? wholesalerId.Value : DBNull.Value;
        using var reader = command.ExecuteReader();
        var list = new List<WholesalerSpendingSummary>();
        while (reader.Read())
        {
            list.Add(new WholesalerSpendingSummary
            {
                WholesalerId = reader.GetInt32("WholesalerID"),
                WholesalerName = reader.GetString("WholesalerName"),
                OrderCount = reader.GetInt32("OrderCount"),
                TotalSpending = reader.GetDecimal("TotalSpending")
            });
        }
        return list;
    }

    private static string BuildOrdersSql(DateTime? fromDate, DateTime? toDate, int? wholesalerId)
    {
        var sql =
            """
            SELECT so.StockOrderID, so.WholesalerID, w.Name AS WholesalerName,
                   so.OrderDate, so.TotalAmount, so.RecordedByUserID, u.FullName AS RecordedByUserName
            FROM StockOrder so
            INNER JOIN Wholesaler w ON so.WholesalerID = w.WholesalerID
            INNER JOIN `User` u ON so.RecordedByUserID = u.UserID
            WHERE 1=1
            """;
        if (fromDate.HasValue)
        {
            sql += " AND so.OrderDate >= @fromDate";
        }
        if (toDate.HasValue)
        {
            sql += " AND so.OrderDate <= @toDate";
        }
        if (wholesalerId.HasValue)
        {
            sql += " AND so.WholesalerID = @wholesalerId";
        }
        return sql + " ORDER BY so.OrderDate DESC, so.StockOrderID DESC";
    }

    private static void AddFilterParameters(MySqlCommand command, DateTime? fromDate, DateTime? toDate, int? wholesalerId)
    {
        if (fromDate.HasValue)
        {
            command.Parameters.Add("@fromDate", MySqlDbType.Date).Value = fromDate.Value.Date;
        }
        if (toDate.HasValue)
        {
            command.Parameters.Add("@toDate", MySqlDbType.Date).Value = toDate.Value.Date;
        }
        if (wholesalerId.HasValue)
        {
            command.Parameters.Add("@wholesalerId", MySqlDbType.Int32).Value = wholesalerId.Value;
        }
    }
}
