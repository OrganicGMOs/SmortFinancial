using Banker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Interfaces
{
    public interface ITransaction
    {
        public DateTime Date { get; set; }
        public Guid AssignedId { get; set; }
        public string Description { get; set; }
        public bool IsDebit { get; set; }
        public float Value { get; set; }
        public ITransactionDefinition TransactionType { get; set; }
        public ITransactionCategory Category { get; set; }
        public List<string> Tags { get; set; }
    }
}
