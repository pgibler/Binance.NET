using System;
using System.IO;
using System.Linq;

namespace Binance.NET.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Read in api key and secret settings.
            var lines = File.ReadAllLines("settings.txt").Select(i => i.Split('=')[1]).ToArray();
            
            string apiKey = lines[0];
            string apiSecret = lines[1];

            // Instantiate binance API object with default exception handling.

            var binance = new BinanceApi(apiKey, apiSecret);

            var exceptionHandler = new Action<BinanceApiException>(exception =>
            {
                Console.WriteLine($"Error: code - ${exception.Code} | message - ${exception.Message}");
            });

            binance.DefaultExceptionCallback = exceptionHandler;

            // API call tests

            binance.Buy("ETH-BTC", 1.0, 0.001);
                                                
            binance.Sell("BTC-USDT", 0.001, 10000);

            binance.Depth("ETH-BTC", depth =>
            {
                // Handle depth response
            });
                        
            var volume = binance.DepthVolume("ETH-BTC");
            
            var sortedBids = binance.SortBids("ETH-BTC");
            
            Console.WriteLine($"Asks: {string.Join(",", sortedBids.Keys)}");
            
            var orderId = "";
            
            binance.CancelOrder("ETH-BTC", orderId, response =>
            {
                Console.WriteLine("Call completed");
                // Handle cancel response.
            });
            
            binance.OrderStatus("ETH-BTC", orderId, response =>
            {
                Console.WriteLine("Call completed");
                // Handle order status response.
            });
            
            
            binance.OpenOrders("ETH-BTC", response =>
            {
                Console.WriteLine("Call completed");
                // Handle open orders response.
            });
            
            binance.AllOrders("ETH-BTC", response =>
            {
                Console.WriteLine("Call completed");
                // Handle all orders response.
            });
            
            binance.Prices(prices =>
            {
                Console.WriteLine("Call completed");
                // Handle prices.
            });
            
            binance.BookTickers(tickers =>
            {
                Console.WriteLine("Call completed");
                // Handle book tickers.
            });
            
            binance.PreviousDay("ETH-BTC", response =>
            {
                Console.WriteLine("Call completed");
                // Handle previous 24 hour response.
            });
            
            binance.Account(response =>
            {
                Console.WriteLine("Call completed");
                // Handle account response.
            });
            
            binance.Balance(balances =>
            {
                Console.WriteLine("Call completed");
                // Handle balance information. Stored as k/v pairs.
            });
            
            binance.Trades("ETH-BTC", response =>
            {
                Console.WriteLine("Call completed");
                // Handle trade response.
            });
            
            binance.DepthStream(new[] {"ETH-BTC", "LTC-BTC"}, response =>
            {
                Console.WriteLine("Call completed");
                // Handle stream responses for specified symbols.
            });
            
            binance.DepthCacheStream(new[] { "ETH-BTC", "LTC-BTC" }, (symbol, depth) =>
            {
                Console.WriteLine("Call completed");
                // Handle symbol and depth data for specified symbols.
            });
            
            binance.TradesStream(new[] {"ETH-BTC", "LTC-BTC"}, response =>
            {
                Console.WriteLine("Call completed");
                // Handle trade stream response.
            });
            
            binance.ChartStream(new[] {"ETH-BTC", "LTC-BTC"}, 9999, (response, interval, ohlcDict) =>
            {
                Console.WriteLine("Call completed");
                // Handle chart stream.
            });
        }
    }
}
