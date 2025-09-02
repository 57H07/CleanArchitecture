using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Events;

namespace CleanArchitecture.Domain.Entities;

public class User : AggregateRoot
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? DateOfBirth { get; set; }
    
    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    
    // Domain methods
    public string GetFullName() => $"{FirstName} {LastName}";
    
    public int GetAge()
    {
        if (!DateOfBirth.HasValue) return 0;
        
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Value.Year;
        
        if (DateOfBirth.Value.Date > today.AddYears(-age))
            age--;
            
        return age;
    }
    
    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            AddDomainEvent(new UserActivatedEvent(Id));
        }
    }
    
    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            AddDomainEvent(new UserDeactivatedEvent(Id));
        }
    }
}
