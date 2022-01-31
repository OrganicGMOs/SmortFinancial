using Banker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    public class CSVDefinition : ICSVDefinition
    {
        public string Name { get; set; }
        public bool HeaderPresent { get; set; }
        public int ColumnCount { get; set; }
        public char LineDelimiter { get; set; }
        public int DebitColumn { get; set; }
        public int CreditColumn { get; set; }
        public int DateColumn { get; set; }
        public int Description { get; set; }
        public string DescriptionerDelimiter { get; set; }
        public char ColumnDelimiter { get; set; }
        public int DescriptionColumn { get; set; }
        public string FilePath { get; set; }
    }
}
