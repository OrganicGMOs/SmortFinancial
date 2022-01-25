// See https://aka.ms/new-console-template for more information
using Banker;
using Banker.Interfaces;
using Banker.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

Console.WriteLine("Hello, lets read your transactions");

var file = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\transactions.CSV";
var banker = await BankerInstance.BankerFactory();
if((await banker.CreateCategoryDefinition(new CategoryDef
{
    Name = "Temp Category",
    Tags = new[] {"cat 1","cat 2","cat 3"},
    Color = "0xFFFFFF",
    ReductionTarget = false
})).IsSuccess)
{
    Console.WriteLine("Success");
    var result = await banker.GetCategoryDefinition("Temp Category");
    var value = result.GetValue<CategoryDefinition>();
    Console.WriteLine(value.AssignedId);
    Console.WriteLine(value.CategoryType);
}
else
{
    Console.WriteLine("Failed to update category");
}
Console.ReadKey();
void JsonCreation()
{
    var TransactionFile = new TransactionHistory()
    {
        Modified = DateTime.Now,
        OldestRecord = DateTime.Now,
    };
    var TCFile = new TransactionCategories();
    var TF = new Banker.Models.TransactionDefinitions();
    var TC = new CategoryDefinitions();
    TransactionFile.Transactions = new List<Transaction>()
    {
        new Transaction
        {
            AssignedId = Guid.NewGuid(),
            Category = new TransactionCategory
            {
                CategoryType = "Unknown",
                Subtype = "Unknown",
                Color = 0xFFFFFF,
                ReductionTarget = false
            },
            Date = DateTime.Now,
            TransactionType = new TransactionDefinition
            {
                AssignedId = Guid.Empty,
                MappedCategory = Guid.Empty,
                Key = "Unknown",
                ReductionTarget = false
            },
            Description = "",
            IsDebit = false,
            Value = 0.00f
        }
    };
    var tfj = JsonConvert.SerializeObject(TransactionFile,Formatting.Indented);
    WriteToScreen(tfj);

    TCFile.Categories = new TransactionCategory[]
    {
        new TransactionCategory
        {
            CategoryType = "Unknown",
            Subtype = "Unknown",
            Color = 0xFFFFFF,
            ReductionTarget = false
        },
    };
    var tcfj = JsonConvert.SerializeObject(TCFile,Formatting.Indented);
    WriteToScreen(tcfj);

    TF.Definitions = new List<TransactionDefinition>
    {
        new TransactionDefinition
        {
            AssignedId = Guid.Empty,
            Key = "Unknown",
            MappedCategory = Guid.Empty,
            ReductionTarget = false
        }
    };
    var tfj2 = JsonConvert.SerializeObject(TF,Formatting.Indented);
    WriteToScreen(tfj2);

    TC.Definitions = new List<CategoryDefinition>()
    {
        new CategoryDefinition
        {
            AssignedId = Guid.Empty,
            CategoryType = "Unknown",
            SubTypes = new[]{""},
            ColorHex = "0xFFFFFF",
            ReductionTarget = false
        }
    };
    var tcj = JsonConvert.SerializeObject(TC,Formatting.Indented);
    WriteToScreen(tcj);

}

void WriteToScreen(string value)
{
    Console.Clear();
    Console.Write(value);
    Console.ReadKey();
}

async Task<IEnumerable<Transaction>> ParseFile(string location){


    Console.WriteLine("Parsing file");
    var response = await banker.LoadNewTransactionFile(file);
    var transactions = banker.GetTransactions();
    foreach (var transaction in response)
        Console.WriteLine(transaction.Description);
    Console.WriteLine(transactions.Count() + " Transactions");
    return response;
}

async Task SaveTransactions(IEnumerable<Transaction> transactions)
{
    await banker.SaveTransactions(transactions);
    Console.WriteLine("Items saved");
    Console.WriteLine("\n\n Ready to read back saved file?");
    Console.ReadKey();
    Console.Clear();
    var saved = banker.GetLastSavedTransactions();
    foreach(var transaction in saved)
        Console.WriteLine(transaction.Description);
}

public class CategoryDef : ICategoryDefinition
{
    public string Name { get; set; }
    public string[] Tags { get; set; }
    public string Color { get; set; }
    public bool ReductionTarget { get; set; }
}

