using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces.Collections;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);
    Task<IPaginatedList<Customer>> GetPagedAsync(CustomerFilterDto filter, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
