using System.Security.Cryptography;

namespace Lagrange.Core.Utility.Cryptography;
 
internal static class AesGcmProvider
{
    public static byte[] Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        using var aes = new AesGcm(key, 16);
        
        Span<byte> iv = stackalloc byte[12];
        RandomNumberGenerator.Fill(iv);
        
        Span<byte> tag = stackalloc byte[16];
        var cipher = new byte[data.Length];
        aes.Encrypt(iv, data, cipher, tag);
        
        var result = new byte[12 + data.Length + 16];
        iv.CopyTo(result);
        cipher.CopyTo(result.AsSpan()[12..^16]);
        tag.CopyTo(result.AsSpan()[^16..]);
        
        return result;
    }
    
    public static byte[] Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        using var aes = new AesGcm(key, 16);
        
        var iv = data[..12];
        var cipher = data[12..^16];
        var tag = data[^16..];
        var result = new byte[data.Length - 28];
        aes.Decrypt(iv, cipher, tag, result);
        
        return result;
    }
}