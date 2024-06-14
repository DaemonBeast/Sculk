using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using Sculk.Protocol.Datatypes;

namespace Sculk.Protocol;

public static class Serializer
{
    // TODO: re-evaluate usage of aggressive inlining

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void UnsafeWriteBool(Span<byte> buffer, bool value)
        // Grabs a pointer to the boolean,
        // converts it into a pointer to a byte
        // and dereferences the pointer to get a byte.
        => buffer[0] = *(byte*) &value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteBool(Span<byte> buffer, bool value)
    {
        if (buffer.IsEmpty)
        {
            return false;
        }

        UnsafeWriteBool(buffer, value);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeWriteByte(Span<byte> buffer, sbyte value)
        => buffer[0] = (byte) value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteByte(Span<byte> buffer, sbyte value)
    {
        if (buffer.IsEmpty)
        {
            return false;
        }

        UnsafeWriteByte(buffer, value);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeWriteByte(Span<byte> buffer, byte value)
        => buffer[0] = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteByte(Span<byte> buffer, byte value)
    {
        if (buffer.IsEmpty)
        {
            return false;
        }

        UnsafeWriteByte(buffer, value);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeWriteInt16(Span<byte> buffer, short value)
        => BinaryPrimitives.WriteInt16BigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteInt16(Span<byte> buffer, short value)
        => BinaryPrimitives.TryWriteInt16BigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeWriteUInt16(Span<byte> buffer, ushort value)
        => BinaryPrimitives.WriteUInt16BigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteUInt16(Span<byte> buffer, ushort value)
        => BinaryPrimitives.TryWriteUInt16BigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeWriteInt32(Span<byte> buffer, int value)
        => BinaryPrimitives.WriteInt32BigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteInt32(Span<byte> buffer, int value)
        => BinaryPrimitives.TryWriteInt32BigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeWriteInt64(Span<byte> buffer, long value)
        => BinaryPrimitives.WriteInt64BigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteInt64(Span<byte> buffer, long value)
        => BinaryPrimitives.TryWriteInt64BigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeWriteSingle(Span<byte> buffer, float value)
        => BinaryPrimitives.TryWriteSingleBigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteSingle(Span<byte> buffer, float value)
        => BinaryPrimitives.TryWriteSingleBigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeWriteDouble(Span<byte> buffer, double value)
        => BinaryPrimitives.WriteDoubleBigEndian(buffer, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteDouble(Span<byte> buffer, double value)
        => BinaryPrimitives.TryWriteDoubleBigEndian(buffer, value);

    public static void UnsafeWriteString(Span<byte> buffer, string value, out int length)
    {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        UnsafeWriteVarInt(buffer, byteCount, out var byteCountLength);
        Encoding.UTF8.GetBytes(value, buffer[byteCountLength..]);

        length = byteCount + byteCountLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteString(Span<byte> buffer, string value, out int length)
        => TryWriteString(buffer, value, 32767, out length);

    // TODO: serialize text component

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeWriteJsonTextComponent(Span<byte> buffer, string value, out int length)
        => UnsafeWriteString(buffer, value, out length);

    /// <remarks>
    /// Does not verify if the given string is valid JSON.
    /// Only verifies if the string is an appropriate size.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteJsonTextComponent(Span<byte> buffer, string value, out int length)
        => TryWriteString(buffer, value, 262144, out length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeWriteIdentifier(Span<byte> buffer, Identifier identifier, out int length)
        => UnsafeWriteString(buffer, identifier, out length);

    public static bool TryWriteIdentifier(Span<byte> buffer, Identifier identifier, out int length)
    {
        if (!Identifier.IsValid(identifier))
        {
            length = 0;
            return false;
        }

        return TryWriteString(buffer, identifier, out length);
    }

    public static void UnsafeWriteVarInt(Span<byte> buffer, int value, out int length)
    {
        const int continueBit = 0x80;

        length = 0;

        // Basically, it loops until there's 7 or fewer bits left to encode
        while (value >= continueBit)
        {
            buffer[length++] = (byte) (value | continueBit);
            value >>>= 7;
        }

        buffer[length++] = (byte) value;
    }

    public static bool TryWriteVarInt(Span<byte> buffer, int value, out int length)
    {
        // A VarInt will never be more than 5 bytes,
        // so can safely be written to a buffer of 5 or more bytes
        if (buffer.Length >= 5)
        {
            UnsafeWriteVarInt(buffer, value, out length);
            return true;
        }

        const int continueBit = 0x80;

        length = 0;

        while (value >= continueBit)
        {
            if (buffer.Length <= length)
            {
                return false;
            }

            buffer[length++] = (byte) (value | continueBit);
            value >>>= 7;
        }

        if (buffer.Length <= length)
        {
            return false;
        }

        buffer[length++] = (byte) value;
        return true;
    }
    
    // TODO: serialize entity metadata
    // TODO: serialize slot
    // TODO: serialize NBT
    // TODO: serialize position
    // TODO: serialize angle
    // TODO: serialize UUID
    // TODO: serialize BitSet
    // TODO: serialize Fixed BitSet
    // TODO: serialize "Optional X"
    // TODO: serialize "Array of X"
    // TODO: serialize "X Enum"
    // TODO: serialize byte array

    private static bool TryWriteString(Span<byte> buffer, string value, int maxUtf16CodeUnits, out int length)
    {
        var maxUtf8CodeUnits = maxUtf16CodeUnits * 3;

        var chars = value.ToCharArray();
        // 1 UTF-8 code unit = 1 byte
        // 1 UTF-16 code unit = 2 bytes
        var utf8CodeUnitCount = Encoding.UTF8.GetByteCount(chars);
        var utf16CodeUnitCount = Encoding.Unicode.GetByteCount(chars) >> 1;     // integer division by 2

        if (utf8CodeUnitCount > maxUtf8CodeUnits ||
            utf16CodeUnitCount > maxUtf16CodeUnits ||
            !TryWriteVarInt(buffer, utf8CodeUnitCount, out var byteCountLength))
        {
            length = 0;
            return false;
        }

        length = utf8CodeUnitCount + byteCountLength;
        if (buffer.Length < length)
        {
            return false;
        }

        Encoding.UTF8.GetBytes(chars, buffer[byteCountLength..]);

        return true;
    }
}