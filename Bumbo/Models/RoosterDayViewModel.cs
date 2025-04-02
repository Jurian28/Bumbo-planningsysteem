namespace Bumbo.Models
{
    public class RoosterDayViewModel
    {
        public int WeekNumber { get; set; }
        public int Year { get; set; }
        public DateOnly Date { get; set; }
        public Dictionary<string, (int RequiredHours, double CurrentHours)> DepartmentHours { get; set; } = new Dictionary<string, (int RequiredHours, double CurrentHours)>();
        public Dictionary<string, List<ShiftViewModel>> DepartmentShifts { get; set; } = new Dictionary<string, List<ShiftViewModel>>();
    }
}
