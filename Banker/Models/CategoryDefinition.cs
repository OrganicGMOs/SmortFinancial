using Banker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    public class CategoryDefinition : ICategoryDefinition
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        public string[] SubCategories { get; set; }
        public bool ReductionTarget { get; set; }
    }
}
