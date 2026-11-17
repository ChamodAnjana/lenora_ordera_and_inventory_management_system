using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;

namespace lenora_ordera_and_inventory_management_system.Services;

internal static class AccountValidator
{
    public static Result Validate(AccountRequest request)
    {
        var name = ValidateFullName(request.FullName);
        if (!name.Success)
        {
            return name;
        }

        var username = ValidateUsername(request.Username);
        if (!username.Success)
        {
            return username;
        }

        return ValidatePassword(request.Password, request.ConfirmPassword);
    }

    private static Result ValidateFullName(string fullName)
    {
        var name = fullName.Trim();
        if (name.Length == 0)
        {
            return Result.Fail(Messages.FullNameRequired);
        }

        if (name.Length > Rules.FullNameMaxLength)
        {
            return Result.Fail(Messages.FullNameTooLong(Rules.FullNameMaxLength));
        }

        return Result.Ok(string.Empty);
    }

    private static Result ValidateUsername(string username)
    {
        var name = username.Trim();
        if (name.Length == 0)
        {
            return Result.Fail(Messages.UsernameRequired);
        }

        if (name.Length > Rules.UsernameMaxLength)
        {
            return Result.Fail(Messages.UsernameTooLong(Rules.UsernameMaxLength));
        }

        return Result.Ok(string.Empty);
    }

    public static Result ValidatePassword(string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Result.Fail(Messages.PasswordRequired);
        }

        if (password.Length < Rules.PasswordMinLength)
        {
            return Result.Fail(Messages.PasswordTooShort(Rules.PasswordMinLength));
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            return Result.Fail(Messages.PasswordsDoNotMatch);
        }

        return Result.Ok(string.Empty);
    }
}
