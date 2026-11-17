using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Services;

internal sealed class AuthService
{
    private readonly UserRepository _users;

    public AuthService(UserRepository users)
    {
        _users = users;
    }

    public Result CheckDatabase()
    {
        try
        {
            using var connection = Db.OpenConnection();
            return Result.Ok(string.Empty);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public bool DatabaseIsEmpty()
    {
        try
        {
            using var connection = Db.OpenConnection();
            using var transaction = connection.BeginTransaction();
            return _users.Count(connection, transaction) == 0;
        }
        catch (MySqlException)
        {
            return false;
        }
    }

    public Result RegisterFirstOwner(AccountRequest request)
    {
        var validation = AccountValidator.Validate(request);
        if (!validation.Success)
        {
            return validation;
        }

        try
        {
            return InsertFirstOwner(request);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result Login(string username, string password)
    {
        var validation = ValidateCredentials(username, password);
        if (!validation.Success)
        {
            return validation;
        }

        try
        {
            return SignIn(username.Trim(), password);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result Logout()
    {
        if (CurrentSession.Current is null)
        {
            return Result.Fail(Messages.NotSignedIn);
        }

        CurrentSession.SignOut();
        return Result.Ok(Messages.LogoutSucceeded);
    }

    public Result ChangePassword(int userId, string newPassword, string confirmPassword)
    {
        var validation = AccountValidator.ValidatePassword(newPassword, confirmPassword);
        if (!validation.Success)
        {
            return validation;
        }

        try
        {
            return ChangePasswordInternal(userId, newPassword);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    private Result ChangePasswordInternal(int userId, string newPassword)
    {
        using var connection = Db.OpenConnection();
        var hash = PasswordHasher.Hash(newPassword);
        if (_users.UpdatePassword(connection, userId, hash) == 0)
        {
            return Result.Fail(Messages.AccountNotFound);
        }

        var current = CurrentSession.Current;
        if (current is not null && current.UserId == userId)
        {
            CurrentSession.SignIn(new User
            {
                UserId = current.UserId,
                FullName = current.FullName,
                Username = current.Username,
                Role = current.Role,
                IsActive = current.IsActive,
                MustChangePassword = false
            });
        }

        return Result.Ok(Messages.PasswordReset);
    }

    public User? CurrentUser()
    {
        return CurrentSession.Current;
    }

    private Result InsertFirstOwner(AccountRequest request)
    {
        using var connection = Db.OpenConnection();
        using var transaction = connection.BeginTransaction();
        if (_users.Count(connection, transaction) > 0)
        {
            return Result.Fail(Messages.OwnerAlreadyExists);
        }

        return SaveOwner(connection, transaction, request);
    }

    private Result SaveOwner(MySqlConnection connection, MySqlTransaction transaction, AccountRequest request)
    {
        try
        {
            _users.Insert(connection, transaction, AccountFactory.Create(request, UserRole.Owner));
            transaction.Commit();
            return Result.Ok(Messages.AccountCreated);
        }
        catch (MySqlException exception) when (exception.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            return Result.Fail(Messages.UsernameTaken);
        }
    }

    private static Result ValidateCredentials(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Result.Fail(Messages.UsernameRequired);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return Result.Fail(Messages.PasswordRequired);
        }

        if (username.Trim().Length > Rules.UsernameMaxLength)
        {
            return Result.Fail(Messages.LoginFailed);
        }

        return Result.Ok(string.Empty);
    }

    private Result SignIn(string username, string password)
    {
        using var connection = Db.OpenConnection();
        var stored = _users.FindByUsername(connection, username);
        if (stored is null || !stored.IsActive || !PasswordHasher.Verify(password, stored.PasswordHash))
        {
            return Result.Fail(Messages.LoginFailed);
        }

        CurrentSession.SignIn(ToSessionUser(stored));
        return Result.Ok(string.Empty);
    }

    private static User ToSessionUser(StoredUser stored)
    {
        return new User
        {
            UserId = stored.UserId,
            FullName = stored.FullName,
            Username = stored.Username,
            Role = stored.Role,
            IsActive = stored.IsActive,
            MustChangePassword = stored.MustChangePassword
        };
    }
}
