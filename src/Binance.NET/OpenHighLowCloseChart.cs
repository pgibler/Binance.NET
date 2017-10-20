using System.Collections.Generic;

namespace Binance.NET
{
    public class OpenHighLowCloseChart
    {
        public IList<double> Open { get; set; }=new List<double>();
        public IList<double> High { get; set; }=new List<double>();
        public IList<double> Low { get; set; }=new List<double>();
        public IList<double> Close { get; set; }=new List<double>();
        public IList<double> Volume { get; set; }= new List<double>();
    }
}
