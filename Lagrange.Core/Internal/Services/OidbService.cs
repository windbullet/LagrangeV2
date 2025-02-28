using Lagrange.Core.Internal.Events;

namespace Lagrange.Core.Internal.Services;

/// <summary>
/// Derive from this class to create a service that handles Oidb packets, enable automatic marshalling and unmarshalling of packets and error handling.
/// </summary>
internal abstract class OidbService<T, TRequest, TResponse> : BaseService<T> where T : ProtocolEvent
{
    protected abstract Task<ProtocolEvent?> ProcessRequest(TRequest request, BotContext context);
    
    protected abstract Task<TResponse> ProcessResponse(TResponse response, BotContext context);
    
    protected override Task<ProtocolEvent?> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        throw new NotImplementedException();
    }

    protected override Task<ReadOnlyMemory<byte>> Build(T input, BotContext context)
    {
        throw new NotImplementedException();
    }
}