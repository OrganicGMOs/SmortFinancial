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
        IEnumerable<ITransaction> Loaded { get; set; }
        //found in loaded transaction collection
        IEnumerable<ITransaction> Duplicate { get; set; }
        //date is older than 365 days away from today (archive item)
        IEnumerable<ITransaction> ArchiveItem { get; set; }
        IEnumerable<string> FailedToParse { get; set; }
    }
}
