namespace Bumbo.Models
{
    public class DepartmentResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int[] Results { get; set; } = new int[7];

        public string[] TemplateName { get; set; } = new string[7];
        public int[] TemplateCustomers { get; set; } = new int[7];
        public int[] TemplateCargo { get; set; } = new int[7];

        public int SelectedYear { get; set; }
        public int SelectedWeek { get; set; }
    }
}