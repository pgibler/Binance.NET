using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Binance.NET.Data;
using Binance.NET.Responses;
using Newtonsoft.Json.Linq;

namespace Binance.NET
{
    /// <summary>
    /// The Binance API facade class.
    /// </summary>
    public class Binance : IDisposable
    {
        /// <summary>
        /// Constructs an instance of the Binance class.
        /// </summary>
        /// <param name="apiKey">The API key provided on your Binance account.</param>
        /// <param name="apiSecret">The API secret key provided on your Binance account.</param>
        public Binance(string apiKey, string apiSecret)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
        }

        private readonly string _apiKey;
        private readonly string _apiSecret;

        private string _listenKey;

        private const string Base = "https://www.binance.com/api/";
        private const string WebsocketBase = "wss://stream.binance.com:9443/ws/";
        private static readonly Dictionary<string, Depth> DepthCacheData = new Dictionary<string, Depth>();

        private static readonly Dictionary<string, IList<JToken>> MessageQueue = new Dictionary<string, IList<JToken>>();
        private static readonly Dictionary<string, Info> Info = new Dictionary<string, Info>();
        private static readonly Dictionary<string, Dictionary<long, IList<JToken>>> KlineQueue = new Dictionary<string, Dictionary<long, IList<JToken>>>();
        private static readonly Dictionary<string, Dictionary<long, Dictionary<long, OpenHighLowClose>>> OpenHighLowCloseData = new Dictionary<string, Dictionary<long, Dictionary<long, OpenHighLowClose>>>();
        private static readonly Dictionary<string, Dictionary<long, OpenHighLowClose>> OpenHighLowCloseLatest = new Dictionary<string, Dictionary<long, OpenHighLowClose>>();

        private static readonly IList<CancellationTokenSource> CancellationTokenSources = new List<CancellationTokenSource>();
        public Action<BinanceException> DefaultExceptionCallback { get; set; }

        // Rest API calls.

        /// <summary>
        /// Returns the current depth cache of the supplied symbol.
        /// </summary>
        /// <param name="symbol">The symbol whose depth cache is being retrieved.</param>
        /// <returns>The current depth cache of the supplied symbol.</returns>
        public Depth DepthCache(string symbol)
        {
            return DepthCacheData.ContainsKey(symbol) ? DepthCacheData[symbol] : new Depth();
        }

        /// <summary>
        /// Returns the volume of bids & asks of a symbol.
        /// </summary>
        /// <param name="symbol">The symbol whose depth volume is being retrieved.</param>
        /// <returns>The depth volume of a symbol.</returns>
        public DepthVolume DepthVolume(string symbol)
        {
            var cache = DepthCache(symbol);
            double bidBase = 0, askBase = 0, bidQty = 0, askQty = 0;
            foreach (var bid in cache.Bids)
            {
                bidBase += bid.Quantity * bid.Price;
                bidQty += bid.Quantity;
            }
            foreach (var ask in cache.Asks)
            {
                askBase += ask.Quantity * ask.Price;
                askQty += ask.Quantity;
            }
            return new DepthVolume
            {
                Bids = bidBase,
                Asks = askBase,
                BidQuantity = bidQty,
                AskQuantity = askQty
            };
        }

        /// <summary>
        /// Returns a sorted list of all bids from the depth cache for the specified symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="max">The number of bids to </param>
        /// <param name="useBaseValue">If true, multiplies the value at each bid price by the quantity.</param>
        /// <returns>A sorted list of all bids from the depth cache for the specified symbol.</returns>
        public Dictionary<double, double> SortBids(string symbol, int max = Int32.MaxValue, bool useBaseValue = false)
        {
            Depth cache = DepthCache(symbol);
            var sortedBids = new Dictionary<double, double>();
            var bids = cache.Bids;
            var sorted = bids.Keys.ToList();
            sorted.Sort();
            foreach (var price in sorted.Take(max))
            {
                if (!useBaseValue)
                {
                    sortedBids[price] = bids[price].Quantity;
                }
                else
                {
                    sortedBids[price] = bids[price].Quantity * price;
                }
            }
            return sortedBids;
        }

        /// <summary>
        /// Returns a sorted list of all asks from the depth cache for the specified symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="max">The number of asks to </param>
        /// <param name="useBaseValue">If true, multiplies the value at each bid price by the quantity.</param>
        /// <returns>A sorted list of all asks from the depth cache for the specified symbol.</returns>
        public Dictionary<double, double> SortAsks(string symbol, int max = Int32.MaxValue, bool useBaseValue = false)
        {
            Depth cache = DepthCache(symbol);
            var sortedAsks = new Dictionary<double, double>();
            var asks = cache.Asks;
            var sorted = asks.Keys.ToList();
            sorted.Sort();
            foreach (var price in sorted.Take(max))
            {
                if (!useBaseValue)
                {
                    sortedAsks[price] = asks[price].Quantity;
                }
                else
                {
                    sortedAsks[price] = asks[price].Quantity * price;
                }
            }
            return sortedAsks;
        }

        /// <summary>
        /// Submits a buy order.
        /// </summary>
        /// <param name="symbol">The asset to submit the buy order for.</param>
        /// <param name="quantity">The amount of the asset to purchase.</param>
        /// <param name="price">The price at which to buy each unit of the asset.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void Buy(string symbol, double quantity, double price, Action<OrderActionResponse> successCallback = null, Action<BinanceException> exceptionCallback = null)
        {
            Buy(symbol, quantity, price, null, successCallback, exceptionCallback);
        }

        /// <summary>
        /// Submits a buy order.
        /// </summary>
        /// <param name="symbol">The asset to submit the buy order for.</param>
        /// <param name="quantity">The amount of the asset to purchase.</param>
        /// <param name="price">The price at which to buy each unit of the asset.</param>
        /// <param name="flags">Additional flags to include in the buy order.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void Buy(string symbol, double quantity, double price, Dictionary<string, string> flags, Action<OrderActionResponse> successCallback = null, Action<BinanceException> exceptionCallback = null)
        {
            Order("BUY", symbol, quantity, price, flags);
        }

        /// <summary>
        /// Submits a sell order.
        /// </summary>
        /// <param name="symbol">The asset to submit the sell order for.</param>
        /// <param name="quantity">The amount of the asset to purchase.</param>
        /// <param name="price">The price at which to sell each unit of the asset.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void Sell(string symbol, double quantity, double price, Action<OrderActionResponse> successCallback = null, Action<BinanceException> exceptionCallback = null)
        {
            Sell(symbol, quantity, price, null, successCallback, exceptionCallback);
        }

        /// <summary>
        /// Submits a sell order.
        /// </summary>
        /// <param name="symbol">The asset to submit the sell order for.</param>
        /// <param name="quantity">The amount of the asset to purchase.</param>
        /// <param name="price">The price at which to sell each unit of the asset.</param>
        /// <param name="flags">Additional flags to include in the sell order.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void Sell(string symbol, double quantity, double price, Dictionary<string, string> flags, Action<OrderActionResponse> successCallback = null, Action<BinanceException> exceptionCallback = null)
        {
            Order("SELL", symbol, quantity, price, flags);
        }

        /// <summary>
        /// Cancels an order.
        /// </summary>
        /// <param name="symbol">The asset to cancel the order of. This is required.</param>
        /// <param name="orderId">The ID of the order to cancel.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void CancelOrder(string symbol, long orderId, Action<OrderActionResponse> successCallback = null, Action<BinanceException> exceptionCallback = null)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol},
                {"orderId", orderId.ToString()}
            };

            SignedRequest($"{Base}v3/order", query, HttpMethod.Delete, response => successCallback?.Invoke(response.ToObject<OrderActionResponse>()), exceptionCallback);
        }

        /// <summary>
        /// Retrieves the status of an order.
        /// </summary>
        /// <param name="symbol">The asset of the order. This is required.</param>
        /// <param name="orderId">The ID of the order.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void OrderStatus(string symbol, long orderId, Action<OrderStatusResponse> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol},
                {"orderId", orderId.ToString()}
            };

            SignedRequest($"{Base}v3/order", query, HttpMethod.Get, response => successCallback(response.ToObject<OrderStatusResponse>()), exceptionCallback);
        }

        /// <summary>
        /// Retrieves all open orders for a specified asset.
        /// </summary>
        /// <param name="symbol">The asset to retrieve all open orders for.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void OpenOrders(string symbol, Action<OpenOrdersResponse> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol}
            };

            SignedRequest($"{Base}v3/openOrders", query, HttpMethod.Get, response => successCallback(response.ToObject<OpenOrdersResponse>()), exceptionCallback);
        }

        /// <summary>
        /// Retrieves all orders ever made by the account associated with the API key and secret supplied in the constructor.
        /// This includes historical and open trades.
        /// </summary>
        /// <param name="symbol">The asset to retrieve all orders for.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void AllOrders(string symbol, Action<AllOrdersResponse> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol}
            };

            SignedRequest($"{Base}v3/allOrders", query, HttpMethod.Get, response => response.ToObject<AllOrdersResponse>(), exceptionCallback);
        }

        /// <summary>
        /// Returns the depth of the supplied symbol.
        /// </summary>
        /// <param name="symbol">The asset to retrieve the depth of.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void Depth(string symbol, Action<Depth> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol}
            };

            PublicRequest($"{Base}v1/depth", query, HttpMethod.Get, response => successCallback(ConvertToDepth(response)), exceptionCallback);
        }

        /// <summary>
        /// Retrieves the price of all assets on the market.
        /// </summary>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void Prices(Action<Dictionary<string, double>> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            var responseCallback = new Action<JToken>(response =>
            {
                var prices = new Dictionary<string, double>();
                foreach (var token in response)
                {
                    var symbol = Convert.ToString(token["symbol"].ToString());
                    var price = Convert.ToDouble(token["price"].ToString());

                    prices[symbol] = price;
                }
                successCallback(prices);
            });

            Request($"{Base}v1/ticker/allPrices", string.Empty, HttpMethod.Get, null, responseCallback, exceptionCallback);
        }

        /// <summary>
        /// Retrieves the book prices for all assets.
        /// </summary>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void BookTickers(Action<Dictionary<string, BookPrice>> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            var responseCallback = new Action<JToken>(response =>
            {
                var prices = new Dictionary<string, BookPrice>();
                foreach (var token in response)
                {
                    var symbol = Convert.ToString(token["symbol"]);
                    var bidPrice = Convert.ToDouble(token["bidPrice"].ToString());
                    var bidQty = Convert.ToDouble(token["bidQty"].ToString());
                    var askPrice = Convert.ToDouble(token["askPrice"].ToString());
                    var askQty = Convert.ToDouble(token["askQty"].ToString());

                    prices[symbol] = new BookPrice
                    {
                        BidPrice = bidPrice,
                        Bids = bidQty,
                        AskPrice = askPrice,
                        Asks = askQty
                    };
                }
                successCallback(prices);
            });

            Request($"{Base}v1/ticker/allBookTickers", string.Empty, HttpMethod.Get, null, responseCallback, exceptionCallback);
        }

        /// <summary>
        /// Retrieves the previous day data about an asset.
        /// </summary>
        /// <param name="symbol">The asset whose data is being retrieved.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void PreviousDay(string symbol, Action<PreviousDayResponse> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol}
            };
            PublicRequest($"{Base}v1/ticker/24hr", query, HttpMethod.Get, response => response.ToObject<PreviousDayResponse>(), exceptionCallback);
        }

        /// <summary>
        /// Retrieves the account information associated with the API key and secret supplied in the constructor.
        /// </summary>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void Account(Action<AccountResponse> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            SignedRequest($"{Base}v3/account", new Dictionary<string, string>(), HttpMethod.Get, response => response.ToObject<AccountResponse>(), exceptionCallback);
        }

        /// <summary>
        /// Retrieves the balance for each asset held by the account.
        /// </summary>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void Balance(Action<Dictionary<string, Balance>> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            SignedRequest($"{Base}v3/account", new Dictionary<string, string>(), HttpMethod.Get, response =>
            {
                var account = response.ToObject<AccountResponse>();
                var balances = account.Balances;
                successCallback(balances.ToDictionary(balance => balance.Asset));
            }, exceptionCallback);
        }

        /// <summary>
        /// Retrieves a list of all trades made by this account for the asset.
        /// </summary>
        /// <param name="symbol">The asset whose trades are being retrieved.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void Trades(string symbol, Action<TradesResponse> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol}
            };
            SignedRequest($"{Base}v3/myTrades", query, HttpMethod.Get, response => response.ToObject<TradesResponse>(), exceptionCallback);
        }

        /// <summary>
        /// Gets candlestick information for the supplied symbol.
        /// </summary>
        /// <param name="symbol">The symbol whose candlestick data is being retrieved.</param>
        /// <param name="interval">The interval to generate the candelesticks in.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        public void Candlesticks(string symbol, string interval, Action<CandlesticksResponse> successCallback, Action<BinanceException> exceptionCallback = null)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol},
                {"interval", interval}
            };

            PublicRequest($"{Base}v1/klines", query, HttpMethod.Get, response => response.ToObject<CandlesticksResponse>(), exceptionCallback);
        }

        /// <summary>
        /// Generates charting data from open/high/low/close data.
        /// </summary>
        /// <param name="chart">The data to put into the chart instance.</param>
        /// <returns>Charting data generated from the input.</returns>
        public OpenHighLowCloseChart OpenHighLowClose(Dictionary<long, OpenHighLowClose> chart)
        {
            var result = new OpenHighLowCloseChart();
            foreach (var timestamp in chart.Keys)
            {
                var chartTime = chart[timestamp];
                result.Open.Add(chartTime.Open);
                result.Close.Add(chartTime.Close);
                result.High.Add(chartTime.High);
                result.Low.Add(chartTime.Low);
                result.Open.Add(chartTime.Open);
                result.Volume.Add(chartTime.Volume);
            }
            return result;
        }

        // WebSocket streams.


        /// <summary>
        /// Opens a stream to the user data.
        /// </summary>
        /// 
        /// <param name="successCallback">Callback to handle the "outboundAccountInfo" case.</param>
        /// <param name="executionCallback">Callback to handle the "executionReport" case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        /// <returns>A cancellation token to stop the stream with.</returns>
        public CancellationTokenSource UserDataStream(Action<UserDataStreamResponse> successCallback, Action<JToken> executionCallback=null, Action<BinanceException> exceptionCallback = null)
        {
            return StreamWithCancellationToken(source =>
            {
                ApiRequest($"{Base}v1/userDataStream", new Dictionary<string, string>(), HttpMethod.Post, async o =>
                {
                    _listenKey = o["listenKey"].ToString();
                    if (executionCallback != null)
                    {
                        Subscribe(_listenKey, source.Token, UserDataHandler(successCallback, executionCallback), exceptionCallback);
                    }
                    else
                    {
                        Subscribe(_listenKey, source.Token, response => successCallback(response.ToObject<UserDataStreamResponse>()));
                    }

                    await SetInterval(() =>
                    {
                        ApiRequest($"{Base}v1/userDataStream", new Dictionary<string, string>(), HttpMethod.Put, jObject => { }, exceptionCallback);
                    }, TimeSpan.FromSeconds(30));
                }, exceptionCallback);
            });
        }

        /// <summary>
        /// Opens a stream to the depth data.
        /// </summary>
        /// <param name="symbols">The assets to observe.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        /// <returns>A cancellation token to stop the stream with.</returns>
        public CancellationTokenSource DepthStream(string[] symbols, Action<DepthStreamResponse> successCallback, Action<BinanceException> exceptionCallback=null)
        {
            return StreamWithCancellationToken(source =>
            {
                foreach (var symbol in symbols)
                {
                    Subscribe($"{symbol.ToLower()}@depth", source.Token, response => response.ToObject<DepthStreamResponse>(), exceptionCallback);
                }
            });
        }

        /// <summary>
        /// Gets a stream of the depth cache for all of the supplied assets.
        /// </summary>
        /// <param name="symbols">The assets to observe.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        /// <returns>A cancellation token to stop the stream with.</returns>
        public CancellationTokenSource DepthCacheStream(string[] symbols, Action<string, Depth> successCallback, Action<BinanceException> exceptionCallback=null)
        {
            return StreamWithCancellationToken(source =>
            {
                foreach (var symbol in symbols)
                {
                    if (!Info.ContainsKey(symbol)) { Info[symbol] = new Info(); }
                    Info[symbol].FirstUpdatedId = 0;
                    DepthCacheData[symbol] = new Depth();
                    MessageQueue[symbol] = new List<JToken>();
                    Subscribe($"{symbol.ToLower()}@depth", source.Token, depth =>
                    {
                        if (Info["symbol"].FirstUpdatedId <= 0)
                        {
                            MessageQueue[symbol].Add(depth);
                        }
                    }, exceptionCallback);

                    PublicRequest($"{Base}v1/depth", new Dictionary<string, string> { { "symbol", symbol } }, HttpMethod.Get, json =>
                    {
                        var lastUpdatedId = Convert.ToInt32(json["lastUpdateId"].ToString());
                        Info[symbol].FirstUpdatedId = lastUpdatedId;
                        DepthCacheData[symbol] = ConvertToDepth(json);
                        foreach (var depth in MessageQueue[symbol])
                        {
                            DepthHandler(depth, lastUpdatedId);
                        }
                        MessageQueue.Remove(symbol);
                        successCallback?.Invoke(symbol, DepthCacheData[symbol]);
                    }, exceptionCallback);
                }
            });
        }

        /// <summary>
        /// Gets a stream of all trades of the supplied assets.
        /// </summary>
        /// <param name="symbols">The assets to observe.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        /// <returns>A cancellation token to stop the stream with.</returns>
        public CancellationTokenSource TradesStream(string[] symbols, Action<TradesStreamResponse> successCallback, Action<BinanceException> exceptionCallback=null)
        {
            return StreamWithCancellationToken(source =>
            {
                foreach (var symbol in symbols)
                {
                    Subscribe($"{symbol.ToLower()}@aggTrade", source.Token, response => response.ToObject<TradesStreamResponse>(), exceptionCallback);
                }
            });
        }

        /// <summary>
        /// Gets a stream of the chart data.
        /// </summary>
        /// <param name="symbols">The assets to observe.</param>
        /// <param name="interval">The interval to generate chart data from.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        /// <returns>A cancellation token to stop the stream with.</returns>
        public CancellationTokenSource ChartStream(string[] symbols, long interval, Action<JToken, long, Dictionary<long, OpenHighLowClose>> successCallback, Action<BinanceException> exceptionCallback=null)
        {
            return StreamWithCancellationToken(source =>
            {
                foreach (var symbol in symbols)
                {
                    if (!Info.ContainsKey(symbol))
                    {
                        Info[symbol] = new Info();
                    }
                    if (!Info[symbol].Intervals.ContainsKey(interval))
                    {
                        Info[symbol].Intervals[interval] = new InfoInterval();
                    }
                    if (!OpenHighLowCloseData.ContainsKey(symbol))
                    {
                        OpenHighLowCloseData[symbol] = new Dictionary<long, Dictionary<long, OpenHighLowClose>>();
                    }
                    if (!OpenHighLowCloseData[symbol].ContainsKey(interval))
                    {
                        OpenHighLowCloseData[symbol][interval] = new Dictionary<long, OpenHighLowClose>();
                    }
                    if (!OpenHighLowCloseLatest.ContainsKey(symbol))
                    {
                        OpenHighLowCloseLatest[symbol] = new Dictionary<long, OpenHighLowClose>();
                    }
                    if (!OpenHighLowCloseLatest[symbol].ContainsKey(interval))
                    {
                        OpenHighLowCloseLatest[symbol][interval] = new OpenHighLowClose();
                    }
                    if (!KlineQueue.ContainsKey(symbol))
                    {
                        KlineQueue[symbol] = new Dictionary<long, IList<JToken>>();
                    }
                    if (!KlineQueue[symbol].ContainsKey(interval))
                    {
                        KlineQueue[symbol][interval] = new List<JToken>();
                    }
                    Info[symbol].Intervals[interval].Timestamp = 0;
                    Subscribe($"{symbol.ToLower()}@kline_${interval}", source.Token, kline =>
                    {
                        if (Info[symbol].Intervals[interval].Timestamp <= 0)
                        {
                            KlineQueue[symbol][interval].Add(kline);
                            return;
                        }

                        KlineHandler(symbol, kline);
                        successCallback?.Invoke(kline, interval, KlineConcat(symbol, interval));
                    }, exceptionCallback);
                    PublicRequest($"{Base}v1/klines", new Dictionary<string, string> { {"symbol",symbol} }, HttpMethod.Get, data =>
                    {
                        KlineData(symbol, interval, data);
                        foreach (var token in KlineQueue[symbol][interval])
                        {
                            KlineHandler(symbol, token, Info[symbol].Intervals[interval].Timestamp);
                        }
                        KlineQueue[symbol].Remove(interval);
                        successCallback?.Invoke(symbol, interval, KlineConcat(symbol, interval));
                    }, exceptionCallback);
                }
            });
        }

        /// <summary>
        /// Gets a stream of the candlestick data.
        /// </summary>
        /// <param name="symbols">The assets to observe.</param>
        /// <param name="interval">The interval to generate candlestick data from.</param>
        /// <param name="successCallback">Callback to handle the success case.</param>
        /// <param name="exceptionCallback">Callback to handle the exception case.</param>
        /// <returns>A cancellation token to stop the stream with.</returns>
        public CancellationTokenSource CandlesticksStream(string[] symbols, long interval, Action<CandlesticksStreamResponse> successCallback, Action<BinanceException> exceptionCallback=null)
        {
            return StreamWithCancellationToken(source =>
            {
                foreach (var symbol in symbols)
                {
                    Subscribe($"{symbol.ToLower()}@kline_${interval}", source.Token, response => response.ToObject<CandlesticksStreamResponse>(), exceptionCallback);
                }
            });
        }
        
        // Private members.

        private CancellationTokenSource StreamWithCancellationToken(Action<CancellationTokenSource> action)
        {
            var source = GenerateCancellationTokenSource();

            action(source);

            return source;
        }

        private void DepthHandler(JToken depth, int firstUpdateId = 0)
        {
            var symbol = depth["s"].ToString();
            var updateId = Convert.ToInt32(depth["u"].ToString());
            if (updateId <= firstUpdateId)
            {
                return;
            }
            foreach (var jToken in depth["b"].ToObject<JArray>())
            {
                var bids = (JArray)jToken;
                double bid = Convert.ToDouble(bids[0].ToString());
                DepthCacheData[symbol].Bids.Set(bid, Convert.ToDouble(bids[1].ToString()));
                if (bids[1].ToString() == "0.00000000")
                {
                    DepthCacheData[symbol].Bids.Remove(Convert.ToDouble(bids[0].ToString()));
                }
            }
            foreach (var jToken in depth["a"].ToObject<JArray>())
            {
                var asks = (JArray)jToken;
                double bid = Convert.ToDouble(asks[0].ToString());
                DepthCacheData[symbol].Asks.Set(bid, Convert.ToDouble(asks[1].ToString()));
                if (asks[1].ToString() == "0.00000000")
                {
                    DepthCacheData[symbol].Asks.Remove(Convert.ToDouble(asks[0].ToString()));
                }
            }
        }

        private Action<JToken> UserDataHandler(Action<UserDataStreamResponse> balanceCallback, Action<JToken> executionCallback)
        {
            return userData =>
            {
                var type = userData["e"].ToString();
                if (type == "outboundAccountInfo")
                {
                    balanceCallback(userData.ToObject<UserDataStreamResponse>());
                }
                else if (type == "executionReport")
                {
                    executionCallback(userData);
                }
                else
                {
                    Console.WriteLine($"Unexpected data: {type}");
                }
            };
        }

        private static async Task SetInterval(Action action, TimeSpan timeout)
        {
            await Task.Delay(timeout).ConfigureAwait(false);

            action();

            await SetInterval(action, timeout);
        }

        private string QueryString(Dictionary<string, string> query)
        {
            return string.Join("&", query.Select(pair => $"{pair.Key}={pair.Value}"));
        }

        private void Request(string url, Dictionary<string, string> query, HttpMethod method, Action<HttpClient> clientHandler, Action<JToken> successCallback, Action<BinanceException> exceptionCallback)
        {
            if (query.ContainsKey("symbol"))
            {
                query["symbol"] = query["symbol"].Replace("-", "");
            }

            Request(url, QueryString(query), method, clientHandler, successCallback, exceptionCallback);
        }

        private void Request(string url, string query, HttpMethod method, Action<HttpClient> clientHandler, Action<JToken> successCallback, Action<BinanceException> exceptionCallback)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; Node C# API)");
            clientHandler?.Invoke(client);

            var queryString = query;
            var uri = new Uri($"{url}?{queryString}");
            var request = new HttpRequestMessage(method, uri)
            {
                Content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            var requestTask = client.SendAsync(request);

            var jsonString = requestTask.Result.Content.ReadAsStringAsync();
            var response = JToken.Parse(jsonString.Result);

            if (response is JObject && response["code"] != null)
            {
                var exception = new BinanceException(Convert.ToInt32(response["code"].ToString()),
                    response["msg"].ToString());

                if (exceptionCallback != null)
                {
                    exceptionCallback(exception);
                }
                else
                {
                    DefaultExceptionCallback?.Invoke(exception);
                }
            }
            else
            {
                successCallback(response);
            }
        }

        private void PublicRequest(string url, Dictionary<string, string> query, HttpMethod method, Action<JToken> successCallback, Action<BinanceException> exceptionCallback)
        {
            Request(url, query, method, client => {}, successCallback, exceptionCallback);
        }

        private void ApiRequest(string url, Dictionary<string, string> query, HttpMethod method, Action<JToken> successCallback, Action<BinanceException> exceptionCallback)
        {
            Request(url, query, method, client => { client.DefaultRequestHeaders.Add("X-MBX-APIKEY", _apiKey); }, successCallback, exceptionCallback);
        }

        private void SignedRequest(string url, Dictionary<string, string> query, HttpMethod method, Action<JToken> successCallback, Action<BinanceException> exceptionCallback)
        {
            query["timestamp"] = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            query["recvWindow"] = query.ContainsKey("recvWindow") && !string.IsNullOrEmpty(query["recvWindow"]) ? query["recvWindow"] : "6500";

            if (query.ContainsKey("symbol"))
            {
                query["symbol"] = query["symbol"].Replace("-", "");
            }

            var queryString = QueryString(query);

            var sha256 = new HMACSHA256(Encoding.ASCII.GetBytes(_apiSecret));
            var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(queryString));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            query["signature"] = signature;
            ApiRequest(url, query, method, successCallback, exceptionCallback);
        }
        
        private void Order(string side, string symbol, double quantity=1, double price=0.00000001, Dictionary<string, string> flags=null, Action<OrderActionResponse> successCallback=null, Action<BinanceException> exceptionCallback=null)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol},
                {"side", side},
                {"type", "LIMIT"},
                {"price", price.ToString("F8")},
                {"quantity", quantity.ToString("F8")},
                {"timeInForce", "GTC"},
                {"recvWindow", 6000000.ToString("G")}
            };

            if (flags != null)
            {
                CopyDictionaryKey(flags, query, "type");
                CopyDictionaryKey(flags, query, "icebergQty");
                CopyDictionaryKey(flags, query, "stopPrice");
            }
            
            SignedRequest($"{Base}v3/order", query, HttpMethod.Post, response => { successCallback?.Invoke(response.ToObject<OrderActionResponse>()); }, exceptionCallback);
        }

        private async void Subscribe(string endpoint, CancellationToken cancellationToken, Action<JToken> successCallback, Action<BinanceException> exceptionCallback=null)
        {
            using (var socket = new ClientWebSocket())
            {
                await socket.ConnectAsync(new Uri($"{WebsocketBase}{endpoint}"), cancellationToken);

                while (socket.State == WebSocketState.Open)
                {
                    var buffer = new ArraySegment<byte>(new byte[512]);
                    await socket.ReceiveAsync(buffer, cancellationToken);
                    string jsonString = Convert.ToString(buffer);

                    var response = JToken.Parse(jsonString);

                    if (response["code"] != null)
                    {
                        var binanceException = new BinanceException(
                            Convert.ToInt32(response["code"]),
                            Convert.ToString(response["message"].ToString()));

                        if (exceptionCallback != null)
                        {
                            exceptionCallback(binanceException);
                        }
                        else
                        {
                            DefaultExceptionCallback?.Invoke(binanceException);
                        }
                    }
                    else
                    {
                        successCallback(JToken.Parse(jsonString));
                    }
                }
            }
        }

        private void KlineData(string symbol, long interval, JToken ticks)
        {
            long lastTime = 0;
            foreach (var tick in ticks)
            {
                long time = Convert.ToInt64(tick["time"].ToString());
                double open = Convert.ToDouble(tick["open"].ToString());
                double high = Convert.ToDouble(tick["high"].ToString());
                double low = Convert.ToDouble(tick["low"].ToString());
                double close = Convert.ToDouble(tick["close"].ToString());
                double volume = Convert.ToDouble(tick["volume"].ToString());

                OpenHighLowCloseData[symbol][interval][time] = new OpenHighLowClose
                {
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    Volume = volume
                };
                lastTime = time;
            }
            Info[symbol].Intervals[interval].Timestamp = lastTime;
        }

        private Dictionary<long, OpenHighLowClose> KlineConcat(string symbol, long interval)
        {
            var output = OpenHighLowCloseData[symbol][interval];
            var latest = OpenHighLowCloseLatest[symbol][interval];
            if (latest.Time > 0)
            {
                long time = latest.Time;
                long lastUpdated = output.Keys.Last();
                if (time > lastUpdated)
                {
                    output[time] = latest;
                    // Reset the time.
                    output[time].Time = 0;
                    output[time].IsFinal = false;
                }
            }
            return output;
        }

        private void KlineHandler(string symbol, JToken kline, long firstTime=0)
        {
            JToken ticks = kline["ticks"];
            
            long interval = Convert.ToInt64(ticks["i"]);
            bool isFinal = Convert.ToBoolean(ticks["x"]);
            long time = Convert.ToInt64(ticks["t"]);

            if (time <= firstTime)
            {
                return;
            }

            if (!isFinal)
            {
                if (OpenHighLowCloseLatest[symbol][interval].Time > 0)
                {
                    if (OpenHighLowCloseLatest[symbol][interval].Time > time)
                    {
                        return;
                    }
                }
            }

            // Clean up an element.
            var firstUpdated = OpenHighLowCloseData[symbol][interval].First().Key;
            if (firstUpdated > 0)
            {
                OpenHighLowCloseData[symbol][interval].Remove(firstUpdated);
            }
        }

        private Depth ConvertToDepth(JToken depth)
        {
            var cache = new Depth();
            foreach (var jToken in depth["bids"])
            {
                var bid = (JArray) jToken;
                var price = bid[0].ToObject<double>();
                var quantity = bid[1].ToObject<double>();
                cache.Bids.Set(price, quantity);
            }
            foreach (var jToken in depth["asks"])
            {
                var ask = (JArray) jToken;
                var price = ask[0].ToObject<double>();
                var quantity = ask[1].ToObject<double>();
                cache.Asks.Set(price, quantity);
            }
            return cache;
        }

        private void CopyDictionaryKey(Dictionary<string, string> first, Dictionary<string, string> second, string key)
        {
            if (first?[key] != null)
            {
                if (second != null)
                {
                    second[key] = first[key];
                }
            }
        }

        private CancellationTokenSource GenerateCancellationTokenSource()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            CancellationTokenSources.Add(cancellationTokenSource);

            return cancellationTokenSource;
        }

        public void Dispose()
        {
            foreach (var token in CancellationTokenSources)
            {
                token.Dispose();
            }
        }
    }
}
