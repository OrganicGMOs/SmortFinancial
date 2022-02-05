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
    public class TransactionConverter : JsonConverter<List<ITransaction>>
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
            throw new NotImplementedException();
        }
    }
    public class TransactionDefinitionConverter : JsonConverter<ITransactionDefinition>
    {
        public override ITransactionDefinition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            TransactionDefinition? result = null;
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var item = jsonDoc.RootElement.GetRawText();
                result = Extensions.Functions.ParseJson<TransactionDefinition>(item);
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, ITransactionDefinition value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
    public class CategoryDefinitionConverter : JsonConverter<ICategoryDefinition>
    {
        public override ICategoryDefinition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            CategoryDefinition? result = null;
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var item = jsonDoc.RootElement.GetRawText();
                result = Extensions.Functions.ParseJson<CategoryDefinition>(item);
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, ICategoryDefinition value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public enum TransactionType
    {
        Transaction,
        TransactionDefintion,
        TransactionCategory,
    }
}
