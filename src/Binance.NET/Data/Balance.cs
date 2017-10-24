using Newtonsoft.Json;

namespace Binance.NET.Data
{
    public class Balance
    {
        [JsonProperty("asset")]
        public string Asset { get; set; }
        [JsonProperty("free")]
        public double Available { get; set; }
        [JsonProperty("locked")]
        public double Locked { get; set; }
    }
}
