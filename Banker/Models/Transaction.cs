using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    public class Transaction
    {
        public DateTime Date { get; set; }
        public Guid AssignedId { get; set; }
        public string Description { get; set; }
        public bool IsDebit { get; set; }
        public float Value { get; set; }
        public TransactionDefinition TransactionType { get; set; }
        public TransactionCategory Category { get; set; }
    }
}
