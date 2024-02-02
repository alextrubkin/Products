using Domain;
using FluentValidation;

namespace Products.Api.Models.Validators;

internal class GetProductsRequestValidator : AbstractValidator<GetProductsRequest>
{
    public GetProductsRequestValidator()
    {
        RuleFor(x => x.Colour).IsEnumName(typeof(Colour), false)
            .When(x => !string.IsNullOrEmpty(x.Colour));
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Size).InclusiveBetween(1, 50);
    }
}