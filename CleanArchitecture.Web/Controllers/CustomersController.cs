using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Interfaces.Services;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Web.Extensions;
using CleanArchitecture.Web.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Web.Controllers;

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

        return Request.IsAjaxRequest() ? PartialView("_CustomerTable", viewModel) : View(viewModel);
    }

    // GET: Customers/Form or Customers/Form/5
    [HttpGet]
    public async Task<IActionResult> Form(int? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return PartialView("_CustomerForm", new CreateCustomerDto());
        }

        var customer = await _customerService.GetByIdAsync(id.Value, cancellationToken);
        if (customer == null)
        {
            return NotFound(new { message = $"Customer with ID {id} was not found." });
        }

        ViewData["CustomerId"] = customer.Id;

        return PartialView("_CustomerForm", customer.Adapt<CreateCustomerDto>());
    }

    // POST: Customers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerDto createCustomerDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = ModelState.ToErrorDictionary() });
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
            return BadRequest(new { errors = ModelState.ToErrorDictionary() });
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
}
