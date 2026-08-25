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

    public async Task<IPaginatedList<Product>> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Include(p => p.User)
            .AsQueryable();

        return await PaginatedList<Product>.CreateAsync(query, pageIndex, pageSize, cancellationToken);
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
}
