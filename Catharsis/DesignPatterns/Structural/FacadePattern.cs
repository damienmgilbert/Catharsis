namespace Catharsis.DesignPatterns.Structural;

///<summary>
///Implements the Facade design pattern.
///</summary>
public class FacadePattern
{
    #region Public methods

    ///<summary>
    ///Facade — hides subsystem complexity behind a single <paramref name="simplifiedOperation"/> that produces a
    public static TResult Facade<T, TResult>(T obj, Func<T, TResult> simplifiedOperation)
    {
        if(simplifiedOperation is null)
        {
            throw new ArgumentNullException(nameof(simplifiedOperation), "Facade operation must not be null.");
        }

        return simplifiedOperation(obj);
    }
    #endregion
}
