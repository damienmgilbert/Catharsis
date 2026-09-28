using System.Buffers;
using CommunityToolkit.HighPerformance.Buffers;

namespace Catharsis.HighPerformance;

///<summary>
///Provides extension methods for <see cref="MemoryOwner{T}"/> and <see cref="SpanOwner{T}"/> from the
///CommunityToolkit.HighPerformance package.
///</summary>
public static class MemoryOwnerExtensions
{
    #region Public methods
    ///<summary>
    ///Clears all elements in the <see cref="MemoryOwner{T}"/> to their default values.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="owner">The memory owner to clear.</param>
    public static void Clear<T>(this MemoryOwner<T> owner) { owner.Span.Clear(); }
    ///<summary>
    ///Fills the entire <see cref="MemoryOwner{T}"/> with the specified value.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="owner">The memory owner to fill.</param>
    ///<param name="value">The value to fill with.</param>
    public static void Fill<T>(this MemoryOwner<T> owner, T value) { owner.Span.Fill(value); }

    ///<summary>
    ///Slices a <see cref="MemoryOwner{T}"/> and returns a new owner with the specified range. The caller is
    ///responsible for disposing both the original and the new owner.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="owner">The source memory owner.</param>
    ///<param name="start">The start index.</param>
    ///<param name="length">The number of elements.</param>
    ///<returns>A new <see cref="MemoryOwner{T}"/> containing the sliced data.</returns>
    public static MemoryOwner<T> SliceCopy<T>(this MemoryOwner<T> owner, int start, int length)
    {
        ReadOnlySpan<T> source = owner.Span.Slice(start, length);
        MemoryOwner<T> result = MemoryOwner<T>.Allocate(length);
        source.CopyTo(result.Span);
        return result;
    }

    ///<summary>
    ///Creates a <see cref="MemoryOwner{T}"/> from the specified span by copying the data.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="span">The source span to copy from.</param>
    ///<returns>A new <see cref="MemoryOwner{T}"/> containing a copy of the data.</returns>
    public static MemoryOwner<T> ToMemoryOwner<T>(this ReadOnlySpan<T> span)
    {
        MemoryOwner<T> owner = MemoryOwner<T>.Allocate(span.Length);
        span.CopyTo(owner.Span);
        return owner;
    }

    ///<summary>
    ///Creates a <see cref="MemoryOwner{T}"/> from the specified memory by copying the data.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="memory">The source memory to copy from.</param>
    ///<returns>A new <see cref="MemoryOwner{T}"/> containing a copy of the data.</returns>
    public static MemoryOwner<T> ToMemoryOwner<T>(this ReadOnlyMemory<T> memory)
    {
        MemoryOwner<T> owner = MemoryOwner<T>.Allocate(memory.Length);
        memory.Span.CopyTo(owner.Span);
        return owner;
    }

    ///<summary>
    ///Writes all elements from a <see cref="MemoryOwner{T}"/> into an <see cref="IBufferWriter{T}"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="owner">The memory owner to write from.</param>
    ///<param name="writer">The buffer writer to write to.</param>
    public static void WriteTo<T>(this MemoryOwner<T> owner, IBufferWriter<T> writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        ReadOnlySpan<T> span = owner.Span;
        Span<T> destination = writer.GetSpan(span.Length);
        span.CopyTo(destination);
        writer.Advance(span.Length);
    }
    #endregion
}
