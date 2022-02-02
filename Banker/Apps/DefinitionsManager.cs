using Banker.Extensions.CustomExceptions;
using Banker.Interfaces;
using Banker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banker.Apps
{
    internal class DefinitionsManager
    {
        private string _definitions;
        private List<TransactionDefinition> _transactionDefs;
        private List<CategoryDefinition> _categoryDefs;
        private CategoryDefinitions _defaultCategoryDefinitions;
        private CategoryDefinitions _userCategoryDefinitions;
        private TransactionDefinitions _defaultTransactionDefinitions;
        private TransactionDefinitions _userTransactionDefinitions;
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
                AssignedId = Guid.NewGuid(),
                CategoryType = category.Name,
                SubTypes = category.SubCategories,
                ColorHex = category.Color,
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
                item.SubTypes = category.SubCategories;
                item.ReductionTarget = category.ReductionTarget;
                item.CategoryType = category.Name;
                item.ColorHex = category.Color;
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

        internal TransactionCategory GetSlimCategory(TransactionDefinition? transactionType)
        {
            if (transactionType == null)
                return new TransactionCategory();
            else
            {
                var category = _categoryDefs
                    .Where(p => p.AssignedId == transactionType.AssignedId).FirstOrDefault();
                if (category == null)
                    return new TransactionCategory();
                else
                {
                    return new TransactionCategory
                    {
                        CategoryId = category.AssignedId,
                        //todo: auto subtype tagging?
                        Subtype = category.SubTypes[0],
                        CategoryType = category.CategoryType,
                        Color = Convert.ToInt32((string)category.ColorHex, 16),
                        ReductionTarget = category.ReductionTarget
                    };
                }
            }
        }

        internal Task<IEnumerable<CategoryDefinition>> GetCategoryDefinitions()
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
        internal Task<IEnumerable<TransactionDefinition>> GetTransactionDefinitions()
        {
            //calls an async method, probably not great since it just returns a collection
            var ahh = _transactionDefs.AsEnumerable();
            return Task.FromResult(_transactionDefs.AsEnumerable());
        }
        #endregion
        #region Helpers
        //add to the two collections. maybe we shouldnt split them? saving is easier though
        //marks serialize object as needing to re-write
        private void UpdateCollections(TransactionDefinition def, bool add = true)
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
        private void UpdateCollections(CategoryDefinition def, bool add = true)
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
        internal CategoryDefinition? GetCategory(string key)
        {
            var item = _categoryDefs.Where(p => p.CategoryType == key)
                .FirstOrDefault();
            return item;
        }
        internal CategoryDefinition? GetCategoryById(Guid id)
        {
            return _categoryDefs.Where(p => p.AssignedId == id)
                .FirstOrDefault();
        }
        internal TransactionDefinition? GetTransaction(string key)
        {
            return _transactionDefs.Where(p => p.Key == key)
                .FirstOrDefault();
        }
        internal TransactionDefinition? GetTransactionById(Guid id)
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
        internal async Task SaveDefinitions()
        {
            var tasks = new List<Task>();
            if (_defaultCategoryDefinitions.NeedsWrite)
                tasks.Add(SaveDefintion<CategoryDefinitions>(
                    _defaultCategoryDefinitions,
                    _definitions + "\\DefaultCategoryDefinitions.json"));
            if (_defaultTransactionDefinitions.NeedsWrite)
                tasks.Add(SaveDefintion<TransactionDefinitions>(
                    _defaultTransactionDefinitions,
                    _definitions + "\\DefaultTransactionDefinitions.json"));
            if (_userCategoryDefinitions.NeedsWrite)
                tasks.Add(SaveDefintion<CategoryDefinitions>(
                    _userCategoryDefinitions,
                    _definitions + "\\CustomCategoryDefinitions.json"
                    ));
            if (_userTransactionDefinitions.NeedsWrite)
                tasks.Add(SaveDefintion<TransactionDefinitions>(
                    _userTransactionDefinitions,
                    _definitions + "\\CustomTransactionDefinitions.json"
                    ));
            await Task.WhenAll(tasks);
        }
        private async Task SaveDefintion<T>(T def, string path)
        {
            var json = JsonConvert.SerializeObject(def);
            await Extensions.Functions.WriteFile(json, path);
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
                var defaultDefs = LoadDefaultDefinitions();
                var userDefs = LoadUserDefinitions();
                await Task.WhenAll(defaultDefs, userDefs);
                await LoadDefaultDefinitions();
                _transactionDefs = _defaultTransactionDefinitions.Definitions.ToList();
                _transactionDefs.AddRange(_userTransactionDefinitions.Definitions);
                _categoryDefs = _defaultCategoryDefinitions.Definitions.ToList();
                _categoryDefs.AddRange(_userCategoryDefinitions.Definitions);

            }catch(System.UnauthorizedAccessException ex)
            {
                Console.WriteLine("Failed to initialize definations manager. Access denied");
            }
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
                    Definitions = new List<CategoryDefinition>()
                    {
                        new CategoryDefinition()
                        {
                            AssignedId = Guid.Empty,
                            CategoryType = "Unknown",
                            SubTypes = new string[0],
                            ColorHex = "0xFFFFFF",
                            ReductionTarget = false,
                        }
                    }
                };
            }
                
            _defaultCategoryDefinitions = categories;
            if (rewrite)
            {
                var json = JsonConvert
                    .SerializeObject(_defaultCategoryDefinitions, Formatting.Indented);
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
                    Definitions = new List<TransactionDefinition>
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
            _defaultTransactionDefinitions = transactions;
            if(rewrite)
            {
                var json = JsonConvert
                    .SerializeObject(_defaultTransactionDefinitions, Formatting.Indented);
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
                    Definitions = new List<CategoryDefinition>()
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
                    Definitions = new List<TransactionDefinition>()
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
                var obj = JsonConvert.DeserializeObject<T>(json);
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
                    var obj = JsonConvert.DeserializeObject<T>(json);
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
