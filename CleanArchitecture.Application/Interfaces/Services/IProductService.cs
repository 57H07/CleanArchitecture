using CleanArchitecture.Application.DTOs;

using CleanArchitecture.Application.Collections;

namespace CleanArchitecture.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetAvailableProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto createProductDto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(int id, CreateProductDto updateProductDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateStockAsync(int id, int quantity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
