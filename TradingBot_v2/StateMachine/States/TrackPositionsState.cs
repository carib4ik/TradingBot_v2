using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using TradingBot_v2.Services;

namespace TradingBot_v2.StateMachine.States;

public class TrackPositionsState : ChatStateBase
{
    private readonly PositionsTrackerService _positions;
    private readonly ChatStateMachine _stateMachine;
    private readonly ITelegramBotClient _botClient;
    
    public TrackPositionsState(ChatStateMachine stateMachine, ITelegramBotClient botClient, PositionsTrackerService positions) : base(stateMachine)
    {
        _positions = positions;
        _stateMachine = stateMachine;
        _botClient = botClient;
    }

    public override async Task OnEnter(long chatId)
    {
        var json = await _positions.GetOpenPositionsAsync();

        var positions = JArray.Parse(json);
        if (!positions.Any())
        {
            await _botClient.SendMessage(chatId, "Нет открытых позиций ✅");
        }
        else
        {
            var msg = string.Join("\n\n", positions.Select(p =>
                $"📈 *{p["Symbol"]}*\n" +
                $"▶️ Сторона: {p["Side"]}\n" +
                $"📏 Размер: {p["Size"]}\n" +
                $"💵 Цена входа: {p["EntryPrice"]}\n" +
                $"📉 PnL: {p["Pnl"]} USDT\n" +
                $"📊 Плечо: x{p["Leverage"]}"
            ));

            await _botClient.SendMessage(chatId, msg, Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        await _stateMachine.TransitTo<IdleState>(chatId);
    }

    public override Task HandleMessage(Message message)
    {
        return Task.CompletedTask;
    }
}