using Products.Api.Models;

namespace Products.Api.Services;

internal interface IProductsService
{    
    /// <summary>
    /// Returns a list of products
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="size">Page size</param>
    /// <param name="colour">Product colour</param>
    /// <returns>Collection of product dto objects</returns>
    Task<IEnumerable<ProductDTO>> GetProductsAsync(int page, int size, string colour);
}