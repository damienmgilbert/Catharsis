namespace Catharsis.DesignPatterns.Creational;

///<summary>
///Implements the Abstract Factory design pattern.
///</summary>
public class AbstractFactoryPattern
{
    #region Public methods
    ///<summary>
    ///Abstract Factory — uses a <paramref name="factory"/> object together with <paramref name="create"/> to produce a
    ///<typeparamref name="TResult"/> from <paramref name="obj"/>.
    ///</summary>
    ///<typeparam name="T">The type of the source object.</typeparam>
    ///<typeparam name="TFactory">The type of the abstract factory.</typeparam>
    ///<typeparam name="TResult">The type of the created product.</typeparam>
    ///<param name="obj">The source object.</param>
    ///<param name="factory">The abstract factory instance.</param>
    ///<param name="create">A delegate that uses the factory and source to produce a product.</param>
    ///<returns>The product created via the abstract factory.</returns>
    public static TResult AbstractFactory<T, TFactory, TResult>(T obj, TFactory factory, Func<TFactory, T, TResult> create)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory), "Factory must not be null.");
        }

        if (create is null)
        {
            throw new ArgumentNullException(nameof(create), "Create function must not be null.");
        }

        return create(factory, obj);
    }
    #endregion
}
