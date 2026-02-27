namespace Catharsis.DesignPatterns.Behavioral;

///<summary>
///Implements the Mediator design pattern.
///</summary>
public class Mediator
{
    #region Public methods
    ///<summary>
    ///Mediator — routes <paramref name="obj"/> through <paramref name="mediator"/> using the supplied <paramref
    ///name="route"/> delegate, returning the result.
    ///</summary>
    ///<typeparam name="T">The type of the colleague/request.</typeparam>
    ///<typeparam name="TMediator">The type of the mediator.</typeparam>
    ///<typeparam name="TResult">The type of the mediated result.</typeparam>
    ///<param name="obj">The colleague or request object.</param>
    ///<param name="mediator">The mediator that coordinates communication.</param>
    ///<param name="route">A delegate that uses the mediator to process the request.</param>
    ///<returns>The result produced by the mediator.</returns>
    public TResult Mediate<T, TMediator, TResult>(T obj, TMediator mediator, Func<TMediator, T, TResult> route)
    {
        if(mediator is null)
        {
            throw new ArgumentNullException(nameof(mediator), "Mediator must not be null.");
        }

        if(route is null)
        {
            throw new ArgumentNullException(nameof(route), "Route function must not be null.");
        }

        return route(mediator, obj);
    }

    ///<summary>
    ///Mediator — routes <paramref name="obj"/> through <paramref name="mediator"/> using the supplied <paramref
    ///name="route"/> action.
    ///</summary>
    ///<typeparam name="T">The type of the colleague/request.</typeparam>
    ///<typeparam name="TMediator">The type of the mediator.</typeparam>
    ///<param name="obj">The colleague or request object.</param>
    ///<param name="mediator">The mediator that coordinates communication.</param>
    ///<param name="route">An action that uses the mediator to process the request.</param>
    ///<returns>The original <paramref name="obj"/>.</returns>
    public T Mediate<T, TMediator>(T obj, TMediator mediator, Action<TMediator, T> route)
    {
        if(mediator is null)
        {
            throw new ArgumentNullException(nameof(mediator), "Mediator must not be null.");
        }

        if(route is null)
        {
            throw new ArgumentNullException(nameof(route), "Route action must not be null.");
        }

        route(mediator, obj);
        return obj;
    }
    #endregion
}
