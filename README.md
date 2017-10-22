# Binance.NET
A .NET Standard 2.0+ wrapper for the Binance API.

### Build

Building Binance.NET is easy. Just clone the repository and run `dotnet build`.

```
git clone https://github.com/pgibler/Binance.NET
cd Binance.NET/src
dotnet build
```

### Usage

- Add the DLLs from the build to your project.
- Reference them from the .csproj file containing the classes that will utilize the `BinanaceApi` class.
- Retrieve your API key & secret from your account on binance.

Once you have done these steps, to use Binance.NET in your C# application, create an instance of `BinanceApi` and invoke it's functionality from your code:

```cs
// Instantiate a binance API service interaction instance.
var binance = new BinanceApi(apiKey, apiSecret);

// Create a buy order.
binance.Buy("ETH-BTC", 1.0, 0.001);

// Create a sell order.
binance.Sell("ETH-BTC", 1.0, 0.0015)
```

### Error handling

In some cases, the API request may fail because of any number of reasons. In the event that the Binance API responds with an error message, the success callback is skipped on the API call and an error callback is invoked. You can set up a custom error handler in 2 ways:

- As a parameter of the API call. This is usually the last parameter of the method.
- Using the `DefaultExceptionCallback` property of the `BinanceApi` class.

You can also mix both methods. If you specify a `DefaultExceptionCallback`, it will be used for all functions exception where you explicitly define an exception callback. This way you can handle exceptions in a general use case and then specifically handle ones that require custom behavior.

For testing purposes, you can choose to eschew the usage of these as you try out the API methods. It is recommended you use them to see what goes wrong when you are testing.

### APIs available

Included in Binance.NET are the following API calls. All of these functions are members of the `BinanceApi` class.

---

```cs
Buy(string symbol, double quantity, double price)
```

Submits a buy order.

<details>
 <summary>View Example</summary>
 
```cs
// Simple buy order.
binance.Buy("ETH-BTC", 1.0, 0.001);

// Buy order handling the response.
binance.Buy("ETH-BTC", 1.0, 0.001,
  response => Console.WriteLine(response.OrderId),
  exception => Console.WriteLine($"Error message: {exception.Message}"));
```
</details>

---

```cs
Sell(string symbol, double quantity, double price)
```

Submits a sell order.

<details>
 <summary>View Example</summary>
 
```cs
// Simple sell order.
binance.Sell("ETH-BTC", 1.0, 0.001);

// Sell order handling the response.
binance.Sell("ETH-BTC", 1.0, 0.001,
  response => Console.WriteLine(response.OrderId),
  exception => Console.WriteLine($"Error message: {exception.Message}"));
```
</details>

---

```cs
CancelOrder(string symbol, string orderId, Action<CancelOrderResponse> successCallback)
```

Cancels an order.

<details>
 <summary>View Example</summary>
 
```cs
// Cancel order handling success and error responses.
long orderId = GetOrderId();
binance.CancelOrder("ETH-BTC", orderId,
  response => Console.WriteLine($"Order #{response.OrderId} cancelled"),
  exception => Console.WriteLine("Order failed to cancel"));
```
</details>

---

```cs
OrderStatus(string symbol, string orderId, Action<OrderResponse> successCallback)
```

Returns the status of an open order.

<details>
 <summary>View Example</summary>
 
```cs
// Order status handling success and error responses.
long orderId = GetOrderId();
binance.OrderStatus("ETH-BTC", orderId,
  response => Console.WriteLine($"Order #{response.OrderId} has status {response.Status}"),
  exception => Console.WriteLine("Order failed to cancel"));
```
</details>

---

```cs
OpenOrders(string symbol, Action<OpenOrdersResponse> successCallback)
```

Returns a list of all open orders.

<details>
 <summary>View Example</summary>
 
```cs
binance.OpenOrders("ETH-BTC", orders => Console.WriteLine($"First order open: {orders.First().OrderId}"));
```
</details>

---

```cs
AllOrders(string symbol, Action<AllOrdersResponse> successCallback)
```

Returns a list of all orders from the account.

<details>
 <summary>View Example</summary>
 
```cs
binance.AllOrders("ETH-BTC", orders => Console.WriteLine($"First order ever: {orders.First().OrderId}"));
```
</details>

---

```cs
Depth(string symbol, Action<DepthCache> callback)
```

Returns the current depth of a symbol.

<details>
 <summary>View Example</summary>
 
```cs
binance.Depth("ETH-BTC", depth => Console.WriteLine($"Asks: {string.Join(",", depth.Asks.Keys)}, Bids: {string.Join(",", depth.Bids.Keys)}"));
```
</details>

---

```cs
Prices(Action<Dictionary<string, double>> callback)
```

Returns all price data.

<details>
 <summary>View Example</summary>
 
```cs
binance.Prices(prices => Console.WriteLine($"Assets on the market: {prices.Count}. First asset price: Symbol - {prices.First().Key}, Price - {prices.First().Value}"));
```
</details>

---

```cs
BookTickers(Action<Dictionary<string, BookPrice>> callback)
```

Returns all book tickers.

<details>
 <summary>View Example</summary>
 
```cs
binance.BookTickers(tickers =>
{
    Console.WriteLine($"Tickers count: {tickers.Count}, First symbol & ask: {tickers.First().Key} & {tickers.First().Value.AskPrice}");
});
```
</details>

---

```cs
PreviousDay(string symbol, Action<PreviousDayResponse> successCallback)
```

Returns the 24hr ticker price change statistics.

<details>
 <summary>View Example</summary>
 
```cs
binance.PreviousDay("ETH-BTC", previousDay => Console.WriteLine($"24 hour % change - ${previousDay.PriceChangePercent}"));
```
</details>

---

```cs
Account(Action<AccountResponse> successCallback)
```

Get the account info associated with the API key & secret.

<details>
 <summary>View Example</summary>
 
```cs
binance.Account(account => Console.WriteLine($"Account can trade: {account.CanTrade}"));
```
</details>

---

```cs
Balance(Action<Dictionary<string, Balance>> callback)
```

Get the balance of all symbols from the account.

<details>
 <summary>View Example</summary>
 
```cs
binance.Balance(balances => Console.WriteLine($"First asset: {balances.First().Key}"));
```
</details>

---

```cs
Trades(string symbol, Action<TradesResponse> successCallback)
```

Get all trades the account is involved in.

<details>
 <summary>View Example</summary>
 
```cs
binance.Trades("ETH-BTC", trades => Console.WriteLine($"First trade price: {trades.First().Price}"));
```
</details>

---

```cs
DepthCache(string symbol)
```

Returns the depth cache of the symbol.

<details>
 <summary>View Example</summary>
 
```cs
var cache = binance.DepthCache("ETH-BTC");
            
Console.WriteLine($"Asks: {string.Join(",", cache.Asks.Keys)}, Bids: {string.Join(",", cache.Bids.Keys)}")
```
</details>

---

```cs
DepthVolume(string symbol)
```

Returns the depth volume of the symbol.

<details>
 <summary>View Example</summary>
 
```cs
var volume = binance.DepthVolume("ETH-BTC");

Console.WriteLine($"Ask volume: {volume.Asks}, Bid volume: {volume.Bids}")
```
</details>

---

```cs
SortBids(string symbol, double max, bool baseValue)
```

Sorts all bids then collects them up until the max number of bids has been collected.

<details>
 <summary>View Example</summary>
 
```cs
var sortedBids = binance.SortBids("ETH-BTC");

Console.WriteLine($"Bids: {string.Join(",", sortedBids.Keys)}");
```
</details>

---

```cs
SortAsks(string symbol, double max, bool baseValue)
```

Sorts all asks then collects them up until the max number of asks has been collected.

<details>
 <summary>View Example</summary>
 
```cs
var sortedAsks = binance.SortAsks("ETH-BTC");

Console.WriteLine($"Asks: {string.Join(",", sortedAsks.Keys)}");
```
</details>

---

### Streams available

Binance.NET comes with a set of streams that you can run to hook into Binance's WebSocket services.

All of these functions return a `CancellationTokenSource` instance so you can cancel their operation as needed.

---

```cs
DepthStream(string[] symbols, Action<DepthStreamResponse> successCallback)
```

Opens a stream that invokes the callback when data is received on any of the specified symbols.

<details>
 <summary>View Example</summary>
 
```cs
binance.DepthStream(new[] {"ETH-BTC", "LTC-BTC"}, response =>
{
    // Handle stream responses for specified symbols
});
```
</details>

---

```cs
DepthCacheStream(string[] symbols, Action<string, DepthCache> callback)
```

Opens a depth cache stream that invokes the callback when data is received on any of the specified symbols.

<details>
 <summary>View Example</summary>
 
```cs
binance.DepthCacheStream(new[] { "ETH-BTC", "LTC-BTC" }, (symbol, depth) =>
{
    // Handle symbol and depth data for specified symbols
});
```
</details>

---

```cs
TradesStream(string[] symbols, Action<TradesStreamResponse> successCallback)
```

Opens a trades stream that invokes the callback when data is received on any of the specified symbols.

<details>
 <summary>View Example</summary>
 
```cs
binance.TradesStream(new[] {"ETH-BTC", "LTC-BTC"}, response =>
{
    // Handle trade stream response
});
```
</details>

---

```cs
ChartStream(string[] symbols, long interval, Action<JToken, long, Dictionary<long, OpenHighLowClose>> successCallback)
```

Opens a charts stream that invokes the callback when data is received on any of the specified symbols.

<details>
 <summary>View Example</summary>
 
```cs
binance.ChartStream(new[] {"ETH-BTC", "LTC-BTC"}, 9999, (response, interval, ohlcDict) =>
{
    // Handle chart stream.
});
```
</details>

---

### Further info

Please message @pgibler with any questions about how to use the library.