using Banker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Interfaces
{
    public interface ITransactionParseResult
    {
        //found in the file
        IEnumerable<Transaction> Loaded { get; set; }
        //found in loaded transaction collection
        IEnumerable<Transaction> Duplicate { get; set; }
        //date is older than 365 days away from today (archive item)
        IEnumerable<Transaction> ArchiveItem { get; set; }
        IEnumerable<string> FailedToParse { get; set; }
    }
}
