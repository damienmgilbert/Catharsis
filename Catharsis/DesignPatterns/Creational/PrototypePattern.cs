namespace Catharsis.DesignPatterns.Creational;

///<summary>
///Implements the Prototype design pattern.
///</summary>
public class PrototypePattern
{
    #region Public methods

    ///<summary>
    ///Prototype — returns a copy of <paramref name="obj"/> produced by the supplied <paramref name="clone"/> delegate.
    ///</summary>
    ///<typeparam name="T">The type of the object to clone.</typeparam>
    ///<param name="obj">The object to clone.</param>
    ///<param name="clone">A delegate that produces a copy of the source object.</param>
    ///<returns>A clone of <paramref name="obj"/>.</returns>
    public static T Prototype<T>(T obj, Func<T, T> clone)
    {
        if(clone is null)
        {
            throw new ArgumentNullException(nameof(clone), "Clone function must not be null.");
        }

        return clone(obj);
    }
    #endregion
}
