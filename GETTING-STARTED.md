# Getting Started

This guide is for developers who know C# and ASP.NET Core MVC but haven't worked in a Clean Architecture codebase before. It explains *why* the code is split into four projects, walks through adding a feature end to end, clears up the type distinctions that trip people up, and shows how to restyle the UI.

Prerequisites and setup live in [README.md](README.md). Short version:

```bash
dotnet build
dotnet run --project CleanArchitecture.Web
dotnet test
```

---

## Table of contents

1. [The big idea](#1-the-big-idea)
2. [The four layers](#2-the-four-layers)
3. [Following one request through the app](#3-following-one-request-through-the-app)
4. [Implementing a feature end to end](#4-implementing-a-feature-end-to-end-supplier)
5. [Key concepts](#5-key-concepts)
   - [Repository vs Service](#51-repository-vs-service)
   - [Entity vs DTO](#52-entity-vs-dto)
   - [DTO vs ViewModel](#53-dto-vs-viewmodel)
6. [Visual customization](#6-visual-customization)
7. [Common mistakes](#7-common-mistakes)

---

## 1. The big idea

### The problem it solves

In a typical MVC app, a controller talks to `DbContext` directly. That works until you want to:

- test the business logic without a database
- swap SQL Server for Postgres, or add a REST API beside the MVC UI
- find where a rule like "a customer with products can't be deleted" actually lives

The reason those get hard is that the business rules end up *tangled with* the infrastructure — a rule inside a controller action needs HTTP to run; a rule inside a repository needs a database to run.

Clean Architecture untangles them with one rule.

### The one rule

> **Dependencies point inward. The business logic at the centre depends on nothing.**

Concentrically, with the four projects in this repo:

```
   ┌─────────────────────────────────────────┐
   │  Web                                    │   MVC, controllers, views
   │   ┌─────────────────────────────────┐   │
   │   │  Infrastructure                 │   │   EF Core, repositories
   │   │   ┌─────────────────────────┐   │   │
   │   │   │  Application            │   │   │   use cases, DTOs, contracts
   │   │   │   ┌─────────────────┐   │   │   │
   │   │   │   │  Domain         │   │   │   │   entities, invariants
   │   │   │   └─────────────────┘   │   │   │
   │   │   └─────────────────────────┘   │   │
   │   └─────────────────────────────────┘   │
   └─────────────────────────────────────────┘

   dependencies point inward only ──────────>
```

An outer ring may reference anything inside it. Nothing may reference outward. The actual `<ProjectReference>` entries:

| Project | References |
| --- | --- |
| `Domain` | *(nothing)* |
| `Application` | Domain |
| `Infrastructure` | Domain, Application |
| `Web` | Application, Infrastructure |

Infrastructure and Web each reference the inner projects **directly**, not transitively — this is not a single `Web -> Infrastructure -> Application -> Domain` chain.

### Why bother

The payoff is that **the compiler enforces it**. `Application` has no reference to `Infrastructure`, so a service physically *cannot* see `ApplicationDbContext`. Writing `_context.Customers` in a service isn't discouraged by code review — it doesn't compile.

That's what makes the tests in `CleanArchitecture.Application.Tests` run with no database at all: services depend on an *interface* (`IUnitOfWork`), so a test hands them a Moq fake instead.

### The trick that makes it work

There's an obvious objection: the Application layer needs to load customers from a database, so surely it depends on the database?

The resolution is **dependency inversion**. The *interface* lives inside, the *implementation* lives outside:

```csharp
// Application/Interfaces/Repositories/ICustomerRepository.cs   <- the inner ring OWNS this
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

// Infrastructure/Repositories/CustomerRepository.cs             <- the outer ring IMPLEMENTS it
public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;   // EF Core lives out here
    ...
}
```

`CustomerService` asks for `ICustomerRepository`. At runtime the DI container hands it a `CustomerRepository`. The service never learns that EF Core exists.

The arrow of *dependency* (Infrastructure → Application) now points the opposite way from the arrow of *control* (a service call reaching the database at runtime). Inverting that is the whole technique.

### Clean vs Onion — same thing?

Near enough. Onion Architecture (Jeffrey Palermo, 2008) and Clean Architecture (Robert C. Martin, 2012) both put the domain at the centre with dependencies pointing inward. Clean generalises the idea and adds vocabulary; the layering rule is the same. This repo says "Clean Architecture" throughout — don't read a distinction into it.

---

## 2. The four layers

### Domain — the business, with nothing attached

Business objects and rules that hold true regardless of how data is stored or displayed. No NuGet dependencies beyond the BCL. **If you can't state a rule without mentioning HTTP, SQL, or JSON, it doesn't belong here.**

| Folder | Holds |
| --- | --- |
| `Common/` | [BaseEntity.cs](CleanArchitecture.Domain/Common/BaseEntity.cs) — `Id`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` |
| `Entities/` | [Product.cs](CleanArchitecture.Domain/Entities/Product.cs), [Customer.cs](CleanArchitecture.Domain/Entities/Customer.cs) |
| `Enums/` | `ProductStatus` |
| `Exceptions/` | [DomainException.cs](CleanArchitecture.Domain/Exceptions/DomainException.cs) and friends |

Entities here aren't just property bags — they carry the rules that protect their own state:

```csharp
// Domain/Entities/Product.cs
public void UpdateStock(int quantity)
{
    if (StockQuantity + quantity < 0)
        throw new InsufficientStockException();

    StockQuantity += quantity;
}
```

Because callers go through `UpdateStock`, nothing outside the entity can produce negative stock. That's an **invariant**: something always true about a valid `Product`.

### Application — what the system *does*

The use cases, plus the contracts the outer layers must satisfy. Reading the service method names here tells you everything the app can do.

| Folder | Holds |
| --- | --- |
| `Interfaces/Repositories/` | `ICustomerRepository`, `IUnitOfWork` — persistence *contracts*, no implementation |
| `Interfaces/Services/` | `ICustomerService`, `IProductService` |
| `Services/` | The use-case implementations |
| `DTOs/` | Input/output shapes, with validation attributes |
| `Mappings/` | Mapster `IRegister` classes |
| `Collections/` | `PagedResult<T>` |
| `Exceptions/` | `EntityNotFoundException`, `DuplicateEntityException`, `BusinessRuleViolationException` |
| `Enums/` | Sort keys and sort order |

### Infrastructure — the technology

Everything specific to a technology choice: EF Core, SQL Server, the concrete repositories. Swapping databases means rewriting this project and nothing else.

| Folder | Holds |
| --- | --- |
| `Data/` | `ApplicationDbContext` |
| `Data/Configurations/` | One `IEntityTypeConfiguration<T>` per entity, including `HasData` seeds |
| `Repositories/` | EF implementations + `UnitOfWork` |
| `Collections/` | `PaginatedList<T>.CreateAsync` — the only place an `IQueryable` is executed |

Entity configuration is never inline in `OnModelCreating`; the context scans for it:

```csharp
// Infrastructure/Data/ApplicationDbContext.cs
modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
```

So a new `XxxConfiguration` file is picked up with no registration step.

### Web — HTTP in, HTML or JSON out

Controllers stay thin: bind input, call **one** service method, pick a result. No business logic.

| Folder | Holds |
| --- | --- |
| `Controllers/` | Home, Products, Customers |
| `ViewModels/` | List/paging/dropdown shapes for views |
| `Views/` | Razor |
| `Middleware/` | `GlobalExceptionMiddleware` |
| `Styles/` | SCSS compiled to `wwwroot/css/app.css` |

### How it's all wired

Each layer registers its own services, and `Program.cs` calls both:

```csharp
// Web/Program.cs
builder.Services.AddInfrastructure(builder.Configuration);   // DbContext, repositories, UnitOfWork
builder.Services.AddApplication();                           // Mapster + services
```

Everything is registered `Scoped` — **one `DbContext`, one `UnitOfWork`, one set of repositories per HTTP request**. That's what makes the Unit of Work pattern work: every repository in a request shares one EF change tracker, so a single `SaveChangesAsync` commits all of them together.

New services go in `Application/DependencyInjection/ServiceCollectionExtensions.cs`; new repositories in the Infrastructure one.

### Rules that apply everywhere

- **Every** repository and service method ends with `CancellationToken cancellationToken = default` and forwards it.
- **Repositories never call `SaveChanges`.** The service does, after mutating through the repository. ([Why](#why-the-repository-never-saves).)
- **`IQueryable` never leaves Infrastructure.** The boundary type is `IPaginatedList<T>`, which the service converts to `PagedResult<TDto>`.
- **Mapping is configured only** in `Application/Mappings` `IRegister` classes.

### Error handling

`Domain/Exceptions` defines abstract bases; `Application/Exceptions` derives concrete ones. [GlobalExceptionMiddleware](CleanArchitecture.Web/Middleware/GlobalExceptionMiddleware.cs) turns them into status codes — 404 / 403 / 422 / 409, anything else 500 — returning JSON for AJAX requests and a toast-plus-redirect otherwise.

Two things that will confuse you if nobody says them out loud:

- **The middleware only runs outside Development.** In Development you get `UseDeveloperExceptionPage` instead, so an exception you expect to become a clean 404 shows a stack trace while you're debugging. That's intended.
- **The two services aren't consistent yet.** `CustomerService` throws the good `EntityNotFoundException`; `ProductService` still throws plain `KeyNotFoundException`, and `ProductsController` catches that explicitly. **Use the domain/application exceptions in new code**, and make sure a controller catches what its service actually throws.

---

## 3. Following one request through the app

Concrete beats abstract. Here's `GET /Customers?searchTerm=north&page=2`.

**1. The controller binds and delegates.** `CustomerFilterDto` is populated from the query string by MVC:

```csharp
// Web/Controllers/CustomersController.cs
public async Task<IActionResult> Index([FromQuery] CustomerFilterDto filter, CancellationToken cancellationToken)
{
    var customers = await _customerService.GetPagedAsync(filter, cancellationToken);
    var viewModel = new CustomersViewModel { Customers = customers, Filter = filter };
    return IsAjaxRequest() ? PartialView("_CustomerTable", viewModel) : View(viewModel);
}
```

**2. The service delegates the query, then converts to DTOs:**

```csharp
// Application/Services/CustomerService.cs
var pagedCustomers = await _unitOfWork.Customers.GetPagedAsync(filter, cancellationToken);
return pagedCustomers.ToPagedResult(dto => dto.Adapt<CustomerDto>());
```

**3. The repository builds the query** — every filter composes into one `IQueryable`, which becomes one SQL statement:

```csharp
// Infrastructure/Repositories/CustomerRepository.cs
var query = _context.Customers.AsQueryable();

if (!string.IsNullOrWhiteSpace(filter.SearchTerm)) { query = query.Where(...); }
if (filter.IsActive.HasValue)                      { query = query.Where(...); }

return await PaginatedList<Customer>.CreateAsync(query, filter.Page, filter.PageSize, cancellationToken);
```

Summarised:

```
Controller ──CustomerFilterDto──> Service ──filter──> Repository ──SQL──> DB
Controller <──PagedResult<Dto>─── Service <─entities─ Repository <───────
     │
     └─> CustomersViewModel ─> Razor
```

Note where types change: **entities never travel above the service**, and **DTOs never reach the `DbContext`**.

A write, `POST /Customers/Create`, adds the validate-and-save steps:

```
Controller: ModelState check on CreateCustomerDto      (is the request well-formed?)
  -> CustomerService.CreateAsync(dto)
       - repository uniqueness check                    (business rule)
       - dto.Adapt<Customer>()                          (DTO -> entity)
       - customer.ValidateBusinessRules()               (domain invariants)
       - Customers.AddAsync(customer)                   (staged only — no SaveChanges here)
       - _unitOfWork.SaveChangesAsync()                 (the service commits)
  <- CustomerDto
Controller: Json(...)  /  catch DuplicateEntityException -> 409
```

---

## 4. Implementing a feature end to end (`Supplier`)

Adding a `Supplier` entity with a paged list and CRUD. The order matters — each step compiles on top of the last, working from the centre outward.

> **The mental model:** start with the business object, then say what you need from storage, then write the use case, *then* implement storage, then expose it over HTTP.

Snippets below show only what's interesting; `...` marks routine members that follow the same shape as their `Customer` equivalents. Copy from [Customer.cs](CleanArchitecture.Domain/Entities/Customer.cs), [CustomerRepository.cs](CleanArchitecture.Infrastructure/Repositories/CustomerRepository.cs) and [CustomerService.cs](CleanArchitecture.Application/Services/CustomerService.cs) when you need the full versions.

### Step 1 — Domain entity

**Create** `Domain/Entities/Supplier.cs`. Inherit `BaseEntity` (gives you `Id`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`) and put the rules that protect the object's own state in methods:

```csharp
public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? Country { get; set; }
    public int LeadTimeDays { get; set; }
    public bool IsPreferred { get; set; }

    // An invariant: true of any Supplier, in any app, forever.
    public void MarkPreferred()
    {
        if (LeadTimeDays > 30)
            throw new SupplierLeadTimeTooLongException();

        IsPreferred = true;
    }

    public void ValidateBusinessRules()
    {
        if (!HasValidName())
            throw new ValidationDomaineException("Supplier name is required and cannot exceed 200 characters", "Name");
        ...
    }

    // HasValidName(), HasValidEmail(), HasValidLeadTime() ...
}
```

**Create** `Domain/Exceptions/SupplierExceptions.cs` — one class per rule, deriving from `DomainException`.

### Step 2 — Say what you need from storage

A **contract**, not an implementation. Ask only for what the use cases need.

**Create** `Application/Interfaces/Repositories/ISupplierRepository.cs`:

```csharp
public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IPaginatedList<Supplier>> GetPagedAsync(SupplierFilterDto filter, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
    // GetAllAsync, UpdateAsync, DeleteAsync, ExistsAsync ...
}
```

Two things to copy: **every method ends with a `CancellationToken`**, and paging returns `IPaginatedList<Supplier>` — never `IQueryable`, so nobody outside can keep building on the query.

**Edit** `IUnitOfWork.cs` to expose it:

```csharp
ISupplierRepository Suppliers { get; }
```

### Step 3 — DTOs and a sort enum

Three DTOs, each with a distinct job. **Create** them in `Application/DTOs/`:

```csharp
// SupplierDto — what the app hands OUT. Includes server-owned fields.
public class SupplierDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    // Name, ContactEmail, Country, LeadTimeDays, IsPreferred, UpdatedAt ...
}

// CreateSupplierDto — what the app accepts IN, for both create and update.
// No Id, no CreatedAt: the server owns those, so a crafted POST can't set them.
public class CreateSupplierDto
{
    [Required(ErrorMessage = "Supplier name is required")]
    [StringLength(200, ErrorMessage = "Supplier name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 365, ErrorMessage = "Lead time must be between 0 and 365 days")]
    [Display(Name = "Lead time (days)")]
    public int LeadTimeDays { get; set; }
    // ContactEmail, Country, IsPreferred ...
}

// SupplierFilterDto — the query string for the list screen.
// Inheriting PagedFilterDto gives you Page/PageSize already clamped
// (page >= 1, size 1-100), so ?pageSize=100000 can't hurt you.
public class SupplierFilterDto : PagedFilterDto
{
    public string? SearchTerm { get; set; }
    public bool? IsPreferred { get; set; }
    public SupplierSortBy SortBy { get; set; } = SupplierSortBy.Name;
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

    public bool HasActiveFilters => !string.IsNullOrWhiteSpace(SearchTerm) || IsPreferred.HasValue;
    // Country ...
}
```

Also **create** `Application/Enums/SupplierSortBy.cs`:

```csharp
public enum SupplierSortBy { Name, Country, LeadTime, CreatedDate }
```

### Step 4 — Mapping

Mapster's `.Adapt<T>()` copies same-named properties automatically; you configure only what it can't guess. **Create** `Application/Mappings/SupplierMappings.cs` — the assembly scan finds it, so there's nothing to register:

```csharp
public class SupplierMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        TypeAdapterConfig<Supplier, SupplierDto>.NewConfig();

        // Also the update mapping (SupplierService.UpdateAsync adapts onto the tracked
        // entity), so null must overwrite or cleared fields would keep their old value.
        TypeAdapterConfig<CreateSupplierDto, Supplier>.NewConfig()
            .IgnoreNullValues(false);
    }
}
```

**`IgnoreNullValues(false)` is not optional.** MVC binds an empty text input to `null`. If nulls were ignored, a user clearing the Country field would submit `null`, Mapster would skip it, and the old country would silently survive. ([More.](#scenario-e--the-update-mapping-trap))

### Step 5 — The use case

**Create** `ISupplierService.cs` (`GetByIdAsync`, `GetPagedAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` — all DTOs in, DTOs out) and `Application/Services/SupplierService.cs`.

The service takes **one** dependency. Every repository hangs off it and they share a change tracker, so one `SaveChangesAsync` commits them together:

```csharp
public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
```

Every write method follows the same shape — **check rules → mutate → save → return a DTO**:

```csharp
    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Suppliers.ExistsByEmailAsync(dto.ContactEmail, null, cancellationToken))
            throw new DuplicateEntityException("Supplier", "contact email", dto.ContactEmail);

        var supplier = dto.Adapt<Supplier>();
        supplier.ValidateBusinessRules();
        supplier.CreatedAt = DateTime.UtcNow;          // server-controlled, never from the client

        await _unitOfWork.Suppliers.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return supplier.Adapt<SupplierDto>();
    }
```

Update differs in one way worth noting — `Adapt` copies onto the **EF-tracked** entity, which is what makes Step 4's `IgnoreNullValues(false)` matter:

```csharp
    public async Task<SupplierDto> UpdateAsync(int id, CreateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Supplier", id);

        dto.Adapt(supplier);                           // onto the tracked entity
        supplier.ValidateBusinessRules();
        supplier.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Suppliers.UpdateAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return supplier.Adapt<SupplierDto>();
    }

    // GetByIdAsync, GetPagedAsync, DeleteAsync — same shape ...
}
```

Reads are one-liners; `GetPagedAsync` delegates and maps:

```csharp
var paged = await _unitOfWork.Suppliers.GetPagedAsync(filter, cancellationToken);
return paged.ToPagedResult(s => s.Adapt<SupplierDto>());
```

**Edit** `Application/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddScoped<ISupplierService, SupplierService>();
```

At this point the whole feature exists and is testable — with no database and no UI.

### Step 6 — Implement storage

**Create** `Infrastructure/Data/Configurations/SupplierConfiguration.cs` — column constraints plus a `HasData` seed. Found automatically by `ApplyConfigurationsFromAssembly`:

```csharp
public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.ContactEmail).IsRequired().HasMaxLength(256);
        // Country, CreatedAt, CreatedBy, UpdatedBy ...

        // The service's ExistsByEmailAsync is a check-then-act that loses a race between
        // concurrent creates; only the database can actually enforce this.
        builder.HasIndex(e => e.ContactEmail).IsUnique();

        builder.HasData(
            new Supplier { Id = 1, Name = "Baltic Components", ContactEmail = "sales@baltic.example", LeadTimeDays = 12, IsPreferred = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
            new Supplier { Id = 2, Name = "Meridian Textiles", ContactEmail = "hello@meridian.example", LeadTimeDays = 45, CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
        );
    }
}
```

**Edit** `ApplicationDbContext.cs`: `public DbSet<Supplier> Suppliers { get; set; }`

**Create** `Infrastructure/Repositories/SupplierRepository.cs`. Most methods are one-liners over `_context.Suppliers`; the only one with real content is `GetPagedAsync`, and the thing to notice is where the query executes:

```csharp
public async Task<IPaginatedList<Supplier>> GetPagedAsync(SupplierFilterDto filter, CancellationToken cancellationToken = default)
{
    // Nothing has hit the database yet — each step just builds up the query.
    var query = _context.Suppliers.AsQueryable();

    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
    {
        var term = filter.SearchTerm;
        query = query.Where(s => s.Name.Contains(term) || s.ContactEmail.Contains(term));
    }

    if (filter.IsPreferred.HasValue)
        query = query.Where(s => s.IsPreferred == filter.IsPreferred.Value);
    // Country filter ...

    var ascending = filter.SortOrder == SortOrder.Ascending;
    query = filter.SortBy switch
    {
        SupplierSortBy.LeadTime => ascending
            ? query.OrderBy(s => s.LeadTimeDays)
            : query.OrderByDescending(s => s.LeadTimeDays),
        // Country, CreatedDate ...
        _ => ascending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name)
    };

    // ...and here it finally runs: one COUNT + one paged SELECT.
    return await PaginatedList<Supplier>.CreateAsync(query, filter.Page, filter.PageSize, cancellationToken);
}
```

The rest delegate straight to EF, and **none of them call `SaveChangesAsync`** — `AddAsync` only *stages* the row:

```csharp
public async Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    => await _context.Suppliers.AddAsync(supplier, cancellationToken);

public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    => await _context.Suppliers.AnyAsync(
        s => s.ContactEmail == email && (!excludeId.HasValue || s.Id != excludeId.Value), cancellationToken);

// GetByIdAsync, GetAllAsync, UpdateAsync, DeleteAsync, ExistsAsync ...
```

**Edit** `UnitOfWork.cs` — take `ISupplierRepository` in the constructor, assign it, expose `public ISupplierRepository Suppliers { get; }`.

**Edit** `Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddScoped<ISupplierRepository, SupplierRepository>();
```

### Step 7 — Expose it over HTTP

**Create** `Web/ViewModels/SuppliersViewModel.cs`. It *wraps* the DTO page rather than copying its fields, and adds what only a screen cares about:

```csharp
public class SuppliersViewModel
{
    public required PagedResult<SupplierDto> Suppliers { get; init; }
    public required SupplierFilterDto Filter { get; init; }

    // "showing 11-20 of 47" arithmetic
    public int StartItem => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);

    // Bootstrap Icons class for the sort header
    public string GetSortIcon(SupplierSortBy sortField)
    {
        if (Filter.SortBy != sortField) return "bi-arrow-down-up text-muted";
        return Filter.SortOrder == SortOrder.Ascending ? "bi-arrow-up" : "bi-arrow-down";
    }

    // CurrentPage/PageSize/TotalItems/TotalPages passthroughs, HasResults,
    // NoResultsMessage, GetSortRouteValues, GetPageRouteValues ...
}
```

Copy the passthroughs and route-value builders from [CustomersViewModel](CleanArchitecture.Web/ViewModels/CustomersViewModel.cs) — they're identical bar the type.

**Create** `Web/Controllers/SuppliersController.cs`. Thin: bind, call **one** service method, pick a result. The only real content is a `catch` per exception the service can throw:

```csharp
public async Task<IActionResult> Index([FromQuery] SupplierFilterDto filter, CancellationToken cancellationToken)
{
    var suppliers = await _supplierService.GetPagedAsync(filter, cancellationToken);
    return View(new SuppliersViewModel { Suppliers = suppliers, Filter = filter });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateSupplierDto dto, CancellationToken cancellationToken)
{
    if (!ModelState.IsValid)
        return BadRequest(new { errors = ModelStateErrors() });

    try
    {
        var supplier = await _supplierService.CreateAsync(dto, cancellationToken);
        return Json(new { success = true, message = "Supplier created successfully!", supplier });
    }
    catch (DuplicateEntityException ex)
    {
        return Conflict(new { errors = new Dictionary<string, string> { ["ContactEmail"] = ex.Message } });
    }
    catch (ValidationDomaineException ex)
    {
        return UnprocessableEntity(new { errors = new Dictionary<string, string> { [ex.FieldName] = ex.Message } });
    }
    // catch (Exception) -> log + 500 ...
}
```

Note there is **no business logic here** — the rules all live behind that one service call.

**Create** `Web/Views/Suppliers/Index.cshtml`, using this project's own component classes (see [§6](#6-visual-customization)) rather than `badge bg-*`:

```cshtml
@model CleanArchitecture.Web.ViewModels.SuppliersViewModel

<div class="page-head">
    <h1>Suppliers</h1>
    <span class="page-count">@Model.TotalItems supplier(s)</span>
</div>

<div class="panel">
    <table class="table align-middle mb-0">
        <thead>
            <tr>
                <th>
                    <a asp-action="Index" asp-all-route-data="@Model.GetSortRouteValues(SupplierSortBy.Name)">
                        Name <i class="bi @Model.GetSortIcon(SupplierSortBy.Name)"></i>
                    </a>
                </th>
                <th>Lead time</th>
                <th>Status</th>
            </tr>
        </thead>
        <tbody>
        @foreach (var supplier in Model.Suppliers)
        {
            <tr>
                <td>@supplier.Name</td>
                <td>@supplier.LeadTimeDays days</td>
                <td>
                    <span class="status @(supplier.IsPreferred ? "status-positive" : "status-neutral")">
                        @(supplier.IsPreferred ? "Preferred" : "Standard")
                    </span>
                </td>
            </tr>
        }
        </tbody>
    </table>
</div>
```

**Edit** `Views/Shared/_Layout.cshtml` to add the nav item.

### Step 8 — Tests

Here's the payoff for the interface work: the service is tested with **no database**, because `IUnitOfWork` is an interface Moq can fake.

`Xunit`, `Moq`, `FluentAssertions` and `AutoFixture` are global usings, and `MappingSetup` already ran `MappingConfig.Configure()` via `[ModuleInitializer]`, so `.Adapt<T>()` uses the real mappings.

**Create** `Application.Tests/Services/SupplierServiceTests.cs`:

```csharp
public class SupplierServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISupplierRepository> _suppliers = new();
    private readonly SupplierService _sut;          // sut = "system under test"

    public SupplierServiceTests()
    {
        _unitOfWork.Setup(u => u.Suppliers).Returns(_suppliers.Object);
        _sut = new SupplierService(_unitOfWork.Object);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ShouldThrow()
    {
        var dto = new CreateSupplierDto { Name = "Dup", ContactEmail = "dup@x.example" };

        _suppliers
            .Setup(r => r.ExistsByEmailAsync(dto.ContactEmail, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<DuplicateEntityException>();

        // The rule held: nothing was written.
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // CreateAsync_ShouldPersistAndReturnDto, UpdateAsync_WhenMissing_ShouldThrow ...
}
```

### Step 9 — Run it

```bash
dotnet build
dotnet test --filter "FullyQualifiedName~SupplierServiceTests"
dotnet run --project CleanArchitecture.Web
```

**No migration step.** `Program.cs` calls `EnsureDeleted()` then `EnsureCreated()` on every start, so your table and seeds appear on the next run — and anything typed into the UI is gone on restart. That's this demo's choice, not a Clean Architecture thing.

### Checklist

| # | File | Action |
| --- | --- | --- |
| 1 | `Domain/Entities/Supplier.cs` | create |
| 1 | `Domain/Exceptions/SupplierExceptions.cs` | create |
| 2 | `Application/Interfaces/Repositories/ISupplierRepository.cs` | create |
| 2 | `Application/Interfaces/Repositories/IUnitOfWork.cs` | edit |
| 3 | `Application/Enums/SupplierSortBy.cs` | create |
| 3 | `Application/DTOs/SupplierDto.cs`, `CreateSupplierDto.cs`, `SupplierFilterDto.cs` | create |
| 4 | `Application/Mappings/SupplierMappings.cs` | create |
| 5 | `Application/Interfaces/Services/ISupplierService.cs` | create |
| 5 | `Application/Services/SupplierService.cs` | create |
| 5 | `Application/DependencyInjection/ServiceCollectionExtensions.cs` | edit |
| 6 | `Infrastructure/Data/Configurations/SupplierConfiguration.cs` | create |
| 6 | `Infrastructure/Data/ApplicationDbContext.cs` | edit |
| 6 | `Infrastructure/Repositories/SupplierRepository.cs` | create |
| 6 | `Infrastructure/Repositories/UnitOfWork.cs` | edit |
| 6 | `Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` | edit |
| 7 | `Web/ViewModels/SuppliersViewModel.cs` | create |
| 7 | `Web/Controllers/SuppliersController.cs` | create |
| 7 | `Web/Views/Suppliers/Index.cshtml` | create |
| 7 | `Web/Views/Shared/_Layout.cshtml` | edit |
| 8 | `Application.Tests/Services/SupplierServiceTests.cs` | create |

---

## 5. Key concepts

Three distinctions that account for most "wait, which one do I use?" moments.

### 5.1 Repository vs Service

Both sit between the controller and the database, which is exactly why they get confused.

| | Repository | Service |
| --- | --- | --- |
| Answers | "How do I get/store this data?" | "What should happen when the user does X?" |
| Lives in | `Infrastructure/Repositories` (interface in Application) | `Application/Services` |
| Knows about | EF Core, `IQueryable`, `DbSet` | Other repositories, business rules |
| Speaks in | **Entities** | **DTOs** in and out; entities internally |
| Calls `SaveChanges` | **Never** | Yes, via `IUnitOfWork` |

> **Rule of thumb**
> A sentence about *data* — "suppliers ordered by lead time, page 2" — is a repository.
> A sentence about the *business* — "a customer who owns products can't be deleted" — is a service.

#### A rule that belongs to the service

Deleting a customer touches two repositories and enforces a rule neither one owns:

```csharp
// Application/Services/CustomerService.cs
public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
{
    if (!await _unitOfWork.Customers.ExistsAsync(id, cancellationToken))
    {
        throw new EntityNotFoundException("Customer", id);
    }

    // The Product -> Customer foreign key is configured with DeleteBehavior.Restrict,
    // so surface the rule here instead of letting EF raise a raw constraint violation.
    if (await _unitOfWork.Products.ExistsForCustomerAsync(id, cancellationToken))
    {
        throw new BusinessRuleViolationException(
            "This customer still owns products and cannot be deleted. Reassign or delete the products first.");
    }

    await _unitOfWork.Customers.DeleteAsync(id, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

`CustomerRepository` couldn't express this — it has no business asking about products. The service can, because `IUnitOfWork` gives it both.

#### Query shaping that belongs to the repository

Filtering and sorting are data concerns, and they must reach SQL. See `CustomerRepository.GetPagedAsync` in [§3](#3-following-one-request-through-the-app) — every `Where` composes into one statement, and the service just maps the result.

**The anti-pattern this prevents:** if filtering leaked into the service, it would have to load everything first.

```csharp
// ✗ Never — loads the whole table, then filters in memory
var all = await _unitOfWork.Customers.GetAllAsync(ct);
var page = all.Where(c => c.Name.Contains(term)).Skip(...).Take(size);
```

This isn't hypothetical: [ProductService.ValidateAndPrepareProductAsync](CleanArchitecture.Application/Services/ProductService.cs) does exactly this to check a duplicate name. Prefer an `ExistsByEmailAsync`-style repository method that pushes the predicate into SQL.

#### Why the repository never saves

`AddAsync` only *stages* the entity in EF's change tracker. The service decides when to commit:

```csharp
await _unitOfWork.Suppliers.AddAsync(supplier, cancellationToken);
await _unitOfWork.Products.AddAsync(product, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);   // one transaction, both rows
```

If `AddAsync` saved on its own, those would be two independent writes and a failure between them would leave the database half-updated. For work spanning multiple saves, `IUnitOfWork` also exposes `BeginTransactionAsync` / `CommitTransactionAsync` / `RollbackTransactionAsync`.

#### So where does domain logic go?

A third place — the entity. The dividing line is **how many objects the rule needs**:

```csharp
product.UpdateStock(-5);                                       // Domain: only this Product
if (await _unitOfWork.Products.ExistsForCustomerAsync(id, ct)) // Service: a second repository
await _context.Customers.AnyAsync(c => c.Email == email, ct);  // Repository: needs SQL
```

`ProductService.UpdateStockAsync` shows the handoff: it loads the entity, calls `product.UpdateStock(quantity)`, stamps `UpdatedAt`, saves. The service never re-implements the invariant.

---

### 5.2 Entity vs DTO

An **entity** is a business object mapped to a table, owned by Domain. A **DTO** (Data Transfer Object) is a message crossing a boundary, owned by Application.

| | Entity (`Customer`) | DTO (`CustomerDto`) |
| --- | --- | --- |
| Project | Domain | Application |
| Purpose | Model the business, hold invariants | Carry data across a boundary |
| Behaviour | Methods (`Activate()`, `ValidateBusinessRules()`) | None — properties only |
| Navigation | `Product.Customer` object reference | Flattened (`CustomerName` string) |
| Lifetime | Tracked by EF | Detached, serializable |

#### Scenario A — returning an entity to a view leaks the object graph

```csharp
// ✗ If a controller returned the entity directly
public async Task<IActionResult> Details(int id) => View(await _repo.GetByIdAsync(id));
```

Every problem here is real:

- `Product.Customer` is a navigation property — serializing pulls the whole customer (email, phone, notes) or cycles on the back-reference.
- The view can now write to the entity; `product.Price = -1` compiles.
- Rename `Product.StockQuantity` in the domain and every Razor view breaks.
- `CreatedBy`/`UpdatedBy` reach the browser whether you meant them to or not.

The DTO is the flat projection instead, and mapping can turn *behaviour* into a *value*:

```csharp
// Application/Mappings/ProductMappings.cs
TypeAdapterConfig<Product, ProductDto>.NewConfig()
    .Map(dest => dest.IsInStock, src => src.IsInStock())              // method -> bindable value
    .Map(dest => dest.CustomerName, src => src.Customer != null ? src.Customer.Name : string.Empty);
```

#### Scenario B — accepting an entity from a form is a security hole

If the action were `Create(Product product)`, a crafted POST could set `Id`, `CreatedAt`, `CreatedBy` or `Status` — fields no form renders. This is **mass assignment**. `CreateProductDto` simply has no such properties, so there's nothing to bind; the service sets them:

```csharp
var product = createProductDto.Adapt<Product>();
product.CreatedAt = DateTime.UtcNow;      // server-controlled
```

#### Scenario C — two DTOs for one entity

`CustomerDto` (out) and `CreateCustomerDto` (in) differ exactly where it matters:

```csharp
public class CustomerDto        // output: includes server-owned fields
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    ...
}

public class CreateCustomerDto  // input: no Id, no CreatedAt, no UpdatedAt
{
    public bool IsActive { get; set; } = true;   // sensible default for a new customer
    ...
}
```

Input DTOs are deliberately smaller than output DTOs.

#### Scenario D — validation in both places isn't duplication

The two answer different questions:

```csharp
// CreateCustomerDto — "is this request well-formed?"  (per field, with a UI message)
[Required(ErrorMessage = "Email is required")]
[EmailAddress(ErrorMessage = "Enter a valid email address")]
public string Email { get; set; } = string.Empty;

// Customer — "is this a legal Customer?"  (true for any caller, forever)
public void ValidateBusinessRules()
{
    if (!HasValidEmail())
        throw new InvalidCustomerEmailException();
    ...
}
```

The DTO's attributes drive client-side validation and `ModelState`. The entity's rules hold even for a caller that never touched MVC — a background job, an importer, a test. `CustomerService.CreateAsync` runs both.

#### Scenario E — the update mapping trap

`CreateCustomerDto -> Customer` is registered once but used twice: to build a new entity, and to apply an edit onto a **tracked** one.

```csharp
var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);  // tracked by EF
updateCustomerDto.Adapt(customer);                                               // copy onto it
```

Which is why the registration must keep:

```csharp
TypeAdapterConfig<CreateCustomerDto, Customer>.NewConfig()
    .IgnoreNullValues(false);
```

MVC binds an empty input to `null`. With nulls ignored, a user clearing "Company" submits `null`, Mapster skips it, and the old value silently survives the save. **Deleting that line is a data-corruption bug.**

#### Where conversion happens

Always in the service:

```
Controller  ──CreateCustomerDto──>  Service  ──Customer──>  Repository  ──> DB
Controller  <───CustomerDto───────  Service  <──Customer──  Repository  <──
```

An entity never appears in a controller signature; a DTO never reaches the `DbContext`.

---

### 5.3 DTO vs ViewModel

A DTO is what the **application layer** exchanges. A ViewModel is what **one screen** needs.

| | DTO | ViewModel |
| --- | --- | --- |
| Project | Application | Web |
| Audience | Any caller — MVC, an API, a job, a test | One view or partial |
| Knows about | Nothing UI-specific | `SelectList`, CSS classes, route values |
| Reusable | Across delivery mechanisms | Not at all, by design |

A ViewModel may reference DTOs; a DTO must never reference a ViewModel — `Application` can't see `Web`.

#### Composition, not duplication

A ViewModel *holds* the DTO rather than copying its fields, so adding a property to the DTO shows up in the view for free:

```csharp
public class CustomersViewModel
{
    public required PagedResult<CustomerDto> Customers { get; init; }
    public required CustomerFilterDto Filter { get; init; }
    ...
}
```

#### What earns a place in a ViewModel

**1. Presentation arithmetic** — the "showing 11–20 of 47" line:

```csharp
public int StartItem => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
```

**2. UI vocabulary** — icon class names, route dictionaries:

```csharp
public string GetSortIcon(CustomerSortBy sortField)
{
    if (Filter.SortBy != sortField) return "bi-arrow-down-up text-muted";
    return Filter.SortOrder == SortOrder.Ascending ? "bi-arrow-up" : "bi-arrow-down";
}
```

**3. Widget data** — `SelectList` is an MVC type and can only live in Web:

```csharp
public SelectList? AvailableCategories { get; set; }
```

#### Why this isn't over-engineering

Put `GetSortIcon` on `CustomerDto` and the Application layer now depends on Bootstrap Icons' class names. A future REST API returning that DTO would ship `"bi-arrow-up"` in its JSON. Keeping it in Web lets the API and the MVC screen share a DTO and disagree only about presentation.

#### The forms exception

This project deliberately does **not** wrap create/edit forms in a ViewModel — they bind straight to the DTO:

```csharp
public async Task<IActionResult> Create(CreateCustomerDto createCustomerDto, CancellationToken cancellationToken)
```

The DTO already carries the validation attributes that drive both client-side and `ModelState` checks, so a wrapper would just forward every property. The convention: **`ViewModels/` hold list and paging shapes only.**

> ⚠️ [ProductViewModel](CleanArchitecture.Web/ViewModels/ProductViewModel.cs) is a full form-ViewModel with `SelectList`s, `FormattedPrice`, breadcrumbs and so on. It shows what the pattern looks like when a form needs real UI state — but it isn't what the Product views bind to today, and its `StatusBadgeClass` still emits the retired `badge bg-*` classes. Treat it as an illustration, not the house style.

#### Choosing

```
Contains SelectList, CSS classes, route values, page-label arithmetic?
  -> ViewModel (Web)

Input or output of a use case, usable by an API or a job as much as a view?
  -> DTO (Application)

A create/edit form whose fields are exactly the DTO's?
  -> bind the DTO directly, no ViewModel
```

---

## 6. Visual customization

### The pipeline

Custom CSS is written as SCSS and compiled on every build — you never edit `wwwroot/css/app.css` by hand:

```
CleanArchitecture.Web/Styles/app.scss
        │   @use base/*  ->  components/*  ->  utilities/*
        │   AspNetCore.SassCompiler (runs on every `dotnet build`)
        v
CleanArchitecture.Web/wwwroot/css/app.css   <- generated; never edit
        v
   _Layout.cshtml
```

Config lives in [sasscompiler.json](CleanArchitecture.Web/sasscompiler.json): Release output is minified, Debug is expanded and readable. `dotnet watch run` recompiles on save.

**Only `app.scss` compiles.** Partials are `_`-prefixed and pulled in by it, in dependency order:

```scss
@use "base/theme";        // tokens first — everything below depends on them
@use "base/typography";
@use "base/layout";

@use "components/navbar";
@use "components/buttons";
@use "components/forms";
@use "components/tables";
@use "components/badges";
@use "components/pagination";
@use "components/modals";
@use "components/toasts";

@use "utilities/motion";   // last
```

A new partial must be `_`-prefixed **and** listed here, or it's silently ignored.

| Path | Owns |
| --- | --- |
| `Styles/base/_theme.scss` | The colour palette, light and dark |
| `Styles/base/_layout.scss` | `body`, `.container`, `.panel`, `.page-head` |
| `Styles/components/_buttons.scss` | `.btn-primary`, `.btn-quiet`, `.btn-danger`, `.btn-icon` |
| `Styles/components/_badges.scss` | `.status`, `.status-*`, `.tag` |
| `Styles/utilities/_motion.scss` | All transitions, disabled under `prefers-reduced-motion` |

### How theming works

Bootstrap 5.3 is **never forked**. The stock `bootstrap.min.css` loads as shipped, and `_theme.scss` retunes it through CSS custom properties. Two blocks: `:root` for light, `[data-bs-theme="dark"]` for dark.

The file defines an `--app-*` palette, then **remaps Bootstrap's own `--bs-*` variables onto it**:

```scss
:root {
  --app-page:         #F5F6F6;
  --app-panel:        #FFFFFF;
  --app-hairline:     #E3E6E6;
  --app-ink:          #15191A;
  --app-muted:        #5A6467;

  --app-accent:       #B5451F;
  --app-accent-hover: #94360F;
  --app-on-accent:    #FFFFFF;

  --app-positive:     #1F6B45;
  --app-danger:       #A32A28;
  ...

  // Remapped onto the tokens above so stock components need no overrides.
  --bs-body-bg:      var(--app-page);
  --bs-body-color:   var(--app-ink);
  --bs-border-color: var(--app-hairline);
  --bs-primary:      var(--app-accent);
  --bs-primary-rgb:  181, 69, 31;
  --bs-link-color:   var(--app-accent);
  ...
}
```

That remap is why the app looks coherent with no per-component overrides — a stock `.card` or `.alert` reads `--bs-body-bg` and lands on the right colour automatically.

⚠️ **`--bs-primary-rgb` must be updated by hand** whenever `--app-accent` changes. Bootstrap composes it into `rgba()` for focus rings and translucent fills, and a `var()` won't work there.

> **The rule:** reach for an existing `--app-*` token or a `--bs-*` variable. A literal hex won't follow the theme.

### Theme switching

Three states — `auto` (follow the OS), `light`, `dark` — persisted in `localStorage["theme"]`. An inline script in `_Layout.cshtml`'s `<head>`, before any stylesheet, stamps `data-bs-theme` on `<html>` so there's no flash of the wrong theme. `wwwroot/js/theme.js` owns the toggle button.

### Component conventions

| Do | Don't |
| --- | --- |
| `.status status-positive\|caution\|danger\|neutral` for state | `badge bg-success` |
| `.tag` for classification chips | `badge bg-secondary` |
| `.panel` for boxed surfaces | shadows on non-overlays |
| `.btn-primary` / `.btn-quiet` / `.btn-danger` | `btn-success`, `btn-outline-*` |
| `.btn-icon` in `.row-actions`, with `title` + `aria-label` | icon buttons with no accessible name |

Status is a dot pill — the dot is generated, so the markup is just text:

```cshtml
<span class="status status-positive">Active</span>
```

Buttons are re-skinned by setting Bootstrap's per-button variables, never by overriding `background` — this keeps its disabled/active/focus states working:

```scss
.btn-quiet {
  --bs-btn-bg: var(--app-panel);
  --bs-btn-border-color: var(--app-hairline);
  --bs-btn-color: var(--app-ink);
  --bs-btn-hover-bg: var(--app-row-hover);
  ...
}
```

### Example 1 — recolour the whole app

Change the accent from terracotta to indigo. Edit **only** `Styles/base/_theme.scss`, both blocks:

```scss
:root {
  --app-accent:       #4338CA;
  --app-accent-hover: #3730A3;
  --app-accent-tint:  #EEF0FC;
  --app-on-accent:    #FFFFFF;

  --bs-primary-rgb:   67, 56, 202;   // must match --app-accent, by hand
}

[data-bs-theme="dark"] {
  --app-accent:       #A5B4FC;
  --app-accent-hover: #C7D2FE;
  --app-accent-tint:  #1E1B4B;
  --app-on-accent:    #10102A;

  --bs-primary-rgb:   165, 180, 252;
}
```

`dotnet build`, reload. Buttons, links, focus rings, active nav items, pagination and panel hover borders all move together in both themes, because every one resolves through `--app-accent`.

Note the dark accent is *lighter* than the light-mode one, and `--app-on-accent` flips to dark ink — a light-mode accent on a dark background fails contrast.

### Example 2 — add a status variant

Add an "Archived" state. **Edit** `_theme.scss` (both blocks):

```scss
:root {
  --app-archived:      #4C5A66;
  --app-archived-tint: #E9EDF0;
}

[data-bs-theme="dark"] {
  --app-archived:      #8FA3B0;
  --app-archived-tint: #1B2227;
}
```

**Edit** `components/_badges.scss`:

```scss
.status-archived { color: var(--app-archived); background: var(--app-archived-tint); }
```

Use it: `<span class="status status-archived">Archived</span>`. Two token declarations and one line of CSS, theme-correct everywhere.

### Example 3 — a new component partial

A `.metric` tile for a dashboard. **Create** `Styles/components/_metrics.scss`:

```scss
.metric {
  background: var(--app-panel);
  border: 1px solid var(--app-hairline);
  border-radius: .5rem;
  padding: 1rem 1.25rem;

  &__label {
    font-size: .8125rem;
    color: var(--app-muted);
  }

  &__value {
    font-size: 1.75rem;
    font-weight: 600;
    color: var(--app-ink);
    font-variant-numeric: tabular-nums;
  }

  &--accent &__value { color: var(--app-accent); }
}
```

**Edit** `app.scss` — components go after base, before utilities:

```scss
@use "components/toasts";
@use "components/metrics";   // <- added

@use "utilities/motion";     // stays last
```

Every colour is a token, so the tile is dark-mode-correct with no extra work.

### Two more things

**Motion** all lives in `utilities/_motion.scss` and is disabled under `prefers-reduced-motion`. Put new animation there, not in a component partial, so the guard keeps covering it.

**Toast markup exists twice** — [_Toast.cshtml](CleanArchitecture.Web/Views/Shared/_Toast.cshtml) (server-rendered from `TempData`) and `wwwroot/js/Helpers/toast.js` (client-built for AJAX). Both emit `toast--<variant>`; **change them together.**

### Troubleshooting

| Symptom | Cause |
| --- | --- |
| SCSS edit has no effect | Partial not `@use`d in `app.scss`, or missing its `_` prefix |
| Right in light, wrong in dark | Token declared only in `:root`; add it to `[data-bs-theme="dark"]` |
| Focus ring still the old accent | `--bs-primary-rgb` not updated to match `--app-accent` |
| A component ignores the theme | It uses a literal hex; replace with an `--app-*` / `--bs-*` variable |

---

## 7. Common mistakes

Things that trip up people new to this codebase.

**Calling `SaveChanges` in a repository.** Repositories stage; services commit. Otherwise two writes in one use case can't be one transaction. ([Why](#why-the-repository-never-saves))

**Returning `IQueryable` from a repository.** It looks flexible, but it makes the service silently depend on the EF provider, and the query then executes somewhere nobody expects. Return `IPaginatedList<T>` or a materialized collection.

**Loading everything, then filtering in memory.** `GetAllAsync()` followed by `.Where(...)` reads the whole table. Add a repository method that pushes the predicate into SQL.

**Putting an entity in a controller signature.** Mass-assignment risk on the way in, object-graph leak on the way out. Use DTOs at that boundary. ([Why](#52-entity-vs-dto))

**Removing `IgnoreNullValues(false)`.** Looks like tidying; silently stops users from clearing optional fields. ([Why](#scenario-e--the-update-mapping-trap))

**Forgetting the `CancellationToken`.** Every repository and service method takes one as its last parameter and forwards it. Skip it and a request that the user abandoned keeps working.

**Adding a project reference that points outward.** If `Application` needs something from `Infrastructure`, the fix is an interface in `Application` that `Infrastructure` implements — not a reference. ([Why](#the-trick-that-makes-it-work))

**Editing `wwwroot/css/app.css`.** It's generated; the next build overwrites you. Edit the SCSS under `Styles/`.

**Using `badge bg-success`.** This project replaced Bootstrap badges with `.status` pills and `.tag` chips. ([Why](#component-conventions))

**Expecting the exception middleware in Development.** It's registered only outside Development; you'll see the developer exception page instead.

---

## Where to look next

| To understand… | Read |
| --- | --- |
| A complete server-rendered CRUD flow | `ProductsController` + `Views/Products/` |
| The AJAX-modal CRUD variant | `CustomersController` + `Views/Customers/` |
| How services are tested without a DB | `Application.Tests/Services/CustomerServiceTests.cs` |
| The repo's conventions in brief | [CLAUDE.md](CLAUDE.md) |
| Setup, tech stack, seed data | [README.md](README.md) |
