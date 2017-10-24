using Newtonsoft.Json;

namespace Binance.NET.Responses
{
    public class TradesStreamResponse
    {
        [JsonProperty("e")]
        public string EventType { get; set; }
        [JsonProperty("E")]
        public long EventTime { get; set; }
        [JsonProperty("s")]
        public string Symbol { get; set; }
        [JsonProperty("a")]
        public long AggregatedTradeId { get; set; }
        [JsonProperty("p")]
        public double Price { get; set; }
        [JsonProperty("f")]
        public long FirstBreakdownTradeId { get; set; }
        [JsonProperty("l")]
        public long LastBreakdownTradeId { get; set; }
        [JsonProperty("T")]
        public long TradeTime { get; set; }
        [JsonProperty("m")]
        public bool IsBuyerMaker { get; set; }
    }
}
