﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Interfaces
{
    public interface ICSVDefinition
    {
        public string FilePath { get; set; }
        //navy federal
        public string Name { get; set; }
        //true
        public bool HeaderPresent { get; set; }
        //5
        public int ColumnCount { get; set; }
        // \n 
        public char LineDelimiter { get; set; }
        public char ColumnDelimiter { get; set; }
        //4
        public int DebitColumn { get; set; }
        //5
        public int CreditColumn { get; set; }
        //1
        public int DateColumn { get; set; }
        //3
        public int DescriptionColumn { get; set; }
        //-
        public string DescriptionerDelimiter { get; set; }
    }
}
