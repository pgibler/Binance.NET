using System.Collections.Generic;
using Binance.NET.Data;

namespace Binance.NET.Data
{
    internal class Info
    {
        public long Timestamp { get; set; }
        public Dictionary<long, InfoInterval> Intervals { get; set; }=new Dictionary<long, InfoInterval>();
        public int FirstUpdatedId { get; set; } = -1;
    }
}
