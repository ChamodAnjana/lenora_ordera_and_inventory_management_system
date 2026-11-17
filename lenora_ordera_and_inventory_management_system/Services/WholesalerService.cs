using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Services;

internal sealed class WholesalerService
{
    private readonly WholesalerRepository _wholesalers;

    public WholesalerService(WholesalerRepository wholesalers)
    {
        _wholesalers = wholesalers;
    }

    public Result ListWholesalers(List<Wholesaler> wholesalers, string? search = null)
    {
        try
        {
            Authorizer.RequireOwner();
            using var connection = Db.OpenConnection();
            wholesalers.AddRange(_wholesalers.List(connection, search));
            return Result.Ok(string.Empty);
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

    public Result GetWholesaler(int wholesalerId, out Wholesaler? wholesaler)
    {
        wholesaler = null;
        try
        {
            Authorizer.RequireOwner();
            using var connection = Db.OpenConnection();
            wholesaler = _wholesalers.FindById(connection, wholesalerId);
            return wholesaler is null ? Result.Fail(Messages.WholesalerNotFound) : Result.Ok(string.Empty);
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

    public Result AddWholesaler(WholesalerRequest request)
    {
        try
        {
            Authorizer.RequireOwner();
            var normalisedRequest = Normalise(request);
            var validation = Validate(normalisedRequest);
            if (!validation.Success)
            {
                return validation;
            }

            using var connection = Db.OpenConnection();
            _wholesalers.Insert(connection, normalisedRequest);
            return Result.Ok(Messages.WholesalerSaved);
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

    public Result UpdateWholesaler(int wholesalerId, WholesalerRequest request)
    {
        try
        {
            Authorizer.RequireOwner();
            var normalisedRequest = Normalise(request);
            var validation = Validate(normalisedRequest);
            if (!validation.Success)
            {
                return validation;
            }

            using var connection = Db.OpenConnection();
            var rows = _wholesalers.Update(connection, wholesalerId, normalisedRequest);
            return rows > 0 ? Result.Ok(Messages.WholesalerSaved) : Result.Fail(Messages.WholesalerNotFound);
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

    public Result DeleteWholesaler(int wholesalerId)
    {
        try
        {
            Authorizer.RequireOwner();
            using var connection = Db.OpenConnection();
            if (_wholesalers.IsWholesalerUsed(connection, wholesalerId))
            {
                return Result.Fail(Messages.WholesalerCannotBeDeleted);
            }

            var rows = _wholesalers.Delete(connection, wholesalerId);
            return rows > 0 ? Result.Ok(Messages.WholesalerDeleted) : Result.Fail(Messages.WholesalerNotFound);
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

    private static WholesalerRequest Normalise(WholesalerRequest request)
    {
        return new WholesalerRequest
        {
            Name = request.Name.Trim(),
            ContactNo = PhoneNumber.Normalise(request.ContactNo),
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim()
        };
    }

    private static Result Validate(WholesalerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > Rules.WholesalerNameMaxLength)
        {
            return Result.Fail(Messages.WholesalerNameRequired);
        }

        if (string.IsNullOrWhiteSpace(request.ContactNo) || request.ContactNo.Length > Rules.WholesalerContactNoMaxLength)
        {
            return Result.Fail(Messages.ContactNoRequired);
        }

        if (request.Address?.Length > Rules.WholesalerAddressMaxLength)
        {
            return Result.Fail(Messages.Address);
        }

        return Result.Ok(string.Empty);
    }
}
