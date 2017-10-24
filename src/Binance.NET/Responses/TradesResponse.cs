using System.Collections.Generic;
using Newtonsoft.Json;

namespace Binance.NET.Responses
{
    public class TradesResponse : List<Trade>
    {
    }

    public class Trade
    {
        public long Id { get; set; }
        public double Price { get; set; }
        [JsonProperty("qty")]
        public double Quantity { get; set; }
        public double Commission { get; set; }
        public string CommissionAsset { get; set; }
        public long Time { get; set; }
        public bool IsBuyer { get; set; }
        public bool IsMaker { get; set; }
        public bool IsBestMatch { get; set; }
    }
}
