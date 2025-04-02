namespace BumboDB.Models;

public class Shift
{
    public int ShiftId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateOnly Date { get; set; }
    public string EmployeeId { get; set; }
    public int ChapterId { get; set; }
    public int DepartmentId { get; set; }
    public bool IsPublished { get; set; }
    public bool IsAvailableForSwap { get; set; } // New property


    public virtual Employee Employee { get; set; }
    public virtual Chapter Chapter { get; set; } 
    public virtual Department Department { get; set; }
}