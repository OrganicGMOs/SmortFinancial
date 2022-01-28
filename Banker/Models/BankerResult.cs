using Banker.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Models
{
    internal class BankerResult : IBankerResult
    {
        internal List<object>? _dynamicCollection;
        internal object? _dynamicObject;
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }

        public T GetValue<T>()
        {
            if (_dynamicObject == null)
                return default;
            if(_dynamicObject is T)
                return (T)_dynamicObject;
            else 
                return default;
        }

        public IEnumerable<T> GetValueCollection<T>()
        {
            if(_dynamicCollection != null)
            {
                var first = _dynamicCollection
                    .FirstOrDefault();
                if (first is T)
                    return _dynamicCollection.Cast<T>();
                else
                    return Enumerable.Empty<T>();
            }
            else 
                return Enumerable.Empty<T>();
        }
    }
}
