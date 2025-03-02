using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Internal.Services;

namespace Lagrange.Core.Internal.Context;

internal class ServiceContext
{
    private const string Tag = nameof(ServiceContext);
    
    private int _sequence = Random.Shared.Next(5000000, 9900000);
    
    private readonly FrozenDictionary<string, IService> _services;
    private readonly FrozenDictionary<Type, List<(ServiceAttribute Attribute, IService Instance)>> _servicesEventType;
    
    private readonly ConcurrentDictionary<string, int> _sessionSequence = new();
    private readonly BotContext _context;
    
    public ServiceContext(BotContext context)
    {
        _context = context;
        var services = new Dictionary<string, IService>();
        var servicesEventType = new Dictionary<Type, List<(ServiceAttribute, IService)>>();

        foreach (var type in typeof(IService).Assembly.GetTypes()) 
        {
            if (type.GetInterfaces().Contains(typeof(IService)) && !type.IsAbstract) servicesEventType[type] = [];

            if (type.GetCustomAttribute<ServiceAttribute>() is { } attr)
            {
                var service = (IService?)Activator.CreateInstance(type) ?? throw new InvalidOperationException("Failed to create service instance");
                services[attr.Command] = service;
                
                foreach (var attribute in type.GetCustomAttributes<EventSubscribeAttribute>())
                {
                    servicesEventType[attribute.EventType].Add((attr, service));
                }
            }
        }
        
        _services = services.ToFrozenDictionary();
        _servicesEventType = servicesEventType.ToFrozenDictionary();
    }

    public ValueTask<ProtocolEvent?> Resolve(in SsoPacket ssoPacket)
    {
        if (!_services.TryGetValue(ssoPacket.Command, out var service))
        {
            _context.LogWarning(Tag, $"Service not found for command: {ssoPacket.Command}");
            return new ValueTask<ProtocolEvent?>(default(ProtocolEvent));
        }
        
        return service.Parse(ssoPacket.Data, _context);
    }

    public async ValueTask<(SsoPacket, ServiceOptions)[]> Resolve(ProtocolEvent @event)
    {
        if (!_servicesEventType.TryGetValue(@event.GetType(), out var handlers)) return [];

        var results = new (SsoPacket, ServiceOptions)[handlers.Count];
        for (var i = 0; i < handlers.Count; i++)
        {
            var (attr, service) = handlers[i];
            results[i] = (new SsoPacket(attr.Command, await service.Build(@event, _context), GetNewSequence()), attr.Options);
        }
        
        return results;
    }
    
    public int SessionSequence(string sessionId) => _sessionSequence.GetOrAdd(sessionId, GetNewSequence());
    
    private int GetNewSequence()
    {
        Interlocked.CompareExchange(ref _sequence, 5000000, 9900000);
        return Interlocked.Increment(ref _sequence);
    }
}