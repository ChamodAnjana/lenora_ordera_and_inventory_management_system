using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Services;

internal sealed class UserService
{
    private readonly UserRepository _users;

    public UserService(UserRepository users)
    {
        _users = users;
    }

    public Result CreateAccount(AccountRequest request)
    {
        try
        {
            return CreateOwnerAccount(request);
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

    private Result CreateOwnerAccount(AccountRequest request)
    {
        Authorizer.RequireOwner();
        var validation = AccountValidator.Validate(request);
        if (!validation.Success)
        {
            return validation;
        }

        if (request.Role is not UserRole role || !Enum.IsDefined(role))
        {
            return Result.Fail(Messages.RoleRequired);
        }

        return InsertAccount(request, role);
    }

    private Result InsertAccount(AccountRequest request, UserRole role)
    {
        using var connection = Db.OpenConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            _users.Insert(connection, transaction, AccountFactory.Create(request, role));
            transaction.Commit();
            return Result.Ok(Messages.AccountSaved);
        }
        catch (MySqlException exception) when (exception.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            return Result.Fail(Messages.UsernameTaken);
        }
    }

    public Result ListAccounts(List<User> accounts)
    {
        try
        {
            return FillAccounts(accounts);
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

    public Result SetActive(int userId, bool active)
    {
        try
        {
            return UpdateActive(userId, active);
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

    public Result ResetPassword(int userId, string password, string confirmPassword)
    {
        try
        {
            return SavePassword(userId, password, confirmPassword);
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

    private Result FillAccounts(List<User> accounts)
    {
        Authorizer.RequireOwner();
        using var connection = Db.OpenConnection();
        accounts.AddRange(_users.List(connection));
        return Result.Ok(string.Empty);
    }

    private Result UpdateActive(int userId, bool active)
    {
        Authorizer.RequireOwner();
        if (BlocksOwnDisable(userId, active))
        {
            return Result.Fail(Messages.CannotDisableSelf);
        }

        if (!active && IsLastActiveOwner(userId))
        {
            return Result.Fail(Messages.CannotDisableLastOwner);
        }

        using var connection = Db.OpenConnection();
        if (_users.SetActive(connection, userId, active) == 0)
        {
            return Result.Fail(Messages.AccountNotFound);
        }

        return Result.Ok(active ? Messages.AccountEnabled : Messages.AccountDisabled);
    }

    private bool IsLastActiveOwner(int userId)
    {
        using var connection = Db.OpenConnection();
        var activeOwnerCount = _users.FindActiveOwnerCount(connection);
        if (activeOwnerCount > 1)
        {
            return false;
        }

        var lastOwner = _users.FindActiveOwner(connection);
        return lastOwner?.UserId == userId;
    }

    private static bool BlocksOwnDisable(int userId, bool active)
    {
        return !active && CurrentSession.Current?.UserId == userId;
    }

    private Result SavePassword(int userId, string password, string confirmPassword)
    {
        Authorizer.RequireOwner();
        var validation = AccountValidator.ValidatePassword(password, confirmPassword);
        if (!validation.Success)
        {
            return validation;
        }

        using var connection = Db.OpenConnection();
        var hash = PasswordHasher.Hash(password);
        if (_users.UpdatePassword(connection, userId, hash) == 0)
        {
            return Result.Fail(Messages.AccountNotFound);
        }

        _users.UpdateMustChangePassword(connection, userId, true);
        return Result.Ok(Messages.PasswordReset);
    }

    public Result UpdateAccount(int userId, string fullName, string username)
    {
        try
        {
            return UpdateAccountInternal(userId, fullName, username);
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

    private Result UpdateAccountInternal(int userId, string fullName, string username)
    {
        Authorizer.RequireOwner();

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > Rules.FullNameMaxLength)
        {
            return Result.Fail(Messages.FullNameInvalid);
        }

        if (string.IsNullOrWhiteSpace(username) || username.Length > Rules.UsernameMaxLength)
        {
            return Result.Fail(Messages.UsernameInvalid);
        }

        using var connection = Db.OpenConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var command = new MySqlCommand(
                """
                UPDATE `User`
                SET FullName = @fullName, Username = @username
                WHERE UserID = @userId
                """,
                connection,
                transaction);
            command.Parameters.Add("@fullName", MySqlDbType.VarChar, Rules.FullNameMaxLength).Value = fullName;
            command.Parameters.Add("@username", MySqlDbType.VarChar, Rules.UsernameMaxLength).Value = username;
            command.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;

            if (command.ExecuteNonQuery() == 0)
            {
                return Result.Fail(Messages.AccountNotFound);
            }

            transaction.Commit();
            return Result.Ok(Messages.AccountSaved);
        }
        catch (MySqlException exception) when (exception.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            return Result.Fail(Messages.UsernameTaken);
        }
    }

    public Result SetMustChangePassword(int userId, bool mustChange)
    {
        try
        {
            return SetMustChangePasswordInternal(userId, mustChange);
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

    private Result SetMustChangePasswordInternal(int userId, bool mustChange)
    {
        Authorizer.RequireOwner();
        using var connection = Db.OpenConnection();
        if (_users.UpdateMustChangePassword(connection, userId, mustChange) == 0)
        {
            return Result.Fail(Messages.AccountNotFound);
        }

        return Result.Ok(mustChange ? Messages.PasswordChangeRequired : Messages.PasswordChangeCleared);
    }
}
