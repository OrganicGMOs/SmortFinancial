using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    public class CategoryDefinition
    {
        public Guid AssignedId { get; set; }
        public string CategoryType { get; set; }
        public string[] SubTypes { get; set; }
        public string ColorHex { get; set; }
        public bool ReductionTarget { get; set; }
    }
}
