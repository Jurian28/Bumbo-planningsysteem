using System.ComponentModel.DataAnnotations;
using BumboDB.Models;   

namespace BumboDB.Models;

public class Chapter
{
    [Key]
    public int ChapterId { get; set; }
    public string Manager { get; set; }

    public string Name { get; set; }
    public int Meters { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    public virtual ICollection<History> Histories { get; set; } = new List<History>();
    public virtual Employee ManagerNavigation { get; set; } = null!;
    public virtual ICollection<Template> Templates { get; set; } = new List<Template>();

}
