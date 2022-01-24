using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    public class TransactionQuery
    {
        public DateTime? Start { get; set; }
        public DateTime? Stop { get; set; }
        public string[]? Categories { get; set; }
        public float? Min { get; set; }
        public float? Max { get; set; }
        public Func<Transaction,bool> Query { get; set; }
    }
}
