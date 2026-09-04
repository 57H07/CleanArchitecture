# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

.NET 10 SDK, xUnit. Run from the solution root.

```bash
dotnet build                                   # build whole solution
dotnet run --project CleanArchitecture.Web     # run the MVC app (LocalDB required)
dotnet watch run --project CleanArchitecture.Web
dotnet test                                    # all tests (CleanArchitecture.Application.Tests)
dotnet test --filter "FullyQualifiedName~ProductServiceTests.GetAllAsync_ShouldReturnAllProducts"   # single test
dotnet test --filter "FullyQualifiedName~UserServiceTests"                                          # one test class
```

`Migrate-NetCore.ps1` bumps every csproj TFM and NuGet packages to a new .NET major version (`-TargetVersion 11`, `-WhatIfMode`). Only use it when asked to upgrade .NET.

## Architecture

Four projects, strict dependency direction: `Domain` <- `Application` <- `Infrastructure` <- `Web`. Application references only Domain; Infrastructure references Domain + Application; Web references Application + Infrastructure. Do not add a reference that breaks this order.

Each layer registers itself via an extension method: `AddApplication()` (Mapster + services) and `AddInfrastructure(config)` (DbContext + repositories + UnitOfWork), both called from `Program.cs`. New services/repositories go in the matching `DependencyInjection/ServiceCollectionExtensions.cs`, all scoped.

### Data access: Unit of Work only

Application services take a single `IUnitOfWork`, which exposes repositories as properties (`Users`, `Products`) plus `SaveChangesAsync` and explicit transaction methods. Repositories never call `SaveChanges`; the service does, after mutating through the repository. Adding an entity means adding an `IXxxRepository` in `Application/Interfaces/Repositories`, its EF implementation in `Infrastructure/Repositories`, and a property on both `IUnitOfWork` and `UnitOfWork`.

Every repository/service method takes a trailing `CancellationToken cancellationToken = default` and forwards it.

### Pagination: no IQueryable above Infrastructure

`IPaginatedList<T>` (Application interface, extends `IReadOnlyList<T>`) is the contract. `Infrastructure/Collections/PaginatedList<T>.CreateAsync(IQueryable, page, size, ct)` materializes the page with EF. Services convert to DTOs with `pagedEntities.ToPagedResult(e => e.Adapt<Dto>())`, which yields the Application-side `PagedResult<TDto>`. Web renders it through `PaginationViewComponent` / `PaginationViewModel`. Page index is 1-based; both classes throw on page or size below 1.

### Mapping: Mapster with scanned `IRegister`

`MappingConfig.Configure()` scans the Application assembly for `IRegister` implementations (`UserMappings`, `ProductMappings`) and compiles the global config. Add new mappings as an `IRegister` class in `Application/Mappings`; do not configure mappings elsewhere. Services use the static `.Adapt<T>()` extension.

### Exceptions and HTTP mapping

`Domain/Exceptions` defines abstract bases (`DomainException`, `RessourceNotFoundException`, `InsufficientRightsException`) and `ValidationDomaineException(message, fieldName)`. Application exceptions (`EntityNotFoundException`, `DuplicateEntityException`, `BusinessRuleViolationException`) derive from `DomainException`. `Web/Middleware/GlobalExceptionMiddleware` maps these to status codes (404 / 403 / 422 / 409, anything else 500), returns JSON for XHR/`application/json` requests, otherwise stores a `ToastMessage` in TempData and redirects to `/`.

Two things to know before changing error handling:
- The middleware is registered only in non-Development; Development uses `UseDeveloperExceptionPage`.
- Services currently throw `KeyNotFoundException` for missing entities and controllers catch it explicitly; the richer domain exception hierarchy is mostly unused by services yet. Prefer the domain/application exceptions for new code, and keep controller catches consistent with whatever the service throws.

### Web conventions

- Controllers are thin, wrap service calls in try/catch, and signal outcome via TempData. Two coexisting styles: plain `TempData["Success"]` / `TempData["Error"]` strings, and serialized `ToastMessage` under `ToastMessage.SuccessKey` / `ErrorKey`. `Views/Shared/_Toast.cshtml` reads both.
- `AutoValidateAntiforgeryTokenAttribute` is a global filter, so every POST form needs the antiforgery token.
- Create/Update forms bind directly to Application DTOs (`CreateProductDto`, `CreateUserDto`) validated with Data Annotations; `ViewModels/` hold list/paging shapes only.
- `PaginationViewComponent` and the shared view models live in namespaces `CleanArchitecture.ViewComponents` / `CleanArchitecture.ViewModels.Shared` (no `.Web`), unlike the rest of the Web project.

### Database

SQL Server LocalDB via `ConnectionStrings:DefaultConnection`. There are no EF migrations: `Program.cs` calls `EnsureDeleted()` then `EnsureCreated()` on every startup, so the database is recreated from the model and `HasData` seeds (in `Infrastructure/Data/Configurations/*Configuration.cs`) each run. Any data entered through the UI is lost on restart. Entity configuration lives in `IEntityTypeConfiguration<T>` classes picked up by `ApplyConfigurationsFromAssembly`; do not configure entities inline in `OnModelCreating`.

### Tests

`CleanArchitecture.Application.Tests` covers services only. Pattern: mock `IUnitOfWork` and its repository properties with Moq, build entities/DTOs with `Helpers/TestDataBuilder`, assert with FluentAssertions. `Xunit`, `Moq`, `FluentAssertions`, `AutoFixture` are global usings in the test project.

## Adding a new entity (established order)

1. Entity in `Domain/Entities` inheriting `BaseEntity` (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy); put invariants as domain methods that throw domain exceptions.
2. `IXxxRepository` in `Application/Interfaces/Repositories`; add it to `IUnitOfWork`.
3. DTOs in `Application/DTOs` with Data Annotations; `IRegister` mapping in `Application/Mappings`.
4. `IXxxService` + `XxxService` in Application; register in `AddApplication()`.
5. `XxxConfiguration : IEntityTypeConfiguration<Xxx>` in Infrastructure (including `HasData` seed), repository implementation, `UnitOfWork` property, register in `AddInfrastructure()`.
6. Controller + Razor views (Bootstrap 5 + Bootstrap Icons) in Web.
