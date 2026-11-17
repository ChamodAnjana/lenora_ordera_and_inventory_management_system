using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Data;

internal sealed class ProductRepository
{
    public IReadOnlyList<Product> ListForOwner(MySqlConnection connection, string? search = null, string? category = null)
    {
        var sql = BuildSearchSql(true, search, category);
        using var command = new MySqlCommand(sql, connection);
        AddFilterParameters(command, search, category);
        using var reader = command.ExecuteReader();
        var list = new List<Product>();
        while (reader.Read())
        {
            list.Add(new Product
            {
                ProductId = reader.GetInt32("ProductID"),
                Name = reader.GetString("Name"),
                Category = reader.GetString("Category"),
                CostPrice = reader.GetDecimal("CostPrice"),
                SellingPrice = reader.GetDecimal("SellingPrice"),
                Quantity = reader.GetInt32("Quantity")
            });
        }
        return list;
    }

    public IReadOnlyList<ProductCatalogItem> ListForAssistant(MySqlConnection connection, string? search = null, string? category = null)
    {
        var sql = BuildSearchSql(false, search, category);
        using var command = new MySqlCommand(sql, connection);
        AddFilterParameters(command, search, category);
        using var reader = command.ExecuteReader();
        var list = new List<ProductCatalogItem>();
        while (reader.Read())
        {
            list.Add(new ProductCatalogItem
            {
                ProductId = reader.GetInt32("ProductID"),
                Name = reader.GetString("Name"),
                Category = reader.GetString("Category"),
                SellingPrice = reader.GetDecimal("SellingPrice"),
                Quantity = reader.GetInt32("Quantity")
            });
        }
        return list;
    }

    public Product? FindById(MySqlConnection connection, int productId)
    {
        using var command = new MySqlCommand(
            """
            SELECT ProductID, Name, Category, CostPrice, SellingPrice, Quantity
            FROM Product
            WHERE ProductID = @productId
            """,
            connection);
        command.Parameters.Add("@productId", MySqlDbType.Int32).Value = productId;
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Product
        {
            ProductId = reader.GetInt32("ProductID"),
            Name = reader.GetString("Name"),
            Category = reader.GetString("Category"),
            CostPrice = reader.GetDecimal("CostPrice"),
            SellingPrice = reader.GetDecimal("SellingPrice"),
            Quantity = reader.GetInt32("Quantity")
        };
    }

    public int Insert(MySqlConnection connection, ProductRequest request)
    {
        using var command = new MySqlCommand(
            """
            INSERT INTO Product (Name, Category, CostPrice, SellingPrice, Quantity)
            VALUES (@name, @category, @costPrice, @sellingPrice, @quantity)
            """,
            connection);
        AddParameters(command, request);
        return command.ExecuteNonQuery();
    }

    public int Update(MySqlConnection connection, int productId, ProductRequest request)
    {
        using var command = new MySqlCommand(
            """
            UPDATE Product
            SET Name = @name, Category = @category, CostPrice = @costPrice,
                SellingPrice = @sellingPrice, Quantity = @quantity
            WHERE ProductID = @productId
            """,
            connection);
        command.Parameters.Add("@productId", MySqlDbType.Int32).Value = productId;
        AddParameters(command, request);
        return command.ExecuteNonQuery();
    }

    public int Delete(MySqlConnection connection, int productId)
    {
        using var command = new MySqlCommand("DELETE FROM Product WHERE ProductID = @productId", connection);
        command.Parameters.Add("@productId", MySqlDbType.Int32).Value = productId;
        return command.ExecuteNonQuery();
    }

    public bool IsProductUsed(MySqlConnection connection, int productId)
    {
        using var command = new MySqlCommand(
            """
            SELECT (
                EXISTS(SELECT 1 FROM OrderItem WHERE ProductID = @productId)
                OR EXISTS(SELECT 1 FROM StockOrderItem WHERE ProductID = @productId)
            )
            """,
            connection);
        command.Parameters.Add("@productId", MySqlDbType.Int32).Value = productId;
        return Convert.ToBoolean(command.ExecuteScalar());
    }

    public int IncreaseStock(MySqlConnection connection, MySqlTransaction transaction, int productId, int amount)
    {
        using var command = new MySqlCommand(
            """
            UPDATE Product
            SET Quantity = Quantity + @amount
            WHERE ProductID = @productId
            """,
            connection,
            transaction);
        command.Parameters.Add("@amount", MySqlDbType.Int32).Value = amount;
        command.Parameters.Add("@productId", MySqlDbType.Int32).Value = productId;
        return command.ExecuteNonQuery();
    }

    public int ReduceStock(MySqlConnection connection, MySqlTransaction transaction, int productId, int amount)
    {
        using var command = new MySqlCommand(
            """
            UPDATE Product
            SET Quantity = Quantity - @amount
            WHERE ProductID = @productId
            """,
            connection,
            transaction);
        command.Parameters.Add("@amount", MySqlDbType.Int32).Value = amount;
        command.Parameters.Add("@productId", MySqlDbType.Int32).Value = productId;
        return command.ExecuteNonQuery();
    }

    public ProductCatalogItem? LockProductForUpdate(MySqlConnection connection, MySqlTransaction transaction, int productId)
    {
        using var command = new MySqlCommand(
            """
            SELECT ProductID, Name, Category, SellingPrice, Quantity
            FROM Product
            WHERE ProductID = @productId
            FOR UPDATE
            """,
            connection,
            transaction);
        command.Parameters.Add("@productId", MySqlDbType.Int32).Value = productId;
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new ProductCatalogItem
        {
            ProductId = reader.GetInt32("ProductID"),
            Name = reader.GetString("Name"),
            Category = reader.GetString("Category"),
            SellingPrice = reader.GetDecimal("SellingPrice"),
            Quantity = reader.GetInt32("Quantity")
        };
    }

    public IReadOnlyList<string> ListCategories(MySqlConnection connection)
    {
        using var command = new MySqlCommand("SELECT DISTINCT Category FROM Product ORDER BY Category", connection);
        using var reader = command.ExecuteReader();
        var list = new List<string>();
        while (reader.Read())
        {
            list.Add(reader.GetString("Category"));
        }
        return list;
    }

    private static void AddParameters(MySqlCommand command, ProductRequest request)
    {
        command.Parameters.Add("@name", MySqlDbType.VarChar, Rules.ProductNameMaxLength).Value = request.Name;
        command.Parameters.Add("@category", MySqlDbType.VarChar, Rules.ProductCategoryMaxLength).Value = request.Category;
        command.Parameters.Add("@costPrice", MySqlDbType.Decimal).Value = request.CostPrice;
        command.Parameters.Add("@sellingPrice", MySqlDbType.Decimal).Value = request.SellingPrice;
        command.Parameters.Add("@quantity", MySqlDbType.Int32).Value = request.Quantity;
    }

    private static string BuildSearchSql(bool includeCost, string? search, string? category)
    {
        var columns = includeCost
            ? "ProductID, Name, Category, CostPrice, SellingPrice, Quantity"
            : "ProductID, Name, Category, SellingPrice, Quantity";
        var where = "WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(search))
        {
            where += " AND Name LIKE @search";
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            where += " AND Category = @category";
        }
        return $"SELECT {columns} FROM Product {where} ORDER BY Name";
    }

    private static void AddFilterParameters(MySqlCommand command, string? search, string? category)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            command.Parameters.Add("@search", MySqlDbType.VarChar).Value = $"%{search.Trim()}%";
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            command.Parameters.Add("@category", MySqlDbType.VarChar).Value = category.Trim();
        }
    }
}
