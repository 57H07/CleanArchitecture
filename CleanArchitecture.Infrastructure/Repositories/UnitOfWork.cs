using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanArchitecture.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(
        ApplicationDbContext context,
        IProductRepository products,
        IUserRepository users,
        ICustomerRepository customers
    )
    {
        _context = context;
        Users = users;
        Products = products;
        Customers = customers;
    }

    public IUserRepository Users { get; }
    public IProductRepository Products { get; }
    public ICustomerRepository Customers { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Une transaction est déjà ouverte sur cette unité de travail.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _transaction?.Dispose();
        _transaction = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
