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
    public class Transaction : ITransaction
    {
        public DateTime Date { get; set; }
        public Guid AssignedId { get; set; }
        public string Description { get; set; }
        public bool IsDebit { get; set; }
        public float Value { get; set; }
        [JsonConverter(typeof(TransactionDefinitionConverter))]
        public ITransactionDefinition TransactionType { get; set; }
        [JsonConverter(typeof(TransactionCategoryConverter))]
        public ITransactionCategory Category { get; set; }
        public List<string> Tags { get; set; }
    }
}
