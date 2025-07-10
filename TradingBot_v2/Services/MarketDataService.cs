using System.Globalization;
using Binance.Net.Clients;
using Binance.Net.Objects.Options;
using CryptoExchange.Net.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skender.Stock.Indicators;
using TradingBot_v2.Extensions;
using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Options;

namespace TradingBot_v2.Services;

public class MarketDataService
{
    private readonly BinanceRestClient _binanceClient;
    private const int CANDLES_LIMIT = 500;

    public MarketDataService(BinanceRestClient  binanceClient)
    {
        _binanceClient = binanceClient;
    }

    public async Task<string> GetDataFromBinance(string symbol, string interval)
    {
        Console.WriteLine("Get Market Data");
        
        Console.WriteLine($"symbol = {symbol}, interval = {interval}, limit = {CANDLES_LIMIT}");
        
        var chart = await _binanceClient.SpotApi.ExchangeData.GetKlinesAsync(
            symbol: symbol,
            interval: interval.ToKlineInterval(),
            limit: CANDLES_LIMIT
        );
        
        Console.WriteLine($"chart.Success = {chart.Success}");
        Console.WriteLine($"chart.Error = {chart.Error}");
        Console.WriteLine($"chart.Data.Count() = {chart.Data?.Count()}");
        
        // Логирование и защита от null
        if (!chart.Success || chart.Data == null || !chart.Data.Any())
        {
            Console.WriteLine("Ошибка при получении данных с binance: " + chart.Error);
            throw new Exception("binance API не вернул данные.");
        }
        
        var jsonChart = JsonConvert.SerializeObject(chart.Data.Select(c => new {
            Time = c.OpenTime.ToString("s"),
            Open = c.OpenPrice,
            High = c.HighPrice,
            Low = c.LowPrice,
            Close = c.ClosePrice
        }), Formatting.Indented);
        
        return jsonChart;
    }
    
    public async Task<double?> GetCurrentRsi(string symbol, string interval, int period = 14)
    {
        var json = await GetDataFromBinance(symbol, interval);
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
        var result = await _binanceClient.UsdFuturesApi.Account.GetPositionInformationAsync();

        if (!result.Success || result.Data == null)
        {
            Console.WriteLine("Ошибка при получении позиций с Binance: " + result.Error);
            return "Ошибка при получении позиций с Binance.";
        }

        // Фильтруем только открытые позиции (с ненулевым объёмом)
        var openPositions = result.Data
            .Where(p => p.Quantity != 0)
            .Select(p => new
            {
                p.Symbol,
                Size = p.Quantity,
                EntryPrice = p.EntryPrice,
                Pnl = p.UnrealizedPnl,
                Leverage = p.Leverage,
                Side = p.Quantity > 0 ? "Long" : "Short"
            });

        return JsonConvert.SerializeObject(openPositions, Formatting.Indented);
    }

}