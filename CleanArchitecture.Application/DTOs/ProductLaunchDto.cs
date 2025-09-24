namespace CleanArchitecture.Application.DTOs;

public class ProductLaunchRequestDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int UserId { get; set; }
    public DateTime LaunchDate { get; set; }
    public PricingStrategyDto PricingStrategy { get; set; } = new();
    public List<InventoryDistributionDto> InventoryDistribution { get; set; } = new();
    public List<int> CompetingProductIds { get; set; } = new();
    public List<MarketingCampaignDto> MarketingCampaigns { get; set; } = new();
    public List<SupplierContractDto> SupplierContracts { get; set; } = new();
    public List<ProductVariantDto> ProductVariants { get; set; } = new();
    public ReorderSettingsDto ReorderSettings { get; set; } = new();
}

public class ProductLaunchDto
{
    public ProductDto Product { get; set; } = new();
    public List<int> CampaignIds { get; set; } = new();
    public DateTime LaunchDate { get; set; }
    public bool IsSuccessful { get; set; }
}

public class PricingStrategyDto
{
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public List<TieredPriceDto> TieredPrices { get; set; } = new();
    public List<PromotionalPriceDto> PromotionalPrices { get; set; } = new();
}

public class TieredPriceDto
{
    public decimal Price { get; set; }
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
}

public class PromotionalPriceDto
{
    public decimal Price { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class InventoryDistributionDto
{
    public int WarehouseId { get; set; }
    public int InitialQuantity { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
}

public class MarketingCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string TargetAudience { get; set; } = string.Empty;
    public List<CampaignChannelDto> Channels { get; set; } = new();
}

public class CampaignChannelDto
{
    public string Channel { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class SupplierContractDto
{
    public int SupplierId { get; set; }
    public string ContractType { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public int MinimumOrderQuantity { get; set; }
    public int LeadTimeDays { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPreferred { get; set; }
}

public class ProductVariantDto
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal PriceModifier { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
}

public class ReorderSettingsDto
{
    public bool EnableAutoReorder { get; set; }
    public int ReorderPoint { get; set; }
    public int ReorderQuantity { get; set; }
    public int MaxStock { get; set; }
    public int CheckFrequencyHours { get; set; }
    public int? PreferredSupplierId { get; set; }
}

public class MarketingCampaignResult
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
