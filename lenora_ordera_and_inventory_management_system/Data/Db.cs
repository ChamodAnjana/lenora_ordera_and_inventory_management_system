using System.Text.Json;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Data;

internal static class Db
{
    public static MySqlConnection OpenConnection()
    {
        var connection = new MySqlConnection(ReadConnectionString());
        connection.Open();
        return connection;
    }

    private static string ReadConnectionString()
    {
        var envConnectionString = Environment.GetEnvironmentVariable("LENORA_DB_CONNECTION");
        if (!string.IsNullOrWhiteSpace(envConnectionString))
        {
            return envConnectionString;
        }

        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var connectionString = document.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("Lenora")
            .GetString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string is missing. Set LENORA_DB_CONNECTION environment variable or configure appsettings.json.");
        }

        return connectionString;
    }
}