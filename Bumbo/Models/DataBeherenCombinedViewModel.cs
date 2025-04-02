using BumboDB.Models;

namespace Bumbo.Models
{
    public class DataBeherenCombinedViewModel
    {
        public IEnumerable<History> Histories_ { get; set; }
        public IEnumerable<int> Numbers { get; set; }
    }
}
