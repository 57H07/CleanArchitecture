using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Application.Interfaces.Services;
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

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return product?.Adapt<ProductDto>();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var pagedProducts = await _unitOfWork.Products.GetPagedAsync(pageIndex, pageSize, cancellationToken);

        return pagedProducts.ToPagedResult(dto => dto.Adapt<ProductDto>());
    }

    public async Task<IEnumerable<ProductDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetByUserIdAsync(userId, cancellationToken);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(category, cancellationToken);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetAvailableProductsAsync(cancellationToken);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto, CancellationToken cancellationToken = default)
    {
        // Validate that the user exists
        if (!await _unitOfWork.Users.ExistsAsync(createProductDto.UserId, cancellationToken))
        {
            throw new KeyNotFoundException($"User with ID {createProductDto.UserId} not found.");
        }

        var product = createProductDto.Adapt<Product>();
        product.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Adapt<ProductDto>();
    }

    public async Task<ProductDto> UpdateAsync(int id, CreateProductDto updateProductDto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        // Validate that the user exists if changing user
        if (product.UserId != updateProductDto.UserId && 
            !await _unitOfWork.Users.ExistsAsync(updateProductDto.UserId, cancellationToken))
        {
            throw new KeyNotFoundException($"User with ID {updateProductDto.UserId} not found.");
        }

        updateProductDto.Adapt(product);
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Adapt<ProductDto>();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.Products.ExistsAsync(id, cancellationToken))
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        await _unitOfWork.Products.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStockAsync(int id, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        product.UpdateStock(quantity);
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Products.ExistsAsync(id, cancellationToken);
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
    public async Task<ResultDto<ProductLaunchDto>> LaunchProductAsync(ProductLaunchRequestDto launchRequest, UserDto currentUser, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(launchRequest);
        ArgumentNullException.ThrowIfNull(currentUser);

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // 1. Validate and prepare product
            var product = await ValidateAndPrepareProductAsync(launchRequest, cancellationToken);
            if (product == null)
            {
                return new ResultDto<ProductLaunchDto>(false, "Un produit avec le même nom et SKU existe déjà.");
            }

            // 2. Handle pricing strategy
            await SetupPricingStrategyAsync(product.Id, launchRequest.PricingStrategy, cancellationToken);

            // 3. Configure inventory across warehouses
            await ConfigureInventoryDistributionAsync(product.Id, launchRequest.InventoryDistribution, cancellationToken);

            // 4. Retire competing products if specified
            await HandleCompetingProductsAsync(product.Id, launchRequest.CompetingProductIds, launchRequest.LaunchDate, cancellationToken);

            // 5. Create marketing campaigns
            var campaigns = await CreateMarketingCampaignsAsync(product.Id, launchRequest.MarketingCampaigns, cancellationToken);

            // 6. Setup supplier relationships
            await EstablishSupplierRelationshipsAsync(product.Id, launchRequest.SupplierContracts, cancellationToken);

            // 7. Configure product variants and bundles
            await SetupProductVariantsAsync(product.Id, launchRequest.ProductVariants, cancellationToken);

            // 8. Create audit trail
            await CreateProductLaunchAuditLogsAsync(product, campaigns, currentUser, cancellationToken);

            // 9. Setup automated reorder rules
            await ConfigureReorderRulesAsync(product.Id, launchRequest.ReorderSettings, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // 10. Execute post-launch procedures
            await ExecutePostLaunchProceduresAsync(product.Id, cancellationToken);

            // 11. Trigger external notifications
            await TriggerLaunchNotificationsAsync(product, campaigns, cancellationToken);

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
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return new ResultDto<ProductLaunchDto>(false, "Une erreur s'est produite lors du lancement du produit.");
        }
    }

    private async Task<Product?> ValidateAndPrepareProductAsync(ProductLaunchRequestDto launchRequest, CancellationToken cancellationToken = default)
    {
        // Check if product already exists by SKU
        var existingProducts = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        var existingProduct = existingProducts.FirstOrDefault(p => p.Name == launchRequest.ProductName);
        
        if (existingProduct != null && existingProduct.Id != launchRequest.ProductId)
        {
            return null; // Duplicate product name
        }

        Product product;
        if (launchRequest.ProductId > 0)
        {
            // Update existing product
            var existingProductById = await _unitOfWork.Products.GetByIdAsync(launchRequest.ProductId, cancellationToken);
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
            await _unitOfWork.Products.AddAsync(product, cancellationToken);
        }

        return product;
    }

    private async Task SetupPricingStrategyAsync(int productId, PricingStrategyDto pricingStrategy, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would work with pricing repositories
        // For now, we'll update the base product price
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product != null)
        {
            product.Price = pricingStrategy.BasePrice;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        }
    }

    private async Task ConfigureInventoryDistributionAsync(int productId, List<InventoryDistributionDto> distributions, CancellationToken cancellationToken = default)
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
            await Task.Delay(1, cancellationToken); // Simulate async operation
        }
    }

    private async Task HandleCompetingProductsAsync(int newProductId, List<int> competingProductIds, DateTime launchDate, CancellationToken cancellationToken = default)
    {
        if (!competingProductIds.Any())
            return;

        foreach (var competitorId in competingProductIds)
        {
            var competitor = await _unitOfWork.Products.GetByIdAsync(competitorId, cancellationToken);
            if (competitor != null)
            {
                // Mark as discontinued (simulated - would need additional properties)
                competitor.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Products.UpdateAsync(competitor, cancellationToken);
            }
        }
    }

    private async Task<List<MarketingCampaignResult>> CreateMarketingCampaignsAsync(int productId, List<MarketingCampaignDto> campaignDtos, CancellationToken cancellationToken = default)
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
            await Task.Delay(1, cancellationToken); // Simulate async operation
        }

        return campaigns;
    }

    private async Task EstablishSupplierRelationshipsAsync(int productId, List<SupplierContractDto> contracts, CancellationToken cancellationToken = default)
    {
        foreach (var contractDto in contracts)
        {
            // Validate supplier exists (simulated)
            if (contractDto.SupplierId <= 0)
            {
                throw new ArgumentException($"Invalid supplier ID: {contractDto.SupplierId}");
            }

            // Create supplier contract records (simulated)
            await Task.Delay(1, cancellationToken); // Simulate async operation
        }
    }

    private async Task SetupProductVariantsAsync(int productId, List<ProductVariantDto> variants, CancellationToken cancellationToken = default)
    {
        foreach (var variantDto in variants)
        {
            // In a real implementation, this would create variant records
            await Task.Delay(1, cancellationToken); // Simulate async operation
        }
    }

    private async Task CreateProductLaunchAuditLogsAsync(Product product, List<MarketingCampaignResult> campaigns, UserDto user, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would create audit log entries
        await Task.Delay(1, cancellationToken); // Simulate async operation
    }

    private async Task ConfigureReorderRulesAsync(int productId, ReorderSettingsDto settings, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would set up automated reorder rules
        await Task.Delay(1, cancellationToken); // Simulate async operation
    }

    private async Task ExecutePostLaunchProceduresAsync(int productId, CancellationToken cancellationToken = default)
    {
        // Execute stored procedures (simulated)
        await Task.Delay(10, cancellationToken); // Simulate procedure execution time
    }

    private async Task TriggerLaunchNotificationsAsync(Product product, List<MarketingCampaignResult> campaigns, CancellationToken cancellationToken = default)
    {
        // This would typically integrate with external services
        // Email service, push notifications, social media, etc.
        await Task.Delay(1, cancellationToken); // Simulate notification trigger
    }
}
