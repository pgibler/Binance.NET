using System;
using System.Collections.Generic;
using Binance.NET.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Binance.NET.Responses
{
    public class UserDataStreamResponse
    {
        [JsonProperty("e")]
        public string EventType { get; set; }
        [JsonProperty("E")]
        public long EventTime { get; set; }
        [JsonProperty("B"), JsonConverter(typeof(BalanceStreamConverter))]
        public List<Balance> Balances { get; set; }
    }

    public class BalanceStreamConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return string.Empty;
            }
            if (reader.TokenType == JsonToken.String)
            {
                return serializer.Deserialize(reader, objectType);
            }

            var token = JArray.FromObject(existingValue);

            var asset = token["a"].ToString();
            var available = Convert.ToDouble(token["f"].ToString());
            var locked = Convert.ToDouble(token["l"].ToString());

            return new Balance
            {
                Asset = asset,
                Available = available,
                Locked = locked
            };
        }

        // Not implemented.
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }

        public override bool CanConvert(Type objectType) { return true; }
    }
}
