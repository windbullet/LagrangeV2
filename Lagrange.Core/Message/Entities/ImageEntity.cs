using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public class ImageEntity : RichMediaEntityBase
{
    internal override Lazy<Stream>? Stream { get; }
    
    public override Task Preprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    public override Task Postprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    internal override Elem[] Build()
    {
        throw new NotImplementedException();
    }

    internal override IMessageEntity? Parse(Elem[] elements, Elem target)
    {
        throw new NotImplementedException();
    }
}