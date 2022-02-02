using Banker.Apps;
using Banker.Extensions.CustomExceptions;
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
        #region CategoryDefinitions
        public async Task<IBankerResult> CreateCategoryDefinition(ICategoryDefinition category)
        {
            await Task.Yield(); //quick action, wait until awaited
            return RunAction<ICategoryDefinition,Task>(DefinitionsManager.CreateCategory,category);
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
        public async Task<IBankerResult> GetCategoryDefinition(string categoryName)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.GetCategory,categoryName);
        }
        public async Task<IBankerResult> GetCategoryDefinitions() 
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.GetCategoryDefinitions);
        }
        #endregion
        #region TransactionDefinitions
        public async Task<IBankerResult> CreateTransactionDefinition(ITransactionDefinition transaction)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.CreateTransaction,transaction);
        }
        public async Task<IBankerResult> ModifyTransactionDefinition(ITransactionDefinition transaction)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.ModifyTransaction, transaction);
        }
        public async Task<IBankerResult> DeleteTransactionDefinition(ITransactionDefinition transaction)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.DeleteTransaction,transaction);
        }
        public async Task<IBankerResult> GetTransactionDefinition(string transactionKey)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.GetTransaction,transactionKey);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns> IEnumerable TransactionDefinition </returns>
        public async Task<IBankerResult> GetTransactionDefinitions() 
        {
            var result = await RunActionAsync(DefinitionsManager.GetTransactionDefinitions);
            return result;
        }
        #endregion
        #region Transactions
        public async Task<IBankerResult> ParseTransactionCSV(ICSVDefinition definition)
        {
            var result = await RunActionAsync(TransactionManager.ParseTransactionCSV, definition);
            return result;
        }
        public async Task<IBankerResult> GetTransaction(ITransaction transaction) 
        {
            await Task.Yield();
            return RunAction(TransactionManager.GetTransaction, transaction);
        }
        public async Task<IBankerResult> QueryTransactions(ITransactionQuery query) 
        {
            var result = await RunActionAsync(TransactionManager.ProcessQuery, query);
            return result;
        }
        public async Task<IBankerResult> SaveTransactions(IEnumerable<ITransaction> transactions)
        {
            var result = await RunActionAsync(TransactionManager.SaveTransactions, transactions);
            return result;
        }
        public async Task<IBankerResult> QueryTransactions2(ITransactionQuery query)
        {
            await Task.Yield();
            var result = RunAction(TransactionManager.QueryTransactions, query);
            return result;

        }
        #endregion

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
        private IBankerResult RunAction<T>(Func<T> toDo)
        {
            try
            {
                var result = toDo.Invoke();
                return ProduceResult(result);
            }catch(Exception ex)
            {
                return ProduceErrorResult(ex);
            }
        }
        private IBankerResult RunAction<T1,T2>(Func<T1,T2> toDo,T1 input)
        {
            try
            {
                var result = toDo.Invoke(input);
                return ProduceResult(result);
            }
            catch (Exception ex)
            {
                return ProduceErrorResult(ex);
            }
        }   
        //For when an item is a task
        private async Task<IBankerResult> RunActionAsync<T>(Func<Task<T>> toDo)
        {
            try
            {
                var result = await toDo.Invoke();
                return ProduceResult(result);
            }
            catch(Exception ex)
            {
                return ProduceErrorResult(ex);
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
                Console.WriteLine(ex.Message);
                return ProduceErrorResult(ex);
            }
        }
        private IBankerResult ProduceResult<T>(T value)
        {
            Console.WriteLine(typeof(T).Name);
            if (value is T) //will always be true?
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
        //todo: more concise exceptions / error messages
        /// <summary>
        /// Main purpose to generate uniform error banker results 
        /// and to not inadverntly expose 'senstive' data via exception
        /// messages. 
        /// <br />
        /// also enables simple try/catch instead of listing each
        /// exception on the above RunAction methods
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private IBankerResult ProduceErrorResult(Exception ex)
        {
            switch(ex)
            {
                case CategoryNotFoundException:
                    return GenerateResult(false, "Failed to locate record");
                default:
                    return GenerateResult(false, "An error occoured, please try again later");
            }
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
            TransactionManager = await TransactionManager.TransactionManagerFactory(location,this);
        }
        #endregion
    }
}