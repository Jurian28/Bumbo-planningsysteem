using System.ComponentModel.DataAnnotations;

namespace BumboDB.Models;

public class Department
{
    [Key]
    public int DepartmentId { get; set; }
    public int Chapter { get; set; }
    public string Name { get; set; } 
    public string Description { get; set; } = null!;
    public virtual Chapter ChapterNavigation { get; set; } = null!;
    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    public virtual ICollection<Norm> Norms { get; set; } = new List<Norm>();
}
