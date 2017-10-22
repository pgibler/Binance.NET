using Newtonsoft.Json;

namespace Binance.NET
{
    public class PreviousDayResponse
    {
        public double PriceChange { get; set; }
        public double PriceChangePercent { get; set; }
        [JsonProperty("weightedAvgPrice")]
        public double WeightedAveragePrice { get; set; }
        [JsonProperty("prevClosePrice")]
        public double PreviousClosePrice { get; set; }
        public double LastPrice { get; set; }
        public double BidPrice { get; set; }
        public double OpenPrice { get; set; }
        public double HighPrice { get; set; }
        public double LowPrice { get; set; }
        public double Volume { get; set; }
        public double CloseTime { get; set; }
        public long FirstId { get; set; }
        public long LastId { get; set; }
        public long Count { get; set; }
    }
}
