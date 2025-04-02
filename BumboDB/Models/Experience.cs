using System.ComponentModel.DataAnnotations;

namespace BumboDB.Models;

public class Experience
{
    [Key]
    public int ExperienceId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
}
