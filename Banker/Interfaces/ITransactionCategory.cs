using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Interfaces
{
    /// <summary>
    /// The actual impelemntation of a category that applies to a transaction, a flatter version of that model.
    /// 
    /// TransactionDefinition -> has refrence to default category defition
    /// Transaction -> has refrence to transaction definition and specific implementation of transaction category
    /// </summary>
    public interface ITransactionCategory
    {
        public Guid CategoryId { get; set; }
        public string CategoryType { get; set; }
        public string Subtype { get; set; }
        public int Color { get; set; }
        public bool ReductionTarget { get; set; }
    }
}
