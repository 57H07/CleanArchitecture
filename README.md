# Clean Architecture ASP.NET Core MVC Template

A comprehensive Clean Architecture template for ASP.NET Core MVC applications following Domain-Driven Design (DDD) principles with modern best practices.

## 🏗️ Architecture

This template follows the Clean Architecture pattern with the following layers:

### 📦 Projects Structure

```
CleanArchitecture/
├── CleanArchitecture.Domain/          # Core business logic and entities
│   ├── Common/                        # Base classes and interfaces
│   ├── Entities/                      # Domain entities
│   └── Events/                        # Domain events
├── CleanArchitecture.Application/     # Application business rules
│   ├── DTOs/                         # Data Transfer Objects
│   ├── Interfaces/                   # Repository and service interfaces
│   ├── Services/                     # Application services
│   └── Mappings/                     # Object mapping configuration
├── CleanArchitecture.Infrastructure/  # External concerns
│   ├── Data/                         # Database context and configuration
│   ├── Repositories/                 # Repository implementations
│   └── DependencyInjection/          # Service registration
└── CleanArchitecture.Web/            # MVC presentation layer
    ├── Controllers/                   # MVC controllers
    ├── Views/                        # Razor views
    └── wwwroot/                      # Static files
```

## 🚀 Technologies Used

- **Framework**: ASP.NET Core 9.0 MVC
- **Database**: Entity Framework Core with SQL Server
- **Mapping**: Mapster for object-to-object mapping
- **UI**: Bootstrap 5 with responsive design
- **Validation**: Data Annotations with client & server-side validation
- **Styling**: SCSS support for custom Bootstrap themes

## ✨ Features

### Domain Layer
- **Base Entity**: Common properties for all entities (Id, CreatedAt, UpdatedAt, etc.)
- **Aggregate Root**: DDD pattern implementation with domain events
- **Domain Events**: Event-driven architecture support
- **Value Objects**: Domain-driven design patterns
- **Business Rules**: Encapsulated domain logic

### Application Layer
- **CQRS Ready**: Separated command and query responsibilities
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Service Layer**: Application business logic
- **DTOs**: Data transfer objects with validation
- **Mapster Integration**: Efficient object mapping

### Infrastructure Layer
- **Entity Framework Core**: ORM with SQL Server
- **Repository Implementation**: Concrete data access
- **Database Seeding**: Initial data setup
- **Dependency Injection**: Service registration and configuration

### Web Layer
- **MVC Controllers**: RESTful endpoints
- **Razor Views**: Server-side rendering
- **Bootstrap UI**: Modern, responsive interface
- **Form Validation**: Client and server-side validation
- **Error Handling**: Global exception handling
- **Success/Error Messages**: User feedback system

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

The application includes seeded data:

### Users
- John Doe (john.doe@example.com)
- Jane Smith (jane.smith@example.com)

### Products
- Laptop Computer ($1,299.99)
- Wireless Mouse ($29.99)
- Office Chair ($249.99)

## 🎯 Key Features Demonstrated

### Form Validation
- **Server-side**: Data Annotations validation
- **Client-side**: jQuery Unobtrusive Validation
- **Custom validation**: Business rule validation
- **Error display**: Bootstrap alert styling

### CRUD Operations
- **Users Management**: Create, read, update, delete users
- **Products Management**: Full product lifecycle
- **Status Management**: Activate/deactivate users
- **Relationship Management**: User-Product associations

### UI/UX Features
- **Responsive Design**: Mobile-first Bootstrap 5
- **Icon Integration**: Font Awesome icons
- **Success/Error Messages**: Toast-style notifications
- **Confirmation Dialogs**: JavaScript confirmations
- **Data Tables**: Sortable, responsive tables

### Architecture Benefits
- **Separation of Concerns**: Clear layer separation
- **Testability**: Easy unit testing
- **Maintainability**: Clean, organized code
- **Scalability**: Ready for growth
- **Domain Focus**: Business logic separation

## 🔧 Customization

### Adding New Entities

1. **Create Domain Entity** in `CleanArchitecture.Domain/Entities/`
2. **Add Repository Interface** in `CleanArchitecture.Application/Interfaces/`
3. **Create DTOs** in `CleanArchitecture.Application/DTOs/`
4. **Implement Repository** in `CleanArchitecture.Infrastructure/Repositories/`
5. **Add Service Interface** in `CleanArchitecture.Application/Interfaces/`
6. **Implement Service** in `CleanArchitecture.Application/Services/`
7. **Create Controller** in `CleanArchitecture.Web/Controllers/`
8. **Add Views** in `CleanArchitecture.Web/Views/`

### Styling Customization

The template supports SCSS customization:
1. Create SCSS files in `wwwroot/scss/`
2. Customize Bootstrap variables
3. Add custom styles
4. Compile to CSS (setup build process)

## 📚 Best Practices Implemented

- **Clean Architecture**: Dependency inversion and separation
- **Domain-Driven Design**: Rich domain models
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **SOLID Principles**: Well-structured code
- **Exception Handling**: Proper error management
- **Logging**: Structured logging with ILogger
- **Validation**: Multiple validation layers
- **Security**: Anti-forgery tokens

## 🤝 Contributing

This template serves as a starting point for Clean Architecture projects. Feel free to:
- Add new features
- Improve existing functionality
- Enhance UI/UX
- Add tests
- Update documentation

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Clean Architecture by Robert C. Martin
- ASP.NET Core team
- Bootstrap team
- Mapster library contributors
- Entity Framework Core team
