using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Interfaces
{
    public interface IBankerResult
    {
        bool IsSuccess { get; }
        string ErrorMessage { get; }
        IEnumerable<T> GetValueCollection<T>();
        T GetValue<T>();
    }
}
