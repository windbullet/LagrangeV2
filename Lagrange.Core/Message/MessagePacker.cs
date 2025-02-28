using System.Reflection;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Message;

internal class MessagePacker
{
    private readonly List<IMessageEntity> _factory;
    
    public MessagePacker()
    {
        _factory = [];
        
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.GetInterfaces().Contains(typeof(IMessageEntity)))
            {
                _factory.Add((IMessageEntity?)Activator.CreateInstance(type) ?? throw new InvalidOperationException());
            }
        }
    }
}