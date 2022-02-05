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
        public Guid AssignedId { get; set; }
        public string CategoryType { get; set; }
        public string[] SubTypes { get; set; }
        public bool ReductionTarget { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        public string[] SubCategories { get; set; }
    }
}
