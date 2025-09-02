using CleanArchitecture.Domain.Common;

namespace CleanArchitecture.Domain.Events;

public class UserActivatedEvent : DomainEvent
{
    public int UserId { get; }

    public UserActivatedEvent(int userId)
    {
        UserId = userId;
    }
}

public class UserDeactivatedEvent : DomainEvent
{
    public int UserId { get; }

    public UserDeactivatedEvent(int userId)
    {
        UserId = userId;
    }
}
