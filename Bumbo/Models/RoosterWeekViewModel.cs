using System.ComponentModel.DataAnnotations;

namespace Bumbo.Models
{
    public class RoosterWeekViewModel
    {
        public DateOnly BeginDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
        public RoosterDayViewModel[] Days { get; set; } = new RoosterDayViewModel[7];
    }
}
