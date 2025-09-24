using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return product?.Adapt<ProductDto>();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetByUserIdAsync(int userId)
    {
        var products = await _unitOfWork.Products.GetByUserIdAsync(userId);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(category);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetAvailableProductsAsync()
    {
        var products = await _unitOfWork.Products.GetAvailableProductsAsync();
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
    {
        // Validate that the user exists
        if (!await _unitOfWork.Users.ExistsAsync(createProductDto.UserId))
        {
            throw new KeyNotFoundException($"User with ID {createProductDto.UserId} not found.");
        }

        var product = createProductDto.Adapt<Product>();
        product.CreatedAt = DateTime.UtcNow;

        var createdProduct = await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return createdProduct.Adapt<ProductDto>();
    }

    public async Task<ProductDto> UpdateAsync(int id, CreateProductDto updateProductDto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        // Validate that the user exists if changing user
        if (product.UserId != updateProductDto.UserId && 
            !await _unitOfWork.Users.ExistsAsync(updateProductDto.UserId))
        {
            throw new KeyNotFoundException($"User with ID {updateProductDto.UserId} not found.");
        }

        updateProductDto.Adapt(product);
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return product.Adapt<ProductDto>();
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _unitOfWork.Products.ExistsAsync(id))
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        await _unitOfWork.Products.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateStockAsync(int id, int quantity)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        product.UpdateStock(quantity);
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.Products.ExistsAsync(id);
    }

    /// <summary>
    /// Complex business method: Launch a new product with all associated operations
    /// - Creates or updates product
    /// - Sets up pricing tiers and promotions
    /// - Configures inventory across warehouses
    /// - Creates marketing campaigns
    /// - Sets up supplier relationships
    /// - Generates audit logs
    /// - Triggers notification workflows
    /// </summary>
    public async Task<ResultDto<ProductLaunchDto>> LaunchProductAsync(ProductLaunchRequestDto launchRequest, UserDto currentUser)
    {
        ArgumentNullException.ThrowIfNull(launchRequest);
        ArgumentNullException.ThrowIfNull(currentUser);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // 1. Validate and prepare product
            var product = await ValidateAndPrepareProductAsync(launchRequest);
            if (product == null)
            {
                return new ResultDto<ProductLaunchDto>(false, "Un produit avec le même nom et SKU existe déjà.");
            }

            // 2. Handle pricing strategy
            await SetupPricingStrategyAsync(product.Id, launchRequest.PricingStrategy);

            // 3. Configure inventory across warehouses
            await ConfigureInventoryDistributionAsync(product.Id, launchRequest.InventoryDistribution);

            // 4. Retire competing products if specified
            await HandleCompetingProductsAsync(product.Id, launchRequest.CompetingProductIds, launchRequest.LaunchDate);

            // 5. Create marketing campaigns
            var campaigns = await CreateMarketingCampaignsAsync(product.Id, launchRequest.MarketingCampaigns);

            // 6. Setup supplier relationships
            await EstablishSupplierRelationshipsAsync(product.Id, launchRequest.SupplierContracts);

            // 7. Configure product variants and bundles
            await SetupProductVariantsAsync(product.Id, launchRequest.ProductVariants);

            // 8. Create audit trail
            await CreateProductLaunchAuditLogsAsync(product, campaigns, currentUser);

            // 9. Setup automated reorder rules
            await ConfigureReorderRulesAsync(product.Id, launchRequest.ReorderSettings);

            await _unitOfWork.CommitTransactionAsync();

            // 10. Execute post-launch procedures
            await ExecutePostLaunchProceduresAsync(product.Id);

            // 11. Trigger external notifications
            await TriggerLaunchNotificationsAsync(product, campaigns);

            var result = new ProductLaunchDto
            {
                Product = product.Adapt<ProductDto>(),
                CampaignIds = campaigns.Select(c => c.Id).ToList(),
                LaunchDate = launchRequest.LaunchDate,
                IsSuccessful = true
            };

            return new ResultDto<ProductLaunchDto>(result, true);
        }
        catch (Exception)
        {
            // Log error when logging is available
            await _unitOfWork.RollbackTransactionAsync();
            return new ResultDto<ProductLaunchDto>(false, "Une erreur s'est produite lors du lancement du produit.");
        }
    }

    private async Task<Product?> ValidateAndPrepareProductAsync(ProductLaunchRequestDto launchRequest)
    {
        // Check if product already exists by SKU
        var existingProducts = await _unitOfWork.Products.GetAllAsync();
        var existingProduct = existingProducts.FirstOrDefault(p => p.Name == launchRequest.ProductName);
        
        if (existingProduct != null && existingProduct.Id != launchRequest.ProductId)
        {
            return null; // Duplicate product name
        }

        Product product;
        if (launchRequest.ProductId > 0)
        {
            // Update existing product
            var existingProductById = await _unitOfWork.Products.GetByIdAsync(launchRequest.ProductId);
            if (existingProductById == null)
                throw new KeyNotFoundException($"Product with ID {launchRequest.ProductId} not found.");

            product = existingProductById;
            // Update product properties
            product.Name = launchRequest.ProductName;
            product.Description = launchRequest.Description;
            product.Category = launchRequest.Category;
        }
        else
        {
            // Create new product
            product = new Product
            {
                Name = launchRequest.ProductName,
                Description = launchRequest.Description,
                Category = launchRequest.Category,
                Price = launchRequest.BasePrice,
                CreatedAt = DateTime.UtcNow,
                UserId = launchRequest.UserId
            };
            product = await _unitOfWork.Products.AddAsync(product);
        }

        return product;
    }

    private async Task SetupPricingStrategyAsync(int productId, PricingStrategyDto pricingStrategy)
    {
        // In a real implementation, this would work with pricing repositories
        // For now, we'll update the base product price
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product != null)
        {
            product.Price = pricingStrategy.BasePrice;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Products.UpdateAsync(product);
        }
    }

    private async Task ConfigureInventoryDistributionAsync(int productId, List<InventoryDistributionDto> distributions)
    {
        // In a real implementation, this would work with inventory repositories
        // For now, we'll simulate inventory setup
        foreach (var distribution in distributions)
        {
            // Validate warehouse exists (simulated)
            if (distribution.WarehouseId <= 0)
            {
                throw new ArgumentException($"Invalid warehouse ID: {distribution.WarehouseId}");
            }

            // Create inventory records (simulated)
            await Task.Delay(1); // Simulate async operation
        }
    }

    private async Task HandleCompetingProductsAsync(int newProductId, List<int> competingProductIds, DateTime launchDate)
    {
        if (!competingProductIds.Any())
            return;

        foreach (var competitorId in competingProductIds)
        {
            var competitor = await _unitOfWork.Products.GetByIdAsync(competitorId);
            if (competitor != null)
            {
                // Mark as discontinued (simulated - would need additional properties)
                competitor.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Products.UpdateAsync(competitor);
            }
        }
    }

    private async Task<List<MarketingCampaignResult>> CreateMarketingCampaignsAsync(int productId, List<MarketingCampaignDto> campaignDtos)
    {
        var campaigns = new List<MarketingCampaignResult>();

        foreach (var campaignDto in campaignDtos)
        {
            // In a real implementation, this would create campaign records
            var campaign = new MarketingCampaignResult
            {
                Id = campaigns.Count + 1, // Simulated ID
                Name = campaignDto.Name,
                ProductId = productId,
                Budget = campaignDto.Budget,
                StartDate = campaignDto.StartDate,
                EndDate = campaignDto.EndDate
            };

            campaigns.Add(campaign);
            await Task.Delay(1); // Simulate async operation
        }

        return campaigns;
    }

    private async Task EstablishSupplierRelationshipsAsync(int productId, List<SupplierContractDto> contracts)
    {
        foreach (var contractDto in contracts)
        {
            // Validate supplier exists (simulated)
            if (contractDto.SupplierId <= 0)
            {
                throw new ArgumentException($"Invalid supplier ID: {contractDto.SupplierId}");
            }

            // Create supplier contract records (simulated)
            await Task.Delay(1); // Simulate async operation
        }
    }

    private async Task SetupProductVariantsAsync(int productId, List<ProductVariantDto> variants)
    {
        foreach (var variantDto in variants)
        {
            // In a real implementation, this would create variant records
            await Task.Delay(1); // Simulate async operation
        }
    }

    private async Task CreateProductLaunchAuditLogsAsync(Product product, List<MarketingCampaignResult> campaigns, UserDto user)
    {
        // In a real implementation, this would create audit log entries
        await Task.Delay(1); // Simulate async operation
    }

    private async Task ConfigureReorderRulesAsync(int productId, ReorderSettingsDto settings)
    {
        // In a real implementation, this would set up automated reorder rules
        await Task.Delay(1); // Simulate async operation
    }

    private async Task ExecutePostLaunchProceduresAsync(int productId)
    {
        // Execute stored procedures (simulated)
        await Task.Delay(10); // Simulate procedure execution time
    }

    private async Task TriggerLaunchNotificationsAsync(Product product, List<MarketingCampaignResult> campaigns)
    {
        // This would typically integrate with external services
        // Email service, push notifications, social media, etc.
        await Task.Delay(1); // Simulate notification trigger
    }
}
