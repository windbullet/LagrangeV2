namespace Lagrange.Core.Internal.Logic;

internal class WtExchangeLogic : ILogic, IDisposable
{
    private readonly Timer _heartBeatTimer;

    private readonly Timer _ssoHeartBeatTimer;

    public WtExchangeLogic(BotContext context)
    {
        _heartBeatTimer = new Timer(OnHeartBeat);
        _ssoHeartBeatTimer = new Timer(OnSsoHeartBeat);
    }

    private void OnHeartBeat(object? state) => Task.Run(() =>
    {
        
    });
    
    private void OnSsoHeartBeat(object? state) => Task.Run(() =>
    {
        
    });

    public void Dispose()
    {
        _heartBeatTimer.Dispose();
        _ssoHeartBeatTimer.Dispose();
    }
}