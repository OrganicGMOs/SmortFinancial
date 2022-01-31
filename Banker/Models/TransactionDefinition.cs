using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    public class TransactionDefinition
    {
        public Guid AssignedId { get; set; }
        public Guid MappedCategory { get; set; }
        public string Key { get; set; }
        public bool ReductionTarget { get; set; }
        public List<string> DefaultTags { get; set; }
    }
}
