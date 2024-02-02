using Products.Api.Models;
using Products.Api.Persistence;

namespace Products.Api.Services.Implementation;

internal class ProductsService(IProductsRepository repository) : IProductsService
{

    public async Task<IEnumerable<ProductDTO>> GetProductsAsync(int page, int size, string colour)
    {
        var products = await repository.GetProductsDataAsync(page, size, colour);
        return products.Select(p => new ProductDTO(p.Id, p.Name, p.Description, p.Colour, p.Price));
    }
}