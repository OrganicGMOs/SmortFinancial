using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Interfaces
{
    /// <summary>
    /// TransactionDefinition represents a vendor that can/does pop up
    /// in your transaction history. it will always have the same key, as thats how
    /// the banks ID it. End users can manipulate the keys
    /// </summary>
    public interface ITransactionDefinition
    {
        public Guid AssignedId { get; set; }
        public Guid MappedCategory { get; set; }
        public string Key { get; set; }
        public bool ReductionTarget { get; set; }
        
    }
}
