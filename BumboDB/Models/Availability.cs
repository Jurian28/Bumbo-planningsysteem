using System.ComponentModel.DataAnnotations;

namespace BumboDB.Models;

public class Availability
{
    [Key]
    public int AvailabilityId { get; set; }
    public DateOnly Day { get; set; }
    public string? Employee { get; set; }
    public bool IsAvailable { get; set; }
    public TimeOnly StartTime { get; set; } 
    public TimeOnly EndTime { get; set; } 
    [Range(0, 10, ErrorMessage = "Hours worked for school must be between 0 and 10.")]
    public int HoursWorkedSchool { get; set; }  
    


    
    public virtual Employee? EmployeeNavigation { get; set; }
}