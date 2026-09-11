using CleanArchitecture.Domain.Exceptions;

namespace CleanArchitecture.Application.Exceptions;

public class EntityNotFoundException : RessourceNotFoundException
{
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} with ID '{id}' was not found.")
    {
        EntityName = entityName;
        Id = id;
    }

    public string EntityName { get; }
    public object Id { get; }
}

public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string entityName, string field, object value) 
        : base($"{entityName} with {field} '{value}' already exists.")
    {
    }

    public DuplicateEntityException(string entityName, string field)
        : base($"Another {entityName.ToLowerInvariant()} already uses this {field}.")
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
