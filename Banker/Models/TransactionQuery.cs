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
        public Guid? TransactionId { get; set; }
        public Guid? CategoryId { get; set; }
        public string? SubCategory { get; set; }
        public IEnumerable<string>? Tags { get; set; }
        public bool? CreditOnly { get; set; }
        public bool? DebitOnly { get; set; }
        public List<Guid>? TransactionIds { get; set; }
        public List<Guid>? CategoryIds { get; set; }
        List<string>? ITransactionQuery.SubCategory { get; set; }
        List<string>? ITransactionQuery.Tags { get; set; }
    }
}
