// See https://aka.ms/new-console-template for more information
using Banker;
using Banker.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

Console.WriteLine("Hello, lets read your transactions");

var file = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\transactions.CSV";
var banker = await BankerInstance.BankerFactory();
Console.WriteLine("loading");
//JsonCreation();
ParseFile(file);




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
    TransactionFile.Transactions = new Transaction[]
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

    TC.Definitions = new CategoryDefinition[]
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

async Task ParseFile(string location){
    Console.WriteLine("Grabbing Categories");
    foreach (var r in banker.GetCategories())
        Console.WriteLine(r.CategoryType);
    foreach (var c in banker.GetTransactionsDefs())
        Console.WriteLine(c.Key);
    var transactions = banker.GetTransactions();

    Console.WriteLine("Parsing file");
    Console.WriteLine(transactions.Count() + " Transactions");
    var response = await banker.LoadNewTransactionFile(file);
    transactions = banker.GetTransactions();
    foreach (var transaction in response)
        Console.WriteLine(transaction.Description);
    Console.WriteLine(transactions.Count() + " Transactions");
}

Console.ReadKey();

