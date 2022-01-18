using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    public class TransactionDefinitions
    {
        public Transactiondefinition[] Definitions { get; set; }
    }

    public class Transactiondefinition
    {
        public Transactiondefinition()
        {
            Key = "unknown";
            ReductionTarget = false;
            TransactionCategory = new Transactioncategory();
        }
        public string Key { get; set; }
        public Transactioncategory TransactionCategory { get; set; }
        public bool ReductionTarget { get; set; }
    }

    public class Transactioncategory
    {
        public string CategoryType { get; set; }
        public string SecondaryType { get; set; }
        public string Color { get; set; }
        public bool ReductionTarget { get; set; }
    }
    //Expand: friendly names
    public enum CategoryType
    {
        food,
        bill,
        subscription,
        Unknown
    }
}




