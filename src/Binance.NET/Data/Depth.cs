namespace Binance.NET.Data
{
    public class Depth
    {
        public PriceQuantityCollection Bids { get; set; } = new PriceQuantityCollection();
        public PriceQuantityCollection Asks { get; set; } = new PriceQuantityCollection();
    }
}
