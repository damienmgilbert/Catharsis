using System.Numerics;

namespace Catharsis.Numerics;

///<summary>
///Provides SIMD-accelerated aggregate operations over spans of numeric values, using the portable
///<see cref="Vector{T}"/> API (which JITs to whatever instruction-set extension is actually available on the
///running hardware) rather than any specific instruction set. Automatically falls back to a scalar loop for
///element counts too small to fill a vector, or for element types <see cref="Vector{T}"/> does not support (e.g.
///<see cref="decimal"/> or <see cref="System.Numerics.BigInteger"/>).
///</summary>
public static class SimdAggregator
{
    #region Public methods
    ///<summary>
    ///Computes the dot product of two equal-length sequences.
    ///</summary>
    ///<typeparam name="T">A value type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    ///<param name="left">The first sequence.</param>
    ///<param name="right">The second sequence, of the same length as <paramref name="left"/>.</param>
    ///<returns>The sum of the element-wise products.</returns>
    ///<exception cref="ArgumentException"><paramref name="left"/> and <paramref name="right"/> have different lengths.</exception>
    public static T Dot<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right) where T : struct, INumber<T>
    {
        if (left.Length != right.Length)
        {
            throw new ArgumentException("Sequences must have the same length.", nameof(right));
        }

        T sum = T.Zero;
        int index = 0;

        if (Vector<T>.IsSupported && Vector.IsHardwareAccelerated && (left.Length >= Vector<T>.Count))
        {
            int vectorSize = Vector<T>.Count;
            Vector<T> accumulator = Vector<T>.Zero;

            for (; index <= (left.Length - vectorSize); index += vectorSize)
            {
                accumulator += new Vector<T>(left.Slice(index, vectorSize)) * new Vector<T>(right.Slice(index, vectorSize));
            }

            sum = Vector.Sum(accumulator);
        }

        for (; index < left.Length; index++)
        {
            sum += left[index] * right[index];
        }

        return sum;
    }

    ///<summary>
    ///Finds the maximum value in a span.
    ///</summary>
    ///<typeparam name="T">A value type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    ///<param name="values">The values to search. Must not be empty.</param>
    ///<returns>The largest value in <paramref name="values"/>.</returns>
    ///<exception cref="ArgumentException"><paramref name="values"/> is empty.</exception>
    public static T Max<T>(ReadOnlySpan<T> values) where T : struct, INumber<T> => Reduce(values, Vector.Max, T.Max);

    ///<summary>
    ///Finds the minimum value in a span.
    ///</summary>
    ///<typeparam name="T">A value type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    ///<param name="values">The values to search. Must not be empty.</param>
    ///<returns>The smallest value in <paramref name="values"/>.</returns>
    ///<exception cref="ArgumentException"><paramref name="values"/> is empty.</exception>
    public static T Min<T>(ReadOnlySpan<T> values) where T : struct, INumber<T> => Reduce(values, Vector.Min, T.Min);

    ///<summary>
    ///Sums every value in a span.
    ///</summary>
    ///<typeparam name="T">A value type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    ///<param name="values">The values to sum.</param>
    ///<returns>The sum of <paramref name="values"/>, or <see cref="INumberBase{TSelf}.Zero"/> if empty.</returns>
    public static T Sum<T>(ReadOnlySpan<T> values) where T : struct, INumber<T>
    {
        T sum = T.Zero;
        int index = 0;

        if (Vector<T>.IsSupported && Vector.IsHardwareAccelerated && (values.Length >= Vector<T>.Count))
        {
            int vectorSize = Vector<T>.Count;
            Vector<T> accumulator = Vector<T>.Zero;

            for (; index <= (values.Length - vectorSize); index += vectorSize)
            {
                accumulator += new Vector<T>(values.Slice(index, vectorSize));
            }

            sum = Vector.Sum(accumulator);
        }

        for (; index < values.Length; index++)
        {
            sum += values[index];
        }

        return sum;
    }
    #endregion

    #region Private methods
    static T Reduce<T>(ReadOnlySpan<T> values, Func<Vector<T>, Vector<T>, Vector<T>> vectorReduce, Func<T, T, T> scalarReduce) where T : struct, INumber<T>
    {
        if (values.IsEmpty)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        int index = 1;
        T result = values[0];

        if (Vector<T>.IsSupported && Vector.IsHardwareAccelerated && (values.Length >= Vector<T>.Count))
        {
            int vectorSize = Vector<T>.Count;
            Vector<T> accumulator = new(values[..vectorSize]);

            for (index = vectorSize; index <= (values.Length - vectorSize); index += vectorSize)
            {
                accumulator = vectorReduce(accumulator, new Vector<T>(values.Slice(index, vectorSize)));
            }

            T[] lanes = new T[vectorSize];
            accumulator.CopyTo(lanes);

            result = lanes[0];

            for (int lane = 1; lane < vectorSize; lane++)
            {
                result = scalarReduce(result, lanes[lane]);
            }
        }

        for (; index < values.Length; index++)
        {
            result = scalarReduce(result, values[index]);
        }

        return result;
    }
    #endregion
}
