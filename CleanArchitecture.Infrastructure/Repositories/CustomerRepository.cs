using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Enums;
using CleanArchitecture.Application.Interfaces.Collections;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Collections;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers.ToListAsync(cancellationToken);
    }

    public async Task<IPaginatedList<Customer>> GetPagedAsync(CustomerFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm;
            query = query.Where(c => c.Name.Contains(searchTerm) ||
                c.Email.Contains(searchTerm) ||
                (c.Company != null && c.Company.Contains(searchTerm)));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == filter.IsActive.Value);
        }

        var ascending = filter.SortOrder == SortOrder.Ascending;
        query = filter.SortBy switch
        {
            CustomerSortBy.Company => ascending
                ? query.OrderBy(c => c.Company)
                : query.OrderByDescending(c => c.Company),
            CustomerSortBy.CreatedDate => ascending
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt),
            _ => ascending
                ? query.OrderBy(c => c.Name)
                : query.OrderByDescending(c => c.Name)
        };

        return await PaginatedList<Customer>.CreateAsync(query, filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.AnyAsync(
            c => c.Email == email && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _context.Entry(customer).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync([id], cancellationToken);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.AnyAsync(c => c.Id == id, cancellationToken);
    }
}
