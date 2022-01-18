// See https://aka.ms/new-console-template for more information
using Banker;
using System;
using System.Linq;

Console.WriteLine("Hello, lets read your transactions");

var file = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\transactions.CSV";
var banker = await BankerInstance.BankerFactory();
Console.WriteLine("loading");
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
Console.ReadKey();

public class TransactionDefinitions
{
    public TransactionDefinition[] Definitions { get; set; }
}

public class TransactionDefinition
{
    public string Key { get; set; }
    public TransactionCategory TransactionCategory { get; set; }
    public bool ReductionTarget { get; set; }
}

public class TransactionCategory
{
    public string CategoryType { get; set; }
    public string SecondaryType { get; set; }
    public string Color { get; set; }
    public bool ReductionTarget { get; set; }
}
