using System.ComponentModel.DataAnnotations;

public class AvailableShift
{
    [Key]
    public int AvailableShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsFilled { get; set; }
}