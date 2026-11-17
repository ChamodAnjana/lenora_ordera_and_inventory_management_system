using System.Data;
using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Data;

internal sealed class CustomerRepository
{
    public Customer? FindByPhone(MySqlConnection connection, string phone, MySqlTransaction? transaction = null)
    {
        using var command = new MySqlCommand(
            """
            SELECT CustomerID, Name, Phone, Address, IsRepeat
            FROM Customer
            WHERE Phone = @phone
            """,
            connection,
            transaction);
        command.Parameters.Add("@phone", MySqlDbType.VarChar, Rules.CustomerPhoneMaxLength).Value = phone;
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return ReadCustomer(reader);
    }

    public IReadOnlyList<Customer> FindByName(MySqlConnection connection, string name)
    {
        using var command = new MySqlCommand(
            """
            SELECT CustomerID, Name, Phone, Address, IsRepeat
            FROM Customer
            WHERE Name LIKE @pattern ESCAPE '\\'
            ORDER BY Name
            LIMIT @limit
            """,
            connection);
        command.Parameters.Add("@pattern", MySqlDbType.VarChar).Value = $"%{EscapeLike(name)}%";
        command.Parameters.Add("@limit", MySqlDbType.Int32).Value = Rules.CustomerSearchLimit;
        using var reader = command.ExecuteReader();
        var matches = new List<Customer>();
        while (reader.Read())
        {
            matches.Add(ReadCustomer(reader));
        }

        return matches;
    }

    private static string EscapeLike(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }

    public int Insert(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string name,
        string phone,
        string address)
    {
        using var command = new MySqlCommand(
            """
            INSERT INTO Customer (Name, Phone, Address, IsRepeat)
            VALUES (@name, @phone, @address, FALSE);
            """,
            connection,
            transaction);
        command.Parameters.Add("@name", MySqlDbType.VarChar, Rules.CustomerNameMaxLength).Value = name;
        command.Parameters.Add("@phone", MySqlDbType.VarChar, Rules.CustomerPhoneMaxLength).Value = phone;
        command.Parameters.Add("@address", MySqlDbType.VarChar, Rules.CustomerAddressMaxLength).Value = address;
        command.ExecuteNonQuery();
        return Convert.ToInt32(command.LastInsertedId);
    }

    public int Update(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int customerId,
        string name,
        string address)
    {
        using var command = new MySqlCommand(
            """
            UPDATE Customer
            SET Name = @name, Address = @address
            WHERE CustomerID = @customerId;
            """,
            connection,
            transaction);
        command.Parameters.Add("@customerId", MySqlDbType.Int32).Value = customerId;
        command.Parameters.Add("@name", MySqlDbType.VarChar, Rules.CustomerNameMaxLength).Value = name;
        command.Parameters.Add("@address", MySqlDbType.VarChar, Rules.CustomerAddressMaxLength).Value = address;
        return command.ExecuteNonQuery();
    }

    public int RecalculateRepeatFlag(
        MySqlConnection connection,
        MySqlTransaction transaction,
        int customerId)
    {
        using var command = new MySqlCommand(
            """
            UPDATE Customer
            SET IsRepeat = ((SELECT COUNT(*) FROM CustomerOrder WHERE CustomerID = @customerId AND Status != 'Cancelled') > 1)
            WHERE CustomerID = @customerId;
            """,
            connection,
            transaction);
        command.Parameters.Add("@customerId", MySqlDbType.Int32).Value = customerId;
        return command.ExecuteNonQuery();
    }

    public IReadOnlyList<Customer> ListRepeatCustomers(MySqlConnection connection, string? search = null)
    {
        var sql = """
            SELECT c.CustomerID, c.Name, c.Phone, c.Address, c.IsRepeat, COUNT(o.OrderID) AS OrderCount
            FROM Customer c
            LEFT JOIN CustomerOrder o ON c.CustomerID = o.CustomerID AND o.Status != 'Cancelled'
            WHERE c.IsRepeat = TRUE
            """;
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND (c.Name LIKE @search OR c.Phone LIKE @search)";
        }
        sql += " GROUP BY c.CustomerID, c.Name, c.Phone, c.Address, c.IsRepeat ORDER BY c.Name";

        using var command = new MySqlCommand(sql, connection);
        if (!string.IsNullOrWhiteSpace(search))
        {
            command.Parameters.Add("@search", MySqlDbType.VarChar).Value = $"%{search.Trim()}%";
        }

        using var reader = command.ExecuteReader();
        var list = new List<Customer>();
        while (reader.Read())
        {
            list.Add(ReadCustomerWithCount(reader));
        }
        return list;
    }

    private static Customer ReadCustomer(MySqlDataReader reader)
    {
        return new Customer
        {
            CustomerId = reader.GetInt32("CustomerID"),
            Name = reader.GetString("Name"),
            Phone = reader.GetString("Phone"),
            Address = reader.GetString("Address"),
            IsRepeat = reader.GetBoolean("IsRepeat")
        };
    }

    private static Customer ReadCustomerWithCount(MySqlDataReader reader)
    {
        return new Customer
        {
            CustomerId = reader.GetInt32("CustomerID"),
            Name = reader.GetString("Name"),
            Phone = reader.GetString("Phone"),
            Address = reader.GetString("Address"),
            IsRepeat = reader.GetBoolean("IsRepeat"),
            OrderCount = Convert.ToInt32(reader["OrderCount"])
        };
    }
}
