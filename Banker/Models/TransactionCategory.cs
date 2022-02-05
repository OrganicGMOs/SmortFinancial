﻿using Banker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    public class TransactionCategory : ITransactionCategory
    {
        public Guid CategoryId { get; set; }
        public string CategoryType { get; set; }
        public string Subtype { get; set; }
        public bool ReductionTarget { get; set; }
    }
}
