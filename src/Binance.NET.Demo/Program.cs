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

            //binance.Buy("ETH-BTC", 1.0, 0.001);
            //                                    
            //binance.Sell("BTC-USDT", 0.001, 10000);
            //                        
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
            //binance.Cancel("ETH-BTC", "jflkjluoiz", token =>
            //{
            //    // Handle cancel response.
            //});

        }
    }
}
