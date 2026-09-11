# CLAUDE.md

Guidance for Claude Code in this repo.

## Commands

.NET 10 SDK, xUnit. Run from solution root.

```bash
dotnet build
dotnet run --project CleanArchitecture.Web     # LocalDB required
dotnet watch run --project CleanArchitecture.Web
dotnet test
dotnet test --filter "FullyQualifiedName~ProductServiceTests.GetAllAsync_ShouldReturnAllProducts"
dotnet test --filter "FullyQualifiedName~CustomerServiceTests"
```

Schema changes: EF migrations only, never hand-edit generated files.

```bash
dotnet ef migrations add <Name> --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web --output-dir Data/Migrations
dotnet ef database update       --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web
dotnet ef migrations has-pending-model-changes --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web
```

Package versions live only in `Directory.Packages.props` (no `Version` in csproj `PackageReference`s). Shared MSBuild settings (TFM, nullable, `TreatWarningsAsErrors`) in `Directory.Build.props` — warnings fail the build. `Migrate-NetCore.ps1` bumps TFM/packages to a new .NET major version; only run when asked to upgrade .NET.

## Architecture

`Domain` <- `Application` <- `Infrastructure` <- `Web`, strict. Application refs only Domain; Infrastructure refs Domain+Application; Web refs Application+Infrastructure. Each layer self-registers: `AddApplication()`, `AddInfrastructure(config)`, called from `Program.cs`. New services/repos go in the matching `DependencyInjection/ServiceCollectionExtensions.cs`, all scoped.

**Domain modelling**: one field, one meaning. `Product.Status` (Draft/Active/Inactive/Discontinued) is sole source of truth for saleability — no separate `IsAvailable` flag. Derived facts are methods, not columns: `IsInStock()`, `IsPublished()`, `IsPurchasable()`. State changes via `Publish()`/`Deactivate()`/`Discontinue()`. Compute anything derivable rather than storing it.

**Data access — Unit of Work only**: services take a single `IUnitOfWork` exposing repos (`Products`, `Customers`) + `SaveChangesAsync` + explicit transaction methods. Repositories never call `SaveChanges`; the service does. New entity ⇒ `IXxxRepository` in `Application/Interfaces/Repositories`, EF impl in `Infrastructure/Repositories`, property on `IUnitOfWork` + `UnitOfWork`. Every repo/service method takes trailing `CancellationToken cancellationToken = default`, forwarded. Read queries use `AsNoTracking()`; `GetByIdAsync` does not (feeds update path, must stay tracked). No `EnableRetryOnFailure` — explicit `BeginTransactionAsync`/`CommitTransactionAsync`/`RollbackTransactionAsync` are incompatible with a retrying execution strategy; adding resiliency requires replacing those first. Services set `CreatedAt`/`UpdatedAt` themselves next to `ValidateBusinessRules()`; `CreatedBy`/`UpdatedBy` only populated by seed data until auth exists.

**Pagination — no IQueryable above Infrastructure**: `IPaginatedList<T>` is the Application contract. `Infrastructure/Collections/PaginatedList<T>.CreateAsync(IQueryable, page, size, ct)` materializes with EF. Services convert via `pagedEntities.ToPagedResult(e => e.Adapt<Dto>())` → `PagedResult<TDto>`.

**Mapping — Mapster, scanned `IRegister`**: `MappingConfig.Configure()` scans Application assembly for `IRegister` impls (`ProductMappings`, `CustomerMappings`). Add mappings only as new `IRegister` classes. Register via the `config` parameter (`config.NewConfig<A,B>()`), never the static `TypeAdapterConfig<A,B>.NewConfig()` (writes to global state, ignores the scan). `CreateXxxDto -> Xxx` doubles as the update mapping (`dto.Adapt(entity)`) — must keep `IgnoreNullValues(false)` since MVC binds empty inputs to null.

**Exceptions/HTTP mapping**: `Domain/Exceptions` has abstract bases (`DomainException`, `RessourceNotFoundException`, `InsufficientRightsException`, `ValidationDomaineException(message, fieldName)`); Application exceptions (`EntityNotFoundException`, `DuplicateEntityException`, `BusinessRuleViolationException`) derive from those. `Web/Middleware/GlobalExceptionMiddleware` maps to status codes (404/403/422/409, else 500); JSON for XHR, else `ToastMessage` in TempData + redirect. Registered in every environment (in Dev it sits inside `UseDeveloperExceptionPage` and rethrows unmapped errors, so real bugs still get the dev page). Controllers stay thin — catch only what becomes a field-level `ModelState` error; no blanket `catch (Exception)`. `EntityNotFoundException` carries `EntityName`/`Id` for `when`-filtering. `UnitOfWork.SaveChangesAsync` maps unique-index violations to `DuplicateEntityException` via its `UniqueIndexes` map — a new unique index needs an entry there or it surfaces as 500.

**Web conventions**: controllers signal outcome via `this.NotifySuccess/NotifyError` (`Web/Extensions/ControllerExtensions.cs`) — the only toast mechanism. `AutoValidateAntiforgeryTokenAttribute` is global — every POST form needs the token. Create/Update forms bind directly to Application DTOs validated with Data Annotations; `ViewModels/` are list/paging shapes only. Product status pill is one partial, `Views/Shared/Products/_ProductStatus.cshtml` — don't re-derive markup inline.

**Styling**: SCSS under `CleanArchitecture.Web/Styles/` compiles to `wwwroot/css/app.css` via `AspNetCore.SassCompiler` on build (`dotnet watch` recompiles on save). Only `app.scss` compiles, `@use`s `base/` → `components/` → `utilities/_motion.scss`. Bootstrap 5.3 is never forked — retuned via CSS custom properties in `base/_theme.scss` (`--app-*` remapped onto `--bs-*`, redeclared under `[data-bs-theme="dark"]`); use existing tokens, not literal colors. Status = dot pill (`.status-positive|caution|danger|neutral`); classification = outline chip (`.tag`); never `badge bg-*`. Boxed surfaces = `.panel` (flat); only overlays get shadow. Buttons: `.btn-primary`/`.btn-quiet`/`.btn-danger`; row actions `.btn-icon` in `.row-actions` with `title`+`aria-label`. Toast markup exists in both `Views/Shared/_Toast.cshtml` and `wwwroot/js/helpers/toast.js` (`toast--<variant>`) — change together. Theme `auto`/`light`/`dark` in `localStorage["theme"]`; inline script in `_Layout.cshtml` head stamps `data-bs-theme` pre-paint, `wwwroot/js/theme.js` owns toggle. Motion confined to `utilities/_motion.scss`, disabled under `prefers-reduced-motion`.

**Localization**: culture pinned to `en-US` in `Program.cs` (`Accept-Language` ignored) because views hardcode `$`/`ToString("C")` and jQuery validation assumes `.` decimals. Change culture only together with those formats.

**Database**: SQL Server LocalDB, `ConnectionStrings:DefaultConnection`. Schema owned by EF migrations in `Infrastructure/Data/Migrations` — no `EnsureCreated`. `Program.cs` applies pending migrations only when `Database:MigrateOnStartup` is true (Development only; false elsewhere — deployed schema changes go through the release pipeline). Entity config via `IEntityTypeConfiguration<T>` + `ApplyConfigurationsFromAssembly`, never inline in `OnModelCreating`. Seed data uses `HasData` with the fixed `SeedData.CreatedAt` constant — never `DateTime.UtcNow` (would make every `migrations add` emit a spurious diff; `has-pending-model-changes` checks this). Old `EnsureCreated`-built DB with no `__EFMigrationsHistory`: `dotnet ef database drop --force --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Web`.

**Tests**: `CleanArchitecture.Application.Tests` covers services, Mapster registrations, product status model. `Helpers/MappingSetup` runs `MappingConfig.Configure()` via `[ModuleInitializer]` so tests hit real mapping config. Mock `IUnitOfWork`/repos with Moq, build data with `Helpers/TestDataBuilder`, assert with FluentAssertions. `Xunit`, `Moq`, `FluentAssertions`, `AutoFixture` are global usings.

## Adding a new entity

1. Entity in `Domain/Entities` : `BaseEntity`; invariants as domain methods throwing domain exceptions; derive rather than store.
2. `IXxxRepository` in `Application/Interfaces/Repositories`; add to `IUnitOfWork`.
3. DTOs in `Application/DTOs` with Data Annotations; `IRegister` mapping in `Application/Mappings` via `config` param, `IgnoreNullValues(false)`.
4. `IXxxService` + `XxxService`; register in `AddApplication()`.
5. `XxxConfiguration : IEntityTypeConfiguration<Xxx>` in Infrastructure (`HasData` using `SeedData.CreatedAt`), repo impl, `UnitOfWork` property, register in `AddInfrastructure()`.
6. `dotnet ef migrations add AddXxx`; check in generated files.
7. Controller + Razor views (Bootstrap 5 + Bootstrap Icons); outcomes via `NotifySuccess`/`NotifyError`.
8. Tests in `CleanArchitecture.Application.Tests/Services`, mocking `IUnitOfWork`, using `TestDataBuilder`.

## Rules

- Avoid unnecessary comments. Code should be self-explanatory unless genuinely complex.
