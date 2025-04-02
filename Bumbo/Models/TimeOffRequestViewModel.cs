namespace Bumbo.ViewModels
{
    public class TimeOffRequestViewModel
    {
        public int TimeOffRequestId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public bool Ziek { get; set; }

        public TimeOffRequestViewModel()
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
        }
    }
}