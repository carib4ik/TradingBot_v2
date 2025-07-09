using System.Globalization;
using CryptoExchange.Net.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skender.Stock.Indicators;

namespace TradingBot_v2.Services;

public class MarketDataService
{
    private readonly Binance _binanceClient;
    private const int CANDLES_LIMIT = 800;

    public MarketDataService(string apiKey, string apiSecret)
    {
        _binanceClient = new Binance(options => { options.ApiCredentials = new ApiCredentials(apiKey, apiSecret); });
    }

    public async Task<string> GetDataFromBinance(string symbol, string interval)
    {
        var url = $"https://api.binance.com/api/v3/klines?symbol={symbol}&interval={interval}&limit={CANDLES_LIMIT}";

        Console.WriteLine("Get Market Data");

        using var client = new HttpClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        Console.WriteLine("Market Data received successfully");

        return await response.Content.ReadAsStringAsync();
    }
    
    public async Task<double?> GetCurrentRsi(string symbol, string interval, int period = 14)
    {
        var json = await GetDataFromByBit(symbol, interval);
        var candles = JArray.Parse(json);

        var quotes = candles.Select(c => new Quote
        {
            Date = DateTime.Parse(c["Time"]!.ToString(), null, DateTimeStyles.RoundtripKind),
            Open = Convert.ToDecimal(c["Open"]!),
            High = Convert.ToDecimal(c["High"]!),
            Low = Convert.ToDecimal(c["Low"]!),
            Close = Convert.ToDecimal(c["Close"]!),
            Volume = 0 // если объём не нужен, можно оставить 0
        }).ToList();

        var rsiResults = quotes.GetRsi(period);
        return rsiResults.LastOrDefault()?.Rsi;
    }

    public async Task<string> GetOpenPositionsAsync()
    {
        var result = await _binanceClient.V5Api.Trading.GetPositionsAsync(category: Category.Linear, settleAsset: "USDT");

        if (!result.Success || result.Data == null)
            return "Ошибка при получении позиций с Bybit.";

        var positions = result.Data.List
            .Where(p => p.PositionValue > 0) // Фильтруем только активные позиции
            .Select(p => new
            {
                p.Symbol,
                Size = p.PositionValue,
                EntryPrice = p.AveragePrice,
                Pnl = p.UnrealizedPnl,
                Leverage = p.Leverage,
                Side = p.Side.ToString()
            });

        return JsonConvert.SerializeObject(positions, Formatting.Indented);
    }
}