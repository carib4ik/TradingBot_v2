using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using TradingBot_v2.Services;
using TradingBot_v2.StateMachine;

namespace TradingBot_v2;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var config = LoadConfig();
        
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Регистрируем конфиг
                services.AddSingleton(config);

                // Регистрация зависимостей через фабрики или напрямую
                services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(config.TelegramBotToken));
                
                services.AddSingleton<UsersDataProvider>();
                services.AddSingleton<MarketDataService>(_ => new MarketDataService(config.BinanceKey, config.BinanceSecret));

                services.AddSingleton<ChatStateMachine>();
                services.AddSingleton<ChatStateController>();
                services.AddSingleton<TelegramBotController>();

                // Фоновая служба
                services.AddHostedService<RsiCheckerService>();
            })
            .Build();

        // Запуск бота
        var bot = host.Services.GetRequiredService<TelegramBotController>();
        bot.StartBot();

        await host.RunAsync();
    }
    
    private static AppConfig.AppConfig LoadConfig()
    {
        return new AppConfig.AppConfig
        {
            // TelegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN"),
            // BinanceKey = Environment.GetEnvironmentVariable("BYBIT_KEY"),
            // BinanceSecret = Environment.GetEnvironmentVariable("BYBIT_SECRET"),
            
            TelegramBotToken = "7036012109:AAE8SVT8DTBBm2vrEchOaKsIpCWw6fxihyI",
            BinanceKey = "vj2Akwxmi5a2frZs2A4LI56KYANOGCdFdoBn1oRro4yto0XdYvDlyuU8smjqAmCT",
            BinanceSecret = "2PbH5Vwtqksufw7nM17MtOltaqcW5AVpoQ3TvxEY4bTIzdn84JXY94KcIjvm2svm",
        };
    }
}