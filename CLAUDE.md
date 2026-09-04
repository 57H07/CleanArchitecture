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
dotnet test --filter "FullyQualifiedName~CustomerServiceTests"                                      # one test class
```

`Migrate-NetCore.ps1` bumps every csproj TFM and NuGet packages to a new .NET major version (`-TargetVersion 11`, `-WhatIfMode`). Only use it when asked to upgrade .NET.

## Architecture

Four projects, strict dependency direction: `Domain` <- `Application` <- `Infrastructure` <- `Web`. Application references only Domain; Infrastructure references Domain + Application; Web references Application + Infrastructure. Do not add a reference that breaks this order.

Each layer registers itself via an extension method: `AddApplication()` (Mapster + services) and `AddInfrastructure(config)` (DbContext + repositories + UnitOfWork), both called from `Program.cs`. New services/repositories go in the matching `DependencyInjection/ServiceCollectionExtensions.cs`, all scoped.

### Data access: Unit of Work only

Application services take a single `IUnitOfWork`, which exposes repositories as properties (`Products`, `Customers`) plus `SaveChangesAsync` and explicit transaction methods. Repositories never call `SaveChanges`; the service does, after mutating through the repository. Adding an entity means adding an `IXxxRepository` in `Application/Interfaces/Repositories`, its EF implementation in `Infrastructure/Repositories`, and a property on both `IUnitOfWork` and `UnitOfWork`.

Every repository/service method takes a trailing `CancellationToken cancellationToken = default` and forwards it.

### Pagination: no IQueryable above Infrastructure

`IPaginatedList<T>` (Application interface, extends `IReadOnlyList<T>`) is the contract. `Infrastructure/Collections/PaginatedList<T>.CreateAsync(IQueryable, page, size, ct)` materializes the page with EF. Services convert to DTOs with `pagedEntities.ToPagedResult(e => e.Adapt<Dto>())`, which yields the Application-side `PagedResult<TDto>`. 

### Mapping: Mapster with scanned `IRegister`

`MappingConfig.Configure()` scans the Application assembly for `IRegister` implementations (`ProductMappings`, `CustomerMappings`) and compiles the global config. Add new mappings as an `IRegister` class in `Application/Mappings`; do not configure mappings elsewhere. Services use the static `.Adapt<T>()` extension.

The `CreateXxxDto -> Xxx` registration is also the *update* mapping: services adapt the DTO onto the tracked entity (`dto.Adapt(entity)`). So it must keep `IgnoreNullValues(false)` — MVC binds an empty text input to `null`, and ignoring nulls would silently keep the old value whenever a user clears an optional field.

### Exceptions and HTTP mapping

`Domain/Exceptions` defines abstract bases (`DomainException`, `RessourceNotFoundException`, `InsufficientRightsException`) and `ValidationDomaineException(message, fieldName)`. Application exceptions (`EntityNotFoundException`, `DuplicateEntityException`, `BusinessRuleViolationException`) derive from `DomainException`. `Web/Middleware/GlobalExceptionMiddleware` maps these to status codes (404 / 403 / 422 / 409, anything else 500), returns JSON for XHR/`application/json` requests, otherwise stores a `ToastMessage` in TempData and redirects to `/`.

Two things to know before changing error handling:
- The middleware is registered only in non-Development; Development uses `UseDeveloperExceptionPage`.
- Services currently throw `KeyNotFoundException` for missing entities and controllers catch it explicitly; the richer domain exception hierarchy is mostly unused by services yet. Prefer the domain/application exceptions for new code, and keep controller catches consistent with whatever the service throws.

### Web conventions

- Controllers are thin, wrap service calls in try/catch, and signal outcome via TempData. Two coexisting styles: plain `TempData["Success"]` / `TempData["Error"]` strings, and serialized `ToastMessage` under `ToastMessage.SuccessKey` / `ErrorKey`. `Views/Shared/_Toast.cshtml` reads both.
- `AutoValidateAntiforgeryTokenAttribute` is a global filter, so every POST form needs the antiforgery token.
- Create/Update forms bind directly to Application DTOs (`CreateProductDto`, `CreateCustomerDto`) validated with Data Annotations; `ViewModels/` hold list/paging shapes only.

### Styling

Custom CSS is authored as SCSS under `CleanArchitecture.Web/Styles/` and compiled to `wwwroot/css/app.css` by the `AspNetCore.SassCompiler` package on every `dotnet build` (config in `sasscompiler.json`; `dotnet watch` recompiles on save). Only `app.scss` compiles — it `@use`s the `_`-prefixed partials, split by concern: `base/` (tokens, typography, layout shell) then `components/` (navbar, buttons, forms, tables, badges, pagination, modals, toasts) then `utilities/_motion.scss` last.

Bootstrap 5.3 is never forked. The stock `lib/bootstrap/dist/css/bootstrap.min.css` is loaded as-is and retuned through its own CSS custom properties: `base/_theme.scss` defines the `--app-*` palette, remaps `--bs-*` onto it, and redeclares every token under `[data-bs-theme="dark"]`. Anything new should reach for an existing `--app-*` token or a Bootstrap `--bs-*` variable rather than a literal colour, or it will not follow the theme.

Conventions worth knowing:
- Status is a dot pill (`.status-positive|caution|danger|neutral`), classification is an outline chip (`.tag`). Do not use `badge bg-*`.
- Boxed surfaces are `.panel` (flat, hairline border); only overlays get a shadow.
- Buttons are `.btn-primary`, `.btn-quiet` or `.btn-danger`; row actions are `.btn-icon` inside `.row-actions`, icon-only with `title` + `aria-label`.
- Toast markup exists twice — `Views/Shared/_Toast.cshtml` and `wwwroot/js/Helpers/toast.js` — and both emit `toast--<variant>`; change them together.
- Theme is `auto` (OS) / `light` / `dark`, persisted in `localStorage["theme"]`. An inline script in `_Layout.cshtml`'s `<head>` stamps `data-bs-theme` before first paint; `wwwroot/js/theme.js` owns the toggle.
- Motion is confined to `utilities/_motion.scss` and is disabled under `prefers-reduced-motion`.

### Database

SQL Server LocalDB via `ConnectionStrings:DefaultConnection`. There are no EF migrations: `Program.cs` calls `EnsureDeleted()` then `EnsureCreated()` on every startup, so the database is recreated from the model and `HasData` seeds (in `Infrastructure/Data/Configurations/*Configuration.cs`) each run. Any data entered through the UI is lost on restart. Entity configuration lives in `IEntityTypeConfiguration<T>` classes picked up by `ApplyConfigurationsFromAssembly`; do not configure entities inline in `OnModelCreating`.

### Tests

`CleanArchitecture.Application.Tests` covers services and the Mapster registrations. `Helpers/MappingSetup` runs `MappingConfig.Configure()` from a `[ModuleInitializer]`, so the suite exercises the real mapping config rather than Mapster's convention fallback. Pattern: mock `IUnitOfWork` and its repository properties with Moq, build entities/DTOs with `Helpers/TestDataBuilder`, assert with FluentAssertions. `Xunit`, `Moq`, `FluentAssertions`, `AutoFixture` are global usings in the test project.

## Adding a new entity (established order)

1. Entity in `Domain/Entities` inheriting `BaseEntity` (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy); put invariants as domain methods that throw domain exceptions.
2. `IXxxRepository` in `Application/Interfaces/Repositories`; add it to `IUnitOfWork`.
3. DTOs in `Application/DTOs` with Data Annotations; `IRegister` mapping in `Application/Mappings`.
4. `IXxxService` + `XxxService` in Application; register in `AddApplication()`.
5. `XxxConfiguration : IEntityTypeConfiguration<Xxx>` in Infrastructure (including `HasData` seed), repository implementation, `UnitOfWork` property, register in `AddInfrastructure()`.
6. Controller + Razor views (Bootstrap 5 + Bootstrap Icons) in Web.

# Global style:

- Code-only, no explanation unless asked. Concise output.
- Do not add comments unless code is hard to understand without them.
