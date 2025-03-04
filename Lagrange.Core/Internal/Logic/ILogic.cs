using Lagrange.Core.Events;
using Lagrange.Core.Internal.Events;

namespace Lagrange.Core.Internal.Logic;

internal interface ILogic
{
    public ValueTask Incoming(ProtocolEvent e) => ValueTask.CompletedTask;
    
    public ValueTask Outgoing(ProtocolEvent e) => ValueTask.CompletedTask;
    
    public ValueTask Outgoing(EventBase e) => ValueTask.CompletedTask;
}