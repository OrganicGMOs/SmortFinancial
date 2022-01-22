using Banker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Apps
{
    internal class TransactionManager
    {
        private TransactionHistory LoadedTransactions;
        private IEnumerable<Transaction> _lastSaved;
        private string _root;
        private string _transactionDir;
        private string _archive;
        private string _history;
        private DateTime OldestTransaction;
        private DateTime _today;

        internal TransactionManager(string root)
        {
            _root = root;
            _transactionDir = root + "\\transactions";
            _archive = _transactionDir + "\\archive";
            _history = _transactionDir + "\\history.json";
                //keeps last 12 months of data
                //auto archives 
            _today = DateTime.Now;
            _lastSaved = new List<Transaction>();
        }
        private Transaction ParseImportTransaction(string[] values) 
        {
            try
            {
                bool outgoing;
                float value;
                var debit = values[3];
                var credit = values[4];
                if (string.IsNullOrWhiteSpace(debit))
                {
                    outgoing = false;
                    value = float.Parse(credit);
                }
                else
                {
                    outgoing = true;
                    value = float.Parse(debit);
                }
                return new Transaction
                {
                    Date = DateTime.Parse(values[0]),
                    AssignedId = Guid.NewGuid(), //we are loading a new transaction here, give it an ID
                    Description = values[2],
                    IsDebit = outgoing,
                    TransactionType = BankerInstance.DefinitionsManager
                    .GetDefinition(values[2]),
                    Value = value
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Transaction();
            }
        }
        public async Task<IEnumerable<Transaction>> ParseTransactions_CSV(string file)
        {
            
            var reader = new StreamReader(file);
            var transactions = new List<Transaction>();
            var text = (await reader.ReadToEndAsync())
                .Replace("\"", String.Empty);
            var lines = text.Split('\n');
            var lineValues = lines.Select(x => x.Split(',')).ToList();
            lineValues.RemoveAt(lineValues.Count - 1);
            lineValues.RemoveAt(0);
            reader.Dispose();
            foreach (var set in lineValues)
            {
                var transaction = ParseImportTransaction(set);
                if(!CheckForDuplicate(transaction,LoadedTransactions.Transactions))
                    transactions.Add(ParseImportTransaction(set));
                

            }
                
            LoadedTransactions.Transactions.AddRange(transactions);
            return transactions;
        }
        public IEnumerable<Transaction>GetTransactions(TransactionQuery query)
        {
            var items = GetTransactions_TimeSpan(query.Start, query.Stop);
            var result = new List<Transaction>();
            foreach (var item in items)
                if (query.Query.Invoke(item))
                    result.Add(item);
            return result;
        }
        internal int GetActiveCount()
        {
            return LoadedTransactions.Transactions.Count();
        }
        public IEnumerable<Transaction> GetLastSaved()
        {
            return _lastSaved;
        }
        private IEnumerable<Transaction>GetTransactions_TimeSpan(DateTime? start, DateTime? end)
        {
            if (start == null && end == null)
            {
                return LoadedTransactions.Transactions.OrderBy(p => p.Date).ToList();
            }
            else if (start == null || end == null)
            {
                var date = start == null ? end : start;
                return LoadedTransactions.Transactions.Where(p => p.Date < date);
            }
            else
            {
                return LoadedTransactions.Transactions
                    .Where(p => p.Date <= start && p.Date >= end)
                    .ToList();
            }
        }
        internal async Task SaveTransactions(IEnumerable<Transaction> transactions)
        {
            var valid = transactions.Where(p => p != null);
            //not called automatically by ParseTransactions_CSV(), "up to user to save"
            if (valid.Count() <= 0)
            {
                //todo: exception and logging
                return;
            }
            else
            {
                await UpdateSavedTransactions(valid);
                _lastSaved = valid;
            }
        }

        internal IEnumerable<Transaction> TransactionQuery(TransactionQuery query)
        {
            //just going to force uber eats because im curious
            var uber = LoadedTransactions
                .Transactions.Where(p => p.Description
                    .Contains("uber eats", StringComparison.OrdinalIgnoreCase));
            return uber;
        }

        #region FileMangement
        private async Task LoadTransactionHistory()
        {
            LoadedTransactions = await GetOrCreateHistory(_history);
        }
        private async Task UpdateSavedTransactions(IEnumerable<Transaction> transactions)
        {
            //determine which file the transactions fall under
            var fileNames = FindFileLocations(transactions);
            var updateTasks = new List<Task>();
            foreach(var file in fileNames)
                updateTasks.Add(UpdateFile(file.Key, file.Value));
            await Task.WhenAll(updateTasks);
        }
        private Dictionary<string,List<Transaction>> FindFileLocations(IEnumerable<Transaction> transactions)
        {
            var dict = new Dictionary<string, List<Transaction>>();
            foreach (var transaction in transactions.OrderBy(p => p.Date))
            {
                var age = _today.Subtract(transaction.Date).TotalDays;
                //if age is over a year its an archive item
                if (age > 365)
                    UpdateOrCreateDictionaryValue(dict, _archive + "\\"+ transaction.Date.Year.ToString()+".json", transaction);
                else
                    UpdateOrCreateDictionaryValue(dict,_history,transaction);
            }
            return dict;
        }
        private void UpdateOrCreateDictionaryValue(Dictionary<string,List<Transaction>> dict,string key,Transaction transaction)
        {
            if(dict.ContainsKey(key))
                dict[key].Add(transaction);
            else
            {
                var list = new List<Transaction>();
                dict.Add(key, list);
                list.Add(transaction);
            }
        }
        private async Task UpdateFile(string file,IEnumerable<Transaction> transactions)
        {
            int duplicatCount = 0;
            //todo: file size / compression
            var history = await GetOrCreateHistory(file);
            if (history.Transactions.Count > 0)
            {

                foreach (var t in transactions)
                {
                    if(CheckForDuplicate(t,history.Transactions))
                    {
                        //todo: logging and exceptions. enable forced "duplicates"?
                        Console.WriteLine("Duplicate item, count: " + ++duplicatCount);
                    }
                    else
                        history.Transactions.Add(t);
                }
            }
            else
                history.Transactions = transactions.ToList();
            //sort
            history.Transactions.Sort((a, b) =>
            {
                return a.Date.CompareTo(b.Date);
            });
            var json = JsonConvert.SerializeObject(history, Formatting.Indented);
            await Extensions.WriteFile(history.FilePath,json);
        }
        private async Task<TransactionHistory> GetOrCreateHistory(string path)
        {
            if (File.Exists(path))
            {
                var json = await Extensions.ReadFile(path);
                if (json != String.Empty)
                {
                    var history = JsonConvert.DeserializeObject<TransactionHistory>(json);
                    if (history != null)
                        return history;
                    else
                        return StartHistory(path);
                }
                else 
                    return StartHistory(path);
            }
            else
                return StartHistory(path);
        }
        private TransactionHistory StartHistory(string path)
        {
            return new TransactionHistory()
            {
                FilePath = path,
                Modified = DateTime.Now,
                Transactions = new List<Transaction>()
            };
        }
        #endregion
        private bool CheckForDuplicate(Transaction transaction, IEnumerable<Transaction> transactions)
        {
            var match = transactions.Where(p =>
            {
                if (p.Date == transaction.Date
                    && p.IsDebit == transaction.IsDebit
                    && p.Value == transaction.Value
                    && p.Description == transaction.Description)
                    return true;
                else
                    return false;
            });
            if (match.Count() > 0)
                return true;
            else
                return false;
        }
        #region Initialization
        public static async Task<TransactionManager> TransactionManagerFactory(string root)
        {
            var manager = new TransactionManager(root);
            await manager.initialize();
            return manager;
        }
        private async Task initialize()
        {
            if(!Directory.Exists(_transactionDir))
                Directory.CreateDirectory(_transactionDir);
            if(!Directory.Exists(_archive))
                Directory.CreateDirectory(_archive);
            await LoadTransactionHistory();
        }
        #endregion
    }
}
