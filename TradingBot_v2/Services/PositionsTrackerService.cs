using Binance.Net.Clients;
using Newtonsoft.Json;

namespace TradingBot_v2.Services;

public class PositionsTrackerService
{
    private readonly BinanceRestClient _binanceClient;

    public PositionsTrackerService(BinanceRestClient  binanceClient)
    {
        _binanceClient = binanceClient;
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