namespace CleanArchitecture.Domain.Exceptions;

public class InvalidCustomerEmailException : ValidationDomaineException
{
    public InvalidCustomerEmailException() : base("A valid email address is required", "Email")
    {
    }

    public InvalidCustomerEmailException(string message) : base(message, "Email")
    {
    }
}
