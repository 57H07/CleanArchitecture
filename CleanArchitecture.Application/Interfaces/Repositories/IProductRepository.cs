using CleanArchitecture.Application.Interfaces.Collections;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IPaginatedList<Product>> GetPagedAsync(int pageIndex, int pageSize);
    Task<IEnumerable<Product>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    Task<IEnumerable<Product>> GetAvailableProductsAsync();
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
