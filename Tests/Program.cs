// See https://aka.ms/new-console-template for more information
using Banker;
using Banker.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

Console.WriteLine("Hello, lets read your transactions");

var file = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\transactions.CSV";
Console.WriteLine("loading");
var banker = await BankerInstance.BankerFactory();
var loadedTransactions = banker.GetLoadedTransactionCount();
Console.WriteLine("Loaded: " + loadedTransactions);
//JsonCreation();
var transactions = await ParseFile(file);
Console.WriteLine("\n\n Ready to save to disk?");
Console.ReadKey();
Console.Clear();
await SaveTransactions(transactions);
Console.WriteLine("Loaded: " + banker.GetLoadedTransactionCount());
Console.ReadKey();
Console.Clear();
var uber = banker.Search_DateRange(new TransactionQuery());
float price = 0;
foreach (var u in uber)
    if (u.IsDebit)
        price += u.Value;
Console.WriteLine(String.Format("Found {0} uber eats entries for a total of ${1} spent in the past year",
    uber.Count(),
    price));
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

    TF.Definitions = new TransactionDefinition[]
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
            ColorHex = 0xFFFFFF,
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
    transactions = banker.GetTransactions();
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

Console.ReadKey();

