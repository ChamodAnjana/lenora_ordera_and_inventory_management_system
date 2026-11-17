using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Data;

internal sealed class WholesalerRepository
{
    public IReadOnlyList<Wholesaler> List(MySqlConnection connection, string? search = null)
    {
        var sql = "SELECT WholesalerID, Name, ContactNo, Address FROM Wholesaler";
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " WHERE Name LIKE @search OR ContactNo LIKE @search";
        }
        sql += " ORDER BY Name";

        using var command = new MySqlCommand(sql, connection);
        if (!string.IsNullOrWhiteSpace(search))
        {
            command.Parameters.Add("@search", MySqlDbType.VarChar).Value = $"%{search.Trim()}%";
        }

        using var reader = command.ExecuteReader();
        var list = new List<Wholesaler>();
        while (reader.Read())
        {
            list.Add(new Wholesaler
            {
                WholesalerId = reader.GetInt32("WholesalerID"),
                Name = reader.GetString("Name"),
                ContactNo = reader.GetString("ContactNo"),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString("Address")
            });
        }
        return list;
    }

    public Wholesaler? FindById(MySqlConnection connection, int wholesalerId)
    {
        using var command = new MySqlCommand(
            """
            SELECT WholesalerID, Name, ContactNo, Address
            FROM Wholesaler
            WHERE WholesalerID = @wholesalerId
            """,
            connection);
        command.Parameters.Add("@wholesalerId", MySqlDbType.Int32).Value = wholesalerId;
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Wholesaler
        {
            WholesalerId = reader.GetInt32("WholesalerID"),
            Name = reader.GetString("Name"),
            ContactNo = reader.GetString("ContactNo"),
            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString("Address")
        };
    }

    public int Insert(MySqlConnection connection, WholesalerRequest request)
    {
        using var command = new MySqlCommand(
            """
            INSERT INTO Wholesaler (Name, ContactNo, Address)
            VALUES (@name, @contactNo, @address)
            """,
            connection);
        AddParameters(command, request);
        return command.ExecuteNonQuery();
    }

    public int Update(MySqlConnection connection, int wholesalerId, WholesalerRequest request)
    {
        using var command = new MySqlCommand(
            """
            UPDATE Wholesaler
            SET Name = @name, ContactNo = @contactNo, Address = @address
            WHERE WholesalerID = @wholesalerId
            """,
            connection);
        command.Parameters.Add("@wholesalerId", MySqlDbType.Int32).Value = wholesalerId;
        AddParameters(command, request);
        return command.ExecuteNonQuery();
    }

    public int Delete(MySqlConnection connection, int wholesalerId)
    {
        using var command = new MySqlCommand("DELETE FROM Wholesaler WHERE WholesalerID = @wholesalerId", connection);
        command.Parameters.Add("@wholesalerId", MySqlDbType.Int32).Value = wholesalerId;
        return command.ExecuteNonQuery();
    }

    public bool IsWholesalerUsed(MySqlConnection connection, int wholesalerId)
    {
        using var command = new MySqlCommand(
            "SELECT EXISTS(SELECT 1 FROM StockOrder WHERE WholesalerID = @wholesalerId)",
            connection);
        command.Parameters.Add("@wholesalerId", MySqlDbType.Int32).Value = wholesalerId;
        return Convert.ToBoolean(command.ExecuteScalar());
    }

    private static void AddParameters(MySqlCommand command, WholesalerRequest request)
    {
        command.Parameters.Add("@name", MySqlDbType.VarChar, Rules.WholesalerNameMaxLength).Value = request.Name;
        command.Parameters.Add("@contactNo", MySqlDbType.VarChar, Rules.WholesalerContactNoMaxLength).Value = request.ContactNo;
        command.Parameters.Add("@address", MySqlDbType.VarChar, Rules.WholesalerAddressMaxLength).Value =
            string.IsNullOrWhiteSpace(request.Address) ? DBNull.Value : request.Address.Trim();
    }
}
