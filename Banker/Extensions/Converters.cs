using Banker.Interfaces;
using Banker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;
using Transaction = Banker.Models.Transaction;

namespace Banker.Extensions
{
    internal class Converters
    {
    }
    public class TransactionsConverter : JsonConverter<List<ITransaction>>
    {
        public override List<ITransaction>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<ITransaction>? result = null;
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var item = jsonDoc.RootElement.GetRawText();
                var transactions = Extensions.Functions
                    .ParseJson<List<Transaction>>(item);
                if(transactions != null)
                    result = transactions.Cast<ITransaction>().ToList();
            }
            if (result == null)
                return new List<ITransaction>();
            else
                return result;
        }

        public override void Write(Utf8JsonWriter writer, List<ITransaction> value, JsonSerializerOptions options)
        {
            var transactions = value.Cast<Transaction>().ToList();
            var text = Extensions.Functions.ParseJson(transactions);
            writer.WriteStringValue(text);
        }
    }
    public class TransactionDefinitionsConverter : JsonConverter<List<ITransactionDefinition>>
    {
        public override List<ITransactionDefinition>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<ITransactionDefinition>? result = null;
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var item = jsonDoc.RootElement.GetRawText();
                var transactions = Extensions.Functions
                    .ParseJson<List<TransactionDefinition>>(item);
                if (transactions != null)
                    result = transactions.Cast<ITransactionDefinition>().ToList();
            }
            if (result == null)
                return new List<ITransactionDefinition>();
            else
                return result;
        }

        public override void Write(Utf8JsonWriter writer, List<ITransactionDefinition> value, JsonSerializerOptions options)
        {
            var transctionDefs = value.Cast<TransactionDefinition>().ToList();
            var text = Extensions.Functions.ParseJson(transctionDefs);
            writer.WriteStringValue(text);
        }
    }
    public class TransactionDefinitionConverter : JsonConverter<ITransactionDefinition>
    {
        public override ITransactionDefinition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ITransactionDefinition? result = null;
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var item = jsonDoc.RootElement.GetRawText();
                result = Extensions.Functions
                    .ParseJson<TransactionDefinition>(item);
            }
            if (result == null)
                return new TransactionDefinition();
            else
                return result;
        }

        public override void Write(Utf8JsonWriter writer, ITransactionDefinition value, JsonSerializerOptions options)
        {
            var transactionDef = (TransactionDefinition)value;
            var text = Extensions.Functions.ParseJson(transactionDef);
            writer.WriteStringValue(text);
        }
    }
    public class TransactionCategoryConverter : JsonConverter<ITransactionCategory>
    {
        public override ITransactionCategory? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            TransactionCategory? result = null;
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var item = jsonDoc.RootElement.GetRawText();
                result = Extensions.Functions.ParseJson<TransactionCategory>(item);
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, ITransactionCategory value, JsonSerializerOptions options)
        {
            var transactionCat = (TransactionCategory)value;
            var text = Extensions.Functions.ParseJson(transactionCat);
            writer.WriteStringValue(text);
        }
    }
    public class CategoryDefinitionConverter : JsonConverter<List<ICategoryDefinition>>
    {
        public override List<ICategoryDefinition>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<ICategoryDefinition>? result = null;
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var item = jsonDoc.RootElement.GetRawText();
                var categories = Extensions.Functions
                    .ParseJson<List<CategoryDefinition>>(item);
                if(categories != null)
                    result = categories.Cast<ICategoryDefinition>().ToList();
            }
            if (result == null)
                return new List<ICategoryDefinition>();
            else
                return result;
        }

        public override void Write(Utf8JsonWriter writer, List<ICategoryDefinition> value, JsonSerializerOptions options)
        {
            var categoryDefs = value.Cast<CategoryDefinition>;
            var text = Extensions.Functions.ParseJson(categoryDefs);
            writer.WriteStringValue(text);
        }
    }

    public enum TransactionType
    {
        Transaction,
        TransactionDefintion,
        TransactionCategory,
    }
}
