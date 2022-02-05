using Banker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    internal class TransactionParseResult : ITransactionParseResult
    {
        public IEnumerable<ITransaction> Loaded { get; set; }
        public IEnumerable<ITransaction> Duplicate { get; set; }
        public IEnumerable<ITransaction> ArchiveItem { get; set; }
        public IEnumerable<string> FailedToParse { get; set; }

        public TransactionParseResult()
        {
            Loaded = new List<ITransaction>();
            Duplicate = new List<ITransaction>();
            ArchiveItem = new List<ITransaction>();
            FailedToParse = new List<string>();
        }
    }
}
