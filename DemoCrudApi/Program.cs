using Microsoft.EntityFrameworkCore;
using DemoCrudApi.Data;
using DemoCrudApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// Add response caching
builder.Services.AddResponseCaching();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add response caching
app.UseResponseCaching();

// Global exception handler
app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext context) =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    return Results.Problem(title: "An error occurred", detail: exception?.Message, statusCode: 500);
});

// Health check endpoint
app.MapHealthChecks("/health");

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// CRUD Endpoints for Product

// GET /products - Get all products with optional pagination
app.MapGet("/products", async (int? page, int? size, AppDbContext db, HttpContext context) =>
{
    context.Response.Headers.CacheControl = "public,max-age=60";
    var query = db.Products.AsQueryable();
    if (page.HasValue && size.HasValue)
    {
        var skip = (page.Value - 1) * size.Value;
        query = query.Skip(skip).Take(size.Value);
    }
    return await query.ToListAsync();
})
.WithName("GetProducts")
.WithOpenApi();

// GET /products/{id} - Get product by id
app.MapGet("/products/{id:int}", async (int id, AppDbContext db, HttpContext context) =>
{
    context.Response.Headers.CacheControl = "public,max-age=60";
    return await db.Products.FindAsync(id) is Product product
        ? Results.Ok(product)
        : Results.NotFound();
})
.WithName("GetProduct")
.WithOpenApi();

// POST /products - Create a new product
app.MapPost("/products", async (Product product, AppDbContext db, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(product.Name) || product.Name.Length > 100)
    {
        logger.LogWarning("Invalid product name: {Name}", product.Name);
        return Results.Problem("Name is required and must be 100 characters or less.", statusCode: 400);
    }

    if (product.Price <= 0)
    {
        logger.LogWarning("Invalid product price: {Price}", product.Price);
        return Results.Problem("Price must be greater than 0.", statusCode: 400);
    }

    db.Products.Add(product);
    await db.SaveChangesAsync();
    logger.LogInformation("Product created: {Id}", product.Id);
    return Results.Created($"/products/{product.Id}", product);
})
.WithName("CreateProduct")
.WithOpenApi();

// PUT /products/{id} - Update a product
app.MapPut("/products/{id:int}", async (int id, Product inputProduct, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(inputProduct.Name) || inputProduct.Name.Length > 100)
        return Results.Problem("Name is required and must be 100 characters or less.", statusCode: 400);

    if (inputProduct.Price <= 0)
        return Results.Problem("Price must be greater than 0.", statusCode: 400);

    product.Name = inputProduct.Name;
    product.Price = inputProduct.Price;
    // CreatedAt not updated

    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateProduct")
.WithOpenApi();

// DELETE /products/{id} - Delete a product
app.MapDelete("/products/{id:int}", async (int id, AppDbContext db, ILogger<Program> logger) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        logger.LogWarning("Product not found: {Id}", id);
        return Results.NotFound();
    }

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    logger.LogInformation("Product deleted: {Id}", id);
    return Results.NoContent();
})
.WithName("DeleteProduct")
.WithOpenApi();

app.Run();