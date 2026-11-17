using System.Text;
using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Services;

internal sealed class ExportService
{
    private readonly ProductRepository _products;
    private readonly OrderRepository _orders;

    public ExportService(ProductRepository products, OrderRepository orders)
    {
        _products = products;
        _orders = orders;
    }

    public Result ExportToFile(ExportRequest request)
    {
        try
        {
            Authorizer.RequireOwner();
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return Result.Fail(Messages.FilePathRequired);
            }

            return GenerateAndSave(request);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Fail(Messages.OwnerOnly);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
        catch (IOException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    private Result GenerateAndSave(ExportRequest request)
    {
        using var connection = Db.OpenConnection();
        var content = request.Type switch
        {
            ExportType.Stock => BuildStockCsv(connection),
            ExportType.Orders => BuildOrdersCsv(connection),
            ExportType.Payments => BuildPaymentsCsv(connection),
            _ => string.Empty
        };

        File.WriteAllText(request.FilePath, content, Encoding.UTF8);
        return Result.Ok(Messages.ExportSucceeded);
    }

    private string BuildStockCsv(MySqlConnection connection)
    {
        var items = _products.ListForOwner(connection);
        var builder = new StringBuilder();
        builder.AppendLine("ProductID,Name,Category,CostPrice,SellingPrice,Quantity");
        foreach (var p in items)
        {
            builder.AppendLine($"{p.ProductId},{Escape(p.Name)},{Escape(p.Category)},{p.CostPrice:F2},{p.SellingPrice:F2},{p.Quantity}");
        }
        return builder.ToString();
    }

    private string BuildOrdersCsv(MySqlConnection connection)
    {
        var orders = _orders.ListOrders(connection);
        var builder = new StringBuilder();
        builder.AppendLine("OrderID,OrderDate,CustomerName,CustomerPhone,CustomerAddress,Status,TotalAmount,CourierName,PostingDate,StatusDate,PaymentReceived,PaymentDate,CreatedBy");
        foreach (var o in orders)
        {
            builder.AppendLine(FormatOrderRow(o));
        }
        return builder.ToString();
    }

    private static string FormatOrderRow(CustomerOrder o)
    {
        var posting = o.PostingDate?.ToString("yyyy-MM-dd") ?? string.Empty;
        var statusDate = o.StatusDate?.ToString("yyyy-MM-dd") ?? string.Empty;
        var paymentDate = o.PaymentDate?.ToString("yyyy-MM-dd") ?? string.Empty;
        return $"{o.OrderId},{o.OrderDate:yyyy-MM-dd},{Escape(o.CustomerName)},{Escape(o.CustomerPhone)},{Escape(o.CustomerAddress)},{o.Status},{o.TotalAmount:F2},{Escape(o.CourierName)},{posting},{statusDate},{o.PaymentReceived},{paymentDate},{Escape(o.CreatedByUserName)}";
    }

    private string BuildPaymentsCsv(MySqlConnection connection)
    {
        var orders = _orders.ListOrders(connection);
        var builder = new StringBuilder();
        builder.AppendLine("OrderID,CustomerName,CustomerPhone,OrderDate,DeliveryDate,Status,TotalAmount,PaymentReceived,AmountReceived,PaymentDate");
        foreach (var o in orders)
        {
            builder.AppendLine(FormatPaymentRow(o));
        }
        return builder.ToString();
    }

    private static string FormatPaymentRow(CustomerOrder o)
    {
        var deliveryDate = o.StatusDate?.ToString("yyyy-MM-dd") ?? string.Empty;
        var paymentDate = o.PaymentDate?.ToString("yyyy-MM-dd") ?? string.Empty;
        var amountReceived = o.AmountReceived.HasValue ? o.AmountReceived.Value.ToString("F2") : string.Empty;
        return $"{o.OrderId},{Escape(o.CustomerName)},{Escape(o.CustomerPhone)},{o.OrderDate:yyyy-MM-dd},{deliveryDate},{o.Status},{o.TotalAmount:F2},{o.PaymentReceived},{amountReceived},{paymentDate}";
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
