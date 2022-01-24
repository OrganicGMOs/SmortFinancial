using Banker.Apps;
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