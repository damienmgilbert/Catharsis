namespace Catharsis.Common;

///<summary>
///A fluent, chainable wrapper around <see cref="HashCode"/> for combining values into a single hash code.
///</summary>
///<remarks>
///<see cref="HashCode"/> already covers most needs via <see cref="HashCode.Combine{T1, T2}(T1, T2)"/> and its
///overloads; this exists for the two cases those don't cover well: chaining an unknown number of values (via
///<see cref="Start"/>) and combining an existing <see cref="IEnumerable{T}"/> in one call (via <see cref="Combine"/>).
///</remarks>
public sealed class HashCodeCombiner
{
    #region Fields
    HashCode _hashCode;
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a value to the combined hash code.
    ///</summary>
    ///<typeparam name="T">The type of the value.</typeparam>
    ///<param name="value">The value to add.</param>
    ///<returns>This instance, to allow chaining.</returns>
    public HashCodeCombiner Add<T>(T value)
    {
        _hashCode.Add(value);
        return this;
    }

    ///<summary>
    ///Computes the combined hash code from every value added so far.
    ///</summary>
    ///<returns>The combined hash code.</returns>
    public int ToHashCode() => _hashCode.ToHashCode();

    ///<summary>
    ///Combines the hash codes of every value in <paramref name="values"/> into a single hash code.
    ///</summary>
    ///<param name="values">The values to combine.</param>
    ///<returns>The combined hash code.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="values"/> is <c>null</c>.</exception>
    public static int Combine(IEnumerable<object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        HashCode hashCode = new();

        foreach(object? value in values)
        {
            hashCode.Add(value);
        }

        return hashCode.ToHashCode();
    }

    ///<summary>
    ///Starts a new, empty <see cref="HashCodeCombiner"/> for chaining.
    ///</summary>
    ///<returns>A new <see cref="HashCodeCombiner"/>.</returns>
    public static HashCodeCombiner Start() => new();
    #endregion
}
