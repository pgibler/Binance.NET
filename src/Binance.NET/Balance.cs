using Newtonsoft.Json;

namespace Binance.NET
{
    public class Balance
    {
        [JsonProperty("asset")]
        public string Symbol { get; set; }
        [JsonProperty("free")]
        public double Available { get; set; }
        [JsonProperty("locked")]
        public double OnOrder { get; set; }
    }
}
