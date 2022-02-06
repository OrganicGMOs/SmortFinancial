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

    public class FileSaveException : Exception
    {
        internal FileSaveException() { }
        internal FileSaveException(string message) : base(message) { }
        internal FileSaveException(string message, Exception innerException)
            : base(message, innerException) { }
    }
    //ItemDoesNotExistException
    public class ItemDoesNotExistException : Exception
    {
        internal ItemDoesNotExistException() { }
        internal ItemDoesNotExistException(string message) : base(message) { }
        internal ItemDoesNotExistException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
