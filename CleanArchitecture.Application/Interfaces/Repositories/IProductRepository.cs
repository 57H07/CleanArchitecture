using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces.Collections;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IPaginatedList<Product>> GetPagedAsync(ProductFilterDto filter, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAvailableProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetDistinctCategoriesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
