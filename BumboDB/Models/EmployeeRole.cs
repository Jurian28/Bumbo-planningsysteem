using System.ComponentModel.DataAnnotations;

namespace BumboDB.Models;

public class EmployeeRole
{
    [Key]
    public string Employee { get; set; }
    public int Department { get; set; }
    public int Experience { get; set; }
    
    public virtual Department DepartmentNavigation { get; set; } = null!;
    public virtual Employee EmployeeNavigation { get; set; } = null!;
    public virtual Experience ExperienceNavigation { get; set; } = null!;
}
