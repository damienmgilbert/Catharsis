namespace Catharsis.Extensions;

///<summary>
///Provides fluent <c>Map</c>/<c>Match</c> extension methods for <see cref="Nullable{T}"/>, in the style of an
///<c>Option</c> type from functional languages.
///</summary>
public static class NullableExtensions
{
    #region Public methods

    ///<summary>
    ///Projects the value if present, leaving <c>null</c> as <c>null</c>.
    ///</summary>
    ///<typeparam name="T">The source value type.</typeparam>
    ///<typeparam name="TResult">The projected value type.</typeparam>
    ///<param name="value">The nullable source value.</param>
    ///<param name="map">A function projecting the value, if present.</param>
    ///<returns>The projected value wrapped in a <see cref="Nullable{TResult}"/>, or <c>null</c> if <paramref name="value"/> has no value.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="map"/> is <c>null</c>.</exception>
    public static TResult? Map<T, TResult>(this T? value, Func<T, TResult> map) where T : struct where TResult : struct
    {
        if(map is null)
        {
            throw new ArgumentNullException(nameof(map), "Map function must not be null.");
        }

        return value.HasValue ? map(value.GetValueOrDefault()) : null;
    }

    ///<summary>
    ///Resolves to one of two results depending on whether the value is present.
    ///</summary>
    ///<typeparam name="T">The source value type.</typeparam>
    ///<typeparam name="TResult">The result type.</typeparam>
    ///<param name="value">The nullable source value.</param>
    ///<param name="some">A function producing the result when a value is present.</param>
    ///<param name="none">A function producing the result when no value is present.</param>
    ///<returns>The result of <paramref name="some"/> or <paramref name="none"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="some"/> or <paramref name="none"/> is <c>null</c>.</exception>
    public static TResult Match<T, TResult>(this T? value, Func<T, TResult> some, Func<TResult> none) where T : struct
    {
        if(some is null)
        {
            throw new ArgumentNullException(nameof(some), "Some function must not be null.");
        }

        if(none is null)
        {
            throw new ArgumentNullException(nameof(none), "None function must not be null.");
        }

        return value.HasValue ? some(value.GetValueOrDefault()) : none();
    }
    #endregion
}
