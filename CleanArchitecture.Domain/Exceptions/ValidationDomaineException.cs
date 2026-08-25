namespace CleanArchitecture.Domain.Exceptions;

public class ValidationDomaineException(string message, string fieldName) : DomainException(message)
{
    public string FieldName { get; } = fieldName;
}
