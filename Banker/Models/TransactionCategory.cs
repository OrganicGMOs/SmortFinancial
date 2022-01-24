using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    public class TransactionCategory
    {
        public string CategoryType { get; set; }
        public string Subtype { get; set; }
        public int Color { get; set; }
        public bool ReductionTarget { get; set; }
    }
}
