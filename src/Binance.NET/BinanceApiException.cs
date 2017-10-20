using System;

namespace Binance.NET
{
    public class BinanceApiException : Exception
    {
        public int Code { get; set; }

        public BinanceApiException(int code, string message) : base(message)
        {
            Code = code;
        }
    }
}
