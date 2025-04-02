using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BumboDB.Models;

public class Prognosis
{
    [Key, Column(Order = 0)]
    public int Department { get; set; }
    [Key, Column(Order = 1)]
    public DateOnly Day { get; set; }
    public int Result { get; set; }
}
