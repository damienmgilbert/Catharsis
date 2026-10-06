using System.Buffers;

namespace Catharsis.Buffers;

///<summary>
///A linked-list segment node used to construct a <see cref="ReadOnlySequence{T}"/>.
///</summary>
///<typeparam name="T">The element type.</typeparam>
sealed class SequenceSegment<T> : ReadOnlySequenceSegment<T>
{
    #region Constructors
    public SequenceSegment(ReadOnlyMemory<T> memory, SequenceSegment<T>? previous)
    {
        Memory = memory;
        if(previous is not null)
        {
            RunningIndex = previous.RunningIndex + previous.Memory.Length;
            previous.Next = this;
        }
    }
    #endregion
}
