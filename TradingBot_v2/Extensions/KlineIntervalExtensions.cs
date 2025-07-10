using Binance.Net.Enums;

namespace TradingBot_v2.Extensions
{
    public static class KlineIntervalExtensions
    {
        public static KlineInterval ToKlineInterval(this string interval)
        {
            return interval switch
            {
                "1m" => KlineInterval.OneMinute,
                "3m" => KlineInterval.ThreeMinutes,
                "5m" => KlineInterval.FiveMinutes,
                "15m" => KlineInterval.FifteenMinutes,
                "30m" => KlineInterval.ThirtyMinutes,
                "1h" => KlineInterval.OneHour,
                "2h" => KlineInterval.TwoHour,
                "4h" => KlineInterval.FourHour,
                "6h" => KlineInterval.SixHour,
                "8h" => KlineInterval.EightHour,
                "12h" => KlineInterval.TwelveHour,
                "1d" => KlineInterval.OneDay,
                "3d" => KlineInterval.ThreeDay,
                "1w" => KlineInterval.OneWeek,
                "1M" => KlineInterval.OneMonth,
                _ => throw new ArgumentException($"Unknown Binance interval: {interval}")
            };
        }
    }
}