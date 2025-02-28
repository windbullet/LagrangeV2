using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Internal.Packets.Login;

internal class WtLogin(BotContext context) : StructBase(context)
{
    public ReadOnlyMemory<byte> BuildPacket(short command)
    {
        var writer = new BinaryPacket();  // TODO: Determine the allocation type of the BinaryPacket
        
        writer.Write((byte)2);
        writer.EnterLengthBarrier<short>();
        
        writer.Write((short)8001);
        writer.Write(command);
        writer.Write((short)0); // sequence
        writer.Write((uint)Keystore.Uin);
        writer.Write((byte)3);
        writer.Write((byte)135);
        writer.Write(0);
        writer.Write((byte)19);
        writer.Write((short)0); // insId
        writer.Write(AppInfo.AppClientVersion); // insId
        writer.Write(0); // retryTime
        BuildEncryptHead(ref writer);
        
        writer.Write((byte)3);
        
        writer.ExitLengthBarrier<short>(true, 1);

        return writer.ToArray();
    }

    private void BuildCode2dPacket(short command)
    {
        var tlv = ReadOnlySpan<byte>.Empty; // TODO: Implement ConstructTlv
        
        var reqBody = new BinaryPacket(); // TODO: Determine the allocation type of the BinaryPacket
        reqBody.Write((uint)DateTimeOffset.Now.ToUnixTimeSeconds());
        
        reqBody.Write((byte)2); // encryptMethod == EncryptMethod.EM_ST || encryptMethod == EncryptMethod.EM_ECDH_ST | Section of length 43 + tlv.Length + 1
        reqBody.EnterLengthBarrier<short>();
        reqBody.Write(command);
        reqBody.Skip(21);
        reqBody.Write((byte)3);
        reqBody.Write((short)0x00); // close
        reqBody.Write((short)0x32); // Version Code: 50
        reqBody.Write(0); // trans_emp sequence
        reqBody.Write(Keystore.Uin); // dummy uin
        reqBody.Write(tlv);
        reqBody.Write((byte)3);
        reqBody.ExitLengthBarrier<short>(true, 1);
        
        var reqSpan = reqBody.CreateReadOnlySpan();
        
        var writer = new BinaryPacket(); // TODO: Determine the allocation type of the BinaryPacket
        writer.Write((byte)0x00);
        writer.Write((ushort)reqSpan.Length);
        writer.Write(AppInfo.AppId);
        writer.Write((uint)0x72); // Role
        writer.Write(ReadOnlySpan<byte>.Empty, Prefix.Int16 | Prefix.LengthOnly); // uSt
        writer.Write(ReadOnlySpan<byte>.Empty, Prefix.Int8 | Prefix.LengthOnly); // rollback
        writer.Write(reqSpan);
    }

    private void BuildEncryptHead(ref BinaryPacket writer)
    {
        Span<byte> random = stackalloc byte[16];
        Random.Shared.NextBytes(random);
        
        writer.Write((byte)1);
        writer.Write((byte)1);
        writer.Write(random);
        writer.Write((short)0x102); // encrypt type
        writer.Write(Keystore.Secp192K1.PackPublic(true));
    }
}