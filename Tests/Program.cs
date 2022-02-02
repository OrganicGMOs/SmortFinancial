// See https://aka.ms/new-console-template for more information
using Banker;
using Banker.Interfaces;
using Banker.Models;
using Newtonsoft.Json;
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
Console.WriteLine(rr.Loaded.Count());
await banker.SaveTransactions(rr.Loaded);
//works but is count vs file is off by 1
foreach(var r in rr.Loaded)
{
    Console.WriteLine(String.Format("transaction results: value {0}, isDebit {1}, Date {2}, Description {3}",
        r.Value,r.IsDebit,r.Date,r.Description));
}
Console.ReadKey();

