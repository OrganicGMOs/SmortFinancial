using Banker.Extensions;
using Banker.Extensions.CustomExceptions;
using Banker.Interfaces;
using Banker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Banker.Apps
{
    internal class TransactionManager
    {
        private BankerInstance Banker;
        private TransactionHistory LoadedTransactions;
        //gets cleared out after a save
        private List<ITransaction> NewTransactions;
        private List<ITransaction> DeletedTransactions;
        private string _root;
        private string _transactionDir;
        private string _archive;
        private string _history;
        private DateTime OldestTransaction;
        private DateTime _today;

        internal TransactionManager(string root, BankerInstance banker)
        {
            _root = root;
            _transactionDir = root + "\\transactions";
            _archive = _transactionDir + "\\archive";
            _history = _transactionDir + "\\history.json";
                //keeps last 12 months of data
                //auto archives 
            _today = DateTime.Now;
            NewTransactions = new List<ITransaction>();
            DeletedTransactions = new List<ITransaction>();
            Banker = banker;
        }
        public ITransaction? GetTransaction(ITransaction transaction)
        {
            var item = LoadedTransactions.Transactions
                .Where(p => p.AssignedId == transaction.AssignedId)
                .FirstOrDefault();
            return item;
        }
        internal int GetActiveCount()
        {
            return LoadedTransactions.Transactions.Count();
        }
        private bool CheckForDuplicate(ITransaction transaction, IEnumerable<ITransaction> transactions)
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
        private bool CheckForArchiveItem(ITransaction transaction, DateTime refrencePoint)
        {
            if (refrencePoint.Subtract(transaction.Date).TotalDays > 365)
                return true;
            else return false;
        }
        //save redo and access redo
        internal Task UpdateTransactionHistory(IEnumerable<ITransaction> transactions)
        {
            NewTransactions.AddRange(transactions);
            OldestTransaction = NewTransactions.OrderBy(p => p.Date).Last().Date;
            return Task.CompletedTask;
        }
        internal Task DeleteTransaction(ITransaction transction)
        {
            var loaded = LoadedTransactions.Transactions
                .Where(p => p.AssignedId == transction.AssignedId).FirstOrDefault();
            if(loaded != null)
            {
                LoadedTransactions.Transactions.Remove(loaded);
                DeletedTransactions.Add(loaded);
            }
            return Task.CompletedTask;
        }
        internal Task DeleteTransactions(List<ITransaction> transactions)
        {
            foreach(var transaction in transactions)
                DeleteTransaction(transaction);
            return Task.CompletedTask;
        }


        #region FileMangement
        //up to library user to save changes or not
        internal async Task<IEnumerable<IDocumentSaveResult>> SaveTransactions()
        {
            return await UpdateSavedTransactions(LoadedTransactions.Transactions);
        }
        private async Task LoadTransactionHistory()
        {
            LoadedTransactions = await GetOrCreateHistory(_history);
        }
        private async Task<IEnumerable<IDocumentSaveResult>> UpdateSavedTransactions(IEnumerable<ITransaction> transactions)
        {
            //determine which file the transactions fall under
            var fileNames = FindFileLocations(transactions);
            var results = new List<DocumentSaveModel>();
            foreach(var file in fileNames)
            {
                try
                {
                    var ordered = file.Value.OrderByDescending(p => p.Date);
                    var history = new TransactionHistory
                    {
                        FilePath = file.Key,
                        Modified = DateTime.Now,
                        NeedsWrite = false,
                        OldestRecord = ordered.Count() > 0 ? ordered.First().Date : DateTime.MinValue,
                        Transactions = ordered.ToList()
                    };
                    await WriteHistory(history);
                    results.Add(new DocumentSaveModel(true, file.Key));
                }
                catch(Exception ex)
                {
                    //todo: catch different exceptions
                    results.Add(new DocumentSaveModel(false,file.Key,ex.Message));
                }
            }
            //if there is nothing to write to the main "history" file that means all transactions are a year+.
                // need to clear the history file transactions.
            if(!fileNames.ContainsKey(_history))
            {
                
                try
                {
                    var history = new TransactionHistory()
                    {
                        FilePath = _history,
                        Modified = DateTime.Now,
                        NeedsWrite = false,
                        OldestRecord = DateTime.MinValue,
                        Transactions = new List<ITransaction>()
                    };
                    await WriteHistory(history);
                    results.Add(new DocumentSaveModel(true, _history));
                }
                catch(Exception ex)
                {
                    results.Add(new DocumentSaveModel(false, _history, "Failed to clear history file"));
                }
            }
            return results;
        }
        private Dictionary<string,List<ITransaction>> FindFileLocations(IEnumerable<ITransaction> transactions)
        {
            var dict = new Dictionary<string, List<ITransaction>>();
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
        private void UpdateOrCreateDictionaryValue(Dictionary<string,List<ITransaction>> dict,string key,ITransaction transaction)
        {
            if(dict.ContainsKey(key))
                dict[key].Add(transaction);
            else
            {
                var list = new List<ITransaction>();
                dict.Add(key, list);
                list.Add(transaction);
            }
        }
        private async Task WriteHistory(TransactionHistory history)
        {
            var json = Functions.ParseJson<TransactionHistory>(history);
            await Functions.WriteFile(history.FilePath, json);

        }
        private async Task<TransactionHistory> GetOrCreateHistory(string path)
        {
            var json = await Extensions.Functions.ReadFile(path);
            TransactionHistory? history = null;
            if (json == null)
                history = StartHistory(path, true); //file dont exist
            else if(string.IsNullOrEmpty(json))
                history = StartHistory(path);       //file exist but couldnt read for somereason
            else
            {
                try
                {
                    history = Extensions.Functions.ParseJson<TransactionHistory>(json);
                    if (history == null)
                        return StartHistory(path); //just return and dont overwrite. makes complier happy
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
            return history;
        }
        private TransactionHistory StartHistory(string path,bool needsCreation = false)
        {
            return new TransactionHistory()
            {
                FilePath = path,
                NeedsWrite = needsCreation,
                Modified = DateTime.Now,
                Transactions = new List<ITransaction>()
            };
        }
        #endregion
        #region CSVParsing
        internal async Task<ITransactionParseResult> ParseTransactionCSV(ICSVDefinition definition)
        {
            var reader = new StreamReader(definition.FilePath);
            var text = await reader.ReadToEndAsync();
            var transactions = ParseCSVString(definition, text);
            return transactions;
        }
        internal ITransactionParseResult ParseCSVString(ICSVDefinition definition, string value)
        {
            var transactions = new List<ITransaction>();
            var duplicates = new List<ITransaction>();
            var archive = new List<ITransaction>();
            var failed = new List<string>();
            var lines = value.Split(definition.LineDelimiter).ToList();
            //is the last line item blank?
            if (definition.HeaderPresent)
                lines.RemoveAt(0);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line.Replace(definition.ColumnDelimiter, ' ')))
                    break;
                else
                {
                    ITransaction? transaction = null;
                    try
                    {
                        var columns = line.Split(definition.ColumnDelimiter);
                        transaction = ParseCSVLine(columns, definition);
                    }
                    catch (Exception ex)
                    {
                        failed.Add(line);
                        //failed parsing
                    }
                    if (transaction != null)
                    {
                        transactions.Add(transaction);
                        if (CheckForDuplicate(transaction, LoadedTransactions.Transactions))
                            duplicates.Add(transaction);
                        else if (CheckForArchiveItem(transaction, LoadedTransactions.Modified))
                            archive.Add(transaction);
                    }
                }
            }

            return new TransactionParseResult
            {
                ArchiveItem = archive,
                Duplicate = duplicates,
                Loaded = transactions
            };
        }
        internal ITransaction ParseCSVLine(string[] data, ICSVDefinition defintion)
        {
            bool outgoing;
            float value;
            var debit = data[defintion.DebitColumn - 1];
            var credit = data[defintion.CreditColumn - 1];
            var description = data[defintion.DescriptionColumn - 1];
            var transactionType = BankerInstance.DefinitionsManager
                .GetTransaction(description);
            if (transactionType == null)
                transactionType = new TransactionDefinition();
            var category = BankerInstance.DefinitionsManager
                .GetSlimCategory(transactionType);
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
                Date = DateTime.Parse(data[defintion.DateColumn - 1]),
                AssignedId = Guid.NewGuid(), //we are loading a new transaction here, give it an ID
                Description = description,
                IsDebit = outgoing,
                TransactionType = transactionType,
                Category = category,
                Tags = transactionType.DefaultTags,
                Value = value
            };
        }
        #endregion
        #region SearchQuery two
        //lots of linq queries, a foreach is probably better
            //todo: possible move to task.run since no cap on transactions...could take a while
                //todo: a callback method for updates if it takes a really long time?
        internal IEnumerable<ITransaction> QueryTransactions(ITransactionQuery query)
        {
            var matches = new List<ITransaction>();
            foreach (var r in LoadedTransactions.Transactions)
            {
                //matches all options or not at all. non specified options = true
                if(DateCheck(r,query.NotBefore,query.NotAfter))
                    if(PriceCheck(r,query.Min,query.Max))
                        if(CheckMembership<Guid>(r.TransactionType.AssignedId,query.TransactionIds))
                            if(CheckMembership<Guid>(r.Category.CategoryId,query.CategoryIds))
                                if(CheckMembership<string>(r.Category.Subtype,query.SubCategory))
                                    if(CheckName(r,query.Name))
                                        if(TagCheck(r,query.Tags))
                                            matches.Add(r);
            }
            return matches;
        }
        private bool DateCheck(ITransaction record, DateTime? start, DateTime? end)
        {
            if (start == null && end == null) //get everything
                return true;
            else
            {
                if (start == null)
                    return record.Date <= end;
                else if (end == null)
                    return record.Date >= start;
                else
                    return record.Date >= start && record.Date <= end;
            }
        }
        private bool PriceCheck(ITransaction record, float? min, float? max)
        {
            if(min == null && max == null)
                return true;
            else
            {
                if (min == null)
                    return record.Value <= max;
                else if (max == null)
                    return record.Value >= min;
                else
                    return record.Value <= max && record.Value >= min;
            }
        }
        private bool CheckMembership<T>(T value, IEnumerable<T>? collection)
        {
            if (collection == null)
                return true;
            else
                return collection.Contains(value);
        }
        private bool CheckName(ITransaction record, string? search)
        {
            if (search == null)
                return true;
            else
            {
                if (string.Equals(record.TransactionType.Key, search, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (record.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        private bool TagCheck(ITransaction record, IEnumerable<string>? tags)
        {
            if (tags == null)
                return true;
            else
            {
                foreach (var tag in record.Tags)
                    if (tags.Contains(tag))
                        return true;
            }
            return false;
        }
        #endregion
        #region Initialization
        public static async Task<TransactionManager> TransactionManagerFactory(string root, BankerInstance banker)
        {
            var manager = new TransactionManager(root,banker);
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
