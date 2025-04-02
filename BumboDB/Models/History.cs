using System.ComponentModel.DataAnnotations;

namespace BumboDB.Models;

public class History
{
    [Key]
    public int Chapter { get; set; }
    public DateOnly Day { get; set; }
    public int Customers { get; set; }
    public int CargoCrates { get; set; }
    public string? Holiday { get; set; }

    public virtual Chapter ChapterNavigation { get; set; } = null!;
}
