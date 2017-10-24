using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Binance.NET
{
    public class PriceQuantityCollection : IEnumerable<PriceQuantity>
    {
        private Dictionary<double, double> BackingDictionary { get; }=new Dictionary<double, double>();
        public IEnumerable<double> Keys => BackingDictionary.Keys;
        public IEnumerable<double> Values => BackingDictionary.Values;

        public PriceQuantity this[double d] => new PriceQuantity
        {
            Price = d,
            Quantity = BackingDictionary[d]
        };

        public void Set(double price, double quantity)
        {
            BackingDictionary[price] = quantity;
        }

        public IEnumerator<PriceQuantity> GetEnumerator()
        {
            return new PriceQuantityEnum(BackingDictionary);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public long Count()
        {
            return BackingDictionary.Count;
        }

        internal void Remove(double price)
        {
            BackingDictionary.Remove(price);
        }
    }

    public class PriceQuantityEnum : IEnumerator<PriceQuantity>
    {
        private readonly List<KeyValuePair<double, double>> _priceQuantityPairs;

        private int _position = -1;

        public PriceQuantityEnum(Dictionary<double, double> backingDictionary)
        {
            _priceQuantityPairs = backingDictionary.ToList();
        }

        public bool MoveNext()
        {
            _position++;
            return _position < _priceQuantityPairs.Count;
        }

        public void Reset()
        {
            _position = -1;
        }

        public PriceQuantity Current
        {
            get
            {
                var pair = _priceQuantityPairs[_position];
                return new PriceQuantity
                {
                    Price = pair.Key,
                    Quantity = pair.Value
                };
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }

    public class PriceQuantity
    {
        public double Price { get; set; }
        public double Quantity { get; set; }
    }
}
