using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Web.ViewModels;
using CleanArchitecture.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CleanArchitecture.Web.Controllers;

/// <summary>
/// Example controller showing proper ViewModel usage
/// </summary>
public class ProductViewModelController : Controller
{
    private readonly IProductService _productService;
    private readonly IUserService _userService;
    private readonly ILogger<ProductViewModelController> _logger;

    public ProductViewModelController(
        IProductService productService,
        IUserService userService,
        ILogger<ProductViewModelController> logger)
    {
        _productService = productService;
        _userService = userService;
        _logger = logger;
    }

    // Example: Using ViewModel for complex list view
    public async Task<IActionResult> Index(int page = 1, string searchTerm = "", string category = "")
    {
        try
        {
            // Get data from application layer (DTOs)
            var products = await _productService.GetAllAsync();
            var users = await _userService.GetAllAsync();
            
            // Create ViewModel with UI-specific logic
            var viewModel = new ProductListViewModel
            {
                Products = products,
                CurrentPage = page,
                SearchTerm = searchTerm,
                SelectedCategory = category,
                
                // UI dropdowns
                AvailableCategories = new SelectList(
                    products.Select(p => p.Category).Distinct().Where(c => !string.IsNullOrEmpty(c)),
                    category),
                    
                AvailableStatuses = new SelectList(
                    Enum.GetValues<ProductStatus>()
                        .Select(s => new { Value = s, Text = s.ToString() }),
                    "Value", "Text"),
                    
                AvailableUsers = new SelectList(users, "Id", "FullName"),
                
                // Pagination
                TotalItems = products.Count(),
                PageSize = 10
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products");
            TempData["Error"] = "Unable to load products. Please try again.";
            return View(new ProductListViewModel());
        }
    }

    // Example: Using ViewModel for create/edit forms
    public async Task<IActionResult> Create()
    {
        try
        {
            var viewModel = await PrepareProductViewModel(new ProductViewModel());
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing create form");
            TempData["Error"] = "Unable to load create form. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // Re-populate UI-specific data
                viewModel = await PrepareProductViewModel(viewModel);
                return View(viewModel);
            }

            // Convert ViewModel to DTO for application layer
            var createDto = new CreateProductDto
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                Price = viewModel.Price,
                StockQuantity = viewModel.StockQuantity,
                Category = viewModel.Category,
                Status = viewModel.Status,
                UserId = viewModel.UserId
            };

            await _productService.CreateAsync(createDto);
            TempData["Success"] = "Product created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            ModelState.AddModelError("", "Unable to create product. Please try again.");
            viewModel = await PrepareProductViewModel(viewModel);
            return View(viewModel);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var productDto = await _productService.GetByIdAsync(id);
            if (productDto == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            // Convert DTO to ViewModel
            var viewModel = new ProductViewModel
            {
                Id = productDto.Id,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                Category = productDto.Category,
                Status = productDto.Status,
                UserId = productDto.UserId
            };

            viewModel = await PrepareProductViewModel(viewModel);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product for edit: {ProductId}", id);
            TempData["Error"] = "Unable to load product. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    // Helper method to prepare UI-specific data
    private async Task<ProductViewModel> PrepareProductViewModel(ProductViewModel viewModel)
    {
        var users = await _userService.GetAllAsync();
        var products = await _productService.GetAllAsync();

        viewModel.AvailableUsers = new SelectList(users, "Id", "FullName", viewModel.UserId);
        
        viewModel.AvailableCategories = new SelectList(
            products.Select(p => p.Category).Distinct().Where(c => !string.IsNullOrEmpty(c)),
            viewModel.Category);
            
        viewModel.AvailableStatuses = new SelectList(
            Enum.GetValues<ProductStatus>()
                .Select(s => new { Value = s, Text = s.ToString() }),
            "Value", "Text", viewModel.Status);

        return viewModel;
    }
}
