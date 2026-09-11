using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Interfaces.Services;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Web.Extensions;
using CleanArchitecture.Web.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CleanArchitecture.Web.Controllers;

public class ProductsController : Controller
{
    private const string ProductEntity = "Product";

    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;

    public ProductsController(IProductService productService, ICustomerService customerService)
    {
        _productService = productService;
        _customerService = customerService;
    }

    // GET: Products
    // Server-side paging, filtering and sorting via query string, e.g.
    public async Task<IActionResult> Index([FromQuery] ProductFilterDto filter, CancellationToken cancellationToken)
    {
        var products = await _productService.GetPagedAsync(filter, cancellationToken);

        return View(await BuildViewModelAsync(products, filter, cancellationToken));
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);

        return product is null ? NotFound() : View(product);
    }

    // GET: Products/Create
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateFormListsAsync(cancellationToken);

        return View();
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto createProductDto, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _productService.CreateAsync(createProductDto, cancellationToken);
                this.NotifySuccess("Product created successfully!");

                return RedirectToAction(nameof(Index));
            }
            catch (EntityNotFoundException ex)
            {
                ModelState.AddModelError(nameof(CreateProductDto.CustomerId), ex.Message);
            }
            catch (ValidationDomaineException ex)
            {
                ModelState.AddModelError(ex.FieldName, ex.Message);
            }
        }

        await PopulateFormListsAsync(cancellationToken, createProductDto.CustomerId);

        return View(createProductDto);
    }

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        var editDto = product.Adapt<CreateProductDto>();

        await PopulateFormListsAsync(cancellationToken, editDto.CustomerId);

        return View(editDto);
    }

    // POST: Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateProductDto updateProductDto, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _productService.UpdateAsync(id, updateProductDto, cancellationToken);
                this.NotifySuccess("Product updated successfully!");

                return RedirectToAction(nameof(Index));
            }
            catch (EntityNotFoundException ex) when (ex.EntityName == ProductEntity)
            {
                return NotFound();
            }
            catch (EntityNotFoundException ex)
            {
                ModelState.AddModelError(nameof(CreateProductDto.CustomerId), ex.Message);
            }
            catch (ValidationDomaineException ex)
            {
                ModelState.AddModelError(ex.FieldName, ex.Message);
            }
        }

        await PopulateFormListsAsync(cancellationToken, updateProductDto.CustomerId);

        return View(updateProductDto);
    }

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);

        return product is null ? NotFound() : View(product);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        this.NotifySuccess("Product deleted successfully!");

        return RedirectToAction(nameof(Index));
    }

    private async Task<ProductsViewModel> BuildViewModelAsync(
        PagedResult<ProductDto> products,
        ProductFilterDto filter,
        CancellationToken cancellationToken)
    {
        var categories = (await _productService.GetDistinctCategoriesAsync(cancellationToken))
            .Select(c => new { Value = c, Text = c });

        var statuses = Enum.GetValues<ProductStatus>()
            .Select(s => new { Value = s.ToString(), Text = s.ToString() });

        var customers = await _customerService.GetAllAsync(cancellationToken);

        return new ProductsViewModel
        {
            Products = products,
            Filter = filter,
            AvailableCategories = new SelectList(categories, "Value", "Text", filter.Category),
            AvailableStatuses = new SelectList(statuses, "Value", "Text", filter.Status?.ToString()),
            AvailableCustomers = new SelectList(customers, "Id", "Name", filter.CustomerId)
        };
    }

    private async Task PopulateFormListsAsync(CancellationToken cancellationToken, int? selectedCustomerId = null)
    {
        var customers = (await _customerService.GetActiveCustomersAsync(cancellationToken)).ToList();

        if (selectedCustomerId is int id && customers.All(c => c.Id != id))
        {
            var owner = await _customerService.GetByIdAsync(id, cancellationToken);
            if (owner != null)
            {
                owner.Name = $"{owner.Name} (inactive)";
                customers.Add(owner);
            }
        }

        ViewBag.Customers = new SelectList(customers.OrderBy(c => c.Name), "Id", "Name", selectedCustomerId);
    }
}
