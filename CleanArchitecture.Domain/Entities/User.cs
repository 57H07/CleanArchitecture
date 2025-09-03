using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Enums;

namespace CleanArchitecture.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    
    public UserRole Role { get; set; } = UserRole.User;
    
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
        }
    }
    
    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
        }
    }
    
    public bool IsAdmin() => Role == UserRole.Admin;
    
    public bool IsModerator() => Role == UserRole.Moderator;
    
    public bool HasAdminPrivileges() => Role == UserRole.Admin || Role == UserRole.Moderator;
    
    public void PromoteToAdmin()
    {
        Role = UserRole.Admin;
    }
    
    public void PromoteToModerator()
    {
        Role = UserRole.Moderator;
    }
    
    public void DemoteToUser()
    {
        Role = UserRole.User;
    }
    
    // Domain validation methods
    public bool IsValidEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
            return false;
            
        try
        {
            var addr = new System.Net.Mail.MailAddress(Email);
            return addr.Address == Email;
        }
        catch
        {
            return false;
        }
    }
    
    public bool HasValidName()
    {
        return !string.IsNullOrWhiteSpace(FirstName) && 
               !string.IsNullOrWhiteSpace(LastName) &&
               FirstName.Length <= 100 &&
               LastName.Length <= 100;
    }
    
    public void ValidateBusinessRules()
    {
        if (!HasValidName())
            throw new ArgumentException("First name and last name are required and must not exceed 100 characters");
            
        if (!IsValidEmail())
            throw new ArgumentException("A valid email address is required");
            
        if (Email.Length > 256)
            throw new ArgumentException("Email cannot exceed 256 characters");
            
        if (!string.IsNullOrEmpty(PhoneNumber) && PhoneNumber.Length > 20)
            throw new ArgumentException("Phone number cannot exceed 20 characters");
    }
}
