namespace Catharsis.DesignPatterns.Creational;

///<summary>
///Implements the Builder design pattern.
///</summary>
public class Builder
{
    #region Public methods
    ///<summary>
    ///Builder — applies an ordered sequence of configuration <paramref name="steps"/> to <paramref name="obj"/>, then
    ///returns the configured object.
    ///</summary>
    ///<typeparam name="T">The type of the object being built.</typeparam>
    ///<param name="obj">The object to configure.</param>
    ///<param name="steps">An ordered set of mutating configuration actions.</param>
    ///<returns>The configured <paramref name="obj"/>.</returns>
    public static T Build<T>(T obj, params Action<T>[] steps)
    {
        if (steps is null)
        {
            throw new ArgumentNullException(nameof(steps), "Build steps must not be null.");
        }

        foreach (Action<T> step in steps)
        {
            step(obj);
        }

        return obj;
    }

    ///<summary>
    ///Builder — applies <paramref name="steps"/> then produces a final <typeparamref name="TResult"/> via <paramref
    ///name="finalizer"/>.
    ///</summary>
    ///<typeparam name="T">The type of the object being built.</typeparam>
    ///<typeparam name="TResult">The type of the final product.</typeparam>
    ///<param name="obj">The object to configure.</param>
    ///<param name="finalizer">A delegate that produces the final product from the configured object.</param>
    ///<param name="steps">An ordered set of mutating configuration actions.</param>
    ///<returns>The product created by <paramref name="finalizer"/> after all steps are applied.</returns>
    public static TResult Build<T, TResult>(T obj, Func<T, TResult> finalizer, params Action<T>[] steps)
    {
        if (finalizer is null)
        {
            throw new ArgumentNullException(nameof(finalizer), "Finalizer function must not be null.");
        }

        if (steps is null)
        {
            throw new ArgumentNullException(nameof(steps), "Build steps must not be null.");
        }

        foreach (Action<T> step in steps)
        {
            step(obj);
        }

        return finalizer(obj);
    }
    #endregion
}
