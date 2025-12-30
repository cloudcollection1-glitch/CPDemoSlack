# DemoCrudApi

A minimal API project built with .NET 8 that provides full CRUD operations for a Product entity using Entity Framework Core with SQLite.

## Features

- **Minimal API**: Uses .NET 8 minimal API features with request delegate mapping.
- **Entity Framework Core**: Integrated with SQLite for data persistence.
- **CRUD Operations**: Complete Create, Read, Update, Delete operations for Product entities.
- **Validation**: Input validation with proper HTTP status codes and ProblemDetails responses.
- **Pagination**: GET endpoint supports optional pagination.
- **OpenAPI/Swagger**: Automatic API documentation generation.
- **Auto Migration**: Database creation and migrations applied on startup.

## Product Entity

```csharp
public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; } // Max 100 characters
    public required decimal Price { get; set; } // Must be > 0
    public DateTime CreatedAt { get; set; } // Auto-set to DateTime.Now
}
```

## API Endpoints

### GET /products
Retrieve all products with optional pagination.

**Query Parameters:**
- `page` (optional): Page number (1-based)
- `size` (optional): Page size

**Example:**
```
GET /products?page=1&size=10
```

### GET /products/{id}
Retrieve a specific product by ID.

### POST /products
Create a new product.

**Request Body:**
```json
{
  "name": "Sample Product",
  "price": 29.99
}
```

### PUT /products/{id}
Update an existing product.

**Request Body:**
```json
{
  "name": "Updated Product",
  "price": 39.99
}
```

### DELETE /products/{id}
Delete a product by ID.

## Running the Application

### Prerequisites
- .NET 8 SDK

### Setup and Run

1. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

2. **Create and apply migrations:**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

Swagger UI will be available at `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`).

## Sample Requests

### Create a Product (POST)
```bash
curl -X POST "https://localhost:5001/products" \
     -H "Content-Type: application/json" \
     -d '{"name": "Laptop", "price": 999.99}'
```

### Get All Products (GET)
```bash
curl -X GET "https://localhost:5001/products"
```

### Get Products with Pagination (GET)
```bash
curl -X GET "https://localhost:5001/products?page=1&size=5"
```

### Get a Specific Product (GET)
```bash
curl -X GET "https://localhost:5001/products/1"
```

### Update a Product (PUT)
```bash
curl -X PUT "https://localhost:5001/products/1" \
     -H "Content-Type: application/json" \
     -d '{"name": "Gaming Laptop", "price": 1299.99}'
```

### Delete a Product (DELETE)
```bash
curl -X DELETE "https://localhost:5001/products/1"
```

### Using HTTPie

```bash
# Create
http POST https://localhost:5001/products name="Laptop" price:=999.99

# Get all
http GET https://localhost:5001/products

# Get with pagination
http GET "https://localhost:5001/products?page=1&size=5"

# Get specific
http GET https://localhost:5001/products/1

# Update
http PUT https://localhost:5001/products/1 name="Gaming Laptop" price:=1299.99

# Delete
http DELETE https://localhost:5001/products/1
```

## Testing

The project includes xUnit tests using Microsoft.AspNetCore.Mvc.Testing.

### Run Tests

```bash
cd ../DemoCrudApi.Tests
dotnet test
```

### Test Coverage

- **GET with Pagination**: Verifies pagination functionality returns correct number of items.
- **POST Validation**: Ensures invalid input returns 400 Bad Request with ProblemDetails.
- **PUT Not Found**: Confirms updating non-existent product returns 404 Not Found.

Tests use an in-memory SQLite database for isolation.

## Project Structure

```
DemoCrudApi/
├── DemoCrudApi.csproj
├── Program.cs
├── Program.Public.cs
├── Models/
│   └── Product.cs
├── Data/
│   └── AppDbContext.cs
├── appsettings.json
├── .editorconfig
├── .gitignore
└── README.md

DemoCrudApi.Tests/
├── DemoCrudApi.Tests.csproj
├── CustomWebApplicationFactory.cs
└── ProductApiTests.cs
```

## Technologies Used

- .NET 8
- ASP.NET Core Minimal API
- Entity Framework Core 8
- SQLite
- Swashbuckle/OpenAPI
- xUnit
- FluentAssertions
- Microsoft.AspNetCore.Mvc.Testing