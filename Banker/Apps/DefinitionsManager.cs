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
        public List<TransactionDefinition> TransactionDefinitions;
        public List<TransactionCategory> Categories;
        public event EventHandler<TransactionDefinition> TransactionDefinitionsChanged;
        public event EventHandler<TransactionCategory> TransactionCategoriesCleared;
        private string _Root;
        private string _Definitions;
        private string _Category;

        private string _root;
        private string _definitions;
        private CategoryDefinitions _defaultCategoryDefinitions;
        private CategoryDefinitions _userCategoryDefinitions;
        private TransactionDefinitions _defaultTransactionDefinitions;
        private TransactionDefinitions _userTransactionDefinitions;
        private const string CATEGORYURL = "https://raw.githubusercontent.com/OrganicGMOs/SmortFinancial/main/Definitions/Categories.json";
        private const string TRANSACTIONURL = "https://raw.githubusercontent.com/OrganicGMOs/SmortFinancial/main/Definitions/Transactions.json";

        #region CRUD
        internal bool CreateDefinition(TransactionDefinition def) 
        {
            //make sure def doesnt exist
            var exist = TransactionDefinitions.Where(p => p.Key == def.Key)
                .FirstOrDefault();
            if (exist == null)
                TransactionDefinitions.Add(def);
            else
                return false;
            return true;
        }
        internal bool CreateCategory(TransactionCategory category) 
        {
            var exist = Categories.Where(p => p.CategoryType == category.CategoryType)
                .FirstOrDefault();
            if (exist == null)
                Categories.Add(category);
            else
                return false;
            return true;
        }
        internal bool UpdateDefinition(TransactionDefinition trans) 
        {
            var item = TransactionDefinitions.Where(p => p.Key == trans.Key)
                .FirstOrDefault();
            if (item == null)
                return false;
            else
            {
                item.ReductionTarget = trans.ReductionTarget;
                item.MappedCategory = trans.MappedCategory;
                return true;
            }            
        }
        internal bool UpdateCategory(TransactionCategory cat) 
        {
            var item = Categories.Where(p => p.CategoryType == cat.CategoryType)
                .FirstOrDefault();
            if (item == null)
                return false;
            else
            {
                item.Subtype = cat.Subtype;
                item.ReductionTarget = cat.ReductionTarget;
                item.CategoryType = cat.CategoryType;
                item.Color = cat.Color;
                item.ReductionTarget = cat.ReductionTarget;
                return true;
            }
        }
        internal bool DeleteDefinition(TransactionDefinition def) 
        {
            var item = TransactionDefinitions.Where(p => p.Key == def.Key)
                .FirstOrDefault();
            if (item == null)
                return false;
            else
                TransactionDefinitions.Remove(item);
            return true;
        }
        internal bool DeleteCategory(TransactionCategory cat) 
        {
            var item = Categories.Where(p => p.CategoryType == cat.CategoryType)
                .FirstOrDefault();
            if (item == null)
                return false;
            else
                Categories.Remove(item);
            return true;
        }
        internal TransactionDefinition GetDefinition(string key)
        {
            var item = TransactionDefinitions.Where(p => p.Key == key)
                .FirstOrDefault();
            if (item == null)
                return new TransactionDefinition();
            return item;
        }
        internal TransactionCategory? GetCategory(string key)
        {
            var item = Categories.Where(p => p.CategoryType == key)
                .FirstOrDefault();
            return item;
        }
        internal IEnumerable<TransactionDefinition> GetTransactionDefinitions() 
        {
            return TransactionDefinitions.ToList();
        }
        internal IEnumerable<TransactionCategory> GetCategories() 
        {
            return Categories.ToList();
        }
        #endregion

        #region Document Saving
        /// <summary>
        /// At application close if defintions have been modified we need
        /// to save them.
        /// </summary>
        /// <returns></returns>
        private async Task SaveDefinitions()
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
            await Extensions.WriteFile(json, path);
        }
        private async Task SaveCategories()
        {
            try
            {
                var text = JsonConvert.SerializeObject(Categories);
                await Extensions.WriteFile(_Category, text);
            }
            catch (Exception ex)
            {
                //todo: exception and logging
                Console.WriteLine(ex.Message);
            }
        }
        private async Task SaveTransactionDefinitions()
        {
            try
            {
                var text = JsonConvert.SerializeObject(TransactionDefinitions);
                await File.WriteAllTextAsync(_Category, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Initialize
        public static async Task<DefinitionsManager> DefinitionsManagerFactory(string root)
        {
            var instance = new DefinitionsManager()
            {
                _root = root,
                _definitions = root +"\\definitions"
            };
            await instance.Initialize();
            instance.Categories = new List<TransactionCategory>();
            instance.TransactionDefinitions = new List<TransactionDefinition>();
            instance._Root = root;
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
                await Extensions.WriteFile(file, json);
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
                await Extensions.WriteFile(file, json);
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
                var json = await Extensions.ReadFile(file);
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
    }
}
