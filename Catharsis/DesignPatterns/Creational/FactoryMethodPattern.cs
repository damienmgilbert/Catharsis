namespace Catharsis.DesignPatterns.Creational;

///<summary>
///Implements the Factory Method design pattern.
///</summary>
public class FactoryMethodPattern
{
    #region Public methods

    ///<summary>
    ///Factory Method — creates a <typeparamref name="TResult"/> from <paramref name="obj"/> using the supplied
    public static TResult FactoryMethod<T, TResult>(T obj, Func<T, TResult> factory)
    {
        if(factory is null)
        {
            throw new ArgumentNullException(nameof(factory), "Factory function must not be null.");
        }

        return factory(obj);
    }
    #endregion
}
