using Banker.Extensions;
using Banker.Interfaces;
using Newtonsoft.Json;
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
        public bool NeedsWrite { get; set; }
        public List<TransactionDefinition> Definitions { get; set; }
    }
    public class TransactionCategories
    {
        public TransactionCategory[] Categories { get; set; }
    }
    public class TransactionHistory
    {
        public string FilePath { get; set; }
        public bool NeedsWrite { get; set; }
        public DateTime Modified { get; set; }
        public DateTime OldestRecord { get; set; }
        public List<ITransaction> Transactions { get; set; }
    }
    public class CategoryDefinitions
    {
        public bool NeedsWrite { get; set; }
        public List<CategoryDefinition> Definitions { get; set; }
    }
}

