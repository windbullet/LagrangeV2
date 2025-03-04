using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Lagrange.Core.Utility.Binary;

[DebuggerDisplay("Offset = {_offset} | Capacity = {_capacity} | Pool = {_bytesToReturnToPool != null}")]
internal ref struct BinaryPacket
{
    private int _offset;
    private int _capacity;

    private int _barrier = -1;
    
    private byte[]? _bytesToReturnToPool;
    private Span<byte> _span;
    private ref byte _buffer;
    
    /// <summary>
    /// Create a Packet for read/write
    /// <para>If packet is created for write, stackalloc is recommended for performance</para>
    /// </summary>
    public BinaryPacket(Span<byte> buffer)
    {
        Debug.Assert(buffer.Length > 0, "Buffer length must be greater than 0");
        
        _offset = 0;
        _capacity = buffer.Length;
        
        _span = buffer;
        _buffer = ref buffer.GetPinnableReference();
    }
    
    public BinaryPacket(ReadOnlyMemory<byte> buffer)
    {
        Debug.Assert(buffer.Length > 0, "Buffer length must be greater than 0");

        _offset = 0;
        _capacity = buffer.Length;

        ref var reference = ref Unsafe.AsRef(in buffer.Span.GetPinnableReference());
        _span = MemoryMarshal.CreateSpan(ref reference, buffer.Length); // unsafe, but we are sure that the reference is valid
        _buffer = ref reference;
    }
    
    public BinaryPacket(ReadOnlySpan<byte> buffer)
    {
        Debug.Assert(buffer.Length > 0, "Buffer length must be greater than 0");

        _offset = 0;
        _capacity = buffer.Length;

        ref var reference = ref Unsafe.AsRef(in buffer.GetPinnableReference());
        _span = buffer.ToArray();
        _buffer = ref reference;
    }
    
    /// <summary>
    /// Create a Packet for write by using <see cref="ArrayPool{T}.Shared.Rent"/>
    /// </summary>
    public BinaryPacket(int capacity)
    {
        Debug.Assert(capacity > 0, "Buffer length must be greater than 0");

        _offset = 0;
        _capacity = capacity;

        _span = ArrayPool<byte>.Shared.Rent(capacity).AsSpan();
        _buffer = ref _span.GetPinnableReference();
    }
    
    public void Write(byte value) => WriteToPacket(in value);

    public void Write(short value)
    {
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        WriteToPacket(in value);
    }
    
    public void Write(int value)
    {
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        WriteToPacket(in value);
    }
    
    public void Write(long value)
    {
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        WriteToPacket(in value);
    }
    
    public void Write(scoped ReadOnlySpan<byte> value)
    {
        if (_offset + value.Length > _capacity) GrowSize(value.Length);
        value.CopyTo(_span[_offset..]);
        
        Increment(value.Length);
    }
    
    public void Write(ushort value)
    {
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        WriteToPacket(in value);
    }
    
    public void Write(uint value)
    {
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        WriteToPacket(in value);
    }
    
    public void Write(ulong value)
    {
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        WriteToPacket(in value);
    }
    
    private void WriteLength(int length, Prefix flag, int addition = 0)
    {
        bool lengthCounted = (flag & Prefix.WithPrefix) != 0; // != 0 is faster than > 0
        int prefixLength = (byte)flag & 0b0111;
        if (lengthCounted) length += prefixLength + addition;
        
        switch (prefixLength)
        {
            case 1:
                Write((byte)length);
                break;
            case 2:
                Write((ushort)length);
                break;
            case 4:
                Write((uint)length);
                break;
        }
    }
    
    public void Write(scoped ReadOnlySpan<byte> buffer, Prefix flag)
    {
        WriteLength(buffer.Length, flag);
        Write(buffer);
    }
    
    public void Write(scoped ReadOnlySpan<char> value, Prefix flag)
    {
        byte[]? rented = null;

        int byteCount = Encoding.UTF8.GetByteCount(value);
        var buffer = byteCount <= 4096 ? stackalloc byte[byteCount] : rented = ArrayPool<byte>.Shared.Rent(byteCount);
        Encoding.UTF8.GetBytes(value, buffer);
        Write(buffer, flag);
        
        if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
    }
    
    public void Write(scoped ReadOnlySpan<char> value)
    {
        byte[]? rented = null;

        int byteCount = Encoding.UTF8.GetByteCount(value);
        var buffer = byteCount <= 4096 ? stackalloc byte[byteCount] : rented = ArrayPool<byte>.Shared.Rent(byteCount);
        Encoding.UTF8.GetBytes(value, buffer);
        Write(buffer);
        
        if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
    }
    
    public void Write(string value, Prefix flag)
    {
        byte[]? rented = null;

        int byteCount = Encoding.UTF8.GetByteCount(value);
        var buffer = byteCount <= 4096 ? stackalloc byte[byteCount] : rented = ArrayPool<byte>.Shared.Rent(byteCount);
        Encoding.UTF8.GetBytes(value, buffer);
        Write(buffer, flag);
        
        if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
    }
    
    private int ReadLength(Prefix flag)
    {
        bool lengthCounted = (flag & Prefix.WithPrefix) != 0; // != 0 is faster than > 0
        int prefixLength = (byte)flag & 0b0111;
        
        int length = prefixLength switch
        {
            1 => ReadByte(),
            2 => ReadInt16(),
            4 => ReadInt32(),
            _ => throw new ArgumentOutOfRangeException(nameof(flag), "Invalid prefix length")
        };
        if (lengthCounted) length -= prefixLength;
        
        return length;
    }

    public ReadOnlySpan<byte> ReadBytes(Prefix flag)
    {
        int length = ReadLength(flag);
        var buffer = _span.Slice(_offset, length);
        
        Increment(length);
        
        return buffer;
    }

    public ReadOnlySpan<byte> ReadBytes()
    {
        var buffer = _span[_offset..];
        Increment(buffer.Length);
        
        return buffer;
    }
    
    public long ReadInt64()
    {
        long value = Unsafe.ReadUnaligned<long>(ref _buffer);
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        
        Increment<long>();
        
        return value;
    }
    
    public int ReadInt32()
    {
        int value = Unsafe.ReadUnaligned<int>(ref _buffer);
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        
        Increment<int>();
        
        return value;
    }
    
    public short ReadInt16()
    {
        short value = Unsafe.ReadUnaligned<short>(ref _buffer);
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        
        Increment<short>();
        
        return value;
    }
    
    public byte ReadByte()
    {
        byte value = _buffer;
        
        Increment<byte>();
        
        return value;
    }
    
    public void ReadBytes(scoped Span<byte> buffer)
    {
        _span[_offset..].CopyTo(buffer);
        Increment(buffer.Length);
    }
    
    public void ReadString(scoped Span<char> buffer)
    {
        int byteCount = Encoding.UTF8.GetChars(_span[_offset..], buffer);
        Encoding.UTF8.GetChars(_span[_offset..], buffer);
        Increment(byteCount);
    }

    public string ReadString(Prefix flag)
    {
        int length = ReadLength(flag);
        var buffer = _span.Slice(_offset, length);
        Increment(length);
        return Encoding.UTF8.GetString(buffer);
    }
    
    public ushort ReadUInt16()
    {
        ushort value = Unsafe.ReadUnaligned<ushort>(ref _buffer);
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        
        Increment<ushort>();
        
        return value;
    }
    
    public uint ReadUInt32()
    {
        uint value = Unsafe.ReadUnaligned<uint>(ref _buffer);
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        
        Increment<uint>();
        
        return value;
    }
    
    public ulong ReadUInt64()
    {
        ulong value = Unsafe.ReadUnaligned<ulong>(ref _buffer);
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        
        Increment<ulong>();
        
        return value;
    }
    
    public void Skip(int count) => Increment(count);
    
    public byte PeekByte() => _buffer;
    
    public short PeekInt16() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<short>(ref _buffer)) : Unsafe.ReadUnaligned<short>(ref _buffer);
    
    public int PeekInt32() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<int>(ref _buffer)) : Unsafe.ReadUnaligned<int>(ref _buffer);
    
    public long PeekInt64() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<long>(ref _buffer)) : Unsafe.ReadUnaligned<long>(ref _buffer);
    
    public ushort PeekUInt16() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<ushort>(ref _buffer)) : Unsafe.ReadUnaligned<ushort>(ref _buffer);
    
    public uint PeekUInt32() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<uint>(ref _buffer)) : Unsafe.ReadUnaligned<uint>(ref _buffer);
    
    public ulong PeekUInt64() => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<ulong>(ref _buffer)) : Unsafe.ReadUnaligned<ulong>(ref _buffer);
    
    public void EnterLengthBarrier<T>() where T : unmanaged
    {
        _barrier = _offset;
        Increment<T>(); // reserve space for length
    }
    
    public unsafe void ExitLengthBarrier<T>(bool includePrefix, int addition = 0) where T : unmanaged
    {
        Debug.Assert(_barrier != -1, "Barrier must be set before exiting");
        
        int written = _offset - _barrier + addition;
        if (!includePrefix) written -= sizeof(T);
        
        if (BitConverter.IsLittleEndian) written = BinaryPrimitives.ReverseEndianness(written);
        Unsafe.WriteUnaligned(ref _span[_barrier], Unsafe.As<int, T>(ref written));
    }
    
    public void ExitCustomBarrier<T>(T barrier) where T : unmanaged, INumber<T>
    {
        Debug.Assert(_barrier != -1, "Barrier must be set before exiting");

        if (BitConverter.IsLittleEndian)
        {
            ulong casted = Unsafe.As<T, ulong>(ref barrier);
            ulong reversed = BinaryPrimitives.ReverseEndianness(casted);
            ulong shifted = reversed >> (sizeof(ulong) - Unsafe.SizeOf<T>()) * 8;
            barrier = Unsafe.As<ulong, T>(ref shifted);
        }
        Unsafe.WriteUnaligned(ref _span[_barrier], barrier);
        
        _barrier = -1;
    }

    /// <summary>
    /// Increase the offset of current packet by the specified value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteToPacket<T>(in T value) where T : unmanaged
    {
        if (_offset + sizeof(T) > _capacity) GrowSize(sizeof(T));
        
        Unsafe.WriteUnaligned(ref _buffer, value);
        Increment<T>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void Increment<T>() where T : unmanaged
    {
        _offset += sizeof(T);
        _buffer = ref Unsafe.Add(ref _buffer, sizeof(T));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Increment(int count)
    {
        _offset += count;
        _buffer = ref Unsafe.Add(ref _buffer, count);
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)] // NoInlining is used to prevent the method from being inlined as it is not used frequently
    private void GrowSize(int additional)
    {
        while (_offset + additional > _capacity) _capacity *= 2;
        _bytesToReturnToPool = ArrayPool<byte>.Shared.Rent(_capacity);

        _span[.._offset].CopyTo(_bytesToReturnToPool.AsSpan());

        _span = _bytesToReturnToPool.AsSpan();
        _buffer = ref _span.GetPinnableReference();
    }
    
    /// <summary>
    /// Write the current packet to a byte array, would create GC,
    /// After calling this method, the packet should be disposed, ensure what you are doing
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToArray()
    {
        var array = GC.AllocateUninitializedArray<byte>(_offset);
        _span[.._offset].CopyTo(array.AsSpan());
        Dispose();
        
        return array;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> CreateReadOnlySpan() => _span[.._offset];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> CreateSpan(int length)
    {
        if (_offset + length > _capacity) GrowSize(length);
        return _span.Slice(_offset, length);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_bytesToReturnToPool is not null) ArrayPool<byte>.Shared.Return(_bytesToReturnToPool.ToArray());
    }
}