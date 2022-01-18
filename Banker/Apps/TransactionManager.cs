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
        private List<Transaction> LoadedTransactions;
        private string _root;
        private string _transactionDir;
        private string _archive;
        private DateTime OldestTransaction;
        private DateTime _today;
        private string[] names = {"0.json", "1.json","2.json","3.json","4.json",
            "5.json","6.json","7.json","8.json","9.json","10.json","11.json","12.json"};

        internal TransactionManager(string root)
        {
            _root = root;
            _transactionDir = root + "\\transactions";
            _archive = _transactionDir + "\\archive";
                //keeps last 12 months of data
                //auto archives 
            _today = DateTime.Now;
        }
        private Transaction ParseTransaction(string[] values) 
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
                    Number = values[1],
                    Description = values[2],
                    Debit = outgoing,
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
                transactions.Add(ParseTransaction(set));
            LoadedTransactions.AddRange(transactions);
            return transactions;
        }
        public IEnumerable<Transaction>GetTransactions(TransActQuery query)
        {
            var items = GetTransactions_TimeSpan(query.Start, query.Stop);
            var result = new List<Transaction>();
            foreach (var item in items)
                if (query.Query.Invoke(item))
                    result.Add(item);
            return result;
        }
        private IEnumerable<Transaction>GetTransactions_TimeSpan(DateTime? start, DateTime? end)
        {
            if (start == null && end == null)
            {
                return LoadedTransactions.OrderBy(p => p.Date).ToList();
            }
            else if (start == null || end == null)
            {
                var date = start == null ? end : start;
                return LoadedTransactions.Where(p => p.Date < date);
            }
            else
            {
                return LoadedTransactions
                    .Where(p => p.Date <= start && p.Date >= end)
                    .ToList();
            }
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
            LoadedTransactions = new List<Transaction>();
            await LoadStaticFiles();
        }
        private async Task LoadStaticFiles()
        {
            var contents = new List<Task<string>>();
            var transactions = new List<Transactiondefinition[]>();
            foreach (var file in names)
            {
                if(File.Exists(_transactionDir + "\\" +file))
                    contents.Add(
                        File.ReadAllTextAsync(_transactionDir + "\\" + file));
                else
                    File.Create(_transactionDir + "\\" + file);
            }
            var fileContents = await Task.WhenAll(contents);
            foreach(var file in fileContents)
            {
                var content = JsonConvert
                    .DeserializeObject<TransactionDefinitions>(file);
                if (content != null)
                    LoadedTransactions.Add(new Transaction());
            }

        }
        #endregion
    }
    public class Transaction
    {
        public DateTime Date { get; set; }
        public string Number { get; set; } //blank in test data
        public Transactiondefinition TransactionType { get; set; }
        public string Description { get; set; }
        public bool Debit { get; set; }
        public float Value { get; set; }
    }
}
