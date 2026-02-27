namespace Catharsis.DesignPatterns.Structural;

///<summary>
///Implements the Facade design pattern.
///</summary>
public class FacadePattern
{
    #region Public methods
    ///<summary>
    ///Facade — hides subsystem complexity behind a single <paramref name="simplifiedOperation"/> that produces a
    ///<typeparamref name="TResult"/>.
    ///</summary>
    ///<typeparam name="T">The type of the subsystem entry point.</typeparam>
    ///<typeparam name="TResult">The simplified result type.</typeparam>
    ///<param name="obj">The subsystem entry point.</param>
    ///<param name="simplifiedOperation">A delegate that orchestrates the subsystem and returns a simplified result.</param>
    ///<returns>The result of the simplified operation.</returns>
    public TResult Facade<T, TResult>(T obj, Func<T, TResult> simplifiedOperation)
    {
        if(simplifiedOperation is null)
        {
            throw new ArgumentNullException(nameof(simplifiedOperation), "Facade operation must not be null.");
        }

        return simplifiedOperation(obj);
    }
    #endregion
}
