using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;

namespace Catharsis.HighPerformance;

/// <summary>
/// Provides bit-level access over a <see cref="Span{T}"/> of bytes,
/// enabling compact boolean arrays and bitwise operations without extra allocation.
/// </summary>
public ref struct BitSpan
{
    private readonly Span<byte> _bytes;
    private readonly int _bitLength;

    /// <summary>
    /// Initializes a FileName <see cref="BitSpan"/> over the specified byte span.
    /// </summary>
    /// <param name="storage">The underlying byte storage.</param>
    /// <param name="bitCount">The number of bits to expose (must be ≤ storage.Length * 8).</param>
    public BitSpan(Span<byte> storage, int bitCount)
    {
        Guard.IsGreaterThan(bitCount, 0);
        Guard.IsLessThanOrEqualTo(bitCount, storage.Length * 8);

        _bytes = storage;
        _bitLength = bitCount;
    }

    /// <summary>Gets the number of bits in this span.</summary>
    public readonly int Length => _bitLength;

    /// <summary>Gets the number of bytes backing this span.</summary>
    public readonly int ByteLength => _bytes.Length;

    /// <summary>
    /// Gets or sets the bit at the specified index.
    /// </summary>
    /// <param name="index">The zero-based bit index.</param>
    public bool this[int index]
    {
        readonly get
        {
            Guard.IsInRange(index, 0, _bitLength);
            return (_bytes[index >> 3] & (1 << (index & 7))) != 0;
        }
        set
        {
            Guard.IsInRange(index, 0, _bitLength);
            if (value)
                _bytes[index >> 3] |= (byte)(1 << (index & 7));
            else
                _bytes[index >> 3] &= (byte)~(1 << (index & 7));
        }
    }

    /// <summary>
    /// Sets all bits to the specified value.
    /// </summary>
    /// <param name="value">The value to set all bits to.</param>
    public void Fill(bool value)
    {
        _bytes.Fill(value ? (byte)0xFF : (byte)0x00);
    }

    /// <summary>
    /// Clears all bits to zero.
    /// </summary>
    public void Clear() => _bytes.Clear();

    /// <summary>
    /// Counts the number of set (1) bits using hardware intrinsics when available.
    /// </summary>
    /// <returns>The population count.</returns>
    public readonly int PopCount()
    {
        int count = 0;
        foreach (byte b in _bytes)
            count += BitOperations.PopCount(b);
        return count;
    }

    /// <summary>
    /// Performs a bitwise AND with another <see cref="BitSpan"/> and stores the result in this span.
    /// </summary>
    /// <param name="other">The other bit span.</param>
    public void And(BitSpan other)
    {
        Guard.IsEqualTo(other.ByteLength, ByteLength);
        for (int i = 0; i < _bytes.Length; i++)
            _bytes[i] &= other._bytes[i];
    }

    /// <summary>
    /// Performs a bitwise OR with another <see cref="BitSpan"/> and stores the result in this span.
    /// </summary>
    /// <param name="other">The other bit span.</param>
    public void Or(BitSpan other)
    {
        Guard.IsEqualTo(other.ByteLength, ByteLength);
        for (int i = 0; i < _bytes.Length; i++)
            _bytes[i] |= other._bytes[i];
    }

    /// <summary>
    /// Inverts all bits in this span.
    /// </summary>
    public void Not()
    {
        for (int i = 0; i < _bytes.Length; i++)
            _bytes[i] = (byte)~_bytes[i];
    }

    /// <summary>
    /// Gets the minimum number of bytes required to store the specified number of bits.
    /// </summary>
    /// <param name="bitCount">The number of bits.</param>
    /// <returns>The byte count needed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetByteCount(int bitCount) => (bitCount + 7) >> 3;
}
