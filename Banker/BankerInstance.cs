using Banker.Apps;
using Banker.Extensions.CustomExceptions;
using Banker.Interfaces;
using Banker.Models;
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
        /// <summary>
        /// Accepts/Creates a categor definition
        /// </summary>
        /// <param name="category"></param>
        /// <returns>IBankerResult value <br /> 
        /// BankerResult => Value of Task
        /// </returns>
        public async Task<IBankerResult> CreateCategoryDefinition(ICategoryDefinition category)
        {
            await Task.Yield(); //quick action, wait until awaited
            return RunAction<ICategoryDefinition,Task>(DefinitionsManager.CreateCategory,category);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Saves all documents with pending changes.
        /// </summary>
        /// <returns>IBankerResult collection <br /> 
        /// BankerResult => ValueCollection of IDocumentSaveResult
        /// </returns>
        public async Task<IEnumerable<IBankerResult>> SaveChanges()
        {
            var definitions = await RunActionAsync(DefinitionsManager.SaveDefinitions)
                .ConfigureAwait(false);
            var transactions = await RunActionAsync(TransactionManager.SaveTransactions);
            return new List<IBankerResult> { definitions,transactions };
        }
        #endregion
        #region TransactionDefinitions
        /// <summary>
        /// Creates/accepts a transaction defition
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>IBankerResult value <br /> 
        /// BankerResult => Value of Task
        /// </returns>
        public async Task<IBankerResult> CreateTransactionDefinition(ITransactionDefinition transaction)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.CreateTransaction,transaction);
        }
        /// <summary>
        /// Removes a transaction definition from memory/file
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>IBankerResult value <br /> 
        /// BankerResult => Value of Task
        /// </returns>
        public async Task<IBankerResult> DeleteTransactionDefinition(ITransactionDefinition transaction)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.DeleteTransaction,transaction);
        }
        /// <summary>
        /// Gets a transaction defintion based on name/key
        /// </summary>
        /// <param name="transactionKey"></param>
        /// <returns>IBankerResult value <br /> 
        /// BankerResult => Value of ITransactionDefinition
        /// </returns>
        public async Task<IBankerResult> GetTransactionDefinition(string transactionKey)
        {
            await Task.Yield();
            return RunAction(DefinitionsManager.GetTransaction,transactionKey);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>IBankerResult ValueCollection <br /> 
        /// BankerResult => ValueCollection of ITransactionDefintion
        /// </returns>
        /// <returns> IEnumerable TransactionDefinition </returns>
        public async Task<IBankerResult> GetTransactionDefinitions() 
        {
            var result = await RunActionAsync(DefinitionsManager.GetTransactionDefinitions);
            return result;
        }
        #endregion
        #region Transactions
        /// <summary>
        /// Parses transactions based off string or file and CSV definition
        /// </summary>
        /// <param name="definition"></param>
        /// <returns>IBankerResult value <br /> 
        /// BankerResult => Value of ITransactionParseResults
        /// </returns>
        public async Task<IBankerResult> ParseTransactionCSV(ICSVDefinition definition)
        {
            var result = await RunActionAsync(TransactionManager.ParseTransactionCSV, definition);
            return result;
        }
        /// <summary>
        /// Gets a transaction that matches inputted ITransaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>IBankerResult value <br /> 
        /// BankerResult => Value of ITransaction
        /// </returns>
        public async Task<IBankerResult> GetTransaction(ITransaction transaction) 
        {
            await Task.Yield();
            return RunAction(TransactionManager.GetTransaction, transaction);
        }
        /// <summary>
        /// returns a set of ITransactions that matches query parameters
        /// </summary>
        /// <param name="query">Null query values result in true for all transactions</param>
        /// <returns>IBankerResult Collection <br /> 
        /// BankerResult => ValueCollection of ITransaction
        /// </returns>
        public async Task<IBankerResult> QueryTransactions(ITransactionQuery query)
        {
            await Task.Yield();
            var result = RunAction(TransactionManager.QueryTransactions, query);
            return result;

        }
        /// <summary>
        /// adds ITransaction collection to loaded transactions collection. Once added,
        /// will be written to file when SaveChanges is called
        /// </summary>
        /// <param name="transactions"></param>
        /// <returns>IBankerResult value <br /> 
        /// BankerResult => Value of Task
        /// </returns>
        public async Task<IBankerResult> AddTransactions(IEnumerable<ITransaction> transactions)
        {
            await Task.Yield();
            var result = RunAction(TransactionManager.UpdateTransactionHistory, transactions);
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
                return ProduceErrorResult(ex);
            }
        }
        private IBankerResult ProduceResult<T>(T value)
        {
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
                case InvalidDataException:
                    return GenerateResult(false, "Invalid data was encountered");
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