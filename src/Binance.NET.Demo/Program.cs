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
                Console.WriteLine($"Catch: code - ${exception.Code} | message - ${exception.Message}");
            });

            binance.DefaultExceptionCallback = exceptionHandler;

            // API call tests

            binance.Buy("ETH-BTC", 1.0, 0.001, order =>
            {
                binance.CancelOrder("ETH-BTC", order.OrderId);
            });
                                                
            binance.Sell("BTC-USDT", 0.001, 10000);

            binance.Depth("ETH-BTC", depth =>
            {
                Console.WriteLine($"Asks: ${string.Join(",", depth.Asks.Keys)}, Bids: ${string.Join(",", depth.Bids.Keys)}");
            });
                        
            var volume = binance.DepthVolume("ETH-BTC");

            Console.WriteLine($"Ask volume: {volume.Asks}, Bid volume: {volume.Bids}");
            
            var sortedBids = binance.SortBids("ETH-BTC");
            
            Console.WriteLine($"Asks: {string.Join(",", sortedBids.Keys)}");

            long orderId = 0;
            
            binance.CancelOrder("ETH-BTC", orderId,
                response =>
                {
                    Console.WriteLine($"Order #${response.OrderId} cancelled");
                },
                exception =>
                {
                    Console.WriteLine($"Error message: {exception.Message}");
                });
            
            binance.OrderStatus("ETH-BTC", orderId, response =>
            {
                Console.WriteLine($"Order #{response.OrderId} has status {response.Status}");
            });
            
            binance.OpenOrders("ETH-BTC", orders =>
            {
                Console.WriteLine($"First order open: {orders.First().OrderId}");
            });
            
            binance.AllOrders("ETH-BTC", orders =>
            {
                Console.WriteLine($"First order ever: {orders.First().OrderId}");
            });
            
            binance.Prices(prices =>
            {
                Console.WriteLine($"Assets on the market: {prices.Count}. First asset price: Asset - {prices.First().Key}, Price - {prices.First().Value}");
            });
            
            binance.BookTickers(tickers =>
            {
                Console.WriteLine($"Tickers count: {tickers.Count}, First symbol & ask: {tickers.First().Key} & {tickers.First().Value.AskPrice}");
            });
            
            binance.PreviousDay("ETH-BTC", previousDay =>
            {
                Console.WriteLine($"24 hour % change - ${previousDay.PriceChangePercent}");
            });
            
            binance.Account(account =>
            {
                Console.WriteLine($"Account can trade: {account.CanTrade}");
            });
            
            binance.Balance(balances =>
            {
                Console.WriteLine($"First asset: {balances.First().Key}");
            });
            
            binance.Trades("ETH-BTC", trades =>
            {
                Console.WriteLine($"First trade price: {trades.First().Price}");
            });

            var cache = binance.DepthCache("ETH-BTC");
            
            binance.DepthStream(new[] {"ETH-BTC", "LTC-BTC"}, depth =>
            {
                Console.WriteLine($"Incoming asks: {depth.Asks.First().Price}. Incoming bids: {depth.Bids.First().Price}");

                if (depth.Asks.Any())
                {
                    var ask = depth.Asks.First();
                    Console.WriteLine($"First ask values: Price {ask.Price}, Quantity {ask.Quantity}");
                }

                if (depth.Bids.Any())
                {
                    var bid = depth.Bids.First();
                    Console.WriteLine($"First bid values: Price {bid.Price}, Quantity {bid.Quantity}");
                }
            });
            
            binance.DepthCacheStream(new[] { "ETH-BTC", "LTC-BTC" }, (symbol, depth) =>
            {
                Console.WriteLine($"Depth cache stream received data for symbol {symbol}.");

                if (depth.Asks.Any())
                {
                    var ask = depth.Asks.First();
                    Console.WriteLine($"First ask values: Price {ask.Price}, Quantity {ask.Quantity}");
                }

                if (depth.Bids.Any())
                {
                    var bid = depth.Bids.First();
                    Console.WriteLine($"First bid values: Price {bid.Price}, Quantity {bid.Quantity}");
                }
            });
            
            binance.TradesStream(new[] {"ETH-BTC", "LTC-BTC"}, trade =>
            {
                Console.WriteLine($"Trade time: {trade.TradeTime}");
            });
            
            binance.ChartStream(new[] {"ETH-BTC", "LTC-BTC"}, 9999, (response, interval, ohlcDict) =>
            {
                Console.WriteLine("Chart call invoked.");
            });

            binance.UserDataStream(account =>
            {
                Console.WriteLine("User data stream invoked.");
            });
        }
    }
}
