using Lagrange.Core.Internal.Events;

namespace Lagrange.Core.Internal.Services;

internal interface IService
{
    public Task<ProtocolEvent?> Parse(ReadOnlyMemory<byte> input, BotContext context);
    
    public Task<ReadOnlyMemory<byte>> Build(ProtocolEvent input, BotContext context);
}