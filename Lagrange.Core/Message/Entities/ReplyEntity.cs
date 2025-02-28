using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public class ReplyEntity(BotMessage source) : IMessageEntity
{
    public Task Preprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    public Task Postprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    Elem[] IMessageEntity.Build()
    {
        throw new NotImplementedException();
    }

    IMessageEntity? IMessageEntity.Parse(Elem[] elements, Elem target)
    {
        throw new NotImplementedException();
    }
}