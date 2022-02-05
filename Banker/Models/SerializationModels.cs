using Banker.Extensions;
using Banker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Banker.Models
{
    internal class SerializationModels {}

    public class TransactionDefinitions
    {
        public bool NeedsWrite { get; set; }
        [JsonConverter(typeof(TransactionDefinitionsConverter))]
        public List<ITransactionDefinition> Definitions { get; set; }
    }
    public class TransactionCategories
    {
        public ITransactionCategory[] Categories { get; set; }
    }
    public class TransactionHistory
    {
        public string FilePath { get; set; }
        public bool NeedsWrite { get; set; }
        public DateTime Modified { get; set; }
        public DateTime OldestRecord { get; set; }
        [JsonConverter(typeof(TransactionsConverter))]
        public List<ITransaction> Transactions { get; set; }
    }
    public class CategoryDefinitions
    {
        public bool NeedsWrite { get; set; }
        [JsonConverter(typeof(CategoryDefinitionConverter))]
        public List<ICategoryDefinition> Definitions { get; set; }
    }
}

