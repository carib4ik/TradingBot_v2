using Telegram.Bot.Types;

namespace TradingBot_v2.StateMachine.States;

public class IdleState : ChatStateBase
{
    public IdleState(ChatStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override async Task OnEnter(long chatId)
    {
        Console.WriteLine("IdleState");
    }

    public override Task HandleMessage(Message message)
    {
        return Task.CompletedTask;
    }
}