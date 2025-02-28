namespace Lagrange.Core.Internal.Services;

[AttributeUsage(AttributeTargets.Class)]
internal class OidbServiceAttribute(int oidbCommand, short serviceType, int reserve = 0) 
    : ServiceAttribute($"OidbSvcTrpcTcp0x{oidbCommand:x}_{serviceType}")
{
    public int OidbCommand { get; } = oidbCommand;
    
    public short ServiceType { get; } = serviceType;
    
    public int Reserve { get; } = reserve;
}