namespace Catharsis.DesignPatterns.Structural;

///<summary>
///Implements the Decorator design pattern.
///</summary>
public class Decorator
{
    #region Public methods
    ///<summary>
    ///Decorator — applies one or more <paramref name="decorators"/> in sequence, each wrapping the result of the
    ///previous transformation.
    ///</summary>
    ///<typeparam name="T">The type of the object being decorated.</typeparam>
    ///<param name="obj">The object to decorate.</param>
    ///<param name="decorators">One or more wrapping transformations applied in order.</param>
    ///<returns>The fully decorated object.</returns>
    public static T Decorate<T>(T obj, params Func<T, T>[] decorators)
    {
        if(decorators is null)
        {
            throw new ArgumentNullException(nameof(decorators), "Decorators must not be null.");
        }

        T? result = obj;

        foreach(Func<T, T> decorator in decorators)
        {
            result = decorator(result);
        }

        return result;
    }
    #endregion
}
