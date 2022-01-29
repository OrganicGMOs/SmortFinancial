using Banker.Apps;
using Banker.Interfaces;
using Banker.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Banker
{
    public class BankerInstance
    {
        internal static DefinitionsManager DefinitionsManager;
        internal static TransactionManager TransactionManager;

        //'api' end points are where we could add input checks if it becomes necessary
        #region api
        //no idea if this is a bad design or not...but less try catch for me
        public async Task<IBankerResult> CreateCategoryDefinition(ICategoryDefinition category)
        {
            await Task.Yield(); //quick action, wait until awaited
            return RunAction<ICategoryDefinition,Task>(DefinitionsManager.CreateCategory,category);
        }
        public async Task<IBankerResult> GetCategoryDefinition(string categoryName)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.GetCategory,categoryName);
        }
        public async Task<IBankerResult> ModifyCategory(ICategoryDefinition category)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.ModifyCategory, category);
        }
        public async Task<IBankerResult> DeleteCategory(ICategoryDefinition category)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.DeleteCategory,category);
        }
        public async Task<IBankerResult> QueryTransactions(ITransactionQuery query) 
        {
            try
            {
                var result = await TransactionManager.ProcessQuery(query);
                return GenerateResult(true,string.Empty,new List<object>(result));
            }
            catch (Exception ex)
            {
                return GenerateResult(false, "implement custom exceptions");
            }
        }
        public async Task Close()
        {
            await DefinitionsManager.SaveDefinitions();
        }
        public async Task SaveChanges()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region helpers
        private IBankerResult GenerateResult(bool success = true, 
            string msg = "",List<object>? coll = null, 
            object? value = null)
        {
            var result = new BankerResult
            {
                IsSuccess = success,
                ErrorMessage = msg,
                _dynamicCollection = coll,
                _dynamicObject = value
            };
            return result;
        }
        //I really dont want to do a million try/catch in api
        private IBankerResult RunAction<T1,T2>(Func<T1,T2> toDo,T1 input)
        {
            try
            {
                var result = toDo.Invoke(input);
                return ProduceResult(result);
            }
            catch (Exception ex)
            {
                var result = new BankerResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    _dynamicCollection = null,
                    _dynamicObject = null
                };
                return result;
            }
        }
        private async Task<IBankerResult> RunActionAsync<T1,T2>(Func<T1,Task<T2>>toDo,T1 input)
        {
            try
            {
                var result = await toDo.Invoke(input);
                return ProduceResult(result);
            }
            catch (Exception ex)
            {
                var result = new BankerResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    _dynamicCollection = null,
                    _dynamicObject = null
                };
                return result;
            }
        }
        private IBankerResult ProduceResult<T>(T value)
        {
            if (value is T)
            {
                //is it a collection? 
                if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
                {
                    var obj = (IEnumerable)value;
                    obj.Cast<object>().ToList();
                    return GenerateResult(true, String.Empty, obj.Cast<object>().ToList(), null);
                }
                else
                {
                    return GenerateResult(true, String.Empty, null, value);
                }
            }
            throw new Exception("Expected results did not align with actual results");
        }
        #endregion

        //load the configuration files
        #region init
        public static async Task<BankerInstance> BankerFactory()
        {
            var instance = new BankerInstance();
            await instance.Initialize();
            return instance;
        }
        private async Task Initialize()
        {
            var location = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) 
                + "\\smortFinance";
            DefinitionsManager = await DefinitionsManager.DefinitionsManagerFactory(location);
            TransactionManager = await TransactionManager.TransactionManagerFactory(location);
        }
        #endregion
    }
}