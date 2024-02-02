using Dapper;
using Domain;

namespace Products.Api.Persistence.Implementation;

internal class ProductsRepository(DapperContext context) : IProductsRepository
{
    public async Task<IEnumerable<Product>> GetProductsDataAsync(int page, int size, string colour)
    {
        var sql = @"SELECT Id, Name, Colour, Description, Price 
            FROM Products
            WHERE (NULLIF(@Colour, '') IS NULL OR Colour = @Colour)
            ORDER BY Id
            OFFSET @Skip ROWS
            FETCH NEXT @Size ROWS ONLY";
        var skip = (page - 1) * size;

        using var _db = context.CreateConnection();
        return await _db.QueryAsync<Product>(sql, new { Size = size, Skip = skip, Colour = colour });
    }
}