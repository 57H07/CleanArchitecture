namespace CleanArchitecture.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
