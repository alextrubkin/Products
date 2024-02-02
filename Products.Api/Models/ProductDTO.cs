using Domain;

namespace Products.Api.Models;

public record ProductDTO(int Id, string Name, string Description, Colour Colour, decimal Price);