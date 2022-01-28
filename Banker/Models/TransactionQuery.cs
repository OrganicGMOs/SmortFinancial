using Banker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    public class TransactionQuery : ITransactionQuery
    {
        public DateTime? NotBefore { get; set; }
        public DateTime? NotAfter { get; set; }
        public float? Min { get; set; }
        public float? Max { get; set; }
        public bool? CreditOnly { get; set; }
        public bool? DebitOnly { get; set; }
        public List<Guid>? TransactionIds { get; set; }
        public List<Guid>? CategoryIds { get; set; }
        public List<string>? SubCategory { get; set; }
        public List<string>? Tags { get; set; }
    }
}
