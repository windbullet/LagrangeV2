namespace Lagrange.Core.Events;

[AttributeUsage(AttributeTargets.Event)]
public class EventPushAttribute<T>() : EventPushAttribute(typeof(T)) where T : EventBase;

[AttributeUsage(AttributeTargets.Event)]
public class EventPushAttribute(Type eventType) : Attribute
{
    public Type EventType { get; } = eventType;
}