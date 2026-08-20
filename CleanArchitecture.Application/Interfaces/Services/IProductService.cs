using CleanArchitecture.Application.DTOs;

using CleanArchitecture.Application.Collections;

namespace CleanArchitecture.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<PaginatedList<ProductDto>> GetPagedAsync(int pageIndex, int pageSize);
    Task<IEnumerable<ProductDto>> GetByUserIdAsync(int userId);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category);
    Task<IEnumerable<ProductDto>> GetAvailableProductsAsync();
    Task<ProductDto> CreateAsync(CreateProductDto createProductDto);
    Task<ProductDto> UpdateAsync(int id, CreateProductDto updateProductDto);
    Task DeleteAsync(int id);
    Task UpdateStockAsync(int id, int quantity);
    Task<bool> ExistsAsync(int id);
}
