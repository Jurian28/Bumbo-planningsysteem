namespace Bumbo.Models
{
    public class ShiftViewModel
    {
        public int ShiftId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public String EmployeeName { get; set; }



    }
}
