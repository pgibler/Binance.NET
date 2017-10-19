# Binance.NET
A .NET wrapper for the Binance API.

### Installation

NuGet
`Install-Package Binance.NET`

.NET CLI
`dotnet add package Binance.NET`

Paket CLI
`paket add Binance.NET`

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
- `DepthVolume(string symbol)`
- `SortBids(string symbol, double max, bool baseValue)`
- `SortAsks(string symbol, double max, bool baseValue)`
- `Buy(string symbol, double quantity, double price, Dictionary<string, string> flags)`
- `Sell(string symbol, double quantity, double price, Dictionary<string, string> flags)`
- `Cancel(string symbol, string orderId, Action<JToken> callback)`
- `OrderStatus(string symbol, string orderId, Action<JToken> callback)`
- `OpenOrders(string symbol, Action<JToken> callback)`
- `AllOrders(string symbol, Action<JToken> callback)`
- `Depth(string symbol, Action<DepthCache> callback)`
- `Prices(Action<Dictionary<string, double>> callback)`
- `BookTickers(Action<Dictionary<string, BookPrice>> callback)`
- `PreviousDay(string symbol, Action<JToken> callback)`
- `Account(Action<JToken> callback)`
- `Balance(Action<Dictionary<string, Balance>> callback)`
- `Trades(string symbol, Action<JToken> callback)`

### Special thanks

Thanks to binance for putting on this competition and whoever from their company fielded the stream of questions I e-mailed over the course of the past month.

Please message @pgibler with any questions about how to use the library.