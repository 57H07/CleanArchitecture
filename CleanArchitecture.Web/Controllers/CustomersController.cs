using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Interfaces.Services;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Web.Controllers;

public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    // GET: Customers
    // Supports server-side paging, filtering and sorting via query string, e.g.
    // /Customers?isActive=true&sortBy=Company&sortOrder=Descending
    // When called via AJAX (X-Requested-With header), returns only the table+pagination partial
    // so the modal-driven create/edit/delete flow can refresh the list without a full page reload.
    public async Task<IActionResult> Index([FromQuery] CustomerFilterDto filter, CancellationToken cancellationToken)
    {
        PagedResult<CustomerDto> customers;
        try
        {
            customers = await _customerService.GetPagedAsync(filter, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving customers");
            TempData["Error"] = "An error occurred while loading customers.";
            customers = PagedResult<CustomerDto>.Empty(filter.Page, filter.PageSize);
        }

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating customer");
            return StatusCode(500, new { message = "An error occurred while creating the customer." });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating customer {CustomerId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the customer." });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting customer {CustomerId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the customer." });
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
