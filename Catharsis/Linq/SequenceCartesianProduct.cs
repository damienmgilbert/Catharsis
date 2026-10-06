namespace Catharsis.Linq;

///<summary>
///Provides a cartesian product extension over a sequence of sequences: every combination formed by picking one
///element from each.
///</summary>
public static class SequenceCartesianProduct
{
    #region Private methods
    private static IEnumerable<IReadOnlyList<T>> CartesianProductIterator<T>(IEnumerable<IEnumerable<T>> sequences)
    {
        IReadOnlyList<T>[] materialized = [.. sequences.Select(static s => (IReadOnlyList<T>)[.. s])];

        if (materialized.Length == 0)
        {
            yield break;
        }

        foreach (IReadOnlyList<T> sequence in materialized)
        {
            if (sequence.Count == 0)
            {
                yield break;
            }
        }

        int[] indices = new int[materialized.Length];

        while (true)
        {
            T[] combination = new T[materialized.Length];

            for (int i = 0; i < materialized.Length; i++)
            {
                combination[i] = materialized[i][indices[i]];
            }

            yield return combination;

            int pointer = materialized.Length - 1;

            while (pointer >= 0)
            {
                indices[pointer]++;

                if (indices[pointer] < materialized[pointer].Count)
                {
                    break;
                }

                indices[pointer] = 0;
                pointer--;
            }

            if (pointer < 0)
            {
                yield break;
            }
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Computes the cartesian product of the specified sequences: every combination formed by picking one element
    ///from each sequence, in the order the sequences were supplied. Each source sequence is fully materialized
    ///since every combination revisits every sequence; if any sequence is empty, the result is empty.
    ///</summary>
    ///<typeparam name="T">The element type shared by every sequence.</typeparam>
    ///<param name="sequences">The sequences to combine.</param>
    ///<returns>A sequence of combinations, each containing one element per input sequence, in input order.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="sequences"/> is <c>null</c>.</exception>
    public static IEnumerable<IReadOnlyList<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
    {
        ArgumentNullException.ThrowIfNull(sequences, nameof(sequences));
        return CartesianProductIterator(sequences);
    }
    #endregion
}
