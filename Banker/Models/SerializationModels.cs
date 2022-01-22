using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    internal class SerializationModels {}

    public class TransactionDefinitions
    {
        public TransactionDefinition[] Definitions { get; set; }
    }
    public class TransactionCategories
    {
        public TransactionCategory[] Categories { get; set; }
    }
    public class TransactionHistory
    {
        public DateTime Modified { get; set; }
        public DateTime OldestRecord { get; set; }
        public Transaction[] Transactions { get; set; }
    }
    public class CategoryDefinitions
    {
        public CategoryDefinition[] Definitions { get; set; }
    }
}

