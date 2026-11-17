using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Data;

internal sealed class UserRepository
{
    public int Count(MySqlConnection connection, MySqlTransaction transaction)
    {
        using var command = new MySqlCommand("SELECT COUNT(*) FROM `User`", connection, transaction);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public StoredUser? FindByUsername(MySqlConnection connection, string username)
    {
        using var command = new MySqlCommand(
            """
            SELECT UserID, FullName, Username, PasswordHash, Role, IsActive, MustChangePassword
            FROM `User`
            WHERE Username = @username
            """,
            connection);
        command.Parameters.Add("@username", MySqlDbType.VarChar, Rules.UsernameMaxLength).Value = username;
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return ReadUser(reader);
    }

    public IReadOnlyList<User> List(MySqlConnection connection)
    {
        using var command = new MySqlCommand(
            """
            SELECT UserID, FullName, Username, Role, IsActive, MustChangePassword
            FROM `User`
            ORDER BY FullName, Username
            """,
            connection);
        using var reader = command.ExecuteReader();
        return ReadAccounts(reader);
    }

    public int SetActive(MySqlConnection connection, int userId, bool isActive)
    {
        using var command = new MySqlCommand(
            """
            UPDATE `User`
            SET IsActive = @isActive
            WHERE UserID = @userId
            """,
            connection);
        command.Parameters.Add("@isActive", MySqlDbType.Bool).Value = isActive;
        command.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
        return command.ExecuteNonQuery();
    }

    public int UpdatePassword(MySqlConnection connection, int userId, string passwordHash)
    {
        using var command = new MySqlCommand(
            """
            UPDATE `User`
            SET PasswordHash = @passwordHash, MustChangePassword = false
            WHERE UserID = @userId
            """,
            connection);
        command.Parameters.Add("@passwordHash", MySqlDbType.VarChar, Rules.PasswordHashMaxLength).Value = passwordHash;
        command.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
        return command.ExecuteNonQuery();
    }

    public int UpdateMustChangePassword(MySqlConnection connection, int userId, bool mustChange)
    {
        using var command = new MySqlCommand(
            """
            UPDATE `User`
            SET MustChangePassword = @mustChange
            WHERE UserID = @userId
            """,
            connection);
        command.Parameters.Add("@mustChange", MySqlDbType.Bool).Value = mustChange;
        command.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
        return command.ExecuteNonQuery();
    }

    public int FindActiveOwnerCount(MySqlConnection connection)
    {
        using var command = new MySqlCommand(
            """
            SELECT COUNT(*) FROM `User`
            WHERE Role = @role AND IsActive = true
            """,
            connection);
        command.Parameters.Add("@role", MySqlDbType.VarChar, Rules.RoleMaxLength).Value = UserRole.Owner.ToString();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public User? FindActiveOwner(MySqlConnection connection)
    {
        using var command = new MySqlCommand(
            """
            SELECT UserID, FullName, Username, Role, IsActive, MustChangePassword
            FROM `User`
            WHERE Role = @role AND IsActive = true
            LIMIT 1
            """,
            connection);
        command.Parameters.Add("@role", MySqlDbType.VarChar, Rules.RoleMaxLength).Value = UserRole.Owner.ToString();
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return ReadAccount(reader);
    }

    public void Insert(MySqlConnection connection, MySqlTransaction transaction, NewUser user)
    {
        using var command = new MySqlCommand(
            """
            INSERT INTO `User` (FullName, Username, PasswordHash, Role, IsActive, MustChangePassword)
            VALUES (@fullName, @username, @passwordHash, @role, @isActive, @mustChangePassword)
            """,
            connection,
            transaction);
        AddInsertParameters(command, user);
        command.ExecuteNonQuery();
    }

    private static void AddInsertParameters(MySqlCommand command, NewUser user)
    {
        command.Parameters.Add("@fullName", MySqlDbType.VarChar, Rules.FullNameMaxLength).Value = user.FullName;
        command.Parameters.Add("@username", MySqlDbType.VarChar, Rules.UsernameMaxLength).Value = user.Username;
        command.Parameters.Add("@passwordHash", MySqlDbType.VarChar, Rules.PasswordHashMaxLength).Value = user.PasswordHash;
        command.Parameters.Add("@role", MySqlDbType.VarChar, Rules.RoleMaxLength).Value = user.Role.ToString();
        command.Parameters.Add("@isActive", MySqlDbType.Bool).Value = user.IsActive;
        command.Parameters.Add("@mustChangePassword", MySqlDbType.Bool).Value = user.MustChangePassword;
    }

    private static List<User> ReadAccounts(MySqlDataReader reader)
    {
        var accounts = new List<User>();
        while (reader.Read())
        {
            accounts.Add(ReadAccount(reader));
        }

        return accounts;
    }

    private static User ReadAccount(MySqlDataReader reader)
    {
        return new User
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            Role = Enum.Parse<UserRole>(reader.GetString(reader.GetOrdinal("Role"))),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            MustChangePassword = reader.GetBoolean(reader.GetOrdinal("MustChangePassword"))
        };
    }

    private static StoredUser ReadUser(MySqlDataReader reader)
    {
        return new StoredUser
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            Role = Enum.Parse<UserRole>(reader.GetString(reader.GetOrdinal("Role"))),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            MustChangePassword = reader.GetBoolean(reader.GetOrdinal("MustChangePassword"))
        };
    }
}
