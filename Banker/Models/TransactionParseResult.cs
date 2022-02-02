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
        public IEnumerable<Transaction> Loaded { get; set; }
        public IEnumerable<Transaction> Duplicate { get; set; }
        public IEnumerable<Transaction> ArchiveItem { get; set; }
        public IEnumerable<string> FailedToParse { get; set; }

        public TransactionParseResult()
        {
            Loaded = new List<Transaction>();
            Duplicate = new List<Transaction>();
            ArchiveItem = new List<Transaction>();
            FailedToParse = new List<string>();
        }
    }
}
