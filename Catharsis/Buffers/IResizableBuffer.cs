using System.Buffers;

namespace Catharsis.Buffers;

///<summary>
///Represents a buffer that can be dynamically resized according to a specified growth strategy.
///</summary>
///<typeparam name="T">The type of elements in the buffer.</typeparam>
public interface IResizableBuffer<T> : IBufferWriter<T>, IDisposable
{
    #region Public methods
    ///<summary>
    ///Ensures the buffer has at least the specified capacity.
    ///</summary>
    ///<param name="capacity">The minimum required capacity.</param>
    void EnsureCapacity(int capacity);

    ///<summary>
    ///Resets the buffer to its initial state without releasing the underlying memory.
    ///</summary>
    void Reset();
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the current capacity of the buffer.
    ///</summary>
    int Capacity { get; }

    ///<summary>
    ///Gets the growth strategy used by this buffer.
    ///</summary>
    BufferGrowthStrategy GrowthStrategy { get; }

    ///<summary>
    ///Gets the number of elements written to the buffer.
    ///</summary>
    int WrittenCount { get; }

    ///<summary>
    ///Gets a <see cref="ReadOnlyMemory{T}"/> over the written portion of the buffer.
    ///</summary>
    ReadOnlyMemory<T> WrittenMemory { get; }

    ///<summary>
    ///Gets a <see cref="ReadOnlySpan{T}"/> over the written portion of the buffer.
    ///</summary>
    ReadOnlySpan<T> WrittenSpan { get; }
    #endregion
}
