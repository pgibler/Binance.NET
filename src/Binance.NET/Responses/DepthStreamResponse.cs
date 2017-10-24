using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Binance.NET.Responses
{
    public class DepthStreamResponse
    {
        [JsonProperty("e")]
        public string EventType { get; set; }
        [JsonProperty("E")]
        public long EventTime { get; set; }
        [JsonProperty("s")]
        public string Symbol { get; set; }
        [JsonProperty("u")]
        public long UpdateId { get; set; }
        [JsonProperty("b"), JsonConverter(typeof(PriceQuantityStreamConverter))]
        public PriceQuantityCollection Bids { get; set; }
        [JsonProperty("a"), JsonConverter(typeof(PriceQuantityStreamConverter))]
        public PriceQuantityCollection Asks { get; set; }
    }

    internal class PriceQuantityStreamConverter : JsonConverter
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

            var priceQuantity = new PriceQuantityCollection();

            foreach (var child in token)
            {
                var array = (JArray) child;
                var price = Convert.ToDouble(array[0].ToString());
                var quantity = Convert.ToDouble(array[1].ToString());
                priceQuantity.Set(price, quantity);
            }

            return priceQuantity;
        }

        // Not implemented.
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }

        public override bool CanConvert(Type objectType) { return true; }
    }
}
