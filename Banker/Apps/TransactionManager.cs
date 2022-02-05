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
    //dispoable so we can save files at the end. google says its a bad idea, but I just wanna make
    //sure the files get updated at the end
    internal class TransactionManager
    {
        private BankerInstance Banker;
        private TransactionHistory LoadedTransactions;
        private IEnumerable<ITransaction> PendingSave;
        private IEnumerable<ITransaction> _lastSaved;
        //not used yet, might be easier instead of TransactionHistory
        private IEnumerable<ITransaction> Loaded;
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
            _lastSaved = new List<Transaction>();
            Banker = banker;
        }

        #region CSVParsing
        internal async Task<ITransactionParseResult> ParseTransactionCSV(ICSVDefinition definition)
        {
            var reader = new StreamReader(definition.FilePath);
            var text = await reader.ReadToEndAsync();
            var transactions = ParseCSVString(definition, text);
            return transactions;
        }
        internal ITransactionParseResult ParseCSVString(ICSVDefinition definition,string value)
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
                    catch(Exception ex)
                    {
                        failed.Add(line);
                        //failed parsing
                    }
                    if(transaction != null)
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
            var debit = data[defintion.DebitColumn-1];
            var credit = data[defintion.CreditColumn-1];
            var description = data[defintion.DescriptionColumn-1];
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
                Date = DateTime.Parse(data[defintion.DateColumn -1]),
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
        public IEnumerable<ITransaction> GetLastSaved()
        {
            return _lastSaved;
        }
        private IEnumerable<ITransaction>GetTransactions_TimeSpan(DateTime? start, DateTime? end)
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
        internal async Task<IEnumerable<string>> SaveTransactions(IEnumerable<ITransaction> transactions)
        {
            var valid = transactions.Where(p => p != null);
            //not called automatically by ParseTransactions_CSV(), "up to user to save"
            if (valid.Count() <= 0)
                throw new FileSaveException("Invalid input");
            else
            {
                var files = await UpdateSavedTransactions(valid);
                _lastSaved = valid;
                return files;
            }
        }
 
        #region FileMangement
        private async Task LoadTransactionHistory()
        {
            LoadedTransactions = await GetOrCreateHistory(_history);
        }
        private async Task<IEnumerable<string>> UpdateSavedTransactions(IEnumerable<ITransaction> transactions)
        {
            //determine which file the transactions fall under
            var fileNames = FindFileLocations(transactions);
            var updateTasks = new List<Task>();
            foreach(var file in fileNames)
                updateTasks.Add(UpdateFile(file.Key, file.Value));
            await Task.WhenAll(updateTasks);
            return fileNames.Keys.ToList();
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
        private async Task UpdateFile(string file,IEnumerable<ITransaction> transactions)
        {
            //todo: file size / compression
            var history = await GetOrCreateHistory(file);
            if (history.Transactions.Count > 0)
            {
                //dont care about duplicates here, user should have been notified after parsing
                foreach (var t in transactions)
                    history.Transactions.Add(t);
            }
            else
                history.Transactions = transactions.ToList();
            history.Transactions.Sort((a, b) =>
            {
                return a.Date.CompareTo(b.Date);
            });
            var json = JsonSerializer.Serialize(history, new JsonSerializerOptions
            {
                WriteIndented = true,
            });
            await Extensions.Functions.WriteFile(history.FilePath, json);
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
        //probably a much better way to do this
        internal async Task<IEnumerable<Transaction>> ProcessQuery(ITransactionQuery query)
        {
            await Task.Yield();
            //lol kinda fun to write that. main filter
            //var transactions = FilterDebitCredit(
            //    PriceFilter(
            //        QueryDates(
            //            query.NotBefore,query.NotAfter),
            //        query.Min,query.Max),query.DebitOnly,query.CreditOnly);
            //transactions = FilterTransactionDefinitions(transactions,query.TransactionIds);
            //transactions = FilterCategoryDefinitions(transactions,query.CategoryIds);
            //transactions = FilterSubCategories(transactions, query.SubCategory);
            //transactions = FilterTags(transactions, query.Tags);
            return new List<Transaction>();
        }
        private IEnumerable<ITransaction> QueryDates(DateTime? start, DateTime? end)
        {
            if (start == null && end == null) //get everything
                return LoadedTransactions.Transactions.ToList();
            else
            {
                if (start == null)
                {
                    return LoadedTransactions.Transactions
                        .Where(p => p.Date <= end)
                        .ToList();
                }
                else if (end == null)
                {
                    return LoadedTransactions.Transactions
                        .Where(p => p.Date >= start)
                        .ToList();
                }
                else
                {
                    return LoadedTransactions.Transactions
                        .Where(p => p.Date >= start && p.Date <= end)
                        .ToList();
                }
            } //else we have a date range
        }
        private IEnumerable<Transaction> PriceFilter(IEnumerable<Transaction> set, float? min, float? max)
        {
            if (min == null && max == null)
                return set;
            else
            {
                if (min == null)
                    return set.Where(p => p.Value <= max);
                else if(max == null)
                    return set.Where(p => p.Value >= min);
                else
                    return set.Where(p => p.Value >= min && p.Value <= max);
            }
        }
        private IEnumerable<Transaction> FilterDebitCredit(IEnumerable<Transaction> set, bool? debit, bool? credit)
        {
            if (debit == null && credit == null)
                return set;
            else
            {
                if (debit == null)
                    return set.Where(p => p.IsDebit == false);
                else if (credit == null)
                    return set.Where(p => p.IsDebit == true);
                else //just return everything
                    return set;
            }
        }
        private IEnumerable<Transaction> FilterTransactionDefinitions(IEnumerable<Transaction>set, IEnumerable<Guid>? definitions)
        {
            if (definitions == null)
                return set;
            else
                return set.Where(
                    p => definitions.Contains(p.TransactionType.AssignedId));
        }
        private IEnumerable<Transaction> FilterCategoryDefinitions(IEnumerable<Transaction>set, IEnumerable<Guid>? definitions)
        {
            if (definitions == null)
                return set;
            else
                return set.Where(p => definitions.Contains(p.TransactionType.MappedCategory));
        }
        private IEnumerable<Transaction> FilterSubCategories(IEnumerable<Transaction>set, IEnumerable<string>? subcategories)
        {
            if (subcategories == null)
                return set;
            else
                return set.Where(p => subcategories.Contains(p.Category.Subtype));
        }
        private IEnumerable<Transaction> FilterTags(IEnumerable<Transaction> set, IEnumerable<string>? tags)
        {
            if (tags == null)
                return set;
            else
            {
               return set.Where(p => 
                p.Tags.Union(tags).Count() > 0);
            }
        }
        #region SearchQuery two
        //lots of linq queries, a foreach is probably better
            //broken since JSON models need updating. ignoring until csv parsing v2
        internal IEnumerable<ITransaction> QueryTransactions(ITransactionQuery query)
        {
            var matches = new List<ITransaction>();
            var records = LoadedTransactions.Transactions.ToArray();
            foreach (var r in records)
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
                if (string.Equals(record.Description, search, StringComparison.OrdinalIgnoreCase))
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
