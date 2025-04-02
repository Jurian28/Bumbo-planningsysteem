using BumboDB.Models;

namespace Bumbo.Models;

public class EmployeeRoosterViewModel
{
    public List<Shift> UserShifts { get; set; }
    public List<Shift> AvailableShifts { get; set; }
}