namespace Catharsis.Linq;

///<summary>
///A fluent builder that accumulates partitioning rules for an <see cref="IEnumerable{T}"/> source. Each rule assigns
///elements to a named partition based on a predicate. Elements that match no rule are placed in a configurable default
///partition. The result is an <see cref="ILookup{TKey,TElement}"/> or sequence of ///<see
///cref="IGrouping{TKey,TElement}"/> keyed by partition name.
///</summary>
///<typeparam name="T">The element type.</typeparam>
public sealed class PartitionBuilder<T>
{
    #region Fields
    private string _defaultPartition = "__unmatched__";
    private bool _firstMatchOnly = true;
    private readonly List<(string Name, Func<T, bool> Predicate)> _rules = [];
    #endregion

    #region Private methods
    private IEnumerable<(string Partition, T Element)> Classify(IEnumerable<T> source)
    {
        foreach (T item in source)
        {
            bool matched = false;

            foreach ((string name, Func<T, bool> predicate) in _rules)
            {
                if (predicate(item))
                {
                    yield return (name, item);
                    matched = true;

                    if (_firstMatchOnly)
                    {
                        break;
                    }
                }
            }

            if (!matched)
            {
                yield return (_defaultPartition, item);
            }
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a partition rule.
    ///</summary>
    ///<param name="name">The partition name.</param>
    ///<param name="predicate">A function that tests whether an element belongs to this partition.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public PartitionBuilder<T> AddPartition(string name, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        _rules.Add((name, predicate));
        return this;
    }

    ///<summary>
    ///Adds a partition rule only when <paramref name="condition"/> is <c>true</c>.
    ///</summary>
    ///<param name="condition">Whether to add the rule.</param>
    ///<param name="name">The partition name.</param>
    ///<param name="predicate">A function that tests whether an element belongs to this partition.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public PartitionBuilder<T> AddPartitionIf(bool condition, string name, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        if (condition)
        {
            _rules.Add((name, predicate));
        }

        return this;
    }

    ///<summary>
    ///Applies the accumulated rules to <paramref name="source"/> and returns the partitioned result as an ///<see
    ///cref="ILookup{TKey,TElement}"/> keyed by partition name.
    ///</summary>
    ///<param name="source">The source sequence to partition.</param>
    ///<returns>A lookup mapping partition names to their elements.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public ILookup<string, T> Apply(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return Classify(source).ToLookup(static p => p.Partition, static p => p.Element);
    }

    ///<summary>
    ///Applies the accumulated rules to <paramref name="source"/> and returns the partitioned result as a sequence of
    ///<see cref="IGrouping{TKey,TElement}"/>.
    ///</summary>
    ///<param name="source">The source sequence to partition.</param>
    ///<returns>A sequence of groupings keyed by partition name.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public IEnumerable<IGrouping<string, T>> ApplyAsGroupings(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        return Classify(source).GroupBy(static p => p.Partition).Select(static g => SequenceFactory.Grouping(g.Key, g.Select(static p => p.Element)));
    }

    ///<summary>
    ///Removes all accumulated rules.
    ///</summary>
    ///<returns>The current builder for fluent chaining.</returns>
    public PartitionBuilder<T> Clear()
    {
        _rules.Clear();
        return this;
    }

    ///<summary>
    ///Sets the name used for elements that match no partition rule. Defaults to <c>"__unmatched__"</c>.
    ///</summary>
    ///<param name="name">The default partition name.</param>
    ///<returns>The current builder for fluent chaining.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public PartitionBuilder<T> WithDefaultPartition(string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        _defaultPartition = name;
        return this;
    }

    ///<summary>
    ///When <c>true</c> (the default), each element is placed in the first matching partition only. When
    public PartitionBuilder<T> WithFirstMatchOnly(bool firstMatchOnly)
    {
        _firstMatchOnly = firstMatchOnly;
        return this;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Returns the number of rules currently in the builder.
    ///</summary>
    public int Count => _rules.Count;
    #endregion
}
