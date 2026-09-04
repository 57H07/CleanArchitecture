namespace CleanArchitecture.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IProductRepository Products { get; }
    ICustomerRepository Customers { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
