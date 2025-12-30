using System.Net;
using System.Net.Http.Json;
using DemoCrudApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DemoCrudApi.Tests;

public class ProductApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange: Add some products
        var products = new List<Product>
        {
            new() { Name = "Product1", Price = 10.0m },
            new() { Name = "Product2", Price = 20.0m },
            new() { Name = "Product3", Price = 30.0m },
            new() { Name = "Product4", Price = 40.0m },
            new() { Name = "Product5", Price = 50.0m }
        };

        foreach (var product in products)
        {
            var response = await _client.PostAsJsonAsync("/products", product);
            response.EnsureSuccessStatusCode();
        }

        // Act: Get page 1 with size 2
        var getResponse = await _client.GetAsync("/products?page=1&size=2");
        getResponse.EnsureSuccessStatusCode();

        var result = await getResponse.Content.ReadFromJsonAsync<List<Product>>();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Product1");
        result[1].Name.Should().Be("Product2");
    }

    [Fact]
    public async Task PostProduct_WithInvalidData_ReturnsBadRequestWithProblemDetails()
    {
        // Arrange
        var invalidProduct = new { Name = "", Price = -10.0m };

        // Act
        var response = await _client.PostAsJsonAsync("/products", invalidProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should().Be(400);
        problemDetails.Detail.Should().Contain("Name is required");
    }

    [Fact]
    public async Task PutProduct_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var updateProduct = new Product { Name = "Updated", Price = 100.0m };

        // Act
        var response = await _client.PutAsJsonAsync("/products/999", updateProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}