namespace CleanArchitecture.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public abstract class RessourceNotFoundException : DomainException
{
    protected RessourceNotFoundException(string message) : base(message)
    {
    }
}

public abstract class InsufficientRightsException : DomainException
{
    protected InsufficientRightsException(string message) : base(message)
    {
    }
}
