using System;
using System.IO;
using System.Linq;

namespace Binance.NET.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var lines = File.ReadAllLines("settings.txt").Select(i => i.Split('=')[1]).ToArray();
            
            string apiKey = lines[0];
            string apiSecret = lines[1];

            var binance = new BinanceApi(apiKey, apiSecret);

            binance.Buy("ETH-BTC", 1.0, 0.001);
            
            binance.Sell("BTC-USDT", 0.001, 10000);

            binance.Depth("ETH-BTC", Console.WriteLine);
        }
    }
}
