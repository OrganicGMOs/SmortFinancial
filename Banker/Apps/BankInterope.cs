using Banker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Apps
{
    /// <summary>
    /// BankInterope has the functions used to translate data from the bank
    /// to a more useful format. Acts as manager of transaction data and search
    /// functions 
    /// 
    /// When provided a csv of transactions will:
    ///     1. read all debits and credits
    ///     2. map debits to new/known transactions/stores/item
    ///         2.1 Tally and return categories of spending
    ///     3. exports data to json
    /// </summary>
    internal class BankInterope
    {
        private ICollection<Transaction> LoadedTransactions;
        public IEnumerable<Transaction> ParseTransactions(string file)
        {
            var reader = new StreamReader(file);
            var transactions = new List<Transaction>();
            //based of bank csv file...room for improvement
            var text = reader.ReadToEnd().Replace("\"",String.Empty);
            var lines = text.Split('\n');
            var lineValues = lines.Select(x => x.Split(',')).ToList();
            string[] values = new string[5];
            lineValues.RemoveAt(lineValues.Count - 1);
            lineValues.RemoveAt(0);
            /* Bank uses columns:
             * Date, No., Description, Debit, Credit
             *   standard for banks?
             */
            reader.Close();
            foreach(var set in lineValues)
            {
                transactions.Add(ParseTransaction(set));
            }
            LoadedTransactions = transactions;
            return transactions;

        }
        //todo map values to data arrays and make more generic
        private Transaction? ParseTransaction(string[] values)
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
                    Debit = outgoing,
                    TransactionType = GetTransactionDefinition(values[2]),
                    Value = value
                };
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Transaction();
            }
        }
        private Transactiondefinition? GetTransactionDefinition(string key)
        {
            throw new NotImplementedException();
        } 
        private void SaveTransactions(ICollection<Transaction> transactions)
        {
            //break transactions up into their months
            //open save folder,open year folder, see if files exist or not
            //create or update file
        }
        public ICollection<Transaction> SearchTransactions(string key)
        {
            var match = LoadedTransactions.Where(p => p.TransactionType.Key == key);
            return match.ToList();
        }
        public IEnumerable<Transaction>TransactionQuery(TransActQuery query)
        {
            var items = GetTransactions_TimeSpan(query.Start,query.Stop);
            var result = new List<Transaction>();
            foreach(var item in items)
                if(query.Query.Invoke(item))
                    result.Add(item);
            return result;

        }
        public IEnumerable<Transaction> GetTransactions_TimeSpan(DateTime? start, DateTime? end)
        {
            if(start == null && end == null)
            {
                return LoadedTransactions.OrderBy(p => p.Date).ToList();
            }
            else if(start == null || end == null)
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
    }
    public class TransActQuery
    {
        public DateTime? Start { get; set; }
        public DateTime? Stop { get; set; }
        public Func<Transaction,bool> Query { get; set; }
    }
    internal enum TransactionType
    {
        //food
        Grocery,
        Restuaraunt,
        Orderingout, // fucking uber
        Subscription, 
        Internet,
        StudentLoan,
        Car,
        Insurnace,
        Phone,
    }
}
