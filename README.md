# Binance.NET
A .NET Standard 2.0+ wrapper for the Binance API.

### Installation

    git clone https://github.com/pgibler/Binance.NET
    cd Binance.NET/src
    dotnet build

### Usage

To run Binance.NET in your C# application, you need your API key & secret handy. Once you get those from your account on binance, you can initialize the `Binance` object as such:

    // Instantiate a binance API service interaction instance.
    var binance = new BinanceApi(apiKey, apiSecret);

    // Create a buy order.
    binance.Buy("ETH-BTC", 1.0, 0.001);

    // Create a sell order.
    binance.Sell("ETH-BTC", 1.0, 0.0015)

### APIs available

- `DepthCache(string symbol)`
Returns the depth cache of the symbol.
- `DepthVolume(string symbol)`
Returns the depth volume of the symbol.
- `SortBids(string symbol, double max, bool baseValue)`
Sorts all bids then collects them up until the max number of bids has been collected.
- `SortAsks(string symbol, double max, bool baseValue)`
Sorts all asks then collects them up until the max number of asks has been collected.
- `Buy(string symbol, double quantity, double price, Dictionary<string, string> flags)`
Submits a buy order.
- `Sell(string symbol, double quantity, double price, Dictionary<string, string> flags)`
Submits a sell order.
- `Cancel(string symbol, string orderId, Action<JToken> callback)`
Cancels an order.
- `OrderStatus(string symbol, string orderId, Action<JToken> callback)`
Returns the status of an open order.
- `OpenOrders(string symbol, Action<JToken> callback)`
Returns a list of all open orders.
- `AllOrders(string symbol, Action<JToken> callback)`
Returns a list of all orders from the account.
- `Depth(string symbol, Action<DepthCache> callback)`
Returns the depth of a symbol.
- `Prices(Action<Dictionary<string, double>> callback)`
Returns all price data.
- `BookTickers(Action<Dictionary<string, BookPrice>> callback)`
Returns all book tickers.
- `PreviousDay(string symbol, Action<JToken> callback)`
Returns the 24hr ticker price change statistics.
- `Account(Action<JToken> callback)`
Get the account info associated with the API key & secret.
- `Balance(Action<Dictionary<string, Balance>> callback)`
Get the balance of all symbols from the account.
- `Trades(string symbol, Action<JToken> callback)`
Get all trades the account is involved in.

### Streams available

- `DepthStream(string[] symbols, Action<JToken> callback)`
Opens a stream that invokes the callback when data is received on any of the specified symbols.
- `DepthCacheStream(string[] symbols, Action<string, DepthCache> callback)`
Opens a depth cache stream that invokes the callback when data is received on any of the specified symbols.
- `TradesStream(string[] symbols, Action<JToken> callback)`
Opens a trades stream that invokes the callback when data is received on any of the specified symbols.
- `Chart(string[] symbols, long interval, Action<JToken, long, Dictionary<long, OpenHighLowClose>> callback)`
Opens a charts stream that invokes the callback when data is received on any of the specified symbols.

### Further info

Please message @pgibler with any questions about how to use the library.