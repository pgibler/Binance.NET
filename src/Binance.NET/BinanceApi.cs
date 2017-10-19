using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Binance.NET
{
    public class BinanceApi
    {
        public BinanceApi(string apiKey, string apiSecret)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
        }

        private readonly string _apiKey;
        private readonly string _apiSecret;

        private const string Base = "https://www.binance.com/api/";
        private static readonly Dictionary<string, DepthCache> DepthCacheData = new Dictionary<string, DepthCache>();

        public DepthCache DepthCache(string symbol)
        {
            return GetDepthCache(symbol);
        }

        public DepthVolume DepthVolume(string symbol)
        {
            var cache = GetDepthCache(symbol);
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
            DepthCache cache = GetDepthCache(symbol);
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
            DepthCache cache = GetDepthCache(symbol);
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

        private string QueryString(Dictionary<string, string> query)
        {
            return string.Join("&", query.Select(pair => $"{pair.Key}={pair.Value}"));
        }

        private void Request(string url, Dictionary<string, string> query, Action<JObject> callback, HttpMethod method, Action<HttpClient> clientHandler)
        {
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

        private void ApiRequest(string url, string query, Action<JObject> callback, HttpMethod method)
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

            var fullQueryString = $"{queryString}&signature={signature}";
            ApiRequest(url, fullQueryString, callback, method);
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
            foreach(var obj in data["balances"])
            {
                var asset = obj["asset"];
                var free = obj["free"];
                var locked = obj["locked"];
                var balance = new Balance
                {
                    Available = Convert.ToDouble(free),
                    OnOrder = Convert.ToDouble(locked)
                };
                balances[Convert.ToString(asset)] = balance;

            }
            return balances;
        }

        private DepthCache DepthData(JToken depth)
        {
            var cache = new DepthCache();
            foreach (var jToken in depth["bids"])
            {
                var obj = (JArray) jToken;
                cache.Bids[obj[0].ToObject<double>()] = obj[1].ToObject<double>();
            }
            foreach (var jToken in depth["asks"])
            {
                var obj = (JArray)jToken;
                cache.Asks[obj[0].ToObject<double>()] = obj[1].ToObject<double>();
            }
            return cache;
        }

        private DepthCache GetDepthCache(string symbol)
        {
            return DepthCacheData.ContainsKey(symbol) ? DepthCacheData[symbol] : new DepthCache();
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
    }
}
