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

Schema changes go through EF migrations (see **Database**), never by editing the generated files:

```bash
dotnet ef migrations add <Name> --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web --output-dir Data/Migrations
dotnet ef database update       --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web
dotnet ef migrations has-pending-model-changes --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web
```

Package versions are centralised in `Directory.Packages.props`; a `PackageReference` in a csproj carries no `Version`. Shared MSBuild settings (TFM, nullable, `TreatWarningsAsErrors`) live in `Directory.Build.props` — the build fails on warnings, so do not leave them behind.

`Migrate-NetCore.ps1` bumps every csproj TFM and NuGet packages to a new .NET major version (`-TargetVersion 11`, `-WhatIfMode`). Only use it when asked to upgrade .NET.

## Architecture

Four projects, strict dependency direction: `Domain` <- `Application` <- `Infrastructure` <- `Web`. Application references only Domain; Infrastructure references Domain + Application; Web references Application + Infrastructure. Do not add a reference that breaks this order.

Each layer registers itself via an extension method: `AddApplication()` (Mapster + services) and `AddInfrastructure(config)` (DbContext + repositories + UnitOfWork), both called from `Program.cs`. New services/repositories go in the matching `DependencyInjection/ServiceCollectionExtensions.cs`, all scoped.

### Domain modelling: one field, one meaning

`Product.Status` (`Draft`/`Active`/`Inactive`/`Discontinued`) is the sole source of truth for whether a product is on sale. There is no `IsAvailable` flag any more — it existed alongside `Status`, nothing kept the two in step, and a Draft product with stock on hand rendered as "Out of stock" while `GetAvailableProductsAsync` happily returned it.

Derived facts are methods, not stored columns: `IsInStock()` (stock only), `IsPublished()`, `IsPurchasable()` (published *and* in stock). State changes go through `Publish()`/`Deactivate()`/`Discontinue()`. When you add a flag that could be computed from an existing field, compute it.

### Data access: Unit of Work only

Application services take a single `IUnitOfWork`, which exposes repositories as properties (`Products`, `Customers`) plus `SaveChangesAsync` and explicit transaction methods. Repositories never call `SaveChanges`; the service does, after mutating through the repository. Adding an entity means adding an `IXxxRepository` in `Application/Interfaces/Repositories`, its EF implementation in `Infrastructure/Repositories`, and a property on both `IUnitOfWork` and `UnitOfWork`.

Every repository/service method takes a trailing `CancellationToken cancellationToken = default` and forwards it.

Read-only repository queries use `AsNoTracking()`. `GetByIdAsync` does not — it feeds the update path and must return a tracked entity.

`AddInfrastructure` deliberately does **not** enable `EnableRetryOnFailure`. `IUnitOfWork` exposes explicit `BeginTransactionAsync`/`CommitTransactionAsync`/`RollbackTransactionAsync`, and EF refuses a user-opened transaction under a retrying execution strategy because it cannot replay it. Adding connection resiliency means first replacing those three with an execution-strategy-aware helper.

Services set `CreatedAt`/`UpdatedAt` themselves, next to `ValidateBusinessRules()`. `CreatedBy`/`UpdatedBy` exist on `BaseEntity` but are only populated by the seed data; wire them up alongside authentication when you add it.

### Pagination: no IQueryable above Infrastructure

`IPaginatedList<T>` (Application interface, extends `IReadOnlyList<T>`) is the contract. `Infrastructure/Collections/PaginatedList<T>.CreateAsync(IQueryable, page, size, ct)` materializes the page with EF. Services convert to DTOs with `pagedEntities.ToPagedResult(e => e.Adapt<Dto>())`, which yields the Application-side `PagedResult<TDto>`. 

### Mapping: Mapster with scanned `IRegister`

`MappingConfig.Configure()` scans the Application assembly for `IRegister` implementations (`ProductMappings`, `CustomerMappings`) and compiles the global config. Add new mappings as an `IRegister` class in `Application/Mappings`; do not configure mappings elsewhere. Services use the static `.Adapt<T>()` extension.

Register through the `config` parameter (`config.NewConfig<TSource, TDest>()`), not the static `TypeAdapterConfig<A, B>.NewConfig()` — the latter ignores the argument the scan hands you and writes straight to global state.

The `CreateXxxDto -> Xxx` registration is also the *update* mapping: services adapt the DTO onto the tracked entity (`dto.Adapt(entity)`). So it must keep `IgnoreNullValues(false)` — MVC binds an empty text input to `null`, and ignoring nulls would silently keep the old value whenever a user clears an optional field.

### Exceptions and HTTP mapping

`Domain/Exceptions` defines abstract bases (`DomainException`, `RessourceNotFoundException`, `InsufficientRightsException`) and `ValidationDomaineException(message, fieldName)`. Application exceptions (`EntityNotFoundException`, `DuplicateEntityException`, `BusinessRuleViolationException`) derive from those. `Web/Middleware/GlobalExceptionMiddleware` maps them to status codes (404 / 403 / 422 / 409, anything else 500), returns JSON for XHR/`application/json` requests, otherwise stores a `ToastMessage` in TempData and redirects to `/`.

Things to know before changing error handling:
- The middleware is registered in **every** environment. In Development it sits *inside* `UseDeveloperExceptionPage`, and rethrows anything it maps to 500 — so recognised domain exceptions behave identically everywhere, while a genuine bug still gets the developer page and its stack trace.
- Because the middleware is always on, controllers are thin: they catch only what they can turn into a field-level `ModelState` error on the form the user is already looking at. Everything else is allowed to reach the middleware. Do not add blanket `catch (Exception)` blocks back.
- Both services throw `EntityNotFoundException` (which derives from `RessourceNotFoundException`, so the middleware maps it to 404). It carries `EntityName`/`Id` so a caller handling two lookups can tell them apart with a `when` filter instead of matching on `Message`. Keep new code on this hierarchy.
- `UnitOfWork.SaveChangesAsync` translates a SQL unique-index violation into `DuplicateEntityException`, keyed off the index names in its `UniqueIndexes` map. A new unique index needs an entry there, or the race it guards surfaces as a 500.

### Web conventions

- Controllers are thin (see **Exceptions and HTTP mapping**) and signal outcome with `this.NotifySuccess(...)` / `this.NotifyError(...)` from `Web/Extensions/ControllerExtensions.cs`, which write a serialized `ToastMessage` under `ToastMessage.SuccessKey` / `ErrorKey`. That is the only toast mechanism — the older bare `TempData["Success"]` strings are gone, and `Views/Shared/_Toast.cshtml` no longer reads them.
- `AutoValidateAntiforgeryTokenAttribute` is a global filter, so every POST form needs the antiforgery token.
- Create/Update forms bind directly to Application DTOs (`CreateProductDto`, `CreateCustomerDto`) validated with Data Annotations; `ViewModels/` hold list/paging shapes only.
- The product status pill is rendered by one partial, `Views/Shared/Products/_ProductStatus.cshtml`. Do not re-derive status markup inline; that is exactly how the old `IsAvailable`/`Status` split went wrong.

### Styling

Custom CSS is authored as SCSS under `CleanArchitecture.Web/Styles/` and compiled to `wwwroot/css/app.css` by the `AspNetCore.SassCompiler` package on every `dotnet build` (config in `sasscompiler.json`; `dotnet watch` recompiles on save). Only `app.scss` compiles — it `@use`s the `_`-prefixed partials, split by concern: `base/` (tokens, typography, layout shell) then `components/` (navbar, buttons, forms, tables, badges, pagination, modals, toasts) then `utilities/_motion.scss` last.

Bootstrap 5.3 is never forked. The stock `lib/bootstrap/dist/css/bootstrap.min.css` is loaded as-is and retuned through its own CSS custom properties: `base/_theme.scss` defines the `--app-*` palette, remaps `--bs-*` onto it, and redeclares every token under `[data-bs-theme="dark"]`. Anything new should reach for an existing `--app-*` token or a Bootstrap `--bs-*` variable rather than a literal colour, or it will not follow the theme.

Conventions worth knowing:
- Status is a dot pill (`.status-positive|caution|danger|neutral`), classification is an outline chip (`.tag`). Do not use `badge bg-*`.
- Boxed surfaces are `.panel` (flat, hairline border); only overlays get a shadow.
- Buttons are `.btn-primary`, `.btn-quiet` or `.btn-danger`; row actions are `.btn-icon` inside `.row-actions`, icon-only with `title` + `aria-label`.
- Toast markup exists twice — `Views/Shared/_Toast.cshtml` and `wwwroot/js/helpers/toast.js` — and both emit `toast--<variant>`; change them together.
- Theme is `auto` (OS) / `light` / `dark`, persisted in `localStorage["theme"]`. An inline script in `_Layout.cshtml`'s `<head>` stamps `data-bs-theme` before first paint; `wwwroot/js/theme.js` owns the toggle.
- Motion is confined to `utilities/_motion.scss` and is disabled under `prefers-reduced-motion`.

### Localization

`Program.cs` pins the request culture to `en-US` via `UseRequestLocalization` with a single supported culture, so `Accept-Language` is ignored. This is load-bearing, not cosmetic: the views hardcode `$` and `ToString("C")` and jQuery validation assumes `.` as the decimal separator, so on a server whose OS locale uses `,` the product form becomes unsubmittable — the model binder and the client validator disagree about every price. Change the culture only together with those formats.

### Database

SQL Server LocalDB via `ConnectionStrings:DefaultConnection`. The schema is owned by **EF migrations** in `Infrastructure/Data/Migrations` — there is no `EnsureCreated`, and nothing drops the database on startup.

`Program.cs` applies pending migrations only when `Database:MigrateOnStartup` is true. It is `true` in `appsettings.Development.json` and `false` in `appsettings.json`, because in a deployed environment schema changes belong to the release pipeline (`dotnet ef database update`), not to whichever instance boots first — two instances racing to migrate is a bad afternoon.

Entity configuration lives in `IEntityTypeConfiguration<T>` classes picked up by `ApplyConfigurationsFromAssembly`; do not configure entities inline in `OnModelCreating`.

Seed data is `HasData` in those configuration classes, with timestamps from the fixed `SeedData.CreatedAt` constant. Never use `DateTime.UtcNow` in `HasData`: it feeds the migration snapshot, so a moving value makes every `migrations add` emit a spurious diff. `has-pending-model-changes` is the check for this.

Coming from an older checkout whose database was built by `EnsureCreated`? It has no `__EFMigrationsHistory`, so migrating fails. Drop it once: `dotnet ef database drop --force --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web`.

### Tests

`CleanArchitecture.Application.Tests` covers services, the Mapster registrations and the product status model (`ProductStatusMappingTests`). `Helpers/MappingSetup` runs `MappingConfig.Configure()` from a `[ModuleInitializer]`, so the suite exercises the real mapping config rather than Mapster's convention fallback. Pattern: mock `IUnitOfWork` and its repository properties with Moq, build entities/DTOs with `Helpers/TestDataBuilder`, assert with FluentAssertions. `Xunit`, `Moq`, `FluentAssertions`, `AutoFixture` are global usings in the test project.

## Adding a new entity (established order)

1. Entity in `Domain/Entities` inheriting `BaseEntity` (Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy); put invariants as domain methods that throw domain exceptions, and derive anything derivable rather than storing it.
2. `IXxxRepository` in `Application/Interfaces/Repositories`; add it to `IUnitOfWork`.
3. DTOs in `Application/DTOs` with Data Annotations; `IRegister` mapping in `Application/Mappings`, registered through the `config` parameter, with `IgnoreNullValues(false)`.
4. `IXxxService` + `XxxService` in Application; register in `AddApplication()`.
5. `XxxConfiguration : IEntityTypeConfiguration<Xxx>` in Infrastructure (`HasData` seed using `SeedData.CreatedAt`), repository implementation, `UnitOfWork` property, register in `AddInfrastructure()`.
6. `dotnet ef migrations add AddXxx` (see **Commands**) and check in the generated files.
7. Controller + Razor views (Bootstrap 5 + Bootstrap Icons) in Web; outcomes go through `NotifySuccess`/`NotifyError`.
8. Tests in `CleanArchitecture.Application.Tests/Services`, mocking `IUnitOfWork` and building data with `TestDataBuilder`.

## Rules

- Avoid unnecessar comments. The code should be self-explanatory unless it is too complex.
