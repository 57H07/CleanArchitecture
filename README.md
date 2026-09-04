# Clean Architecture ASP.NET Core MVC Demo

A practical demonstration of Clean Architecture principles in an ASP.NET Core MVC application featuring User and Product management with modern web development practices.

## 🏗️ Architecture

This template follows the Clean Architecture pattern with the following layers:

### 📦 Projects Structure

```
CleanArchitecture/
├── CleanArchitecture.Domain/          # Core business entities and exceptions
│   ├── Common/                        # Base entity class
│   ├── Entities/                      # User and Product entities
│   ├── Enums/                         # UserRole, ProductStatus
│   └── Exceptions/                    # Domain exception hierarchy
├── CleanArchitecture.Application/     # Application services and contracts
│   ├── Collections/                   # PagedResult<T> and mapping extensions
│   ├── DTOs/                         # Data Transfer Objects with validation
│   ├── DependencyInjection/          # AddApplication() registration
│   ├── Enums/                        # Sorting enums
│   ├── Exceptions/                   # Application-level exceptions
│   ├── Interfaces/                   # Repository, service and IPaginatedList contracts
│   ├── Mappings/                     # Mapster IRegister configurations
│   └── Services/                     # Application services (User & Product)
├── CleanArchitecture.Infrastructure/  # Data access and external concerns
│   ├── Collections/                   # EF Core PaginatedList<T>
│   ├── Data/                         # DbContext and entity configurations
│   │   └── Configurations/           # Separate EF entity configurations + seed data
│   ├── DependencyInjection/          # AddInfrastructure() registration
│   └── Repositories/                 # Repository and Unit of Work implementations
├── CleanArchitecture.Web/            # MVC presentation layer
│   ├── Controllers/                   # Home, Users, Products controllers
│   ├── Middleware/                   # Global exception handling
│   ├── Models/                       # ErrorViewModel, ToastMessage
│   ├── ViewComponents/               # Pagination view component
│   ├── ViewModels/                   # List and paging view models
│   ├── Views/                        # Razor views with Bootstrap UI
│   └── wwwroot/                      # Static assets (CSS, JS, libraries)
└── CleanArchitecture.Application.Tests/  # xUnit tests for application services
```

Dependency direction is strict: Domain ← Application ← Infrastructure ← Web. Application references only Domain.

## 🚀 Technologies Used

- **Framework**: ASP.NET Core 10.0 MVC
- **Database**: Entity Framework Core 10.0 with SQL Server LocalDB
- **Mapping**: Mapster 10.0.12 for object-to-object mapping
- **UI**: Bootstrap 5 with Bootstrap Icons
- **Validation**: Data Annotations with client & server-side validation
- **Testing**: xUnit, Moq, FluentAssertions, AutoFixture
- **Development**: .NET 10.0 with nullable reference types enabled

## ✨ Features

### Domain Layer
- **Base Entity**: Common properties for all entities (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- **User Entity**: Complete user model with validation and relationships
- **Product Entity**: Product model with pricing, inventory, and categorization
- **Domain Exceptions**: Exception hierarchy (`DomainException`, `RessourceNotFoundException`, `InsufficientRightsException`, `ValidationDomaineException`) that the Web layer maps to HTTP status codes

### Application Layer
- **Repository Pattern**: Data access abstraction with interfaces
- **Unit of Work**: Single entry point for repositories, `SaveChangesAsync` and explicit transactions
- **Service Layer**: UserService and ProductService for business logic
- **DTOs**: Validated data transfer objects for Create/Update operations
- **Pagination Contract**: `IPaginatedList<T>` and `PagedResult<T>` so no `IQueryable` leaks above Infrastructure
- **Cancellation**: Every service and repository method accepts and forwards a `CancellationToken`
- **Mapster Integration**: `IRegister` mapping classes discovered by assembly scan

### Infrastructure Layer
- **Entity Framework Core**: Code-first approach with SQL Server
- **Separate Entity Configurations**: Individual configuration files for each entity
- **Repository Implementation**: Concrete implementations of repository interfaces
- **EF Pagination**: `PaginatedList<T>.CreateAsync` materializes one page from an `IQueryable`
- **Database Seeding**: Pre-populated sample data for Users and Products via `HasData`
- **Dependency Injection**: Clean service registration and configuration

### Web Layer
- **MVC Controllers**: Home, Users, and Products controllers with full CRUD
- **Razor Views**: Server-side rendered views with Bootstrap 5 styling
- **Pagination**: Reusable `PaginationViewComponent` driven by `PaginationViewModel`
- **Form Validation**: Client and server-side validation with error display
- **Global Exception Handling**: Middleware maps domain exceptions to 404/403/409/422, returns JSON for AJAX requests and a toast + redirect otherwise (active outside Development only)
- **Responsive UI**: Mobile-friendly interface with Bootstrap components

### Tests
- **CleanArchitecture.Application.Tests**: xUnit tests for the application services, mocking `IUnitOfWork` with Moq and asserting with FluentAssertions

## 🛠️ Getting Started

### Prerequisites
- .NET 10.0 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2026 or Visual Studio Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd CleanArchitecture
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string** (if needed)
   Edit `CleanArchitecture.Web/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CleanArchitectureDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run --project .\CleanArchitecture.Web
   ```

6. **Run the tests**
   ```bash
   dotnet test
   dotnet test --filter "FullyQualifiedName~ProductServiceTests"   # one test class
   ```

### Upgrading the .NET version

`Migrate-NetCore.ps1` updates every project's target framework and NuGet packages to a new .NET major version. Run it from the solution root:

```powershell
.\Migrate-NetCore.ps1 -TargetVersion 11 -WhatIfMode     # preview
.\Migrate-NetCore.ps1 -TargetVersion 11 -AutoUpdatePackages
```

## 📊 Sample Data

The application includes pre-seeded data for demonstration:

### Users
- **John Doe** (john.doe@example.com) - Sample user with products
- **Jane Smith** (jane.smith@example.com) - Sample user with products

### Products
- **Laptop Computer** ($1,299.99) - Electronics category, owned by John Doe
- **Wireless Mouse** ($29.99) - Electronics category, owned by John Doe  
- **Office Chair** ($249.99) - Furniture category, owned by Jane Smith

### Database
- Uses SQL Server LocalDB for development
- No EF migrations: on **every startup** `Program.cs` calls `EnsureDeleted()` then `EnsureCreated()`, so the database is dropped, recreated from the model and reseeded
- Any data entered through the UI is lost on the next run

## 🎯 What This Application Demonstrates

### User Management
- **CRUD Operations**: Create, view, edit, and delete users
- **Form Validation**: Required fields, email format, string length validation
- **Data Relationships**: Users can own multiple products
- **Responsive Interface**: Mobile-friendly user management interface

### Product Management  
- **Inventory System**: Products with stock quantities and availability status
- **Categorization**: Products organized by categories (Electronics, Furniture, etc.)
- **Pricing**: Decimal precision pricing with proper formatting
- **User Assignment**: Products associated with specific users

### Technical Demonstrations
- **Clean Architecture**: Proper separation of concerns across layers
- **Entity Framework**: Code-first approach with separate entity configurations
- **Repository Pattern**: Abstracted data access with interfaces
- **Dependency Injection**: Proper service registration and resolution
- **Exception Handling**: Global middleware with user-friendly error messages
- **Mapster Mapping**: Efficient object-to-object mapping configuration

### UI/UX Features
- **Bootstrap 5**: Modern, responsive design framework
- **Bootstrap Icons**: Consistent iconography throughout the application
- **Form Validation**: Real-time client-side validation with server-side backup
- **Success/Error Messages**: User feedback using TempData and alerts
- **Responsive Tables**: Mobile-friendly data display

## 🔧 Extending the Application

### Adding New Entities

Follow the established pattern demonstrated by User and Product entities:

1. **Create Domain Entity** in `CleanArchitecture.Domain/Entities/`
   - Inherit from `BaseEntity` for common properties
   - Put invariants in domain methods that throw domain exceptions
2. **Add Repository Interface** in `CleanArchitecture.Application/Interfaces/Repositories/`
   - Every method takes a trailing `CancellationToken`
   - Expose it as a property on `IUnitOfWork`
3. **Create DTOs** in `CleanArchitecture.Application/DTOs/`
   - Add validation attributes for data transfer objects
4. **Add Mapping** in `CleanArchitecture.Application/Mappings/`
   - Implement Mapster `IRegister`; it is discovered automatically
5. **Add Service Interface and Implementation** in `CleanArchitecture.Application/`
   - Register the service in `Application/DependencyInjection/ServiceCollectionExtensions.cs` (`AddApplication`)
6. **Create Entity Configuration** in `CleanArchitecture.Infrastructure/Data/Configurations/`
   - Implement `IEntityTypeConfiguration<T>` for EF mapping and `HasData` seed
7. **Implement Repository** in `CleanArchitecture.Infrastructure/Repositories/`
   - Add the property to `UnitOfWork`
   - Register the repository in `Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` (`AddInfrastructure`)
8. **Create Controller** in `CleanArchitecture.Web/Controllers/`
   - MVC controller with CRUD actions
9. **Add Views** in `CleanArchitecture.Web/Views/`
   - Razor views for the user interface
10. **Add Tests** in `CleanArchitecture.Application.Tests/Services/`
    - Mock `IUnitOfWork` and build data with `TestDataBuilder`

## 📚 Architecture Patterns Implemented

- **Clean Architecture**: Dependency inversion with clear layer separation
- **Repository Pattern**: Data access abstraction with interface contracts
- **Unit of Work**: Transaction management across multiple repositories
- **Service Layer**: Business logic encapsulation in application services
- **SOLID Principles**: Single responsibility, dependency inversion, and interface segregation
- **Exception Handling**: Layered exception handling with global middleware
- **Dependency Injection**: Constructor injection with proper service lifetimes
- **Separation of Concerns**: Each layer has distinct responsibilities
- **Configuration Pattern**: Separate entity configurations using EF Core best practices

## 🤝 About This Project

This project serves as a **practical demonstration** of Clean Architecture principles in a real-world ASP.NET Core MVC application. It's designed to:

- **Showcase best practices** in modern .NET development
- **Demonstrate proper layering** and dependency management
- **Provide a working example** of Clean Architecture implementation
- **Serve as a learning resource** for developers studying these patterns
- **Illustrate modern web development** with ASP.NET Core MVC

Feel free to:
- Study the code structure and patterns
- Use it as a reference for your own projects
- Extend functionality to learn more about the architecture
- Adapt the patterns to your specific needs

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

- **Robert C. Martin** - Clean Architecture principles
- **Microsoft** - ASP.NET Core and Entity Framework Core frameworks
- **Mapster Team** - Efficient object mapping library  
- **Bootstrap Team** - UI framework and components
