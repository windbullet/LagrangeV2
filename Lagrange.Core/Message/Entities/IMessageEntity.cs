using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public interface IMessageEntity
{
    internal Task Preprocess(BotContext context, BotMessage message);
    
    internal Task Postprocess(BotContext context, BotMessage message);

    internal Elem[] Build();

    internal IMessageEntity? Parse(Elem[] elements, Elem target);
}