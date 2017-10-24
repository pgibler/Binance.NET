using Newtonsoft.Json;

namespace Binance.NET.Responses
{
    public class OrderStatusResponse
    {
        public long OrderId { get; set; }
        public string ClientOrderId { get; set; }
        public double Price { get; set; }
        [JsonProperty("origQty")]
        public double OriginalQuantity { get; set; }
        [JsonProperty("executedQty")]
        public double ExecutedQuantity { get; set; }
        public string Status { get; set; }
        public string TimeInForce { get; set; }
        public string Type { get; set; }
        public string Side { get; set; }
        public double StopPrice { get; set; }
        [JsonProperty("icebergQty")]
        public double IcebergQuantity { get; set; }
        public long Time { get; set; }
    }
}
