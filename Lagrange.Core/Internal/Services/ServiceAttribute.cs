namespace Lagrange.Core.Internal.Services;

[AttributeUsage(AttributeTargets.Class)]
internal class ServiceAttribute(string command, ServiceOptions? options = null) : Attribute
{
    public string Command { get; } = command;
    
    public ServiceOptions Options { get; } = options ?? ServiceOptions.Default;
}