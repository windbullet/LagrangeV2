namespace Lagrange.Core.Events;

public abstract class EventBase : System.EventArgs
{
    public DateTime EventTime { get; }
    
    internal EventBase() => EventTime = DateTime.Now;

    public abstract string ToEventMessage();
    
    public override string ToString() => $"[{EventTime:yyyy-MM-dd HH:mm:ss}] {ToEventMessage()}";
}