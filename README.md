# Clean Architecture ASP.NET Core MVC Demo

A practical demonstration of Clean Architecture principles in an ASP.NET Core MVC application featuring Product and Customer management with modern web development practices. Products demonstrate the pattern with fully server-rendered pages; Customers demonstrate the same CRUD through an AJAX-driven, Bootstrap-modal flow.

## 🏗️ Architecture

This template follows the Clean Architecture pattern with the following layers:

### 📦 Projects Structure

```
CleanArchitecture/
├── CleanArchitecture.Domain/          # Core business entities and exceptions
│   ├── Common/                        # Base entity class
│   ├── Entities/                      # Product and Customer entities
│   ├── Enums/                         # ProductStatus
│   └── Exceptions/                    # Domain exception hierarchy
├── CleanArchitecture.Application/     # Application services and contracts
│   ├── Collections/                   # PagedResult<T> and mapping extensions
│   ├── DTOs/                         # Data Transfer Objects with validation
│   ├── DependencyInjection/          # AddApplication() registration
│   ├── Enums/                        # Sorting enums
│   ├── Exceptions/                   # Application-level exceptions
│   ├── Interfaces/                   # Repository, service and IPaginatedList contracts
│   ├── Mappings/                     # Mapster IRegister configurations
│   └── Services/                     # Application services (Product & Customer)
├── CleanArchitecture.Infrastructure/  # Data access and external concerns
│   ├── Collections/                   # EF Core PaginatedList<T>
│   ├── Data/                         # DbContext, migrations and entity configurations
│   │   ├── Configurations/           # Separate EF entity configurations + seed data
│   │   └── Migrations/               # EF Core migrations — the source of truth for schema
│   ├── DependencyInjection/          # AddInfrastructure() registration
│   └── Repositories/                 # Repository and Unit of Work implementations
├── CleanArchitecture.Web/            # MVC presentation layer
│   ├── Controllers/                   # Home, Products, Customers controllers
│   ├── Extensions/                   # Toast helpers, migration bootstrap, view helpers
│   ├── Middleware/                   # Global exception handling
│   ├── Models/                       # ErrorViewModel, ToastMessage
│   ├── ViewModels/                   # List and paging view models
│   ├── Views/                        # Razor views with Bootstrap UI
│   └── wwwroot/                      # Static assets, including reusable JS helpers
│       └── js/helpers/               # ajax.js (fetch + antiforgery + validation) and toast.js (Bootstrap toasts)
└── CleanArchitecture.Application.Tests/  # xUnit tests for application services
```

Dependency direction is strict: Domain ← Application ← Infrastructure ← Web. Application references only Domain.

Package versions live in `Directory.Packages.props` (central package management) and shared MSBuild settings in `Directory.Build.props`, which turns warnings into errors for every project.

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
- **Product Entity**: Product model with pricing, inventory and categorization. `Status` is the single source of truth for whether a product is on sale; everything else (`IsInStock()`, `IsPublished()`, `IsPurchasable()`) is derived rather than stored
- **Customer Entity**: Customer model (name, email, phone, company, notes, active flag) with `ValidateBusinessRules()` and `Activate`/`Deactivate` domain methods
- **Domain Exceptions**: Exception hierarchy (`DomainException`, `RessourceNotFoundException`, `InsufficientRightsException`, `ValidationDomaineException`) that the Web layer maps to HTTP status codes

### Application Layer
- **Repository Pattern**: Data access abstraction with interfaces
- **Unit of Work**: Single entry point for repositories, `SaveChangesAsync` and explicit transactions
- **Service Layer**: ProductService and CustomerService for business logic
- **DTOs**: Validated data transfer objects for Create/Update operations
- **Pagination Contract**: `IPaginatedList<T>` and `PagedResult<T>` so no `IQueryable` leaks above Infrastructure; the `Page`/`PageSize` clamping shared by every `*FilterDto` lives in a common `PagedFilterDto` base class
- **Cancellation**: Every service and repository method accepts and forwards a `CancellationToken`
- **Mapster Integration**: `IRegister` mapping classes discovered by assembly scan

### Infrastructure Layer
- **Entity Framework Core**: Code-first approach with SQL Server
- **Separate Entity Configurations**: Individual configuration files for each entity
- **Repository Implementation**: Concrete implementations of repository interfaces
- **EF Pagination**: `PaginatedList<T>.CreateAsync` materializes one page from an `IQueryable`
- **EF Migrations**: Schema owned by checked-in migrations; `dotnet ef database update` is the deployment step
- **Database Seeding**: Pre-populated sample data for Products and Customers via `HasData`, with fixed timestamps so the migration snapshot stays deterministic
- **Dependency Injection**: Clean service registration and configuration

### Web Layer
- **MVC Controllers**: Home, Products, and Customers controllers with full CRUD
- **Razor Views**: Server-side rendered views with Bootstrap 5 styling
- **Pagination**: Server-side paging rendered from `ProductsViewModel`/`CustomersViewModel`, which build the sort and page route values
- **Form Validation**: Client and server-side validation with error display
- **AJAX Modal CRUD**: The Customers screen renders its filterable/sortable table and Bootstrap create/edit/delete modals without full-page navigation, using two generic, reusable `wwwroot/js/helpers` modules:
  - `ajax.js` — a small `fetch` wrapper that reads the Razor antiforgery token off any form (or the page) and sends it as the `RequestVerificationToken` header, serializes plain objects or `<form>`s (`FormData`), and distinguishes JSON error payloads from the Development exception page. `applyValidationErrors(form, errors)` maps a ModelState-shaped error dictionary onto `asp-validation-for` spans, mimicking unobtrusive validation without jQuery.
  - `toast.js` — builds/shows Bootstrap toasts for all four semantic variants (`Toast.success/error/warning/info`), and can also drive a toast Razor already rendered from `TempData` via `Toast.showExisting(el)`.
- **Global Exception Handling**: Middleware maps domain exceptions to 404/403/409/422, returns JSON for AJAX requests and a toast + redirect otherwise. Active in **every** environment; in Development it sits inside the developer exception page and rethrows anything it would map to 500, so real bugs still show a stack trace while domain errors behave identically everywhere. Controllers are correspondingly thin — they catch only what becomes a field-level validation error or a retryable conflict
- **Responsive UI**: Mobile-friendly interface with Bootstrap components

### Tests
- **CleanArchitecture.Application.Tests**: xUnit tests for the application services, mocking `IUnitOfWork` with Moq and asserting with FluentAssertions

## 🛠️ Getting Started

### Prerequisites
- .NET 10.0 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2026 or Visual Studio Code
- EF Core tools, for creating or applying migrations: `dotnet tool install --global dotnet-ef`

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
   The default in `CleanArchitecture.Web/appsettings.json` targets SQL Server LocalDB:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CleanArchitectureDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```
   To point Development at a different server (e.g. a local Docker SQL Server container) **without** committing credentials, use [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) instead of editing `appsettings.Development.json`:
   ```bash
   dotnet user-secrets init --project CleanArchitecture.Web
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CleanArchitectureDb;User Id=sa;Password=<your-password>;TrustServerCertificate=true;MultipleActiveResultSets=true" --project CleanArchitecture.Web
   ```
   User secrets are only loaded in the Development environment and are stored outside the repository.

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Create the database**

   The schema comes from EF migrations checked into `CleanArchitecture.Infrastructure/Data/Migrations`. In Development the app applies pending migrations at startup (`Database:MigrateOnStartup` is `true` in `appsettings.Development.json`), so running it is enough. To do it explicitly, or for any other environment:
   ```bash
   dotnet ef database update --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web
   ```
   If you are updating an older checkout whose database was created by the previous `EnsureCreated()` startup code, it has no migration history and the update will fail. Drop it once:
   ```bash
   dotnet ef database drop --force --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web
   ```

6. **Run the application**
   ```bash
   dotnet run --project .\CleanArchitecture.Web
   ```
7. **Run the tests**
   ```bash
   dotnet test
   dotnet test --filter "FullyQualifiedName~CustomerServiceTests"   # one test class
   ```

### Changing the schema

```bash
dotnet ef migrations add <Name> --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web --output-dir Data/Migrations
dotnet ef migrations has-pending-model-changes --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web
```

Seed timestamps are deliberately a fixed constant (`SeedData.CreatedAt`) rather than `DateTime.UtcNow`: `HasData` feeds the migration snapshot, so a moving value would make every `migrations add` emit a spurious diff.

### Upgrading the .NET version

`Migrate-NetCore.ps1` updates every project's target framework and NuGet packages to a new .NET major version. Run it from the solution root:

```powershell
.\Migrate-NetCore.ps1 -TargetVersion 11 -WhatIfMode     # preview
.\Migrate-NetCore.ps1 -TargetVersion 11 -AutoUpdatePackages
```

## 📊 Sample Data

The application includes pre-seeded data for demonstration:

### Products
- **Laptop Computer** ($1,299.99) - Electronics category, owned by Alice Martin
- **Wireless Mouse** ($29.99) - Electronics category, owned by Alice Martin
- **Office Chair** ($249.99) - Furniture category, owned by Bruno Legrand

### Customers
- **Alice Martin** (Northwind Traders) - Active
- **Bruno Legrand** (Contoso Ltd) - Active
- **Chloe Dubois** (Adventure Works) - Inactive, with notes

### Database
- Uses SQL Server LocalDB for development
- Schema is owned by EF migrations; nothing is dropped or recreated on startup, and data entered through the UI persists across runs
- `Database:MigrateOnStartup` is `true` in Development and `false` everywhere else, so deployed instances never race each other to migrate

## 🎯 What This Application Demonstrates

### Product Management
- **Server-Rendered CRUD**: Create, view, edit, and delete products through classic full-page Razor views
- **Form Validation**: Required fields, ranges, and string length validation via Data Annotations
- **Inventory System**: Products with stock quantities and a `Status` lifecycle (Draft → Active → Inactive/Discontinued), editable from the form and filterable from the list
- **Categorization**: Products organized by categories (Electronics, Furniture, etc.)
- **Pricing**: Decimal precision pricing with proper formatting
- **Customer Assignment**: Each product belongs to a customer — a foreign key rendered as a `SelectList` dropdown, eager-loaded with `.Include()`, and filterable from the list
- **Server-Side Filtering & Sorting**: Search, category/status/customer filters, sortable columns and paging, all driven from the query string

### Customer Management
- **AJAX Modal CRUD**: Create, edit and delete customers through a Bootstrap modal without a full-page reload; the table, sorting, and pagination are also refreshed via AJAX
- **Server-Side Filtering & Sorting**: Search by name/email/company, filter by active status, sort by name/company/created date — mirrors the Products list's query-string-driven approach
- **Inline Field Validation**: ModelState and domain validation errors (e.g. duplicate email) are rendered directly on the offending form field, no page reload
- **Reusable JS Helpers**: The `ajax.js` and `toast.js` helpers used here are generic enough to drive the same modal-CRUD pattern for any future entity

### Technical Demonstrations
- **Clean Architecture**: Proper separation of concerns across layers
- **Entity Framework**: Code-first approach with separate entity configurations
- **Repository Pattern**: Abstracted data access with interfaces
- **Dependency Injection**: Proper service registration and resolution
- **Exception Handling**: Global middleware with user-friendly error messages, active in every environment
- **Central Package Management**: One `Directory.Packages.props` for all versions; warnings are build errors
- **Mapster Mapping**: Efficient object-to-object mapping configuration

### UI/UX Features
- **Bootstrap 5**: Modern, responsive design framework
- **Bootstrap Icons**: Consistent iconography throughout the application
- **Form Validation**: Real-time client-side validation with server-side backup
- **Success/Error Messages**: One toast mechanism — `this.NotifySuccess(...)` / `this.NotifyError(...)` writing a serialized `ToastMessage` to TempData
- **Responsive Tables**: Mobile-friendly data display

## 🔧 Extending the Application

### Adding New Entities

Follow the established pattern demonstrated by the Product and Customer entities:

1. **Create Domain Entity** in `CleanArchitecture.Domain/Entities/`
   - Inherit from `BaseEntity` for common properties
   - Put invariants in domain methods that throw domain exceptions
   - Derive what can be derived; do not store a second field that means the same thing as an existing one
2. **Add Repository Interface** in `CleanArchitecture.Application/Interfaces/Repositories/`
   - Every method takes a trailing `CancellationToken`
   - Expose it as a property on `IUnitOfWork`
3. **Create DTOs** in `CleanArchitecture.Application/DTOs/`
   - Add validation attributes to the create/update DTO
4. **Add Mapping** in `CleanArchitecture.Application/Mappings/`
   - Implement Mapster `IRegister`; it is discovered automatically
5. **Add Service Interface and Implementation** in `CleanArchitecture.Application/`
   - Register the service in `Application/DependencyInjection/ServiceCollectionExtensions.cs` (`AddApplication`)
6. **Create Entity Configuration** in `CleanArchitecture.Infrastructure/Data/Configurations/`
   - Implement `IEntityTypeConfiguration<T>` for EF mapping and `HasData` seed (fixed timestamps), then add a migration
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
