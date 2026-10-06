using Catharsis.Generics;
using Catharsis.Operators;

namespace Catharsis.Patterns.Composed;

///<summary>
///The context of the strategy pattern for pricing: it holds named <see cref="IPricingStrategy{TItem}"/> instances and
///prices an item with whichever one the caller names, so the choice can come from configuration or user input. Names
///are case-insensitive.
///</summary>
///<typeparam name="TItem">The kind of item being priced.</typeparam>
public sealed class PricingContext<TItem>
{
    #region Fields
    private readonly TypedRegistry<string, IPricingStrategy<TItem>> _strategies = new(StringComparer.OrdinalIgnoreCase);
    #endregion

    #region Public methods
    ///<summary>
    ///Prices an item with the named strategy.
    ///</summary>
    ///<param name="strategyName">The name of a registered strategy.</param>
    ///<param name="item">The item to price.</param>
    ///<returns>The price.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="strategyName"/> is <c>null</c>.</exception>
    ///<exception cref="KeyNotFoundException">No strategy has that name.</exception>
    public Money Price(string strategyName, TItem item) => _strategies[strategyName].Price(item);

        ///<summary>
///Registers a strategy under a name.
///</summary>
    ///<param name="name">The name callers use to select it.</param>
    ///<param name="strategy">The strategy.</param>
    ///<returns>This context for chaining.</returns>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    ///<exception cref="InvalidOperationException">The name is already registered.</exception>
    public PricingContext<TItem> Register(string name, IPricingStrategy<TItem> strategy)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(strategy);

        _strategies.Register(name, strategy);
        return this;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the registered strategy names.
    ///</summary>
    public IReadOnlyCollection<string> Names => _strategies.Keys;
    #endregion
}
