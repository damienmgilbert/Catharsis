using Catharsis.DataStructures;

namespace Catharsis.Linq;

///<summary>
///Provides streaming top-N/bottom-N selection over <see cref="IEnumerable{T}"/> without a full sort, using a
///<see cref="MinMaxHeap{T}"/> bounded to <c>count</c> elements. Runs in O(n log count) time and O(count) space.
///</summary>
public static class SequenceTopN
{
    #region Public methods
    ///<summary>
    ///Selects the <paramref name="count"/> smallest elements, ordered smallest-first.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="count">The maximum number of elements to return. Must be at least 1.</param>
    ///<param name="comparer">The comparer to order by, or <c>null</c> to use <see cref="Comparer{T}.Default"/>.</param>
    ///<returns>Up to <paramref name="count"/> of the smallest elements, ascending.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public static IReadOnlyList<T> BottomN<T>(this IEnumerable<T> source, int count, IComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1, nameof(count));

        MinMaxHeap<T> heap = new(comparer);

        foreach (T item in source)
        {
            heap.Add(item);

            if (heap.Count > count)
            {
                heap.ExtractMax();
            }
        }

        List<T> result = new(heap.Count);

        while (heap.Count > 0)
        {
            result.Add(heap.ExtractMin());
        }

        return result;
    }

    ///<summary>
    ///Selects the <paramref name="count"/> largest elements, ordered largest-first.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="source">The source sequence.</param>
    ///<param name="count">The maximum number of elements to return. Must be at least 1.</param>
    ///<param name="comparer">The comparer to order by, or <c>null</c> to use <see cref="Comparer{T}.Default"/>.</param>
    ///<returns>Up to <paramref name="count"/> of the largest elements, descending.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public static IReadOnlyList<T> TopN<T>(this IEnumerable<T> source, int count, IComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1, nameof(count));

        MinMaxHeap<T> heap = new(comparer);

        foreach (T item in source)
        {
            heap.Add(item);

            if (heap.Count > count)
            {
                heap.ExtractMin();
            }
        }

        List<T> result = new(heap.Count);

        while (heap.Count > 0)
        {
            result.Add(heap.ExtractMax());
        }

        return result;
    }
    #endregion
}
