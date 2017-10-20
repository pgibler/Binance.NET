# Binance.NET
A .NET Standard 2.0+ wrapper for the Binance API.

### Installation

```
git clone https://github.com/pgibler/Binance.NET
cd Binance.NET/src
dotnet build
```

### Usage

To run Binance.NET in your C# application, you need your API key & secret handy. Once you get those from your account on binance, you can initialize the `Binance` object as such:

```cs
// Instantiate a binance API service interaction instance.
var binance = new BinanceApi(apiKey, apiSecret);

// Create a buy order.
binance.Buy("ETH-BTC", 1.0, 0.001);

// Create a sell order.
binance.Sell("ETH-BTC", 1.0, 0.0015)
```

### APIs available

```cs
DepthCache(string symbol)
```

Returns the depth cache of the symbol.

<details>
 <summary>View Example</summary>
 
```cs
var depthCache = binance.DepthCache("ETH-BTC");

Console.WriteLine($"Asks: {depthCache.Asks.Keys.Count}, Bids: {depthCache.Bids.Keys.Count}");

// Outputs - "Asks: System.Collections.Generic.Dictionary`2[System.Double,System.Double], Bids: System.Collections.Generic.Dictionary`2[System.Double,System.Double]"
```
</details>


```cs
DepthVolume(string symbol)
```

Returns the depth volume of the symbol.

<details>
 <summary>View Example</summary>
 
```cs
var volume = binance.DepthVolume("ETH-BTC");

Console.WriteLine($"Bids: {volume.Bids}, Asks: {volume.Asks}, BidQuantity: {volume.BidQuantity}, AskQuantity: {volume.AskQuantity}");

// Outputs - "Bids: 234113, Asks: 534561, BidQuantity: 2342341.32, AskQuantity: 8942894.234"
```
</details>


```cs
SortBids(string symbol, double max, bool baseValue)
```

Sorts all bids then collects them up until the max number of bids has been collected.

<details>
 <summary>View Example</summary>
 
```cs
var sortedBids = binance.SortBids("ETH-BTC");

Console.WriteLine($"Bids: {string.Join(",", sortedBids.Keys)}");
// Outputs - "Bids: [50.234,50.235,50.23453,50.23454]"
```
</details>


```cs
SortAsks(string symbol, double max, bool baseValue)
```

Sorts all asks then collects them up until the max number of asks has been collected.

<details>
 <summary>View Example</summary>
 
```cs
var sortedAsks = binance.SortBids("ETH-BTC");

Console.WriteLine($"Asks: {string.Join(",", sortedAsks.Keys)}");
// Outputs - "Asks: [50.234,50.235,50.23453,50.23454]"
```
</details>


```cs
Buy(string symbol, double quantity, double price, Dictionary<string, string> flags)
```

Submits a buy order.

<details>
 <summary>View Example</summary>
 
```cs
binance.Buy("ETH-BTC", 1.0, 0.001);
```
</details>


```cs
Sell(string symbol, double quantity, double price, Dictionary<string, string> flags)
```

Submits a sell order.

<details>
 <summary>View Example</summary>
 
```cs
binance.Sell("ETH-BTC", 1.0, 0.001);
```
</details>


```cs
Cancel(string symbol, string orderId, Action<JToken> callback)
```

Cancels an order.

<details>
 <summary>View Example</summary>
 
```cs
string orderId = "jzp890p1zjaje3a"
binance.Cancel("ETH-BTC", orderId, response =>
{
  // Handle cancel response.
});
```
</details>


```cs
OrderStatus(string symbol, string orderId, Action<JToken> callback)
```

Returns the status of an open order.

<details>
 <summary>View Example</summary>
 
```cs
string orderId = "jzp890p1zjaje3a"
binance.OrderStatus("ETH-BTC", orderId, response =>
{
  // Handle cancel response.
});
```
</details>


```cs
OpenOrders(string symbol, Action<JToken> callback)
```

Returns a list of all open orders.

<details>
 <summary>View Example</summary>
 
```cs
binance.OpenOrders("ETH-BTC", response =>
{
  // Handle open orders response
});
```
</details>


```cs
AllOrders(string symbol, Action<JToken> callback)
```

Returns a list of all orders from the account.

<details>
 <summary>View Example</summary>
 
```cs
binance.AllOrders("ETH-BTC", response =>
{
  // Handle all orders response
});
```
</details>


```cs
Depth(string symbol, Action<DepthCache> callback)
```

Returns the depth of a symbol.

<details>
 <summary>View Example</summary>
 
```cs
binance.Depth("ETH-BTC", depth =>
{
  Console.WriteLine($"Depth - Asks: ${depth.Asks.Keys.Count}, Bids: ${depth.Bids.Keys.Count}");
});

// Outputs - "Depth - Asks: 15234, Bids: 24892"
```
</details>


```cs
Prices(Action<Dictionary<string, double>> callback)
```

Returns all price data.

<details>
 <summary>View Example</summary>
 
```cs
binance.Prices(prices =>
{
    // Handle price data.
});
```
</details>


```cs
BookTickers(Action<Dictionary<string, BookPrice>> callback)
```

Returns all book tickers.

<details>
 <summary>View Example</summary>
 
```cs
binance.BookTickers(tickers =>
{
    // Handle book tickers
});
```
</details>


```cs
PreviousDay(string symbol, Action<JToken> callback)
```

Returns the 24hr ticker price change statistics.

<details>
 <summary>View Example</summary>
 
```cs
binance.PreviousDay("ETH-BTC", response =>
{
    // Handle previous 24 hour response
});
```
</details>


```cs
Account(Action<JToken> callback)
```

Get the account info associated with the API key & secret.

<details>
 <summary>View Example</summary>
 
```cs
binance.Account(response =>
{
    // Handle account response
});
```
</details>


```cs
Balance(Action<Dictionary<string, Balance>> callback)
```

Get the balance of all symbols from the account.

<details>
 <summary>View Example</summary>
 
```cs
binance.Balance(balances =>
{
    // Handle balance information. Stored as k/v pairs.
});
```
</details>


```cs
Trades(string symbol, Action<JToken> callback)
```

Get all trades the account is involved in.

<details>
 <summary>View Example</summary>
 
```cs
binance.Trades("ETH-BTC", response =>
{
    // Handle trade response
});
```
</details>


### Streams available

```cs
DepthStream(string[] symbols, Action<JToken> callback)
```

<details>
 <summary>View Example</summary>
 
```cs
binance.DepthStream(new[] {"ETH-BTC", "LTC-BTC"}, response =>
{
    // Handle stream responses for specified symbols
});
```
</details>


Opens a stream that invokes the callback when data is received on any of the specified symbols.

```cs
DepthCacheStream(string[] symbols, Action<string, DepthCache> callback)
```

<details>
 <summary>View Example</summary>
 
```cs
binance.DepthCacheStream(new[] { "ETH-BTC", "LTC-BTC" }, (symbol, depth) =>
{
    // Handle symbol and depth data for specified symbols
});
```
</details>


Opens a depth cache stream that invokes the callback when data is received on any of the specified symbols.

```cs
TradesStream(string[] symbols, Action<JToken> callback)
```

<details>
 <summary>View Example</summary>
 
```cs
binance.TradesStream(new[] {"ETH-BTC", "LTC-BTC"}, response =>
{
    // Handle trade stream response
});
```
</details>


Opens a trades stream that invokes the callback when data is received on any of the specified symbols.

```cs
Chart(string[] symbols, long interval, Action<JToken, long, Dictionary<long, OpenHighLowClose>> callback)
```

Opens a charts stream that invokes the callback when data is received on any of the specified symbols.

<details>
 <summary>View Example</summary>
 
```cs
binance.Chart(new[] {"ETH-BTC", "LTC-BTC"}, 9999, (response, interval, ohlcDict) =>
{
    // Handle chart stream.
});
```
</details>


### Further info

Please message @pgibler with any questions about how to use the library.