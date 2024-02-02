using Domain;

namespace Products.Api.Persistence;

internal interface IProductsRepository
{
    /// <summary>
    /// Returns a list of products
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="size">Page size</param>
    /// <param name="colour">Product colour</param>
    /// <returns>Collection of product domain model objects</returns>
    Task<IEnumerable<Product>> GetProductsDataAsync(int page, int size, string colour);
}