using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Services;

namespace Lagrange.Core.Internal.Context;

internal class ServiceContext
{
    private int _sequence = Random.Shared.Next(5000000, 9900000);
    
    private readonly FrozenDictionary<string, IService> _services;
    private readonly FrozenDictionary<Type, List<(ServiceAttribute Attribute, IService Instance)>> _servicesEventType;
    
    private readonly ConcurrentDictionary<string, int> _sessionSequence = new();
    
    public ServiceContext(BotContext context)
    {
        var services = new Dictionary<string, IService>();
        var servicesEventType = new Dictionary<Type, List<(ServiceAttribute, IService)>>();
        
        foreach (var type in typeof(IService).Assembly.GetTypes())
        {
            if (type.GetInterfaces().Contains(typeof(IService)))
            {
                servicesEventType[type] = [];
            }
        }

        foreach (var type in typeof(IService).Assembly.GetTypes()) 
        {
            var serviceAttribute = type.GetCustomAttribute<ServiceAttribute>();

            if (serviceAttribute != null)
            {
                var service = (IService?)Activator.CreateInstance(type) ?? throw new InvalidOperationException("Failed to create service instance");
                services[serviceAttribute.Command] = service;
                
                foreach (var attribute in type.GetCustomAttributes<EventSubscribeAttribute>())
                {
                    servicesEventType[attribute.EventType].Add((serviceAttribute, service));
                }
            }
        }
        
        _services = services.ToFrozenDictionary();
        _servicesEventType = servicesEventType.ToFrozenDictionary();
    }
    
    public int SessionSequence(string sessionId) => _sessionSequence.GetOrAdd(sessionId, GetNewSequence());
    
    private int GetNewSequence()
    {
        Interlocked.CompareExchange(ref _sequence, 5000000, 9900000);
        return Interlocked.Increment(ref _sequence);
    }
}