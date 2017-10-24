using System;

namespace Binance.NET
{
    public class BinanceException : Exception
    {
        public int Code { get; set; }

        public BinanceException(int code, string message) : base(message)
        {
            Code = code;
        }
    }
}
