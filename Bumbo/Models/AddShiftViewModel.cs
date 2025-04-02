using BumboDB.Models;

namespace Bumbo.Models
{
    public class AddShiftViewModel
    {
        public Department SelectedDepartment { get; set; }
        public List<Department> AvailableDepartments { get; set; } = new List<Department>();
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Employee SelectedEmployee { get; set; }
        public List<Employee> AvailableEmployees { get; set; } = new List<Employee>();
        public bool OtherChapters { get; set; }
        public int HoursLeft { get; set; }
        public string EmpName { get; set; }
    }
}
