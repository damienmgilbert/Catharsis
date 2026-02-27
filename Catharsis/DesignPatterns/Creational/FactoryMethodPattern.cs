namespace Catharsis.DesignPatterns.Creational;

///<summary>
///Implements the Factory Method design pattern.
///</summary>
public class FactoryMethodPattern
{
    #region Public methods
    ///<summary>
    ///Factory Method — creates a <typeparamref name="TResult"/> from <paramref name="obj"/> using the supplied
    ///<paramref name="factory"/> delegate.
    ///</summary>
    ///<typeparam name="T">The type of the source object.</typeparam>
    ///<typeparam name="TResult">The type of the created product.</typeparam>
    ///<param name="obj">The source object passed to the factory.</param>
    ///<param name="factory">A delegate that produces a <typeparamref name="TResult"/> from the source.</param>
    ///<returns>The product created by <paramref name="factory"/>.</returns>
    public TResult FactoryMethod<T, TResult>(T obj, Func<T, TResult> factory)
    {
        if(factory is null)
        {
            throw new ArgumentNullException(nameof(factory), "Factory function must not be null.");
        }

        return factory(obj);
    }
    #endregion
}
