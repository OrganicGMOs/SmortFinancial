using Banker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    internal class DocumentSaveModel : IDocumentSaveResult
    {
        public bool Success { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string? ErrorMessage { get; set; }

        public DocumentSaveModel(bool success, string filePath, string? error = null)
        {
            Success = success;
            FilePath = filePath;
            ErrorMessage = error;
            FileName = Extensions.Functions.GetFileNameFromPath(filePath);
        }
    }
}
