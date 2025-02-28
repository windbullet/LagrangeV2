using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Message;

public class MessageBuilder
{
    public List<IMessageEntity> Entities { get; } = [];
    
    internal async Task Build(BotContext context, BotMessage message)
    {
        foreach (var entity in Entities)
        {
            await entity.Preprocess(context, message);
            // TODO: Elem
        }
    }
    
    public static MessageBuilder operator +(MessageBuilder builder, IMessageEntity entity)
    {
        builder.Entities.Add(entity);
        return builder;
    }
    
    public static MessageBuilder operator +(MessageBuilder self, MessageBuilder other)
    {
        self.Entities.AddRange(other.Entities);
        return self;
    }
}