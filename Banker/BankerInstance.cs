using Banker.Apps;
using Banker.Interfaces;
using Banker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Banker
{
    public class BankerInstance
    {
        internal static DefinitionsManager DefinitionsManager;
        internal static TransactionManager TransactionManager;

        #region api
        public async Task<IEnumerable<Transaction>> LoadNewTransactionFile(string file)
        {
            //make sure file exists
            if (File.Exists(file))
                try
                {
                    var t = await TransactionManager.ParseTransactions_CSV(file);
                    return t;
                }
                catch (Exception ex)
                {
                    return null;
                }
            else
                throw new Exception("File doesnt exist");
                
        }
        public async Task SaveTransactions(IEnumerable<Transaction> transactions)
        {
            try
            {
                //todo: api return types
                await TransactionManager.SaveTransactions(transactions);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
        public IEnumerable<Transaction> GetLastSavedTransactions()
        {
            return TransactionManager.GetLastSaved();
        }
        public IEnumerable<Transaction> LoadTransactionFiles(string[] files) { throw new NotImplementedException(); }
        //todo: replace hard code look for uber eats lol
        public IEnumerable<Transaction> Search_DateRange(TransactionQuery query)
        {
            return TransactionManager.TransactionQuery(query);
        }
        public IEnumerable<Transaction> GetOutgoing(DateTime? start, DateTime? stop) 
        {
            return TransactionManager.GetTransactions(new TransactionQuery()
            {
                Start = start,
                Stop = stop,
                Query = new Func<Transaction, bool>((p) =>
                {
                    return p.IsDebit;
                })
            });
        }
        public IEnumerable<Transaction> GetIncomming() { throw new NotImplementedException(); }
        public IEnumerable<Transaction> GetReduceTargets() { throw new NotImplementedException(); }
        public IEnumerable<Transaction> GetSummary() { throw new NotImplementedException(); }
        public IEnumerable<Transaction> GetCategory(string category) { throw new NotImplementedException(); }
        //todo: Category query class?
        public IEnumerable<CategoryDefinition>GetCategories() 
        {
            return DefinitionsManager.GetCategories();
        }
        //todo: clean query system 
        public IEnumerable<TransactionDefinition>GetTransactionsDefs()
        {
            return DefinitionsManager.GetTransactionDefinitions();
        }
        public IEnumerable<Transaction> GetTransactions()
        {
            return TransactionManager.GetTransactions(new TransactionQuery
            {
                Start = null,
                Stop = null,
                Query = ((p) => { return true; })
            });
        }
        public int GetLoadedTransactionCount()
        {
            return TransactionManager.GetActiveCount();
        }
        public bool CreateCategoryDefinition(CategoryDefinition def)
        {
            return true;
        }
        public bool CreateTrasactionDefinition()
        {
            return true;
        }
        #endregion

        //'api' end points are where we could add input checks if it becomes necessary
        #region api2
        public async Task<IBankerResult> CreateCategoryDefinition(ICategoryDefinition category)
        {
            try
            {
                await Task.Yield();
                var cat = new CategoryDefinition()
                {
                    AssignedId = Guid.NewGuid(),
                    CategoryType = category.Name,
                    SubTypes = category.Tags,
                    ColorHex = category.Color,
                    ReductionTarget = category.ReductionTarget
                };
                DefinitionsManager.CreateCategory(cat);
                return GenerateResult();
            }
            catch (Exception ex)
            {
                return GenerateResult(false, "implement custom exceptions");
            }
        }
        public async Task<IBankerResult> GetCategoryDefinition(string categoryName)
        {
            var category = DefinitionsManager.GetCategory(categoryName);
            await Task.Yield();
            return GenerateResult(value: category);
        }

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
        private async Task LoadTransactionCategories(string root)
        {

        }
        private async Task LoadTransactionDefinitions()
        {

        }
        #endregion
    }
}