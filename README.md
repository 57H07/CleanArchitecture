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
│   └── Exceptions/                    # Domain-specific exceptions
├── CleanArchitecture.Application/     # Application services and contracts
│   ├── DTOs/                         # Data Transfer Objects with validation
│   ├── Interfaces/                   # Repository and service interfaces
│   ├── Services/                     # Application services (User & Product)
│   ├── Mappings/                     # Mapster configuration
│   └── Exceptions/                   # Application-level exceptions
├── CleanArchitecture.Infrastructure/  # Data access and external concerns
│   ├── Data/                         # DbContext and entity configurations
│   │   └── Configurations/           # Separate EF entity configurations
│   ├── Repositories/                 # Repository implementations
│   └── DependencyInjection/          # Service registration
└── CleanArchitecture.Web/            # MVC presentation layer
    ├── Controllers/                   # Home, Users, Products controllers
    ├── Views/                        # Razor views with Bootstrap UI
    ├── Models/                       # View models
    ├── Middleware/                   # Global exception handling
    └── wwwroot/                      # Static assets (CSS, JS, libraries)
```

## 🚀 Technologies Used

- **Framework**: ASP.NET Core 9.0 MVC
- **Database**: Entity Framework Core 9.0 with SQL Server LocalDB
- **Mapping**: Mapster 7.4.0 for object-to-object mapping
- **UI**: Bootstrap 5 with Bootstrap Icons
- **Validation**: Data Annotations with client & server-side validation
- **Development**: .NET 9.0 with nullable reference types enabled

## ✨ Features

### Domain Layer
- **Base Entity**: Common properties for all entities (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- **User Entity**: Complete user model with validation and relationships
- **Product Entity**: Product model with pricing, inventory, and categorization
- **Domain Exceptions**: Custom exception classes for business rule violations

### Application Layer
- **Repository Pattern**: Data access abstraction with interfaces
- **Unit of Work**: Transaction management pattern
- **Service Layer**: UserService and ProductService for business logic
- **DTOs**: Validated data transfer objects for Create/Update operations
- **Mapster Integration**: Efficient object mapping between entities and DTOs

### Infrastructure Layer
- **Entity Framework Core**: Code-first approach with SQL Server
- **Separate Entity Configurations**: Individual configuration files for each entity
- **Repository Implementation**: Concrete implementations of repository interfaces
- **Database Seeding**: Pre-populated sample data for Users and Products
- **Dependency Injection**: Clean service registration and configuration

### Web Layer
- **MVC Controllers**: Home, Users, and Products controllers with full CRUD
- **Razor Views**: Server-side rendered views with Bootstrap 5 styling
- **Form Validation**: Client and server-side validation with error display
- **Global Exception Handling**: Middleware for centralized error management
- **Responsive UI**: Mobile-friendly interface with Bootstrap components

## 🛠️ Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or Visual Studio Code

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
   cd CleanArchitecture.Web
   dotnet run
   ```

6. **Open in browser**
   Navigate to `https://localhost:5001` or `http://localhost:5000`

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
- Database created automatically on first run
- Entity Framework ensures schema creation

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
   - Add domain-specific properties and validation
2. **Add Repository Interface** in `CleanArchitecture.Application/Interfaces/`
   - Define methods for data operations
3. **Create DTOs** in `CleanArchitecture.Application/DTOs/`
   - Add validation attributes for data transfer objects
4. **Create Entity Configuration** in `CleanArchitecture.Infrastructure/Data/Configurations/`
   - Implement `IEntityTypeConfiguration<T>` for EF mapping
5. **Implement Repository** in `CleanArchitecture.Infrastructure/Repositories/`
   - Concrete implementation of repository interface
6. **Add Service Interface** in `CleanArchitecture.Application/Interfaces/`
   - Define business operations
7. **Implement Service** in `CleanArchitecture.Application/Services/`
   - Business logic implementation
8. **Create Controller** in `CleanArchitecture.Web/Controllers/`
   - MVC controller with CRUD actions
9. **Add Views** in `CleanArchitecture.Web/Views/`
   - Razor views for the user interface
10. **Register Services** in `DependencyInjection/ServiceCollectionExtensions.cs`
    - Add new services to the DI container

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

This project is provided as-is for educational and demonstration purposes.

## 🙏 Acknowledgments

- **Robert C. Martin** - Clean Architecture principles
- **Microsoft** - ASP.NET Core and Entity Framework Core frameworks
- **Mapster Team** - Efficient object mapping library  
- **Bootstrap Team** - UI framework and components
