# Copilot Instructions for Clean Architecture Project

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview
This is an ASP.NET Core MVC application following Clean Architecture and Domain-Driven Design principles.

## Architecture Guidelines

### Project Structure
- **Domain**: Core business logic, entities, value objects, domain events
- **Application**: Use cases, DTOs, interfaces, services, mappings
- **Infrastructure**: Data access, external services, implementations
- **Web**: MVC controllers, views, presentation logic

### Dependency Rules
- Domain has no dependencies
- Application depends only on Domain
- Infrastructure depends on Domain and Application
- Web depends on Application and Infrastructure

### Coding Standards

#### Entity Framework
- Use DbContext for data access
- Implement repository pattern
- Use Unit of Work for transactions
- Configure entities in OnModelCreating

#### Mapster Usage
- Configure mappings in MappingConfig
- Use TypeAdapterConfig for custom mapping rules
- Leverage Adapt<T>() extension methods
- Register mappings in DI container

#### Validation
- Use Data Annotations for basic validation
- Implement business rule validation in domain
- Provide client-side validation with jQuery
- Display validation errors with Bootstrap styling

#### Controllers
- Keep controllers thin
- Use dependency injection
- Handle exceptions gracefully
- Return appropriate HTTP status codes
- Use TempData for success/error messages

#### Views
- Use Bootstrap 5 for styling
- Implement responsive design
- Include Font Awesome icons
- Use Razor syntax efficiently
- Implement proper form validation

### Naming Conventions
- Use PascalCase for public members
- Use camelCase for private fields
- Prefix interfaces with 'I'
- Use descriptive names for methods and properties
- Follow Microsoft's C# naming guidelines

### Exception Handling
- Use try-catch blocks in controllers
- Log exceptions with ILogger
- Return user-friendly error messages
- Use appropriate exception types

### Security
- Use anti-forgery tokens
- Validate all inputs
- Sanitize data before display
- Follow OWASP guidelines

## When Adding New Features
1. Start with domain entities and business rules
2. Define application interfaces and DTOs
3. Implement infrastructure repositories
4. Create application services
5. Add MVC controllers and views
6. Configure dependency injection
7. Add appropriate validation
8. Include error handling and logging
