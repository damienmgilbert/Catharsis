namespace Catharsis.DesignPatterns.Behavioral;

///<summary>
///Implements the Strategy design pattern.
///</summary>
public class StrategyPattern
{
    #region Public methods
    ///<summary>
    ///Strategy — applies the interchangeable <paramref name="strategy"/> algorithm to <paramref name="obj"/> and
    ///returns the result.
    ///</summary>
    ///<typeparam name="T">The type of the context.</typeparam>
    ///<typeparam name="TResult">The result type of the algorithm.</typeparam>
    ///<param name="obj">The context object.</param>
    ///<param name="strategy">The algorithm to apply.</param>
    ///<returns>The result of the strategy.</returns>
    public static TResult Strategy<T, TResult>(T obj, Func<T, TResult> strategy)
    {
        if(strategy is null)
        {
            throw new ArgumentNullException(nameof(strategy), "Strategy function must not be null.");
        }

        return strategy(obj);
    }

    ///<summary>
    ///Strategy — applies the interchangeable <paramref name="strategy"/> algorithm to <paramref name="obj"/> and
    ///returns <paramref name="obj"/> for fluent chaining.
    ///</summary>
    ///<typeparam name="T">The type of the context.</typeparam>
    ///<param name="obj">The context object.</param>
    ///<param name="strategy">The algorithm to apply.</param>
    ///<returns>The original <paramref name="obj"/>.</returns>
    public static T Strategy<T>(T obj, Action<T> strategy)
    {
        if(strategy is null)
        {
            throw new ArgumentNullException(nameof(strategy), "Strategy action must not be null.");
        }

        strategy(obj);
        return obj;
    }
    #endregion
}
