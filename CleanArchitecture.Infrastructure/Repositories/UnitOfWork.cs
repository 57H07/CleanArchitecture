using System.Diagnostics.CodeAnalysis;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanArchitecture.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private const int DuplicateKeyRow = 2601;
    private const int UniqueConstraintViolation = 2627;

    // The services check for duplicates before saving, but that check-then-act loses a
    // race; the unique indexes are the only real guard. Translating here keeps the
    // provider's exception type out of the Application layer, which cannot reference EF.
    private static readonly IReadOnlyDictionary<string, (string Entity, string Field)> UniqueIndexes =
        new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            ["IX_Customers_Email"] = ("Customer", "email address")
        };

    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(
        ApplicationDbContext context,
        IProductRepository products,
        ICustomerRepository customers
    )
    {
        _context = context;
        Products = products;
        Customers = customers;
    }

    public IProductRepository Products { get; }
    public ICustomerRepository Customers { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (TryTranslateDuplicate(ex, out var duplicate))
        {
            throw duplicate;
        }
    }

    private static bool TryTranslateDuplicate(
        DbUpdateException exception,
        [NotNullWhen(true)] out DuplicateEntityException? duplicate)
    {
        duplicate = null;

        if (exception.InnerException is not SqlException sql)
        {
            return false;
        }

        if (sql.Number is not (DuplicateKeyRow or UniqueConstraintViolation))
        {
            return false;
        }

        foreach (var (indexName, target) in UniqueIndexes)
        {
            if (sql.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase))
            {
                duplicate = new DuplicateEntityException(target.Entity, target.Field);
                return true;
            }
        }

        return false;
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
