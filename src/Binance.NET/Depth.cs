using System.Collections.Generic;

namespace Binance.NET
{
    public class Depth
    {
        public Dictionary<double, double> Bids { get; set; }= new Dictionary<double, double>();
        public Dictionary<double, double> Asks { get; set; } = new Dictionary<double, double>();
    }
}
