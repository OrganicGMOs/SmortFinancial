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
Console.WriteLine("Banker initialized,grabing data");
var watch = new Stopwatch();
watch.Start();
//var transactions = (await banker.QueryTransactions(new TransactionQuery()))
    //.GetValueCollection<Transaction>();

var transactions = (await banker.QueryTransactions2(new TransactionQuery()))
    .GetValueCollection<Transaction>();

Console.WriteLine("Data fetched...printing");
await Task.Delay(1000);
Console.Clear();
foreach(var td in transactions)
    Console.WriteLine(td.Value);

watch.Stop();
Console.Clear();
Console.WriteLine("Took " + watch.ElapsedMilliseconds + " milliseconds to parse, query, and display");
Console.ReadKey();

