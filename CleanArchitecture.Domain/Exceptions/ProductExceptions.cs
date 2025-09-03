using CleanArchitecture.Domain.Exceptions;

namespace CleanArchitecture.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException() : base("Insufficient stock available")
    {
    }
    
    public InsufficientStockException(string message) : base(message)
    {
    }
}

public class InvalidPriceException : DomainException
{
    public InvalidPriceException() : base("Price must be greater than zero")
    {
    }
    
    public InvalidPriceException(string message) : base(message)
    {
    }
}
