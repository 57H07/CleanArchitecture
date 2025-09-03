namespace CleanArchitecture.Application.Enums;

/// <summary>
/// Represents the sort order for query results
/// </summary>
public enum SortOrder
{
    /// <summary>
    /// Sort in ascending order
    /// </summary>
    Ascending = 1,
    
    /// <summary>
    /// Sort in descending order
    /// </summary>
    Descending = 2
}

/// <summary>
/// Represents different sorting options for products
/// </summary>
public enum ProductSortBy
{
    /// <summary>
    /// Sort by product name
    /// </summary>
    Name = 1,
    
    /// <summary>
    /// Sort by creation date
    /// </summary>
    CreatedDate = 2,
    
    /// <summary>
    /// Sort by price
    /// </summary>
    Price = 3
}
