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

        #region CRUD
        internal bool CreateDefinition(TransactionDefinition def) 
        {
            if (CheckForDuplicateDefinition(def))
                UpdateCollections(def);
            else
                return false;
            return true;
        }
        internal bool CreateCategory(CategoryDefinition category) 
        {
            if (!CheckForDuplicateDefinition(category))
                UpdateCollections(category);
            else
                return false;
            return true;

        }
        internal bool UpdateDefinition(TransactionDefinition trans) 
        {
            var item = GetSingleDefinition(trans);
            if (item == null)
                return false;
            else
            {
                item.ReductionTarget = trans.ReductionTarget;
                item.MappedCategory = trans.MappedCategory;
                return true;
            }            
        }
        internal bool UpdateCategory(CategoryDefinition cat) 
        {
            var item = GetSingleDefinition(cat);
            if (item == null)
                return false;
            else
            {
                item.SubTypes = cat.SubTypes;
                item.ReductionTarget = cat.ReductionTarget;
                item.CategoryType = cat.CategoryType;
                item.ColorHex = cat.ColorHex;
                item.ReductionTarget = cat.ReductionTarget;
                return true;
            }
        }
        internal bool DeleteDefinition(TransactionDefinition def) 
        {
            if (CheckForDuplicateDefinition(def))
                UpdateCollections(def, false);
            else
                return false;
            return true;
        }
        internal bool DeleteCategory(CategoryDefinition cat) 
        {
            if (CheckForDuplicateDefinition(cat))
                UpdateCollections(cat, false);
            else
                return false;
            return true;
        }
        /// <summary>
        /// gets a described transaction defintion if it exists. <br/>
        /// returns default transaction if item does not exist
        /// </summary>
        /// <param name="key">key value of transaction</param>
        /// <returns>TransactionDefition with inputted key</returns>
        internal TransactionDefinition GetDefinition(string key)
        {
            var item = _transactionDefs.Where(p => p.Key == key)
                .FirstOrDefault();
            if (item == null)
                return new TransactionDefinition();
            return item;
        }
        internal CategoryDefinition GetCategory(string key)
        {
            var item = _categoryDefs.Where(p => p.CategoryType == key)
                .FirstOrDefault();
            if(item == null)
                return new CategoryDefinition();
            return item;
        }
        internal IEnumerable<TransactionDefinition> GetTransactionDefinitions() 
        {
            return _transactionDefs.ToList();
        }
        internal IEnumerable<CategoryDefinition> GetCategories() 
        {
            return _categoryDefs.ToList();
        }
        private bool CheckForDuplicateDefinition(TransactionDefinition def)
        {
            var exist = _transactionDefs.Where(p => p.Key == def.Key)
             .FirstOrDefault();
            if (exist == null)
                return false;
            else
                return true;
        }
        private bool CheckForDuplicateDefinition(CategoryDefinition def)
        {
            var exist = _categoryDefs.Where(p => p.CategoryType == def.CategoryType)
             .FirstOrDefault();
            if (exist == null)
                return false;
            else
                return true;
        }
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
            
            _userCategoryDefinitions.NeedsWrite = true;
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
        private TransactionDefinition GetSingleDefinition(TransactionDefinition def) {
            var item = _transactionDefs.Where(p => p.AssignedId == def.AssignedId)
                .FirstOrDefault();
            return item;
        }
        private CategoryDefinition GetSingleDefinition(CategoryDefinition df) { throw new NotImplementedException(); }
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

        internal async Task Close()
        {
            await SaveDefinitions();
        }
    }
}
