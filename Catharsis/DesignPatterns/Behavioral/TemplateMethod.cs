namespace Catharsis.DesignPatterns.Behavioral;

///<summary>
///Implements the Template Method design pattern.
///</summary>
public class TemplateMethod
{
    #region Public methods
    ///<summary>
    ///Template Method — executes an invariant algorithm skeleton of <paramref name="setup"/> → <paramref name="hook"/>
    ///→ <paramref name="teardown"/> on <paramref name="obj"/>, where <paramref name="hook"/> is the customizable step.
    ///</summary>
    ///<typeparam name="T">The type of the context.</typeparam>
    ///<param name="obj">The context object.</param>
    ///<param name="setup">The invariant setup step.</param>
    ///<param name="hook">The customizable hook step.</param>
    ///<param name="teardown">The invariant teardown step.</param>
    ///<returns>The original <paramref name="obj"/> after the algorithm completes.</returns>
    public T Template<T>(T obj, Action<T> setup, Action<T> hook, Action<T> teardown)
    {
        if(setup is null)
        {
            throw new ArgumentNullException(nameof(setup), "Setup action must not be null.");
        }

        if(hook is null)
        {
            throw new ArgumentNullException(nameof(hook), "Hook action must not be null.");
        }

        if(teardown is null)
        {
            throw new ArgumentNullException(nameof(teardown), "Teardown action must not be null.");
        }

        setup(obj);
        hook(obj);
        teardown(obj);
        return obj;
    }

    ///<summary>
    ///Template Method — executes <paramref name="setup"/> → <paramref name="operation"/> → <paramref name="teardown"/>
    ///on <paramref name="obj"/>, returning the result of the customizable <paramref name="operation"/> step.
    ///</summary>
    ///<typeparam name="T">The type of the context.</typeparam>
    ///<typeparam name="TResult">The result type of the operation step.</typeparam>
    ///<param name="obj">The context object.</param>
    ///<param name="setup">The invariant setup step.</param>
    ///<param name="operation">The customizable operation step.</param>
    ///<param name="teardown">The invariant teardown step.</param>
    ///<returns>The result of <paramref name="operation"/>.</returns>
    public TResult Template<T, TResult>(T obj, Action<T> setup, Func<T, TResult> operation, Action<T> teardown)
    {
        if(setup is null)
        {
            throw new ArgumentNullException(nameof(setup), "Setup action must not be null.");
        }

        if(operation is null)
        {
            throw new ArgumentNullException(nameof(operation), "Operation must not be null.");
        }

        if(teardown is null)
        {
            throw new ArgumentNullException(nameof(teardown), "Teardown action must not be null.");
        }

        setup(obj);
        TResult? result = operation(obj);
        teardown(obj);
        return result;
    }
    #endregion
}
