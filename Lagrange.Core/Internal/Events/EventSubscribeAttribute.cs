namespace Lagrange.Core.Internal.Events;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class EventSubscribeAttribute<T>() : EventSubscribeAttribute(typeof(T));

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class EventSubscribeAttribute(Type type) : Attribute
{
    public Type EventType { get; } = type;
}