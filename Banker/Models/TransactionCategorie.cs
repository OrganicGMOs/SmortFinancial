using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    internal class TransactionCategories
    {
        public TransactionCategory[] Categories { get; set; }

    }
    public class TransactionCategory
    {
        public TransactionCategory()
        {
            CategoryType = "Unknown";
            SubTypes = new string[0];
            AssignedColor = "Black";
            ReductionTarget = false;
        }
        public string CategoryType { get; set; }
        public string[] SubTypes { get; set; }
        public string AssignedColor { get; set; }
        public bool ReductionTarget { get; set; }
    }

}
