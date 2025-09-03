namespace CleanArchitecture.Domain.Enums;

/// <summary>
/// Represents the different states a product can be in
/// </summary>
public enum ProductStatus
{
    /// <summary>
    /// Product is in draft state, not visible to customers
    /// </summary>
    Draft = 1,
    
    /// <summary>
    /// Product is active and available for purchase
    /// </summary>
    Active = 2,
    
    /// <summary>
    /// Product is temporarily unavailable
    /// </summary>
    Inactive = 3,
    
    /// <summary>
    /// Product is discontinued and no longer available
    /// </summary>
    Discontinued = 4
}
