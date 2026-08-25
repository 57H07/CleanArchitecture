namespace CleanArchitecture.Domain.Exceptions;

public class InsufficientStockException : ValidationDomaineException
{
    public InsufficientStockException() : base("Insufficient stock available", "Stock")
    {
    }

    public InsufficientStockException(string message) : base(message, "Stock")
    {
    }
}

public class InvalidPriceException : ValidationDomaineException
{
    public InvalidPriceException() : base("Price must be greater than zero", "Price")
    {
    }

    public InvalidPriceException(string message) : base(message, "Price")
    {
    }
}
