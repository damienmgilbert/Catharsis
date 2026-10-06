using System.Collections.Immutable;

namespace Catharsis.Events;

///<summary>
///An immutable pipeline of <c>Func&lt;T, T&gt;</c> delegates, where each step receives the previous step's result.
///Adding a step returns a new chain, so a chain can be shared and extended safely.
///</summary>
///<typeparam name="T">The type flowing through the chain.</typeparam>
public sealed class DelegateChain<T>
{
    #region Fields
    private readonly ImmutableArray<Func<T, T>> _steps;
    #endregion

    #region Constructors
    private DelegateChain(ImmutableArray<Func<T, T>> steps) { _steps = steps; }
    public DelegateChain() : this([])
    {
    }
    #endregion

    #region Operators
    ///<summary>
    ///Returns a chain with <paramref name="step"/> appended.
    ///</summary>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static DelegateChain<T> operator +(DelegateChain<T> chain, Func<T, T> step)
    {
        ArgumentNullException.ThrowIfNull(chain);
        ArgumentNullException.ThrowIfNull(step);

        return new DelegateChain<T>(chain._steps.Add(step));
    }
    ///<summary>
    ///Returns a chain running <paramref name="first"/>'s steps and then <paramref name="second"/>'s.
    ///</summary>
    ///<exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static DelegateChain<T> operator +(DelegateChain<T> first, DelegateChain<T> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        return new DelegateChain<T>(first._steps.AddRange(second._steps));
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Runs every step in order.
    ///</summary>
    ///<param name="input">The value given to the first step.</param>
    ///<returns>The result of the last step, or <paramref name="input"/> if the chain is empty.</returns>
    public T Invoke(T input)
    {
        T current = input;

        foreach(Func<T, T> step in _steps)
        {
            current = step(current);
        }

        return current;
    }

        ///<summary>
///Returns a chain with <paramref name="step"/> appended.
///</summary>
    ///<exception cref="ArgumentNullException"><paramref name="step"/> is <c>null</c>.</exception>
    public DelegateChain<T> Then(Func<T, T> step) => this + step;

    ///<summary>
    ///Collapses the chain into a single delegate.
    ///</summary>
    public Func<T, T> ToDelegate() => Invoke;
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of steps.
    ///</summary>
    public int Count => _steps.Length;
    #endregion
}
