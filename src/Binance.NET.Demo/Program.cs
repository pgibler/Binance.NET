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

            //binance.Depth("ETH-BTC", depth =>
            //{
            //    Console.WriteLine($"Depth - Asks: ${depth.Asks.Keys.Count}, Bids: ${depth.Bids.Keys.Count}");
            //});
            //            
            //var depthCache = binance.Depth("ETH-BTC");
            //            
            //Console.WriteLine($"Asks: {depthCache.Asks.Keys.Count}, Bids: {depthCache.Bids.Keys.Count}");
            //            
            //var volume = binance.DepthVolume("ETH-BTC");
            //                        
            //Console.WriteLine($"Bids: {volume.Bids}, Asks: {volume.Asks}, BidQuantity: {volume.BidQuantity}, AskQuantity: {volume.AskQuantity}");
            //
            //var sortedBids = binance.SortBids("ETH-BTC");
            //
            //Console.WriteLine($"Asks: {string.Join(",", sortedBids.Keys)}");
            //
            //var orderId = "jflkjluoiz";
            //
            //binance.Cancel("ETH-BTC", , response =>
            //{
            //    // Handle cancel response.
            //});
            //
            //binance.OrderStatus("ETH-BTC", orderId, response =>
            //{
            //                
            //});
            //
            //
            //binance.OpenOrders("ETH-BTC", response =>
            //{
            //    // Handle open orders response
            //});
            //
            //binance.AllOrders("ETH-BTC", response =>
            //{
            //    // Handle all orders response
            //});
            //
            //binance.Prices(prices =>
            //{
            //    // Handle price data.
            //});
            //
            //binance.BookTickers(tickers =>
            //{
            //    // Handle book tickers
            //});
            //
            //binance.PreviousDay("ETH-BTC", response =>
            //{
            //    // Handle previous 24 hour response
            //});
            //
            //binance.Account(response =>
            //{
            //    // Handle account response
            //});
            //
            //binance.Balance(balances =>
            //{
            //    // Handle balance information. Stored as k/v pairs.
            //});
            //
            //binance.Trades("ETH-BTC", response =>
            //{
            //    // Handle trade response
            //});
            //
            //binance.DepthStream(new[] {"ETH-BTC", "LTC-BTC"}, response =>
            //{
            //    // Handle stream responses for specified symbols
            //});
            //
            //binance.DepthCacheStream(new[] { "ETH-BTC", "LTC-BTC" }, (symbol, depth) =>
            //{
            //    // Handle symbol and depth data for specified symbols
            //});
            //
            //binance.TradesStream(new[] {"ETH-BTC", "LTC-BTC"}, response =>
            //{
            //    // Handle trade stream response
            //});
            //
            //binance.Chart(new[] {"ETH-BTC", "LTC-BTC"}, 9999, (response, interval, ohlcDict) =>
            //{
            //    // Handle chart stream.
            //});
        }
    }
}
