using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;

namespace CleanArchitecture.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerFilterDto filter, CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(CreateCustomerDto createCustomerDto, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(int id, CreateCustomerDto updateCustomerDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
