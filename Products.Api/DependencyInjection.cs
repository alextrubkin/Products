using System.Security.Claims;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Products.Api.Models;
using Products.Api.Models.Validators;
using Products.Api.Persistence;
using Products.Api.Persistence.Implementation;
using Products.Api.Services;
using Products.Api.Services.Implementation;

namespace Products.Api;

internal static class DependencyInjection
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IProductsService, ProductsService>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<IValidator<GetProductsRequest>, GetProductsRequestValidator>();
    }

    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.UseInlineDefinitionsForEnums();

            // Add Bearer support
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description =
                    "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new[] { "" }
                }
            });
        });
    }

    public static void ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("Products.Read", policy =>
                policy
                    .RequireClaim(ClaimTypes.Role, "reader")
                    .RequireClaim("scope", "products_api"));
        services.AddAuthentication()
            .AddJwtBearer();
    }
}