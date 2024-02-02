using Products.Api;
using Products.Api.Endpoints;
using Products.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();
builder.Services.RegisterServices();
builder.Services.ConfigureAuthorization();

var connectionString = builder.Configuration.GetConnectionString("ProductsDb");
builder.Services.AddSingleton(new DapperContext(connectionString));

builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapHealthChecks("/");

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api")
    .MapProductEndpoints();

app.Run();