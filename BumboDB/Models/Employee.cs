using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BumboDB.Models;

public class Employee : IdentityUser
{

    [Required]
    public int Chapter { get; set; }

    [Required]
    [MaxLength(45)]
    public string Name { get; set; } = null!;

    [Required]
    [CustomValidation(typeof(Employee), nameof(ValidateBirthday))]
    public DateOnly Birthday { get; set; }

    [Required]
    [MaxLength(45)]
    [RegularExpression(@"^\d{4}\s?[A-Za-z]{2}$", ErrorMessage = "Zipcode must be in the format '1234 AB'")]
    public string Zipcode { get; set; } = null!;

    [Required]
    [MaxLength(45)]
    [RegularExpression(@"^\d+[A-Za-z]?$", ErrorMessage = "HouseNumber must start with a number and can have at most one letter after it")]
    public string HouseNumber { get; set; } = null!;

    [Required]
    public DateOnly EmployedSince { get; set; }

    [Required]
    [Range(0, 10)]
    public int PayScale { get; set; }

    [Required]
    public bool IsAvailable { get; set; }

    [Required]
    [MaxLength(10)]
    public string Status { get; set; } = "Active";

    public DateOnly? FiredDate { get; set; }

    
    public int getAge() {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        int age = today.Year - Birthday.Year;

        // Adjust if the birthday hasn't occurred yet this year
        if (today < Birthday.AddYears(age)) {
            age--;
        }

        return age;
    }

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    public virtual ICollection<Shift>? Shifts { get; set; }
    public virtual ICollection<TimeOffRequest> TimeOffRequests { get; set; } = new List<TimeOffRequest>(); // Navigation property


    public static ValidationResult? ValidateBirthday(DateOnly birthday, ValidationContext context) { // to validate that the birthday is not in the future
        if (birthday > DateOnly.FromDateTime(DateTime.Now)) {
            return new ValidationResult("Birthday cannot be in the future.");
        }
        if (birthday < new DateOnly(1900, 1, 1)) {
            return new ValidationResult("Birthday cannot be before 1-1-1900");
        }
        return ValidationResult.Success;
    }
}
