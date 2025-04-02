using System.ComponentModel.DataAnnotations;

namespace Bumbo.Models
{
    public class PrognoseViewModel
    {
        public int[] SelectedTemplates { get; set; } = new int[7];

        public int Week { get; set; }

        public int Year { get; set; }
    }
}
