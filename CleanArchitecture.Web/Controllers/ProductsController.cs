using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Enums;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Web.ViewModels;
using CleanArchitecture.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CleanArchitecture.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService, 
        ICustomerService customerService, 
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _customerService = customerService;
        _logger = logger;
    }

    // GET: Products
    // Supports server-side paging, filtering and sorting via query string, e.g.
    // /Products?category=Electronics&status=Active&sortBy=Price&sortOrder=Descending
    public async Task<IActionResult> Index([FromQuery] ProductFilterDto filter)
    {
        try
        {
            PagedResult<ProductDto> products = await _productService.GetPagedAsync(filter);

            return View(await BuildViewModelAsync(products, filter));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving products");
            TempData["Error"] = "An error occurred while loading products.";

            var empty = PagedResult<ProductDto>.Empty(filter.Page, filter.PageSize);

            return View(await BuildViewModelAsync(empty, filter));
        }
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving product {ProductId}", id);
            TempData["Error"] = "An error occurred while loading product details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Products/Create
    public async Task<IActionResult> Create()
    {
        await PopulateCustomersDropDown();
        return View();
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto createProductDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _productService.CreateAsync(createProductDto);
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError("CustomerId", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product");
                ModelState.AddModelError("", "An error occurred while creating the product.");
            }
        }
        
        await PopulateCustomersDropDown(createProductDto.CustomerId);
        return View(createProductDto);
    }

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var editDto = new CreateProductDto
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category,
                Status = product.Status,
                CustomerId = product.CustomerId
            };

            await PopulateCustomersDropDown(editDto.CustomerId);
            return View(editDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving product {ProductId} for editing", id);
            TempData["Error"] = "An error occurred while loading product for editing.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateProductDto updateProductDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _productService.UpdateAsync(id, updateProductDto);
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                if (ex.Message.Contains("Product"))
                {
                    return NotFound();
                }
                ModelState.AddModelError("CustomerId", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product {ProductId}", id);
                ModelState.AddModelError("", "An error occurred while updating the product.");
            }
        }
        
        await PopulateCustomersDropDown(updateProductDto.CustomerId);
        return View(updateProductDto);
    }

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving product {ProductId} for deletion", id);
            TempData["Error"] = "An error occurred while loading product for deletion.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product {ProductId}", id);
            TempData["Error"] = "An error occurred while deleting the product.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<ProductsViewModel> BuildViewModelAsync(PagedResult<ProductDto> products, ProductFilterDto filter)
    {
        var viewModel = new ProductsViewModel
        {
            Products = products,
            Filter = filter
        };

        try
        {
            var categories = (await _productService.GetDistinctCategoriesAsync())
                .Select(c => new { Value = c, Text = c });
            viewModel.AvailableCategories = new SelectList(categories, "Value", "Text", filter.Category);

            var statuses = Enum.GetValues<ProductStatus>()
                .Select(s => new { Value = s.ToString(), Text = s.ToString() });
            viewModel.AvailableStatuses = new SelectList(statuses, "Value", "Text", filter.Status?.ToString());

            // Deactivating a customer does not reassign their products, so the filter
            // must list every customer, not just the active ones.
            var customers = await _customerService.GetAllAsync();
            viewModel.AvailableCustomers = new SelectList(customers, "Id", "Name", filter.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading product filters");
        }

        return viewModel;
    }

    private async Task PopulateCustomersDropDown(int? selectedCustomerId = null)
    {
        try
        {
            var customers = (await _customerService.GetActiveCustomersAsync()).ToList();

            if (selectedCustomerId is int id && customers.All(c => c.Id != id))
            {
                var owner = await _customerService.GetByIdAsync(id);
                if (owner != null)
                {
                    owner.Name = $"{owner.Name} (inactive)";
                    customers.Add(owner);
                }
            }

            ViewBag.Customers = new SelectList(customers.OrderBy(c => c.Name), "Id", "Name", selectedCustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading customers for dropdown");
            ViewBag.Customers = new SelectList(new List<CustomerDto>(), "Id", "Name");
        }
    }
}
