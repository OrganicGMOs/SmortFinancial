using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Interfaces
{
    public interface ICategoryDefinition
    {
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public string Color { get; set; }
        public bool ReductionTarget { get; set; }
    }
}
