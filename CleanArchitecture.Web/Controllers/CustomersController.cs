using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Interfaces.Services;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Web.Controllers;

// The AJAX counterpart to ProductsController: every mutating action answers with JSON that
// wwwroot/js/views/customers.js renders into the modal. Field-level failures are shaped as
// { errors: { Field: message } } so the client can attach them to the right input;
// everything else falls through to GlobalExceptionMiddleware, which returns
// { error: { message } } for XHR - the shape ajax.js already understands.
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // GET: Customers
    // Supports server-side paging, filtering and sorting via query string, e.g.
    public async Task<IActionResult> Index([FromQuery] CustomerFilterDto filter, CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetPagedAsync(filter, cancellationToken);

        var viewModel = new CustomersViewModel { Customers = customers, Filter = filter };

        return IsAjaxRequest() ? PartialView("_CustomerTable", viewModel) : View(viewModel);
    }

    // GET: Customers/GetDetails/5
    // Feeds the edit modal via AJAX.
    [HttpGet]
    public async Task<IActionResult> GetDetails(int id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            return NotFound(new { message = $"Customer with ID {id} was not found." });
        }

        return Json(customer);
    }

    // POST: Customers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerDto createCustomerDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = ModelStateErrors() });
        }

        try
        {
            var customer = await _customerService.CreateAsync(createCustomerDto, cancellationToken);

            return Json(new { success = true, message = "Customer created successfully!", customer });
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new { errors = new Dictionary<string, string> { ["Email"] = ex.Message } });
        }
        catch (ValidationDomaineException ex)
        {
            return UnprocessableEntity(new { errors = new Dictionary<string, string> { [ex.FieldName] = ex.Message } });
        }
    }

    // POST: Customers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateCustomerDto updateCustomerDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = ModelStateErrors() });
        }

        try
        {
            var customer = await _customerService.UpdateAsync(id, updateCustomerDto, cancellationToken);

            return Json(new { success = true, message = "Customer updated successfully!", customer });
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new { message = $"Customer with ID {id} was not found." });
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new { errors = new Dictionary<string, string> { ["Email"] = ex.Message } });
        }
        catch (ValidationDomaineException ex)
        {
            return UnprocessableEntity(new { errors = new Dictionary<string, string> { [ex.FieldName] = ex.Message } });
        }
    }

    // POST: Customers/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _customerService.DeleteAsync(id, cancellationToken);

            return Json(new { success = true, message = "Customer deleted successfully!" });
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new { message = $"Customer with ID {id} was not found." });
        }
        catch (BusinessRuleViolationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
    }

    private bool IsAjaxRequest() => Request.Headers.XRequestedWith == "XMLHttpRequest";

    private Dictionary<string, string> ModelStateErrors() =>
        ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors[0].ErrorMessage);
}
