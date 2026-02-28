namespace Catharsis.Linq;

///<summary>
///Provides extension methods for partitioning <see cref="IEnumerable{T}"/> sequences into ///<see
///cref="IGrouping{TKey,TElement}"/> or <see cref="ILookup{TKey,TElement}"/> based on numeric ranges, boundary values,
///quantile buckets, and custom range definitions.
///</summary>
public static class RangePartitioner
{
    #region Private methods
    private static TKey FindBucket<TKey>(TKey value, IReadOnlyList<TKey> boundaries, IComparer<TKey> comparer)
    {
        // Binary search for the largest boundary <= value
        int lo = 0;
        int hi = boundaries.Count - 1;
        int result = 0;

        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2;

            if (comparer.Compare(boundaries[mid], value) <= 0)
            {
                result = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return boundaries[result];
    }

    private static int FloorToRange(int value, int rangeSize)
    { return value >= 0 ? (value / rangeSize) * rangeSize : ((value - rangeSize + 1) / rangeSize) * rangeSize; }
    private static double FloorToRange(double value, double rangeSize)
    { return Math.Floor(value / rangeSize) * rangeSize; }
    #endregion

    #region Public methods
    ///<summary>
    ///Partitions elements using explicit boundary values. Each element is assigned to the bucket whose lower boundary
    ///is the largest boundary that does not exceed the element's value. The boundaries list must be sorted in ascending
    ///order.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The boundary/key type, which must be comparable.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="valueSelector">A function that extracts the comparable value.</param>
    ///<param name="boundaries">A sorted list of boundary values. Must contain at least one element.</param>
    ///<param name="comparer">An optional comparer; defaults to the default comparer.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/> keyed by the matching boundary.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="valueSelector"/>, or <paramref name="boundaries"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="boundaries"/> is empty.</exception>
    public static ILookup<TKey, T> PartitionByBoundaries<T, TKey>(this IEnumerable<T> source, Func<T, TKey> valueSelector, IReadOnlyList<TKey> boundaries, IComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(valueSelector, nameof(valueSelector));
        ArgumentNullException.ThrowIfNull(boundaries, nameof(boundaries));

        if (boundaries.Count == 0)
        {
            throw new ArgumentException("Boundaries must contain at least one element.", nameof(boundaries));
        }

        IComparer<TKey> cmp = comparer ?? Comparer<TKey>.Default;
        return source.ToLookup(item => FindBucket(valueSelector(item), boundaries, cmp));
    }

    ///<summary>
    ///Partitions elements by boundaries, returning groupings.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The boundary/key type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="valueSelector">A function that extracts the comparable value.</param>
    ///<param name="boundaries">A sorted list of boundary values.</param>
    ///<param name="comparer">An optional comparer.</param>
    ///<returns>A sequence of groupings keyed by the matching boundary.</returns>
    ///<exception cref="ArgumentNullException">Any required argument is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="boundaries"/> is empty.</exception>
    public static IEnumerable<IGrouping<TKey, T>> PartitionByBoundariesAsGroupings<T, TKey>(this IEnumerable<T> source, Func<T, TKey> valueSelector, IReadOnlyList<TKey> boundaries, IComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(valueSelector, nameof(valueSelector));
        ArgumentNullException.ThrowIfNull(boundaries, nameof(boundaries));

        if (boundaries.Count == 0)
        {
            throw new ArgumentException("Boundaries must contain at least one element.", nameof(boundaries));
        }

        IComparer<TKey> cmp = comparer ?? Comparer<TKey>.Default;
        return source.GroupBy(item => FindBucket(valueSelector(item), boundaries, cmp));
    }

    ///<summary>
    ///Partitions elements into <paramref name="bucketCount"/> approximately equal-sized quantile buckets. Elements are
    ///first sorted by <paramref name="valueSelector"/>, then divided. The bucket key is the zero-based bucket index.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The sort key type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="valueSelector">A function that extracts the sort key.</param>
    ///<param name="bucketCount">The number of quantile buckets. Must be at least 1.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/> keyed by zero-based bucket index.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="valueSelector"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="bucketCount"/> is less than 1.</exception>
    public static ILookup<int, T> PartitionByQuantile<T, TKey>(this IEnumerable<T> source, Func<T, TKey> valueSelector, int bucketCount)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(valueSelector, nameof(valueSelector));
        ArgumentOutOfRangeException.ThrowIfLessThan(bucketCount, 1, nameof(bucketCount));

        List<T> sorted = source.OrderBy(valueSelector).ToList();

        if (sorted.Count == 0)
        {
            return LookupFactory.Empty<int, T>();
        }

        int bucketSize = Math.Max(1, (int)Math.Ceiling((double)sorted.Count / bucketCount));
        return sorted.ToLookup(item => Math.Min(sorted.IndexOf(item) / bucketSize, bucketCount - 1));
    }

    ///<summary>
    ///Partitions elements into quantile buckets, returning groupings. Uses a more efficient index-based approach.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<typeparam name="TKey">The sort key type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="valueSelector">A function that extracts the sort key.</param>
    ///<param name="bucketCount">The number of quantile buckets. Must be at least 1.</param>
    ///<returns>A sequence of groupings keyed by zero-based bucket index.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="valueSelector"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="bucketCount"/> is less than 1.</exception>
    public static IEnumerable<IGrouping<int, T>> PartitionByQuantileAsGroupings<T, TKey>(this IEnumerable<T> source, Func<T, TKey> valueSelector, int bucketCount)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(valueSelector, nameof(valueSelector));
        ArgumentOutOfRangeException.ThrowIfLessThan(bucketCount, 1, nameof(bucketCount));

        List<T> sorted = source.OrderBy(valueSelector).ToList();

        if (sorted.Count == 0)
        {
            yield break;
        }

        int bucketSize = Math.Max(1, (int)Math.Ceiling((double)sorted.Count / bucketCount));

        for (int bucket = 0; bucket < bucketCount; bucket++)
        {
            int start = bucket * bucketSize;

            if (start >= sorted.Count)
            {
                break;
            }

            int count = Math.Min(bucketSize, sorted.Count - start);
            IEnumerable<T> range = sorted.GetRange(start, count);
            yield return SequenceFactory.Grouping(bucket, range);
        }
    }

    ///<summary>
    ///Partitions elements into groups based on which integer range they fall into. Ranges are defined as ///<c>[lower,
    ///upper)</c> half-open intervals. The range key is the lower bound.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="valueSelector">A function that extracts the integer value used for partitioning.</param>
    ///<param name="rangeSize">The size of each range bucket. Must be at least 1.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/> keyed by the lower bound of each range.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="valueSelector"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="rangeSize"/> is less than 1.</exception>
    public static ILookup<int, T> PartitionByRange<T>(this IEnumerable<T> source, Func<T, int> valueSelector, int rangeSize)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(valueSelector, nameof(valueSelector));
        ArgumentOutOfRangeException.ThrowIfLessThan(rangeSize, 1, nameof(rangeSize));
        return source.ToLookup(item => FloorToRange(valueSelector(item), rangeSize));
    }

    ///<summary>
    ///Partitions elements into groups based on which double-precision range they fall into. The range key is the lower
    ///bound of the bucket.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="valueSelector">A function that extracts the double value used for partitioning.</param>
    ///<param name="rangeSize">The size of each range bucket. Must be greater than zero.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/> keyed by the lower bound of each range.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="valueSelector"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="rangeSize"/> is less than or equal to zero.</exception>
    public static ILookup<double, T> PartitionByRange<T>(this IEnumerable<T> source, Func<T, double> valueSelector, double rangeSize)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(valueSelector, nameof(valueSelector));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(rangeSize, 0.0, nameof(rangeSize));
        return source.ToLookup(item => FloorToRange(valueSelector(item), rangeSize));
    }

    ///<summary>
    ///Partitions elements by integer range, returning <see cref="IGrouping{TKey,TElement}"/> instances.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="valueSelector">A function that extracts the integer value.</param>
    ///<param name="rangeSize">The size of each range bucket. Must be at least 1.</param>
    ///<returns>A sequence of groupings keyed by lower bound.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="valueSelector"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="rangeSize"/> is less than 1.</exception>
    public static IEnumerable<IGrouping<int, T>> PartitionByRangeAsGroupings<T>(this IEnumerable<T> source, Func<T, int> valueSelector, int rangeSize)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(valueSelector, nameof(valueSelector));
        ArgumentOutOfRangeException.ThrowIfLessThan(rangeSize, 1, nameof(rangeSize));
        return source.GroupBy(item => FloorToRange(valueSelector(item), rangeSize));
    }

    ///<summary>
    ///Partitions elements into two groups keyed by <c>true</c> and <c>false</c> based on <paramref name="predicate"/>.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="predicate">A function that tests each element.</param>
    ///<returns>An <see cref="ILookup{TKey,TElement}"/> with <c>true</c> and <c>false</c> keys.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
    public static ILookup<bool, T> PartitionToLookup<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        return source.ToLookup(predicate);
    }
    #endregion
}
