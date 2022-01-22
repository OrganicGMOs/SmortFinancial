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
        private async Task SaveCategories()
        {
            try
            {
                var text = JsonConvert.SerializeObject(Categories);
                await File.WriteAllTextAsync(_Category, text);

            }
            catch (Exception ex)
            {
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
            var instance = new DefinitionsManager();
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
                _Definitions = _Root + "\\TransactionDefinitions";
                _Category = _Root + "\\Categories";
                if(!Directory.Exists(_Category))
                    Directory.CreateDirectory(_Category);
                if(!Directory.Exists(_Definitions))
                    Directory.CreateDirectory(_Definitions);
                var def = LoadTransactionDefinitions();
                var cat = LoadCategories();
                await Task.WhenAll(def, cat);
                if (TransactionDefinitions.Count >= 0)
                    TransactionDefinitions.Add(new TransactionDefinition()); // add default
                if(Categories.Count >= 0)
                    Categories.Add(new TransactionCategory());              // add default

            }catch(System.UnauthorizedAccessException ex)
            {
                Console.WriteLine("Failed to initialize definations manager. Access denied");
            }
        }
        private async Task LoadTransactionDefinitions()
        {
            var content = new List<Task<string>>();
            var files = Directory.GetFiles(_Definitions);
            foreach (var file in files)
            {
                content.Add(File.ReadAllTextAsync(file));
            }    
           var results = await Task.WhenAll(content);
            foreach(var json in results)
            {
                var def = JsonConvert
                    .DeserializeObject<TransactionDefinitions>(json);
                if (def != null)
                    TransactionDefinitions.AddRange(def.Definitions);
            }
        }
        //todo: LoadCategories and defs are really similar
        private async Task LoadCategories()
        {
            var content = new List<Task<string>>();
            var files = Directory.GetFiles(_Category);
            foreach (var file in files)
            {
                content.Add(File.ReadAllTextAsync(file));
            }
            var results = await Task.WhenAll(content);
            foreach (var json in results)
            {
                var def = JsonConvert
                    .DeserializeObject<TransactionCategory>(json);
                if (def.CategoryType != null)
                    Categories.Append(def);
                else
                {
                    Console.WriteLine("puppy scared");
                }
            }
        }
        private async Task<ICollection<TransactionCategory>> GetDefaultCategories()
        {
            throw new NotImplementedException();
        }
        private async Task<ICollection<TransactionDefinition>> GetDefaultDefinitions()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
