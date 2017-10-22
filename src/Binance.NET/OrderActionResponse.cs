using Newtonsoft.Json;

namespace Binance.NET
{
    public class OrderActionResponse
    {
        [JsonProperty("origClientOrderId")]
        public string OriginalClientOrderId { get; set; }
        [JsonProperty("clientOrderId")]
        public string ClientOrderId { get; set; }
        public long OrderId { get; set; }
    }
}
