using System.Collections.Generic;

namespace Binance.NET
{
    internal class Info
    {
        public long Timestamp { get; set; }
        public Dictionary<long, InfoInterval> Intervals { get; set; }=new Dictionary<long, InfoInterval>();
        public int FirstUpdatedId { get; set; } = -1;
    }
}
