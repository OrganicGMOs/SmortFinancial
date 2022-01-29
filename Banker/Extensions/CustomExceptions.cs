using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Extensions.CustomExceptions
{
    public class CategoryNotFoundException : Exception
    {
        internal CategoryNotFoundException () { }
        internal CategoryNotFoundException (string message) : base (message) { }
        internal CategoryNotFoundException (string message,Exception innerException) 
            : base(message, innerException) { }
    }
    public class TransactionNotFoundException : Exception
    {
        internal TransactionNotFoundException() { }
        internal TransactionNotFoundException(string message) : base(message) { }
        internal TransactionNotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
