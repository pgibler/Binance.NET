using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Binance.NET
{
    public class BinanceApi : IDisposable
    {
        public BinanceApi(string apiKey, string apiSecret)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
        }

        private readonly string _apiKey;
        private readonly string _apiSecret;

        private Action<JToken> _executionCallback;
        private Action<JToken> _balanceCallback;

        private const string Base = "https://www.binance.com/api/";
        private const string WebsocketBase = "wss://stream.binance.com:9443/ws/";
        private static readonly Dictionary<string, DepthCache> DepthCacheData = new Dictionary<string, DepthCache>();

        private static readonly Dictionary<string, IList<JToken>> MessageQueue = new Dictionary<string, IList<JToken>>();
        private static readonly Dictionary<string, Info> Info = new Dictionary<string, Info>();
        private static readonly Dictionary<string, Dictionary<long, IList<JToken>>> KlineQueue = new Dictionary<string, Dictionary<long, IList<JToken>>>();
        private static readonly Dictionary<string, Dictionary<long, Dictionary<long, OpenHighLowClose>>> OpenHighLowCloseData = new Dictionary<string, Dictionary<long, Dictionary<long, OpenHighLowClose>>>();
        private static readonly Dictionary<string, Dictionary<long, OpenHighLowClose>> OpenHighLowCloseLatest = new Dictionary<string, Dictionary<long, OpenHighLowClose>>();

        private static readonly IList<CancellationTokenSource> CancellationTokenSources = new List<CancellationTokenSource>();

        public DepthCache DepthCache(string symbol)
        {
            return DepthCacheData.ContainsKey(symbol) ? DepthCacheData[symbol] : new DepthCache();
        }

        public DepthVolume DepthVolume(string symbol)
        {
            var cache = DepthCacheData[symbol];
            double bidBase = 0, askBase = 0, bidQty = 0, askQty = 0;
            foreach (var price in cache.Bids.Keys)
            {
                var quantity = cache.Bids[price];
                bidBase += quantity * price;
                bidQty += quantity;
            }
            foreach (var price in cache.Asks.Keys)
            {
                var quantity = cache.Asks[price];
                askBase += quantity * price;
                askQty += quantity;
            }
            return new DepthVolume
            {
                Bids = bidBase,
                Asks = askBase,
                BidQty = bidQty,
                AskQty = askQty
            };
        }

        public Dictionary<double, double> SortBids(string symbol, double max = Double.PositiveInfinity, bool baseValue = false)
        {
            var count = 0;
            DepthCache cache = DepthCacheData[symbol];
            var obj = new Dictionary<double, double>();
            var bids = cache.Bids;
            var sorted = bids.Keys.ToList();
            sorted.Sort();
            foreach (var price in sorted)
            {
                if (!baseValue)
                {
                    obj[price] = bids[price];
                }
                else
                {
                    obj[price] = bids[price] * price;
                }
                if (++count > max)
                {
                    break;
                }
            }
            return obj;
        }

        public Dictionary<double, double> SortAsks(string symbol, double max = Double.PositiveInfinity, bool baseValue = false)
        {
            var count = 0;
            DepthCache cache = DepthCacheData[symbol];
            var obj = new Dictionary<double, double>();
            var asks = cache.Asks;
            var sorted = asks.Keys.ToList();
            sorted.Sort();
            foreach (var price in sorted)
            {
                if (!baseValue)
                {
                    obj[price] = asks[price];
                }
                else
                {
                    obj[price] = asks[price] * price;
                }
                if (++count > max)
                {
                    break;
                }
            }
            return obj;
        }

        public void Buy(string symbol, double quantity, double price, Dictionary<string, string> flags=null)
        {
            Order("BUY", symbol, quantity, price, flags);
        }

        public void Sell(string symbol, double quantity, double price, Dictionary<string, string> flags = null)
        {
            Order("SELL", symbol, quantity, price, flags);
        }

        public void Cancel(string symbol, string orderId, Action<JToken> callback)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol},
                {"orderId", orderId}
            };
            SignedRequest($"{Base}v3/order", query, callback, HttpMethod.Delete);
        }

        public void OrderStatus(string symbol, string orderId, Action<JToken> callback)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol},
                {"orderId", orderId}
            };
            SignedRequest($"{Base}v3/order", query, callback, HttpMethod.Get);
        }

        public void OpenOrders(string symbol, Action<JToken> callback)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol}
            };
            SignedRequest($"{Base}v3/openOrders", query, callback, HttpMethod.Get);
        }

        public void AllOrders(string symbol, Action<JToken> callback)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol}
            };
            SignedRequest($"{Base}v3/allOrders", query, callback, HttpMethod.Get);
        }

        public void Depth(string symbol, Action<DepthCache> callback)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol}
            };
            PublicRequest($"{Base}v1/depth", query, o =>
            {
                callback(DepthData(o));
            }, HttpMethod.Get);
        }

        public void Prices(Action<Dictionary<string, double>> callback)
        {
            Request($"{Base}v1/ticker/allPrices", string.Empty, o =>
            {
                callback(PriceData(o));
            }, HttpMethod.Get, null);
        }

        public void BookTickers(Action<Dictionary<string, BookPrice>> callback)
        {
            Request($"{Base}v1/ticker/allBookTickers", string.Empty, o =>
            {
                callback(BookPriceData(o));
            }, HttpMethod.Get, null);
        }
        public void PreviousDay(string symbol, Action<JToken> callback)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol}
            };
            PublicRequest($"{Base}v1/depth", query, callback, HttpMethod.Get);
        }

        public void Account(Action<JToken> callback)
        {
            SignedRequest($"{Base}v3/account", null, callback, HttpMethod.Get);
        }

        public void Balance(Action<Dictionary<string, Balance>> callback)
        {
            SignedRequest($"{Base}v3/account", null, o =>
            {
                callback(BalanceData(o));
            }, HttpMethod.Get);
        }

        public void Trades(string symbol, Action<JToken> callback)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol}
            };
            SignedRequest($"{Base}v3/myTrades", query, callback, HttpMethod.Get);
        }

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

        public void Candlesticks(string symbol, string interval, Action<JToken> callback)
        {
            var query = new Dictionary<string, string>
            {
                {"symbol", symbol},
                {"interval", interval}
            };

            PublicRequest($"{Base}v1/klines", query, callback, HttpMethod.Get);
        }

        private string _listenKey;

        public CancellationTokenSource UserDataStream(Action<JToken> callback, Action<JToken> executionCallback)
        {
            return StreamWithCancellationToken(source =>
            {
                ApiRequest($"{Base}v1/userDataStream", new Dictionary<string, string>(), async o =>
                {
                    _listenKey = o["listenKey"].ToString();
                    await SetInterval(() =>
                    {
                        ApiRequest($"{Base}v1/userDataStream", new Dictionary<string, string>(), jObject => { }, HttpMethod.Put);
                    }, TimeSpan.FromSeconds(30));
                    if (executionCallback != null)
                    {
                        _balanceCallback = callback;
                        _executionCallback = executionCallback;
                        Subscribe(_listenKey, UserDataHandler, source.Token);
                    }
                }, HttpMethod.Post);
            });
        }

        public CancellationTokenSource DepthStream(string[] symbols, Action<JToken> callback)
        {
            return StreamWithCancellationToken(source =>
            {
                foreach (var symbol in symbols)
                {
                    Subscribe($"{symbol.ToLower()}@depth", callback, source.Token);
                }
            });
        }

        public CancellationTokenSource DepthCacheStream(string[] symbols, Action<string, DepthCache> callback)
        {
            return StreamWithCancellationToken(source =>
            {
                foreach (var symbol in symbols)
                {
                    if (!Info.ContainsKey(symbol)) { Info[symbol] = new Info(); }
                    Info[symbol].FirstUpdatedId = 0;
                    DepthCacheData[symbol] = new DepthCache();
                    MessageQueue[symbol] = new List<JToken>();
                    Subscribe($"{symbol.ToLower()}@depth", depth =>
                    {
                        if (Info["symbol"].FirstUpdatedId <= 0)
                        {
                            MessageQueue[symbol].Add(depth);
                        }
                    }, source.Token);

                    PublicRequest($"{Base}v1/depth", new Dictionary<string, string> { { "symbol", symbol } }, json =>
                    {
                        var lastUpdatedId = Convert.ToInt32(json["lastUpdateId"]);
                        Info[symbol].FirstUpdatedId = lastUpdatedId;
                        DepthCacheData[symbol] = DepthData(json);
                        foreach (var depth in MessageQueue[symbol])
                        {
                            DepthHandler(depth, lastUpdatedId);
                        }
                        MessageQueue.Remove(symbol);
                        callback?.Invoke(symbol, DepthCacheData[symbol]);
                    }, HttpMethod.Get);
                }
            });
        }

        public CancellationTokenSource TradesStream(string[] symbols, Action<JToken> callback)
        {
            return StreamWithCancellationToken(source =>
            {
                foreach (var symbol in symbols)
                {
                    Subscribe($"{symbol.ToLower()}@aggTrade", callback, source.Token);
                }
            });
        }

        public CancellationTokenSource Chart(string[] symbols, long interval, Action<JToken, long, Dictionary<long, OpenHighLowClose>> callback)
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
                    Subscribe($"{symbol.ToLower()}@kline_${interval}", kline =>
                    {
                        if (Info[symbol].Intervals[interval].Timestamp <= 0)
                        {
                            KlineQueue[symbol][interval].Add(kline);
                            return;
                        }

                        KlineHandler(symbol, kline);
                        callback?.Invoke(kline, interval, KlineConcat(symbol, interval));
                    }, source.Token);
                    PublicRequest($"{Base}v1/klines", new Dictionary<string, string> { {"symbol",symbol} }, data =>
                    {
                        foreach (var token in KlineQueue[symbol][interval])
                        {
                            KlineHandler(symbol, token, Info[symbol].Intervals[interval].Timestamp);
                        }
                        KlineQueue[symbol].Remove(interval);
                        callback?.Invoke(symbol, interval, KlineConcat(symbol, interval));
                    }, HttpMethod.Get);
                }
            });
        }

        public CancellationTokenSource CandlesticksStream(string[] symbols, long interval, Action<JToken> callback)
        {
            return StreamWithCancellationToken(source =>
            {
                foreach (var symbol in symbols)
                {
                    Subscribe($"{symbol.ToLower()}@kline_${interval}", callback, source.Token);
                }
            });
        }

        private CancellationTokenSource StreamWithCancellationToken(Action<CancellationTokenSource> action)
        {
            var source = GenerateCancellationTokenSource();

            action(source);

            return source;
        }

        private void DepthHandler(JToken depth, int firstUpdateId = 0)
        {
            var symbol = depth["s"].ToString();
            var updateId = Convert.ToInt32(depth["u"]);
            if (updateId <= firstUpdateId)
            {
                return;
            }
            foreach (var jToken in depth["b"].ToObject<JArray>())
            {
                var bids = (JArray)jToken;
                double bid = Convert.ToDouble(bids[0]);
                DepthCacheData[symbol].Bids[bid] = Convert.ToDouble(bids[1]);
                if (bids[1].ToString() == "0.00000000")
                {
                    DepthCacheData[symbol].Bids.Remove(Convert.ToDouble(bids[0]));
                }
            }
            foreach (var jToken in depth["a"].ToObject<JArray>())
            {
                var asks = (JArray)jToken;
                double bid = Convert.ToDouble(asks[0]);
                DepthCacheData[symbol].Asks[bid] = Convert.ToDouble(asks[1]);
                if (asks[1].ToString() == "0.00000000")
                {
                    DepthCacheData[symbol].Asks.Remove(Convert.ToDouble(asks[0]));
                }
            }
        }

        private void UserDataHandler(JToken userData)
        {
            var type = userData["e"].ToString();
            if (type == "outboundAccountInfo")
            {
                _balanceCallback(userData);
            }
            else if (type == "executionReport")
            {
                _executionCallback(userData);
            }
            else
            {
                Console.WriteLine($"Unexpected data: {type}");
            }
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

        private void Request(string url, Dictionary<string, string> query, Action<JObject> callback, HttpMethod method, Action<HttpClient> clientHandler)
        {
            query["symbol"] = !string.IsNullOrEmpty(query["symbol"]) ? query["symbol"].Replace("-", "") : "";

            Request(url, QueryString(query), callback, method, clientHandler);
        }

        private void Request(string url, string query, Action<JObject> callback, HttpMethod method, Action<HttpClient> clientHandler)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; Node C# API)");
            clientHandler(client);

            var queryString = query;
            var uri = new Uri($"{url}?{queryString}");
            var request = new HttpRequestMessage(method, uri)
            {
                Content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            var task = client.SendAsync(request);
            var jsonString = task.Result.Content.ReadAsStringAsync();
            var responseData = JObject.Parse(jsonString.Result);

            callback(responseData);
        }

        private void PublicRequest(string url, Dictionary<string, string> query, Action<JObject> callback, HttpMethod method)
        {
            Request(url, query, callback, method, client => {});
        }

        private void ApiRequest(string url, Dictionary<string, string> query, Action<JObject> callback, HttpMethod method)
        {
            Request(url, query, callback, method, client => { client.DefaultRequestHeaders.Add("X-MBX-APIKEY", _apiKey); });
        }

        private void SignedRequest(string url, Dictionary<string, string> query, Action<JObject> callback, HttpMethod method)
        {
            query["timestamp"] = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            query["symbol"] = !string.IsNullOrEmpty(query["symbol"]) ? query["symbol"].Replace("-", "") : "";
            query["recvWindow"] = !string.IsNullOrEmpty(query["recvWindow"]) ? query["recvWindow"] : "6500";

            var queryString = QueryString(query);

            var sha256 = new HMACSHA256(Encoding.ASCII.GetBytes(_apiSecret));
            var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(queryString));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            query["signature"] = signature;
            ApiRequest(url, query, callback, method);
        }
        
        private void Order(string side, string symbol, double quantity=1, double price=0.00000001, Dictionary<string, string> flags=null)
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

            SignedRequest($"{Base}v3/order", query, response => { Console.WriteLine(response.ToString()); }, HttpMethod.Post);
        }

        private async void Subscribe(string endpoint, Action<JToken> callback, CancellationToken token)
        {
            using (var socket = new ClientWebSocket())
            {
                await socket.ConnectAsync(new Uri($"{WebsocketBase}{endpoint}"), token);

                while (socket.State == WebSocketState.Open)
                {
                    var buffer = new ArraySegment<byte>();
                    await socket.ReceiveAsync(buffer, token);
                    string jsonString = Convert.ToString(buffer);
                    callback(JToken.Parse(jsonString));
                }
            }
        }

        private Dictionary<string, double> PriceData(JToken data)
        {
            var prices = new Dictionary<string, double>();
            foreach (var token in data)
            {
                var symbol = Convert.ToString(token["symbol"]);
                var price = Convert.ToDouble(token["price"]);

                prices[symbol] = price;
            }
            return prices;
        }

        private Dictionary<string, BookPrice> BookPriceData(JToken data)
        {
            var prices = new Dictionary<string, BookPrice>();
            foreach (var token in data)
            {
                var symbol = Convert.ToString(token["symbol"]);
                var bidPrice = Convert.ToDouble(token["bidPrice"]);
                var bidQty = Convert.ToDouble(token["bidQty"]);
                var askPrice = Convert.ToDouble(token["askPrice"]);
                var askQty = Convert.ToDouble(token["askQty"]);

                prices[symbol] = new BookPrice
                {
                    BidPrice = bidPrice,
                    Bids = bidQty,
                    AskPrice = askPrice,
                    Asks = askQty
                };
            }
            return prices;
        }

        private Dictionary<string, Balance> BalanceData(JToken data)
        {
            var balances = new Dictionary<string, Balance>();
            foreach(var userBalance in data["balances"])
            {
                var asset = userBalance["asset"];
                var free = userBalance["free"];
                var locked = userBalance["locked"];
                var balance = new Balance
                {
                    Available = Convert.ToDouble(free),
                    OnOrder = Convert.ToDouble(locked)
                };
                balances[Convert.ToString(asset)] = balance;
            }
            return balances;
        }

        private void KlineData(string symbol, long interval, JToken ticks)
        {
            long lastTime = 0;
            foreach (var tick in ticks)
            {
                long time = Convert.ToInt64(tick["time"]);
                double open = Convert.ToDouble(tick["open"]);
                double high = Convert.ToDouble(tick["high"]);
                double low = Convert.ToDouble(tick["low"]);
                double close = Convert.ToDouble(tick["close"]);
                double volume = Convert.ToDouble(tick["volume"]);

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

        private DepthCache DepthData(JToken depth)
        {
            var cache = new DepthCache();
            foreach (var jToken in depth["bids"])
            {
                var bid = (JArray) jToken;
                cache.Bids[bid[0].ToObject<double>()] = bid[1].ToObject<double>();
            }
            foreach (var jToken in depth["asks"])
            {
                var ask = (JArray) jToken;
                cache.Asks[ask[0].ToObject<double>()] = ask[1].ToObject<double>();
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
