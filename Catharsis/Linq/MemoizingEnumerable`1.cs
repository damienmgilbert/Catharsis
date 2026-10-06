using System.Collections;

namespace Catharsis.Linq;

///<summary>
///Wraps a sequence so its source is enumerated at most once, no matter how many times or how many concurrently
///interleaved enumerators consume the wrapper: each produced item is cached, and any enumerator — including ones
///that start after caching has begun — replays cached items and only pulls further from the source when it reaches
///the end of what has been cached so far.
///</summary>
///<typeparam name="T">The element type.</typeparam>
///<example>
///<code>
///using MemoizingEnumerable&lt;int&gt; memoized = new(ExpensiveSequence());
///
///// The underlying sequence is only ever enumerated once, no matter how many times 'memoized' is enumerated.
///int first = memoized.First();
///int total = memoized.Sum();
///</code>
///</example>
public sealed class MemoizingEnumerable<T> : IEnumerable<T>, IDisposable
{
    #region Fields
    readonly Lock _gate = new();
    readonly List<T> _cache = [];
    IEnumerator<T>? _source;
    bool _completed;
    #endregion

    #region Explicit interface implementations
    ///<inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Public methods
    ///<summary>
    ///Wraps the specified source sequence.
    ///</summary>
    ///<param name="source">The sequence to memoize.</param>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public MemoizingEnumerable(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        _source = source.GetEnumerator();
    }

    ///<summary>
    ///Disposes the underlying source enumerator, if it has not already run to completion. Already-cached items
    ///remain available to enumerate.
    ///</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            _source?.Dispose();
            _source = null;
        }
    }

    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        int index = 0;

        while (true)
        {
            (bool hasItem, T item, bool done) = TryGetAt(index);

            if (done)
            {
                yield break;
            }

            if (hasItem)
            {
                yield return item;
                index++;
            }
        }
    }

    (bool HasItem, T Item, bool Done) TryGetAt(int index)
    {
        lock (_gate)
        {
            if (index < _cache.Count)
            {
                return (true, _cache[index], false);
            }

            if (_completed || _source is null)
            {
                return (false, default!, true);
            }

            if (_source.MoveNext())
            {
                T item = _source.Current;
                _cache.Add(item);
                return (true, item, false);
            }

            _completed = true;
            _source.Dispose();
            _source = null;
            return (false, default!, true);
        }
    }
    #endregion
}
