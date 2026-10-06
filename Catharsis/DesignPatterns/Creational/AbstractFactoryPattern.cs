namespace Catharsis.DesignPatterns.Creational;

///<summary>
///Implements the Abstract Factory design pattern.
///</summary>
public class AbstractFactoryPattern
{
    #region Public methods

    ///<summary>
    ///Abstract Factory — uses a <paramref name="factory"/> object together with <paramref name="create"/> to produce a
    public static TResult AbstractFactory<T, TFactory, TResult>(T obj, TFactory factory, Func<TFactory, T, TResult> create)
    {
        if(factory is null)
        {
            throw new ArgumentNullException(nameof(factory), "Factory must not be null.");
        }

        if(create is null)
        {
            throw new ArgumentNullException(nameof(create), "Create function must not be null.");
        }

        return create(factory, obj);
    }
    #endregion
}
