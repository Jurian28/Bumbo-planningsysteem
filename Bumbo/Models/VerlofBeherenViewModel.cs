using BumboDB.Models;

namespace Bumbo.Models {
    public class VerlofBeherenViewModel {
        public List<TimeOffRequest> timeOffRequests { get; set; }
        public List<Employee> employees { get; set; }
    }
}
