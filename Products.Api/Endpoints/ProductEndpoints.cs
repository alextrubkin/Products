using FluentValidation;
using Products.Api.Models;
using Products.Api.Services;

namespace Products.Api.Endpoints;

internal static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/products",
                async ([AsParameters] GetProductsRequest request, IValidator<GetProductsRequest> validator,
                    IProductsService productService) =>
                {
                    var validationResult = await validator.ValidateAsync(request);
                    if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());

                    return Results.Ok(await productService.GetProductsAsync(request.Page, request.Size,
                        request.Colour ?? string.Empty));
                })
            .WithTags("Products")
            .WithName("GetAllProducts")
            .ProducesValidationProblem()
            .Produces<List<ProductDTO>>()
            .RequireAuthorization("Products.Read");

        return group;
    }
}