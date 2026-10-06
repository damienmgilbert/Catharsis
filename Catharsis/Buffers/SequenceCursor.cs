using System.Buffers;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Buffers;

///<summary>
///Provides a cursor-based reader over a <see cref="ReadOnlySequence{T}"/>, enabling segment-aware sequential access.
///</summary>
///<typeparam name="T">The type of elements in the sequence.</typeparam>
///<remarks>
///Initializes a new <see cref="SequenceCursor{T}"/> over the specified sequence.
///</remarks>
///<param name="sequence">The sequence to read from.</param>
public ref struct SequenceCursor<T>(in ReadOnlySequence<T> sequence) where T : IEquatable<T>
{
    #region Struct fields
    private ReadOnlySequence<T> _sequence = sequence;
    private SequencePosition _position = sequence.Start;
    private ReadOnlySpan<T> _currentSpan = sequence.FirstSpan;
    private int _currentIndex = 0;
    #endregion

    #region Private methods
    private bool MoveToNextSegment()
    {
        SequencePosition nextPos = _sequence.GetPosition(_currentIndex, _position);
        if(_sequence.TryGet(ref nextPos, out ReadOnlyMemory<T> memory))
        {
            _position = nextPos;
            _currentSpan = memory.Span;
            _currentIndex = 0;
            return true;
        }

        return false;
    }

    private bool TryReadNextSegment(out T value)
    {
        if(MoveToNextSegment() && (_currentSpan.Length > 0))
        {
            value = _currentSpan[_currentIndex++];
            Consumed++;
            return true;
        }

        value = default!;
        return false;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Advances the cursor by the specified number of elements.
    ///</summary>
    ///<param name="count">The number of elements to advance.</param>
    public void Advance(long count)
    {
        Guard.IsGreaterThanOrEqualTo(count, 0);

        while(count > 0)
        {
            int available = _currentSpan.Length - _currentIndex;
            if(count <= available)
            {
                _currentIndex += (int)count;
                Consumed += count;
                return;
            }

            count -= available;
            Consumed += available;

            if(!MoveToNextSegment())
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(count), "Cannot advance past the end of the sequence.");
            }
        }
    }

    ///<summary>
    ///Searches for the first occurrence of the specified value from the current position.
    ///</summary>
    ///<param name="value">The value to search for.</param>
    ///<returns>The offset from the current position, or -1 if not found.</returns>
    public readonly long IndexOf(T value)
    {
        ReadOnlySequence<T> remaining = _sequence.Slice(Position);
        SequencePosition? found = remaining.PositionOf(value);
        if(found is null)
        {
            return -1;
        }

        return remaining.Slice(remaining.Start, found.Value).Length;
    }

    ///<summary>
    ///Rewinds the cursor to the start of the sequence.
    ///</summary>
    public void Reset()
    {
        _position = _sequence.Start;
        _currentSpan = _sequence.FirstSpan;
        _currentIndex = 0;
        Consumed = 0;
    }

    ///<summary>
    ///Attempts to peek at the next element without advancing the cursor.
    ///</summary>
    ///<param name="value">The next element, if available.</param>
    ///<returns><c>true</c> if an element is available; <c>false</c> if the end was reached.</returns>
    public readonly bool TryPeek(out T value)
    {
        if(_currentIndex < _currentSpan.Length)
        {
            value = _currentSpan[_currentIndex];
            return true;
        }

        value = default!;
        return false;
    }

    ///<summary>
    ///Attempts to read the next element.
    ///</summary>
    ///<param name="value">The element read, if successful.</param>
    ///<returns><c>true</c> if an element was read; <c>false</c> if the end was reached.</returns>
    public bool TryRead(out T value)
    {
        if(_currentIndex < _currentSpan.Length)
        {
            value = _currentSpan[_currentIndex++];
            Consumed++;
            return true;
        }

        return TryReadNextSegment(out value);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the total number of consumed elements from the start.
    ///</summary>
    public long Consumed { get; private set; }

    ///<summary>
    ///Gets whether there are remaining elements to read.
    ///</summary>
    public readonly bool HasRemaining => Consumed < _sequence.Length;

    ///<summary>
    ///Gets the current position within the sequence.
    ///</summary>
    public readonly SequencePosition Position => _sequence.GetPosition(_currentIndex, _position);

    ///<summary>
    ///Gets the total number of remaining elements.
    ///</summary>
    public readonly long Remaining => _sequence.Length - Consumed;
    #endregion
}
