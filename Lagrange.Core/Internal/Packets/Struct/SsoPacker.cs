using System.Buffers.Binary;
using Lagrange.Core.Common;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Internal.Packets.Struct;

internal class SsoPacker(BotContext context) : StructBase(context)
{
    public BinaryPacket BuildProtocol12(ref SsoPacket sso, SsoSecureInfo? secInfo) // TODO: sso_reserve_fields
    {
        var head = new BinaryPacket(stackalloc byte[0x200]);
        
        head.Write(sso.Sequence); // sequence
        head.Write(AppInfo.SubAppId); // subAppId
        head.Write(2052); // unknown
        head.Write([0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
        head.Write(Keystore.WLoginSigs.A2, Prefix.Int32 | Prefix.WithPrefix); // tgt
        head.Write(sso.Command, Prefix.Int32 | Prefix.WithPrefix); // command
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix); // message_cookies
        head.Write(Keystore.Guid, Prefix.Int32 | Prefix.WithPrefix); // guid
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(AppInfo.CurrentVersion, Prefix.Int16 | Prefix.WithPrefix);
        head.Write([0], Prefix.Int32 | Prefix.WithPrefix); // sso_reserve_fields
        
        var headSpan = head.CreateReadOnlySpan();
        var result = new BinaryPacket(headSpan.Length + sso.Data.Length + 2 * 4); // 2 * 4 for the length of the payload
        
        result.Write(headSpan, Prefix.Int32 | Prefix.WithPrefix);
        result.Write(sso.Data.Span, Prefix.Int32 | Prefix.WithPrefix); // payload
        
        return result;
    }
    
    public BinaryPacket BuildProtocol13(ref SsoPacket sso)
    {
        var head = new BinaryPacket(stackalloc byte[0x200]);
        
        head.Write(sso.Command, Prefix.Int32 | Prefix.WithPrefix); // command
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix); // message_cookies
        head.Write([0], Prefix.Int32 | Prefix.WithPrefix); // sso_reserve_fields
        
        var headSpan = head.CreateReadOnlySpan();
        var result = new BinaryPacket(headSpan.Length + sso.Data.Length + 2 * 4); // 2 * 4 for the length of the payload
        
        result.Write(headSpan, Prefix.Int32 | Prefix.WithPrefix);
        result.Write(sso.Data.Span, Prefix.Int32 | Prefix.WithPrefix); // payload
        
        return result;
    }

    public SsoPacket Parse(ref BinaryPacket sso)
    {
        throw new NotImplementedException();
    }
}