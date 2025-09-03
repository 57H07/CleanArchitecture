using CleanArchitecture.Domain.Exceptions;

namespace CleanArchitecture.Application.Exceptions;

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id) 
        : base($"{entityName} with ID '{id}' was not found.")
    {
    }
}

public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string entityName, string field, object value) 
        : base($"{entityName} with {field} '{value}' already exists.")
    {
    }
}

public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) 
        : base(message)
    {
    }
}
