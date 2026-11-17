using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using MySqlConnector;

namespace lenora_ordera_and_inventory_management_system.Services;

internal sealed class ProductService
{
    private readonly ProductRepository _products;

    public ProductService(ProductRepository products)
    {
        _products = products;
    }

    public Result ListProductsForOwner(List<Product> products, string? search = null, string? category = null)
    {
        try
        {
            Authorizer.RequireOwner();
            using var connection = Db.OpenConnection();
            products.AddRange(_products.ListForOwner(connection, search, category));
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

    public Result ListProductsForAssistant(List<ProductCatalogItem> products, string? search = null, string? category = null)
    {
        try
        {
            using var connection = Db.OpenConnection();
            products.AddRange(_products.ListForAssistant(connection, search, category));
            return Result.Ok(string.Empty);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    public Result GetProduct(int productId, out Product? product)
    {
        product = null;
        try
        {
            Authorizer.RequireOwner();
            using var connection = Db.OpenConnection();
            product = _products.FindById(connection, productId);
            return product is null ? Result.Fail(Messages.ProductNotFound) : Result.Ok(string.Empty);
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

    public Result AddProduct(ProductRequest request)
    {
        try
        {
            Authorizer.RequireOwner();
            var validation = Validate(request);
            if (!validation.Success)
            {
                return validation;
            }

            using var connection = Db.OpenConnection();
            _products.Insert(connection, request);
            return Result.Ok(Messages.ProductSaved);
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

    public Result UpdateProduct(int productId, ProductRequest request)
    {
        try
        {
            Authorizer.RequireOwner();
            var validation = Validate(request);
            if (!validation.Success)
            {
                return validation;
            }

            using var connection = Db.OpenConnection();
            var rows = _products.Update(connection, productId, request);
            return rows > 0 ? Result.Ok(Messages.ProductSaved) : Result.Fail(Messages.ProductNotFound);
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

    public Result DeleteProduct(int productId)
    {
        try
        {
            Authorizer.RequireOwner();
            using var connection = Db.OpenConnection();
            if (_products.IsProductUsed(connection, productId))
            {
                return Result.Fail(Messages.ProductCannotBeDeleted);
            }

            var rows = _products.Delete(connection, productId);
            return rows > 0 ? Result.Ok(Messages.ProductDeleted) : Result.Fail(Messages.ProductNotFound);
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

    public Result ListCategories(List<string> categories)
    {
        try
        {
            using var connection = Db.OpenConnection();
            categories.AddRange(_products.ListCategories(connection));
            return Result.Ok(string.Empty);
        }
        catch (MySqlException)
        {
            return Result.Fail(Messages.DatabaseFailed);
        }
    }

    private static Result Validate(ProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > Rules.ProductNameMaxLength)
        {
            return Result.Fail(Messages.ProductNameRequired);
        }

        if (string.IsNullOrWhiteSpace(request.Category) || request.Category.Length > Rules.ProductCategoryMaxLength)
        {
            return Result.Fail(Messages.CategoryRequired);
        }

        if (request.CostPrice < 0)
        {
            return Result.Fail(Messages.CostPriceInvalid);
        }

        if (request.SellingPrice <= 0)
        {
            return Result.Fail(Messages.SellingPriceInvalid);
        }

        if (request.Quantity < 0)
        {
            return Result.Fail(Messages.QuantityInvalid);
        }

        return Result.Ok(string.Empty);
    }
}
