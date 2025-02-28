using Lagrange.Core.Internal.Packets.Struct;

namespace Lagrange.Core.Internal.Services;

internal class ServiceOptions(RequestType requestType, EncryptType encryptType)
{
    public static ServiceOptions Default => new(RequestType.D2Auth, EncryptType.EncryptD2Key);
    
    public RequestType RequestType { get; } = requestType;
    
    public EncryptType EncryptType { get; } = encryptType;
}