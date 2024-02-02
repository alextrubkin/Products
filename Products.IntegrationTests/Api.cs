using System.Net.Http.Headers;
using System.Security.Claims;
using Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Products.Api.Models;
using Testcontainers.MsSql;

namespace Products.IntegrationTests;

public sealed class ApiTest : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer
        = new MsSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        await using var connection = new SqlConnection(_msSqlContainer.GetConnectionString());
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"CREATE TABLE Products
    (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(50) NOT NULL,
        Description NVARCHAR(MAX),
        Price DECIMAL(18, 2) NOT NULL,
        Colour NVARCHAR(50) NOT NULL
    );
    CREATE INDEX IX_Products_Colour ON Products (Colour);

    DECLARE @i INT = 1;

    WHILE @i <= 10
    BEGIN
        INSERT INTO Products (Name, Description, Price, Colour)
	    VALUES ('iPhone 13', 'Apple iPhone 13 with 128GB storage', 699.00, 'Blue'),
           ('Samsung Galaxy S21', 'Samsung Galaxy S21 with 128GB storage', 799.99, 'Black'),
           ('Google Pixel 6', 'Google Pixel 6 with 128GB storage', 599.00, 'Green'),
           ('OnePlus 9', 'OnePlus 9 with 128GB storage', 729.00, 'Silver'),
           ('Motorola Edge 20', 'Motorola Edge 20 with 128GB storage', 499.99, 'Grey'),
           ('Nokia 8.3', 'Nokia 8.3 with 128GB storage', 479.00, 'Blue'),
           ('Sony Xperia 5 III', 'Sony Xperia 5 III with 128GB storage', 949.99, 'Black'),
           ('LG Velvet', 'LG Velvet with 128GB storage', 599.99, 'White'),
           ('Huawei P40 Pro', 'Huawei P40 Pro with 256GB storage', 899.99, 'Silver'),
           ('Xiaomi Mi 11', 'Xiaomi Mi 11 with 128GB storage', 749.00, 'Blue');

        SET @i = @i + 1;
    END";

        await command.ExecuteScalarAsync();
    }

    public Task DisposeAsync()
    {
        return _msSqlContainer.DisposeAsync().AsTask();
    }

    public sealed class ProductsApi : IClassFixture<ApiTest>, IDisposable
    {
        private const string Path = "api/products";
        private const int DefaultPageSize = 20;

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        private readonly IServiceScope _serviceScope;
        private readonly WebApplicationFactory<Program> _webApplicationFactory;

        public ProductsApi(ApiTest apiTest)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__ProductsDb",
                apiTest._msSqlContainer.GetConnectionString());
            _webApplicationFactory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
                {
                    services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                    {
                        var config = new OpenIdConnectConfiguration
                        {
                            Issuer = MockJwtTokens.Issuer
                        };
                        config.SigningKeys.Add(MockJwtTokens.SecurityKey);
                        options.Configuration = config;
                    });
                }));

            _serviceScope = _webApplicationFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

            _httpClient = _webApplicationFactory.CreateClient();

            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _serviceScope.Dispose();
            _webApplicationFactory.Dispose();
        }

        [Fact]
        [Trait("Category", nameof(ProductsApi))]
        public async Task Get_ProductsWithoutToken_ReturnsUnauthorised()
        {
            // When
            var response = await _httpClient.GetAsync(Path);

            // Then
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        [Trait("Category", nameof(ProductsApi))]
        public async Task Get_ProductsWithoutRequiredClaims_ReturnsForbidden()
        {
            // Given
            var token = MockJwtTokens.GenerateJwtToken(new[]
            {
                new Claim("scope", "products_api")
            });
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // When
            var response = await _httpClient.GetAsync(Path);

            // Then
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        [Trait("Category", nameof(ProductsApi))]
        public async Task Get_Products_ReturnsResult()
        {
            // Given
            const string path = "api/products";
            var token = MockJwtTokens.GenerateJwtToken(new[]
            {
                new Claim(ClaimTypes.Role, "reader"),
                new Claim("scope", "products_api")
            });
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // When
            var response = await _httpClient.GetAsync(path);
            var responseString = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<IEnumerable<ProductDTO>>(responseString, _options);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(products);
            Assert.Equal(DefaultPageSize, products.Count());
        }

        [Fact]
        [Trait("Category", nameof(ProductsApi))]
        public async Task Get_ProductsFilteredByColor_ReturnsResultWithDefaultPagination()
        {
            // Given
            const string path = "api/products?colour=white";
            var token = MockJwtTokens.GenerateJwtToken(new[]
            {
                new Claim(ClaimTypes.Role, "reader"),
                new Claim("scope", "products_api")
            });
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // When
            var response = await _httpClient.GetAsync(path);
            var responseString = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<IEnumerable<ProductDTO>>(responseString, _options);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(products);
            Assert.Equal(10, products.Count());
            Assert.All(products, product => Assert.True(product.Colour == Colour.White));
        }

        [Fact]
        [Trait("Category", nameof(ProductsApi))]
        public async Task Get_ProductsPaginated_ReturnsResult()
        {
            // Given
            const string path = "api/products?page=2&size=10";
            var token = MockJwtTokens.GenerateJwtToken(new[]
            {
                new Claim(ClaimTypes.Role, "reader"),
                new Claim("scope", "products_api")
            });
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // When
            var response = await _httpClient.GetAsync(path);
            var responseString = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<IEnumerable<ProductDTO>>(responseString, _options);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(products);
            Assert.Equal(10, products.Count());
            Assert.All(products, product => Assert.InRange(product.Id, 11, 21));
        }

        [Fact]
        [Trait("Category", nameof(ProductsApi))]
        public async Task Get_ProductsInvalidParams_ReturnsBadRequest()
        {
            // Given
            const string path = "api/products?page=0&size=0&colour=invalid";
            var token = MockJwtTokens.GenerateJwtToken(new[]
            {
                new Claim(ClaimTypes.Role, "reader"),
                new Claim("scope", "products_api")
            });
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // When
            var response = await _httpClient.GetAsync(path);
            var responseString = await response.Content.ReadAsStringAsync();
            var validationProblemDetails =
                JsonSerializer.Deserialize<HttpValidationProblemDetails>(responseString, _options);

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(validationProblemDetails);
            Assert.Equal(3, validationProblemDetails.Errors.Count);
            Assert.Equal("'Colour' has a range of values which does not include 'invalid'.",
                validationProblemDetails.Errors["Colour"][0]);
            Assert.Equal("'Page' must be greater than '0'.", validationProblemDetails.Errors["Page"][0]);
            Assert.Equal("'Size' must be between 1 and 50. You entered 0.", validationProblemDetails.Errors["Size"][0]);
        }
    }

    public sealed class Healthcheck : IClassFixture<ApiTest>
    {
        private readonly string _connectionString;

        public Healthcheck(ApiTest apiTest)
        {
            _connectionString = apiTest._msSqlContainer.GetConnectionString();
        }

        [Fact]
        [Trait("Category", nameof(ProductsApi))]
        public async Task Get_HealthCheckWithDb_ReturnsHealthy()
        {
            // Given
            const string path = "/";
            Environment.SetEnvironmentVariable("ConnectionStrings__ProductsDb",
                _connectionString);
            await using var webApplicationFactory = new WebApplicationFactory<Program>();
            using var httpClient = webApplicationFactory.CreateClient();

            // When
            var response = await httpClient.GetAsync(path);
            var responseString = await response.Content.ReadAsStringAsync();

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Healthy", responseString);
        }

        [Fact]
        [Trait("Category", nameof(ProductsApi))]
        public async Task Get_HealthCheckWithoutDb_ReturnsUnhealthy()
        {
            // Given
            const string path = "/";
            Environment.SetEnvironmentVariable("ConnectionStrings__ProductsDb",
                "Server=127.0.0.1,32893;Database=master;User Id=sa;Password=InvalidPassword;TrustServerCertificate=True");
            await using var webApplicationFactory = new WebApplicationFactory<Program>();
            using var httpClient = webApplicationFactory.CreateClient();

            // When
            var response = await httpClient.GetAsync(path);
            var responseString = await response.Content.ReadAsStringAsync();

            // Then
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("Unhealthy", responseString);
        }
    }
}