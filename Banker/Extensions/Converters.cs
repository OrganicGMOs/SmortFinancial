using Banker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Banker.Extensions
{
    internal class Converters
    {
    }
    public class TransactionHistoryConverter : JsonConverter<TransactionHistory>
    {
        public override void WriteJson(JsonWriter writer, TransactionHistory value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override TransactionHistory ReadJson(JsonReader reader, Type objectType, TransactionHistory existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return new TransactionHistory();
            throw new NotImplementedException();
        }
    }
}
