namespace Catharsis.Randomization;

///<summary>
///Extension methods for randomly shuffling collections in place using the Fisher-Yates algorithm.
///</summary>
public static class ShuffleExtensions
{
    #region Public methods
    ///<summary>
    ///Shuffles the elements of the specified list in place, using the Fisher-Yates algorithm.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="list">The list to shuffle.</param>
    ///<param name="random">The random source to use, or <c>null</c> to use <see cref="Random.Shared"/>.</param>
    ///<exception cref="ArgumentNullException"><paramref name="list"/> is <c>null</c>.</exception>
    public static void Shuffle<T>(this IList<T> list, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(list);

        Random rng = random ?? Random.Shared;

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    ///<summary>
    ///Shuffles the elements of the specified span in place, using the Fisher-Yates algorithm.
    ///</summary>
    ///<typeparam name="T">The element type.</typeparam>
    ///<param name="span">The span to shuffle.</param>
    ///<param name="random">The random source to use, or <c>null</c> to use <see cref="Random.Shared"/>.</param>
    public static void Shuffle<T>(this Span<T> span, Random? random = null) => (random ?? Random.Shared).Shuffle(span);
    #endregion
}
