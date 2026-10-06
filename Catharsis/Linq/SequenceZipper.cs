namespace Catharsis.Linq;

///<summary>
///Provides a padded-zip extension for <see cref="IEnumerable{T}"/>, continuing until both sequences are exhausted
///rather than stopping at the shorter one as <see cref="Enumerable.Zip{TFirst, TSecond}"/> does. Named
///<c>ZipPadded</c> rather than the more common "ZipLongest" to avoid colliding with
///<see cref="AsyncSequenceMerger.ZipLongest{TFirst, TSecond}"/>, which serves the same purpose for
///<see cref="IAsyncEnumerable{T}"/> sequences.
///</summary>
public static class SequenceZipper
{
    #region Private methods
    private static IEnumerable<(TFirst? First, TSecond? Second)> ZipPaddedIterator<TFirst, TSecond>(IEnumerable<TFirst> first, IEnumerable<TSecond> second)
    {
        using IEnumerator<TFirst> firstEnumerator = first.GetEnumerator();
        using IEnumerator<TSecond> secondEnumerator = second.GetEnumerator();

        bool firstHasNext = firstEnumerator.MoveNext();
        bool secondHasNext = secondEnumerator.MoveNext();

        while(firstHasNext || secondHasNext)
        {
            TFirst? firstValue = firstHasNext ? firstEnumerator.Current : default;
            TSecond? secondValue = secondHasNext ? secondEnumerator.Current : default;

            yield return (firstValue, secondValue);

            firstHasNext = firstHasNext && firstEnumerator.MoveNext();
            secondHasNext = secondHasNext && secondEnumerator.MoveNext();
        }
    }

    private static IEnumerable<TResult> ZipPaddedIterator<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, TFirst firstDefault, TSecond secondDefault, Func<TFirst, TSecond, TResult> resultSelector)
    {
        using IEnumerator<TFirst> firstEnumerator = first.GetEnumerator();
        using IEnumerator<TSecond> secondEnumerator = second.GetEnumerator();

        bool firstHasNext = firstEnumerator.MoveNext();
        bool secondHasNext = secondEnumerator.MoveNext();

        while(firstHasNext || secondHasNext)
        {
            TFirst firstValue = firstHasNext ? firstEnumerator.Current : firstDefault;
            TSecond secondValue = secondHasNext ? secondEnumerator.Current : secondDefault;

            yield return resultSelector(firstValue, secondValue);

            firstHasNext = firstHasNext && firstEnumerator.MoveNext();
            secondHasNext = secondHasNext && secondEnumerator.MoveNext();
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Zips two sequences together, continuing until both are exhausted. Once the shorter sequence ends, its slots
    ///are filled with <c>default</c>.
    ///</summary>
    ///<typeparam name="TFirst">The first sequence's element type.</typeparam>
    ///<typeparam name="TSecond">The second sequence's element type.</typeparam>
    ///<param name="first">The first sequence.</param>
    ///<param name="second">The second sequence.</param>
    ///<returns>A sequence of paired elements, padded with <c>default</c> once the shorter sequence ends.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
    public static IEnumerable<(TFirst? First, TSecond? Second)> ZipPadded<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
    {
        ArgumentNullException.ThrowIfNull(first, nameof(first));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        return ZipPaddedIterator(first, second);
    }

    ///<summary>
    ///Zips two sequences together using the specified fallback values and result selector, continuing until both
    ///are exhausted.
    ///</summary>
    ///<typeparam name="TFirst">The first sequence's element type.</typeparam>
    ///<typeparam name="TSecond">The second sequence's element type.</typeparam>
    ///<typeparam name="TResult">The projected result type.</typeparam>
    ///<param name="first">The first sequence.</param>
    ///<param name="second">The second sequence.</param>
    ///<param name="firstDefault">The value substituted for <paramref name="first"/> once it is exhausted.</param>
    ///<param name="secondDefault">The value substituted for <paramref name="second"/> once it is exhausted.</param>
    ///<param name="resultSelector">A function that combines each pair into a result.</param>
    ///<returns>A sequence of projected results, continuing until both sequences are exhausted.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="first"/>, <paramref name="second"/>, or <paramref name="resultSelector"/> is <c>null</c>.
    ///</exception>
    public static IEnumerable<TResult> ZipPadded<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, TFirst firstDefault, TSecond secondDefault, Func<TFirst, TSecond, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(first, nameof(first));
        ArgumentNullException.ThrowIfNull(second, nameof(second));
        ArgumentNullException.ThrowIfNull(resultSelector, nameof(resultSelector));
        return ZipPaddedIterator(first, second, firstDefault, secondDefault, resultSelector);
    }
    #endregion
}
