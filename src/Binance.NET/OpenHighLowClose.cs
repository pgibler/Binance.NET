namespace Binance.NET
{
    public class OpenHighLowClose
    {
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public long Time { get; set; }
        public bool IsFinal { get; set; }
    }
}
