using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Interfaces
{
    public interface IDocumentSaveResult
    {
        bool Success { get; set; }
        string FileName { get; set; }
        string FilePath { get; set; }
        string? ErrorMessage { get; set; }
    }
}
