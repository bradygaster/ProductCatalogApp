# Product Catalog Application

A modern e-commerce product catalog application built with .NET 10, featuring a RESTful Web API backend and an ASP.NET Core MVC frontend.

## Overview

This application consists of two main components:
- **ProductServiceLibrary**: ASP.NET Core Web API providing product catalog services
- **ProductCatalog**: ASP.NET Core MVC web application for browsing and purchasing products

## Features

- Browse product catalog with filtering and search
- Shopping cart functionality
- Order submission with file-based queue system
- RESTful API with Swagger documentation
- Responsive design with Bootstrap
- Cross-platform compatibility (Windows, Linux, macOS)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A modern web browser

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/bradygaster/ProductCatalogApp.git
cd ProductCatalogApp
```

### 2. Build the Solution

```bash
dotnet build
```

### 3. Run the Applications

You'll need to run both applications in separate terminal windows:

#### Terminal 1 - Web API
```bash
cd ProductServiceLibrary
dotnet run
```

The API will start at: `https://localhost:5001`
Swagger UI available at: `https://localhost:5001/swagger`

#### Terminal 2 - Web Application
```bash
cd ProductCatalog
dotnet run
```

The web app will start at: `https://localhost:5002`

### 4. Browse the Application

Open your browser and navigate to `https://localhost:5002` to start shopping!

## API Documentation

Once the Web API is running, you can explore the API endpoints using Swagger UI at `https://localhost:5001/swagger`.

### Available Endpoints

- **Products**
  - `GET /api/products` - Get all products
  - `GET /api/products/{id}` - Get specific product
  - `GET /api/products/category/{category}` - Get products by category
  - `GET /api/products/search?searchTerm={term}` - Search products
  - `POST /api/products` - Create new product
  - `PUT /api/products/{id}` - Update product
  - `DELETE /api/products/{id}` - Delete product

- **Categories**
  - `GET /api/categories` - Get all categories

## Configuration

### ProductServiceLibrary (Web API)

Edit `ProductServiceLibrary/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### ProductCatalog (MVC Web App)

Edit `ProductCatalog/appsettings.json`:

```json
{
  "ProductApiUrl": "https://localhost:5001",
  "OrderQueuePath": ""
}
```

- `ProductApiUrl`: URL where the Web API is running
- `OrderQueuePath`: Directory for order queue files (leave empty for system temp directory)

## Project Structure

```
ProductCatalogApp/
├── ProductServiceLibrary/          # Web API
│   ├── Controllers/                # API Controllers
│   ├── Models/                     # Data models (Product, Category)
│   ├── Program.cs                  # Application entry point
│   └── appsettings.json           # Configuration
│
├── ProductCatalog/                 # MVC Web Application
│   ├── Controllers/                # MVC Controllers
│   ├── Models/                     # View models
│   ├── Views/                      # Razor views
│   ├── wwwroot/                    # Static files (CSS, JS, images)
│   ├── Services/                   # Business logic services
│   ├── Program.cs                  # Application entry point
│   └── appsettings.json           # Configuration
│
└── MIGRATION.md                    # Migration guide from .NET Framework
```

## Technology Stack

- **.NET 10**: Latest .NET framework
- **ASP.NET Core**: Web framework
- **ASP.NET Core MVC**: Model-View-Controller pattern
- **ASP.NET Core Web API**: RESTful API
- **Bootstrap 5**: Frontend framework
- **jQuery**: JavaScript library
- **Swashbuckle**: Swagger/OpenAPI documentation
- **Newtonsoft.Json**: JSON serialization

## Development

### Running Tests

```bash
dotnet test
```

### Building for Production

```bash
dotnet publish -c Release
```

## Shopping Flow

1. **Browse Products**: View all available products on the home page
2. **Add to Cart**: Click "Add to Cart" on any product
3. **View Cart**: Click the cart icon in the navigation to see your cart
4. **Adjust Quantities**: Increase/decrease quantities or remove items
5. **Submit Order**: Click "Proceed to Checkout" to place your order
6. **Confirmation**: View your order confirmation

## Order Processing

Orders are stored in a file-based queue system as JSON files. Each order includes:
- Order ID (GUID)
- Customer session ID
- Order items with quantities and prices
- Calculated subtotal, tax, and shipping
- Order timestamp

## Migration

This application was migrated from .NET Framework 4.8.1 to .NET 10. See [MIGRATION.md](MIGRATION.md) for detailed migration information.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License.

## Support

For issues and questions:
- Open an issue on GitHub
- Check the [MIGRATION.md](MIGRATION.md) for migration-specific questions

## Acknowledgments

- Built with .NET 10
- Uses Bootstrap for responsive design
- Swagger UI for API documentation
