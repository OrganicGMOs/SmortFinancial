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
    internal class DefinitionsManager
    {
        private string _definitions;
        private List<ITransactionDefinition> _transactionDefs;           //all transaction definitions
        private List<ICategoryDefinition> _categoryDefs;                 //all category definitions
        private CategoryDefinitions _defaultCategoryDefinitions;        //serialization object
        private CategoryDefinitions _userCategoryDefinitions;           //serialization object
        private TransactionDefinitions _defaultTransactionDefinitions; //serialization object
        private TransactionDefinitions _userTransactionDefinitions;    //serialzation object
        private const string CATEGORYURL = "https://raw.githubusercontent.com/OrganicGMOs/SmortFinancial/main/Definitions/Categories.json";
        private const string TRANSACTIONURL = "https://raw.githubusercontent.com/OrganicGMOs/SmortFinancial/main/Definitions/Transactions.json";

        //Data validation should/will be at BankerInstnace
        #region Crud
        internal Task CreateCategory(ICategoryDefinition category)
        {
            //make sure it doesnt exist
            var current = GetCategory(category.Name);
            if(current != null)
                throw new InvalidDataException("Category already exists");
            UpdateCollections(new CategoryDefinition
            {
                CategoryId = Guid.NewGuid(),
                Name = category.Name,
                SubCategories = category.SubCategories,
                ReductionTarget = category.ReductionTarget,
            });
            return Task.CompletedTask;
        }
        internal Task ModifyCategory(ICategoryDefinition category)
        {
            var item = GetCategoryById(category.CategoryId);
            if (item == null)
                throw new CategoryNotFoundException();
            else
            {
                item.SubCategories = category.SubCategories;
                item.ReductionTarget = category.ReductionTarget;
                item.Name = category.Name;
                item.ReductionTarget = category.ReductionTarget;
            }
            return Task.CompletedTask;
        }
        internal Task DeleteCategory(ICategoryDefinition category)
        {
            var item = GetCategoryById(category.CategoryId);
            if (item == null)
                throw new CategoryNotFoundException();
            else
                UpdateCollections(item,false);
            return Task.CompletedTask;
        }

        internal TransactionCategory GetSlimCategory(ITransactionDefinition? transactionType)
        {
            if (transactionType == null)
                return new TransactionCategory();
            else
            {
                var category = _categoryDefs
                    .Where(p => p.CategoryId == transactionType.AssignedId).FirstOrDefault();
                if (category == null)
                    return new TransactionCategory();
                else
                {
                    return new TransactionCategory
                    {
                        CategoryId = category.CategoryId,
                        //todo: auto subtype tagging?
                        Subtype = category.SubCategories == null ? String.Empty : category.SubCategories[0],
                        CategoryType = category.Name,
                        ReductionTarget = category.ReductionTarget
                    };
                }
            }
        }

        internal Task<IEnumerable<ICategoryDefinition>> GetCategoryDefinitions()
        {
            return Task.FromResult(_categoryDefs.AsEnumerable());
        }

        internal Task CreateTransaction(ITransactionDefinition transaction)
        {
            var current = GetTransaction(transaction.Key);
            if (current != null)
                throw new InvalidDataException("Transaction Definition already exists");
            else
            {
                UpdateCollections(new TransactionDefinition
                {
                    AssignedId = Guid.NewGuid(),
                    MappedCategory = transaction.MappedCategory,
                    Key = transaction.Key,
                    ReductionTarget = transaction.ReductionTarget,
                });
            }
            return Task.CompletedTask;
        }
        internal Task ModifyTransaction(ITransactionDefinition transaction)
        {
            var current = GetTransactionById(transaction.AssignedId);
            if (current != null)
            {
                current.MappedCategory = transaction.MappedCategory;
                current.Key = transaction.Key;
                current.AssignedId = transaction.AssignedId;
                current.ReductionTarget = transaction.ReductionTarget;
                if (_defaultTransactionDefinitions.Definitions.Contains(current))
                    _defaultTransactionDefinitions.NeedsWrite = true;
                else
                    _userTransactionDefinitions.NeedsWrite = true;
                return Task.CompletedTask;
            }
            else
                throw new TransactionNotFoundException();
                
        }
        internal Task DeleteTransaction(ITransactionDefinition transaction)
        {
            var toDelete = GetTransactionById(transaction.AssignedId);
            if (toDelete == null)
                throw new TransactionNotFoundException();
            else
                UpdateCollections(toDelete, false);
            return Task.CompletedTask;
        }
        internal Task<IEnumerable<ITransactionDefinition>> GetTransactionDefinitions()
        {
            //calls an async method, probably not great since it just returns a collection
            var ahh = _transactionDefs.AsEnumerable();
            return Task.FromResult(_transactionDefs.AsEnumerable());
        }
        #endregion
        #region Helpers
        //add to the two collections. maybe we shouldnt split them? saving is easier though
        //marks serialize object as needing to re-write
        private void UpdateCollections(ITransactionDefinition def, bool add = true)
        {
            if (add)
            {
                _transactionDefs.Add(def);
                _userTransactionDefinitions.Definitions.Add(def);
            }
            else
            {
                _transactionDefs.Remove(def);
                _userTransactionDefinitions.Definitions.Remove(def);
            }

            _userTransactionDefinitions.NeedsWrite = true;
        }
        private void UpdateCollections(ICategoryDefinition def, bool add = true)
        {
            if (add)
            {
                _categoryDefs.Add(def);
                _userCategoryDefinitions.Definitions.Add(def);
            }
            else
            {
                _categoryDefs.Remove(def);
                _userCategoryDefinitions.Definitions.Remove(def);
            }
            _userCategoryDefinitions.NeedsWrite = true;
        }
        internal ICategoryDefinition? GetCategory(string key)
        {
            var item = _categoryDefs.Where(p => p.Name == key)
                .FirstOrDefault();
            return item;
        }
        internal ICategoryDefinition? GetCategoryById(Guid id)
        {
            return _categoryDefs.Where(p => p.CategoryId == id)
                .FirstOrDefault();
        }
        internal ITransactionDefinition? GetTransaction(string key)
        {
            return _transactionDefs.Where(p => p.Key == key)
                .FirstOrDefault();
        }
        internal ITransactionDefinition? GetTransactionById(Guid id)
        {
            return _transactionDefs.Where(p => p.AssignedId == id)
                .FirstOrDefault();
        }
        #endregion
        #region Document Saving
        /// <summary>
        /// At application close if defintions have been modified we need
        /// to save them.
        /// </summary>
        /// <returns></returns>
        internal async Task<IEnumerable<IDocumentSaveResult>> SaveDefinitions()
        {
            var tasks = new List<Task<IDocumentSaveResult>>();
            if (_defaultCategoryDefinitions.NeedsWrite)
            {
                _defaultCategoryDefinitions.NeedsWrite = false;     //set to false so we dont write true
                tasks.Add(SaveDefintion<CategoryDefinitions>(
                    _defaultCategoryDefinitions,
                    _definitions + "\\DefaultCategoryDefinitions.json"));
            }
            if (_defaultTransactionDefinitions.NeedsWrite)
            {
                _defaultTransactionDefinitions.NeedsWrite = false;
                tasks.Add(SaveDefintion<TransactionDefinitions>(
                    _defaultTransactionDefinitions,
                    _definitions + "\\DefaultTransactionDefinitions.json"));
            }
            if (_userCategoryDefinitions.NeedsWrite)
            {
                _userCategoryDefinitions.NeedsWrite = false;
                tasks.Add(SaveDefintion<CategoryDefinitions>(
                    _userCategoryDefinitions,
                    _definitions + "\\CustomCategoryDefinitions.json"
                    ));
            }
            if (_userTransactionDefinitions.NeedsWrite)
            {
                _userTransactionDefinitions.NeedsWrite = false;
                tasks.Add(SaveDefintion<TransactionDefinitions>(
                    _userTransactionDefinitions,
                    _definitions + "\\CustomTransactionDefinitions.json"
                    ));
            }
            if (tasks.Count > 0)
                return await Task.WhenAll(tasks);
            else
                return new List<IDocumentSaveResult>()
                {
                    new DocumentSaveModel(false,"","No definition files required changes")
                };
        }
        private async Task<IDocumentSaveResult> SaveDefintion<T>(T def, string path)
        {
            try
            {
                var json = Functions.ParseJson<T>(def);
                await Functions.WriteFile(path, json);
                return new DocumentSaveModel(true, path);
            }
            catch (Exception ex)
            {
                return new DocumentSaveModel(false, path,ex.Message);
            }
            
        }
        #endregion

        #region Initialize
        public static async Task<DefinitionsManager> DefinitionsManagerFactory(string root)
        {
            var instance = new DefinitionsManager()
            {
                _definitions = root +"\\definitions"
            };
            await instance.Initialize();
            return instance;
        }
        private async Task Initialize()
        {
            try
            {
                DirectoryCheck();
                var defaultDefs = LoadDefaultDefinitions();
                var userDefs = LoadUserDefinitions();
                await Task.WhenAll(defaultDefs, userDefs);
                //make sure no duplicates between user defined and default
                foreach (var t in _defaultTransactionDefinitions.Definitions)
                {
                    var duplicates = _userTransactionDefinitions.Definitions
                        .Where(p => p.AssignedId == t.AssignedId || p.Key == p.Key).ToList();
                    if(duplicates.Count() > 0)
                    {

                        foreach (var dup in duplicates)
                            _userTransactionDefinitions.Definitions.Remove(dup);
                        _userTransactionDefinitions.NeedsWrite = true;
                    }
                }
                foreach(var c in _defaultCategoryDefinitions.Definitions)
                {
                    var duplicates = _userCategoryDefinitions.Definitions
                       .Where(p => p.CategoryId == c.CategoryId || p.Name == c.Name).ToList();
                    if (duplicates.Count() > 0)
                    {

                        foreach (var dup in duplicates)
                            _userCategoryDefinitions.Definitions.Remove(dup);
                        _userCategoryDefinitions.NeedsWrite = true;
                    }
                }
                //now combine
                _transactionDefs = _defaultTransactionDefinitions.Definitions.ToList();
                _transactionDefs.AddRange(_userTransactionDefinitions.Definitions);
                _categoryDefs = _defaultCategoryDefinitions.Definitions.ToList();
                _categoryDefs.AddRange(_userCategoryDefinitions.Definitions);

            }catch(System.UnauthorizedAccessException ex)
            {
                Console.WriteLine("Failed to initialize definations manager. Access denied");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void DirectoryCheck()
        {
            if(!Directory.Exists(_definitions))
                Directory.CreateDirectory(_definitions);
        }
        private async Task LoadDefaultDefinitions()
        {
            var categoryDefinitions = _definitions + "\\DefaultCategoryDefinitions.json";
            var transactionDefinitions = _definitions + "\\DefaultTransactionDefinitions.json";
            var tasks = new Task[]
            {
                LoadDefaultCategoryDefinitions(categoryDefinitions),
                LoadDefaultTransactionDefinitions(transactionDefinitions)
            };
            await Task.WhenAll(tasks);
        }
        private async Task LoadDefaultCategoryDefinitions(string file)
        {
            bool rewrite = false;
            var categories = await ReadandParse<CategoryDefinitions>(file);
            if(categories == null)
            {
                rewrite = true;
                categories = await AttemptLoadDefault<CategoryDefinitions>(CATEGORYURL);
            }
            if (categories == null)
            {
                categories = new CategoryDefinitions()
                {
                    Definitions = new List<ICategoryDefinition>()
                    {
                        new CategoryDefinition()
                        {
                            CategoryId = Guid.Empty,
                            Name = "Unknown",
                            SubCategories = new string[0],
                            ReductionTarget = false,
                        }
                    }
                };
            }
            else if (categories.Definitions == null)
                categories.Definitions = new List<ICategoryDefinition>();
                
            _defaultCategoryDefinitions = categories;
            if (rewrite)
            {
                var json = Functions.ParseJson(_defaultCategoryDefinitions);
                await Extensions.Functions.WriteFile(file, json);
            }
        }
        private async Task LoadDefaultTransactionDefinitions(string file)
        {
            bool rewrite = false;
            var transactions = await ReadandParse<TransactionDefinitions>(file);
            if(transactions == null)
            {
                rewrite = true;
                transactions = await AttemptLoadDefault<TransactionDefinitions>(TRANSACTIONURL);
            }
            if (transactions == null)
                transactions = new TransactionDefinitions()
                {
                    Definitions = new List<ITransactionDefinition>
                    {
                        new TransactionDefinition()
                        {
                            AssignedId = Guid.Empty,
                            Key = "Unkown",
                            MappedCategory = Guid.Empty,
                            ReductionTarget = false
                        }
                    }
                };
            else if(transactions.Definitions == null)
                transactions.Definitions = new List<ITransactionDefinition>();
            _defaultTransactionDefinitions = transactions;
            if(rewrite)
            {
                var json = Functions.ParseJson(_defaultTransactionDefinitions);
                await Extensions.Functions.WriteFile(file, json);
            }    
        }
        private async Task LoadUserDefinitions() 
        {
            var categoryDefinitions = _definitions + "\\CustomCategoryDefinitions.json";
            var transactionDefinitions = _definitions + "\\CustomTransactionDefinitions.json";
            var tasks = new Task[]
            {
                LoadUserCategoryDefinitions(categoryDefinitions),
                LoadUserTransactionDefinitions(transactionDefinitions)
            };
            await Task.WhenAll(tasks);
        }
        private async Task LoadUserCategoryDefinitions(string file) 
        { 
            var categories = await ReadandParse<CategoryDefinitions>(file);
            if (categories == null)
                _userCategoryDefinitions = new CategoryDefinitions
                {
                    Definitions = new List<ICategoryDefinition>()
                };
            else
                _userCategoryDefinitions = categories;
        }
        private async Task LoadUserTransactionDefinitions(string file) 
        {
            var transactions = await ReadandParse<TransactionDefinitions>(file);
            if (transactions == null)
                _userTransactionDefinitions = new TransactionDefinitions
                {
                    Definitions = new List<ITransactionDefinition>()
                };
            else
                _userTransactionDefinitions = transactions;
        }
        private async Task<T?> ReadandParse<T>(string file)
        {
            try
            {
                var json = await Extensions.Functions.ReadFile(file);
                if(json == null)
                    return default(T);
                if(string.IsNullOrEmpty(json))
                    return default(T?);
                var obj = Extensions.Functions.ParseJson<T>(json);
                return obj;
            }
            catch(Exception ex)
            {
                //todo: exception and logging
                Console.Error.WriteLine(ex.Message);
                return default(T);
            }
            
        }
        private async Task<T?> AttemptLoadDefault<T>(string location) 
        {
            using(var client = new HttpClient())
            {
                var response = await client.GetAsync(location);
                if(response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var obj = JsonSerializer.Deserialize<T>(json);
                    if (obj == null)
                        return default(T);
                    else
                        return obj;
                }
                else
                    return default(T);
            }
        }
        #endregion

        internal async Task Close()
        {
            await SaveDefinitions();
        }
    }
}
