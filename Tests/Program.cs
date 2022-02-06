// See https://aka.ms/new-console-template for more information
using Banker;
using Banker.Interfaces;
using Banker.Models;

using System;
using System.Diagnostics;
using System.Linq;

//todo give insight into instrance creation
var banker = await BankerInstance.BankerFactory();
Console.WriteLine("Banker initialized");
//var transactions = (await banker.QueryTransactions2(new TransactionQuery()))
//.GetValueCollection<Transaction>();
var result = await banker.ParseTransactionCSV(new CSVDefinition
{
    //"C:\Users\Jeff\Desktop\testdata.csv"
    FilePath = @"C:\Users\Jeff\Desktop\transactions.csv",
    Name = "Navy Federal",
    HeaderPresent = true,
    ColumnCount = 5,
    LineDelimiter = '\n',
    ColumnDelimiter = ',',
    DebitColumn = 4,
    CreditColumn = 5,
    DateColumn = 1,
    DescriptionColumn = 3,
    DescriptionerDelimiter = " - "
});
var rr = result.GetValue<ITransactionParseResult>();
var add = await banker.AddTransactions(rr.Loaded);
//empty should return all loaded transactions
var saveresults = await banker.SaveChanges();
foreach(var sr in saveresults)
{
    var r = sr.GetValueCollection<IDocumentSaveResult>();
    foreach (var d in r)
        Console.WriteLine(d.FilePath + " : " + d.Success);
}
Console.ReadKey();

