using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters")]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [StringLength(200, ErrorMessage = "Company cannot exceed 200 characters")]
    public string? Company { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
