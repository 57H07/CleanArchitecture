using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Application.Interfaces.Services;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        return customer?.Adapt<CustomerDto>();
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        return customers.Adapt<IEnumerable<CustomerDto>>();
    }

    public async Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _unitOfWork.Customers.GetActiveCustomersAsync(cancellationToken);
        return customers.Adapt<IEnumerable<CustomerDto>>();
    }

    public async Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerFilterDto filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var pagedCustomers = await _unitOfWork.Customers.GetPagedAsync(filter, cancellationToken);

        return pagedCustomers.ToPagedResult(dto => dto.Adapt<CustomerDto>());
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto createCustomerDto, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Customers.ExistsByEmailAsync(createCustomerDto.Email, null, cancellationToken))
        {
            throw new DuplicateEntityException("Customer", "email", createCustomerDto.Email);
        }

        var customer = createCustomerDto.Adapt<Customer>();
        customer.ValidateBusinessRules();
        customer.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.Adapt<CustomerDto>();
    }

    public async Task<CustomerDto> UpdateAsync(int id, CreateCustomerDto updateCustomerDto, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            throw new EntityNotFoundException("Customer", id);
        }

        if (await _unitOfWork.Customers.ExistsByEmailAsync(updateCustomerDto.Email, id, cancellationToken))
        {
            throw new DuplicateEntityException("Customer", "email", updateCustomerDto.Email);
        }

        updateCustomerDto.Adapt(customer);
        customer.ValidateBusinessRules();
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.Adapt<CustomerDto>();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.Customers.ExistsAsync(id, cancellationToken))
        {
            throw new EntityNotFoundException("Customer", id);
        }

        // The Product -> Customer foreign key is configured with DeleteBehavior.Restrict,
        // so surface the rule here instead of letting EF raise a raw constraint violation.
        if (await _unitOfWork.Products.ExistsForCustomerAsync(id, cancellationToken))
        {
            throw new BusinessRuleViolationException(
                "This customer still owns products and cannot be deleted. Reassign or delete the products first.");
        }

        await _unitOfWork.Customers.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Customers.ExistsAsync(id, cancellationToken);
    }
}
