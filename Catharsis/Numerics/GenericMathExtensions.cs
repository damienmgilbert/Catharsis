using System.Numerics;

namespace Catharsis.Numerics;

///<summary>
///Provides extension methods generic over any type implementing <see cref="INumber{TSelf}"/>, filling small gaps left
///by .NET's generic math interfaces. Clamping is deliberately not included here: <c>T.Clamp(value, min, max)</c> is
///already a static member of <see cref="INumber{TSelf}"/> itself.
///</summary>
public static class GenericMathExtensions
{
    #region Public methods

    ///<summary>
    ///Determines whether <paramref name="value"/> falls within the inclusive range [<paramref name="min"/>, <paramref
    ///name="max"/>].
    ///</summary>
    ///<typeparam name="T">A type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    ///<param name="value">The value to test.</param>
    ///<param name="min">The inclusive lower bound.</param>
    ///<param name="max">The inclusive upper bound.</param>
    ///<returns>
    ///<c>true</c> if <paramref name="value"/> is within [<paramref name="min"/>, <paramref name="max"/>]; otherwise
    ///<c>false</c>.
    ///</returns>
    public static bool IsBetween<T>(this T value, T min, T max) where T : INumber<T> => (value >= min) && (value <= max);

    ///<summary>
    ///Linearly interpolates between <paramref name="start"/> and <paramref name="end"/>.
    ///</summary>
    ///<remarks>
    ///The interpolation is performed in <see cref="double"/> precision via <see
    ///cref="INumberBase{TSelf}.CreateChecked{TOther}"/> and converted back to <typeparamref name="T"/>, so extremely
    ///large integral values may lose precision.
    ///</remarks>
    ///<typeparam name="T">A type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    ///<param name="start">The value at <paramref name="t"/> = 0.</param>
    ///<param name="end">The value at <paramref name="t"/> = 1.</param>
    ///<param name="t">
    ///The interpolation factor. Values outside [0, 1] extrapolate beyond <paramref name="start"/>/<paramref
    ///name="end"/>.
    ///</param>
    ///<returns>The interpolated value.</returns>
    public static T Lerp<T>(T start, T end, double t) where T : INumber<T>
    {
        double startValue = double.CreateChecked(start);
        double endValue = double.CreateChecked(end);
        return T.CreateChecked(startValue + ((endValue - startValue) * t));
    }

    ///<summary>
    ///Normalizes <paramref name="value"/> to its fractional position within [<paramref name="min"/>, <paramref
    ///name="max"/>], where <paramref name="min"/> maps to 0.0 and <paramref name="max"/> maps to 1.0.
    ///</summary>
    ///<typeparam name="T">A type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    ///<param name="value">The value to normalize.</param>
    ///<param name="min">The value that maps to 0.0.</param>
    ///<param name="max">The value that maps to 1.0.</param>
    ///<returns>
    ///The normalized position, which falls outside [0, 1] if <paramref name="value"/> is outside [<paramref
    ///name="min"/>, <paramref name="max"/>].
    ///</returns>
    ///<exception cref="ArgumentException"><paramref name="min"/> equals <paramref name="max"/>.</exception>
    public static double NormalizeTo<T>(this T value, T min, T max) where T : INumber<T>
    {
        if(min == max)
        {
            throw new ArgumentException("Minimum and maximum must not be equal.", nameof(max));
        }

        double normalizedValue = double.CreateChecked(value);
        double minValue = double.CreateChecked(min);
        double maxValue = double.CreateChecked(max);

        return (normalizedValue - minValue) / (maxValue - minValue);
    }
    #endregion
}
