using Catharsis.Generics;

namespace Catharsis.Patterns.Composed;

///<summary>
///A factory that creates instances of <typeparamref name="T"/> by key. Creators are registered up front, so callers
///select an implementation by a name or enum value without knowing the concrete class. Every ///<see cref="Create"/>
///call runs the creator again, so it returns a new instance unless the creator itself shares one.
///</summary>
///<typeparam name="TKey">The key type.</typeparam>
///<typeparam name="T">The product type.</typeparam>
///<param name="comparer">Compares keys, or <c>null</c> for the default comparer.</param>
public sealed class KeyedFactory<TKey, T>(IEqualityComparer<TKey>? comparer) where TKey : notnull
{
    #region Fields
    private readonly TypedRegistry<TKey, Func<T>> _creators = new(comparer);
    #endregion

    #region Constructors
    ///<summary>
    ///Creates a factory using the default key comparer.
    ///</summary>
    public KeyedFactory() : this(null)
    {
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Creates the product registered under <paramref name="key"/>.
    ///</summary>
    ///<exception cref="KeyNotFoundException">The key is not registered.</exception>
    public T Create(TKey key) => _creators[key]();

    ///<summary>
    ///Registers a class with a public parameterless constructor.
    ///</summary>
    ///<typeparam name="TImplementation">The concrete product.</typeparam>
    ///<param name="key">The key.</param>
    ///<returns>This factory for chaining.</returns>
    ///<exception cref="InvalidOperationException">The key is already registered.</exception>
    public KeyedFactory<TKey, T> Register<TImplementation>(TKey key) where TImplementation : T, new() { return Register(key, static() => new TImplementation()); }

        ///<summary>
///Registers a creator delegate.
///</summary>
    ///<param name="key">The key.</param>
    ///<param name="creator">Produces the product.</param>
    ///<returns>This factory for chaining.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">The key is already registered.</exception>
    public KeyedFactory<TKey, T> Register(TKey key, Func<T> creator)
    {
        ArgumentNullException.ThrowIfNull(creator);

        _creators.Register(key, creator);
        return this;
    }

    ///<summary>
    ///Attempts to create the product registered under <paramref name="key"/>.
    ///</summary>
    ///<returns><c>true</c> if the key is registered.</returns>
    public bool TryCreate(TKey key, out T product)
    {
        if(_creators.TryGet(key, out Func<T>? creator))
        {
            product = creator();
            return true;
        }

        product = default!;
        return false;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the registered keys.
    ///</summary>
    public IReadOnlyCollection<TKey> Keys => _creators.Keys;
    #endregion
}
