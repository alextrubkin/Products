using Domain;
using Moq;
using Products.Api.Persistence;
using Products.Api.Services.Implementation;

namespace Products.UnitTests.Services.Implementation;

public class ProductsServiceTests
{
    private readonly Mock<IProductsRepository> _mockRepo;
    private readonly ProductsService _productsService;

    public ProductsServiceTests()
    {
        _mockRepo = new Mock<IProductsRepository>();
        _productsService = new ProductsService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsProducts()
    {
        // Arrange
        var page = 1;
        var size = 10;
        var colour = "Red";
        var mockProducts = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Colour = Colour.Black, Description = "Description 1", Price = 100 },
            new() { Id = 2, Name = "Product 2", Colour = Colour.Blue, Description = "Description 2", Price = 200 }
        };
        _mockRepo.Setup(repo => repo.GetProductsDataAsync(page, size, colour)).ReturnsAsync(mockProducts);

        // Act
        var result = await _productsService.GetProductsAsync(page, size, colour);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mockProducts.Count, result.Count());
        Assert.All(result, p =>
        {
            var mockProduct = mockProducts.Single(mp => mp.Id == p.Id);
            Assert.Equal(mockProduct.Name, p.Name);
            Assert.Equal(mockProduct.Colour, p.Colour);
            Assert.Equal(mockProduct.Description, p.Description);
            Assert.Equal(mockProduct.Price, p.Price);
        });
    }
}