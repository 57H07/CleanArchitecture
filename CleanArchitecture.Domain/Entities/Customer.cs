using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Exceptions;

namespace CleanArchitecture.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Company { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public bool HasValidName() => !string.IsNullOrWhiteSpace(Name) && Name.Length <= 200;

    public bool HasValidEmail() =>
        !string.IsNullOrWhiteSpace(Email) && Email.Length <= 256 && Email.Contains('@');

    public bool HasValidPhone() => Phone == null || Phone.Length <= 30;

    public bool HasValidCompany() => Company == null || Company.Length <= 200;

    public bool HasValidNotes() => Notes == null || Notes.Length <= 1000;

    public void ValidateBusinessRules()
    {
        if (!HasValidName())
            throw new ValidationDomaineException("Customer name is required and cannot exceed 200 characters", "Name");

        if (!HasValidEmail())
            throw new InvalidCustomerEmailException();

        if (!HasValidPhone())
            throw new ValidationDomaineException("Phone number cannot exceed 30 characters", "Phone");

        if (!HasValidCompany())
            throw new ValidationDomaineException("Company cannot exceed 200 characters", "Company");

        if (!HasValidNotes())
            throw new ValidationDomaineException("Notes cannot exceed 1000 characters", "Notes");
    }
}
