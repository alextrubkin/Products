using Microsoft.AspNetCore.Mvc;

namespace Products.Api.Models;

internal record GetProductsRequest([FromQuery] string? Colour, [FromQuery] int Page = 1, [FromQuery] int Size = 20);