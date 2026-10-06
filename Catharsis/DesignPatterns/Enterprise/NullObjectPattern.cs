namespace Catharsis.DesignPatterns.Enterprise;

///<summary>
///Implements the Null Object design pattern.
///</summary>
public class NullObjectPattern
{
    #region Public methods

    ///<summary>
    ///Null Object — returns <paramref name="value"/> if it is not <c>null</c>, otherwise a do-nothing stand-in produced
    ///by <paramref name="nullObjectFactory"/>, so callers never need to null-check the result.
    ///</summary>
    ///<typeparam name="T">The type of the value.</typeparam>
    ///<param name="value">The value to use, or <c>null</c> to fall back to the null object.</param>
    ///<param name="nullObjectFactory">A delegate that produces the null-object stand-in.</param>
    ///<returns><paramref name="value"/>, or the result of <paramref name="nullObjectFactory"/> if it was <c>null</c>.</returns>
    public static T NullObject<T>(T? value, Func<T> nullObjectFactory) where T : class
    {
        if(nullObjectFactory is null)
        {
            throw new ArgumentNullException(nameof(nullObjectFactory), "Null object factory must not be null.");
        }

        return value ?? nullObjectFactory();
    }
    #endregion
}
