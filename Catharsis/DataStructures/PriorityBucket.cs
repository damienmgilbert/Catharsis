using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Catharsis.DataStructures;

///<summary>
///An enhanced priority queue that wraps <see cref="PriorityQueue{TElement, TPriority}"/> and adds <see
///cref="IReadOnlyCollection{T}"/> support, <see cref="Contains"/>, and <see cref="TryPeek"/> convenience.
///</summary>
///<typeparam name="TElement">The type of elements in the queue.</typeparam>
///<typeparam name="TPriority">The type used to determine element priority.</typeparam>
public class PriorityBucket<TElement, TPriority> : IEnumerable<TElement>, IReadOnlyCollection<TElement>
{
    #region Fields
    readonly HashSet<TElement> _elements;
    readonly PriorityQueue<TElement, TPriority> _queue;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new empty <see cref="PriorityBucket{TElement, TPriority}"/>.
    ///</summary>
    public PriorityBucket() : this(Comparer<TPriority>.Default)
    {
    }

    ///<summary>
    ///Initializes a new empty <see cref="PriorityBucket{TElement, TPriority}"/> with the specified priority
    ///comparer.
    ///</summary>
    ///<param name="comparer">The comparer used to order priorities.</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="comparer"/> is <c>null</c>.</exception>
    public PriorityBucket(IComparer<TPriority> comparer)
    {
        if(comparer is null)
        {
            throw new ArgumentNullException(nameof(comparer), "Priority comparer must not be null.");
        }

        _queue = new PriorityQueue<TElement, TPriority>(comparer);
        _elements = [];
    }
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    #endregion

    #region Public methods
    ///<summary>
    ///Removes all elements from the queue.
    ///</summary>
    public void Clear()
    {
        _queue.Clear();
        _elements.Clear();
    }

    ///<summary>
    ///Determines whether the queue contains the specified element.
    ///</summary>
    ///<param name="element">The element to look for.</param>
    ///<returns><c>true</c> if the element is in the queue; otherwise <c>false</c>.</returns>
    public bool Contains(TElement element) { return _elements.Contains(element); }

    ///<summary>
    ///Removes and returns the element with the lowest priority value.
    ///</summary>
    ///<returns>The element with the lowest priority.</returns>
    ///<exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
    public TElement Dequeue()
    {
        TElement? element = _queue.Dequeue();
        _elements.Remove(element);
        return element;
    }

    ///<summary>
    ///Adds an element with the specified priority.
    ///</summary>
    ///<param name="element">The element to add.</param>
    ///<param name="priority">The priority of the element.</param>
    public void Enqueue(TElement element, TPriority priority)
    {
        _queue.Enqueue(element, priority);
        _elements.Add(element);
    }

    ///<inheritdoc/>
    public IEnumerator<TElement> GetEnumerator()
    {
        foreach (var (element, _) in _queue.UnorderedItems)
        {
            yield return element;
        }
    }

    ///<summary>
    ///Returns the element with the lowest priority value without removing it.
    ///</summary>
    ///<returns>The element with the lowest priority.</returns>
    ///<exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
    public TElement Peek() { return _queue.Peek(); }

    ///<summary>
    ///Attempts to remove and return the element with the lowest priority value.
    ///</summary>
    ///<param name="element">The dequeued element, if successful.</param>
    ///<param name="priority">The priority of the dequeued element, if successful.</param>
    ///<returns><c>true</c> if an element was dequeued; <c>false</c> if the queue was empty.</returns>
    public bool TryDequeue([MaybeNullWhen(false)] out TElement element, [MaybeNullWhen(false)] out TPriority priority)
    {
        if(_queue.TryDequeue(out element, out priority))
        {
            _elements.Remove(element);
            return true;
        }

        return false;
    }

    ///<summary>
    ///Attempts to return the element with the lowest priority value without removing it.
    ///</summary>
    ///<param name="element">The peeked element, if successful.</param>
    ///<param name="priority">The priority of the peeked element, if successful.</param>
    ///<returns><c>true</c> if the queue is non-empty; otherwise <c>false</c>.</returns>
    public bool TryPeek([MaybeNullWhen(false)] out TElement element, [MaybeNullWhen(false)] out TPriority priority) { return _queue.TryPeek(out element, out priority); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of elements in the queue.
    ///</summary>
    public int Count => _queue.Count;

    ///<summary>
    ///Gets whether the queue contains no elements.
    ///</summary>
    public bool IsEmpty => _queue.Count == 0;
    #endregion
}
