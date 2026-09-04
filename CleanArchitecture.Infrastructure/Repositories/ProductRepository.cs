using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Enums;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Application.Interfaces.Collections;
using CleanArchitecture.Infrastructure.Collections;

namespace CleanArchitecture.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.User)
            .ToListAsync(cancellationToken);
    }

    public async Task<IPaginatedList<Product>> GetPagedAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Include(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm;
            query = query.Where(p => p.Name.Contains(searchTerm) ||
                (p.Description != null && p.Description.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(p => p.Category == filter.Category);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(p => p.Status == filter.Status.Value);
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(p => p.UserId == filter.UserId.Value);
        }

        var ascending = filter.SortOrder == SortOrder.Ascending;
        query = filter.SortBy switch
        {
            ProductSortBy.Price => ascending
                ? query.OrderBy(p => p.Price)
                : query.OrderByDescending(p => p.Price),
            ProductSortBy.CreatedDate => ascending
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            _ => ascending
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name)
        };

        return await PaginatedList<Product>.CreateAsync(query, filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.User)
            .Where(p => p.Category == category)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.User)
            .Where(p => p.IsAvailable && p.StockQuantity > 0)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Entry(product).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync([id], cancellationToken);
        if (product != null)
        {
            _context.Products.Remove(product);
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetDistinctCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.Category != null)
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }
}
