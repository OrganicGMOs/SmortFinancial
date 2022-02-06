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
//var result = await banker.ParseTransactionCSV(new CSVDefinition
//{
//    //"C:\Users\Jeff\Desktop\testdata.csv"
//    FilePath = @"C:\Users\Jeff\Desktop\transactions.csv",
//    Name = "Navy Federal",
//    HeaderPresent = true,
//    ColumnCount = 5,
//    LineDelimiter = '\n',
//    ColumnDelimiter = ',',
//    DebitColumn = 4,
//    CreditColumn = 5,
//    DateColumn = 1,
//    DescriptionColumn = 3,
//    DescriptionerDelimiter = " - "
//});
//var rr = result.GetValue<ITransactionParseResult>();
//Console.WriteLine(rr.Loaded.Count());
//await banker.SaveTransactions(rr.Loaded);
//works but is count vs file is off by 1

//empty should return all loaded transactions
var def = new CategoryDefinition
{
    Name = "Food",
    SubCategories = new string[] { "Grocery", "Resturant", "Order out" },
    CategoryId = Guid.NewGuid(),
    ReductionTarget = false
};
var cat = await banker.CreateCategoryDefinition(def);
var trans = new TransactionDefinition
{
    AssignedId = Guid.NewGuid(),
    DefaultTags = new List<string> { "food", "unnecessary" },
    Key = "Uber Eats",
    MappedCategory = def.CategoryId
};
if (cat.IsSuccess)
{
    var transactionDef = await banker.CreateTransactionDefinition(trans);
    if(transactionDef.IsSuccess)
    {
        //get all the transactions
        var transactions = (await banker.QueryTransactions(new TransactionQuery
        {
            Name = "Uber"
        })).GetValueCollection<ITransaction>();
        //wasnt doing deep copies like I thought, but its actually not necessary and
        //probably a waste of memory
        foreach (var transaction in transactions)
        {
            transaction.TransactionType = trans;
            transaction.Category = new TransactionCategory
            {
                CategoryId = def.CategoryId,
                CategoryType = def.Name,
                //subtype doesnt need to match up but search might not pick it up
                Subtype = def.SubCategories[2],
                ReductionTarget = true
            };
            transaction.Tags = transaction.TransactionType.DefaultTags;
        }

    }
}
else
{
    Console.WriteLine("Failed to create category");
}

var searchCat = (await banker.GetCategoryDefinition("Food")).GetValue<ICategoryDefinition>();
var saves = await banker.SaveChanges();
foreach(var save in saves)
{
    var result = save.GetValueCollection<IDocumentSaveResult>();
    foreach(var item in result)
    {

        Console.WriteLine(String.Format("Save result: {0} was saved = {1}, error? {2} ",
       item.FileName, item.Success, item.ErrorMessage));
        Console.WriteLine(item.FilePath);
    }
   
}
Console.ReadKey();

