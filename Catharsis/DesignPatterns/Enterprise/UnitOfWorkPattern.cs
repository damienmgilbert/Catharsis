namespace Catharsis.DesignPatterns.Enterprise;

///<summary>
///Implements the Unit of Work design pattern.
///</summary>
public class UnitOfWorkPattern
{
    #region Public methods

    ///<summary>
    ///Unit of Work — runs <paramref name="work"/> inside a transactional boundary, invoking <paramref name="commit"/>
    ///if it completes without throwing, or <paramref name="rollback"/> before the exception propagates if it throws.
    ///</summary>
    ///<param name="work">The unit of work to perform.</param>
    ///<param name="commit">Invoked after <paramref name="work"/> completes successfully.</param>
    ///<param name="rollback">Invoked if <paramref name="work"/> throws, before the exception propagates.</param>
    public static void UnitOfWork(Action work, Action commit, Action rollback)
    {
        if(work is null)
        {
            throw new ArgumentNullException(nameof(work), "Work must not be null.");
        }

        if(commit is null)
        {
            throw new ArgumentNullException(nameof(commit), "Commit must not be null.");
        }

        if(rollback is null)
        {
            throw new ArgumentNullException(nameof(rollback), "Rollback must not be null.");
        }

        try
        {
            work();
            commit();
        } catch
        {
            rollback();
            throw;
        }
    }

    ///<summary>
    ///Unit of Work — runs <paramref name="work"/> inside a transactional boundary and returns its result, invoking
    public static TResult UnitOfWork<TResult>(Func<TResult> work, Action commit, Action rollback)
    {
        if(work is null)
        {
            throw new ArgumentNullException(nameof(work), "Work must not be null.");
        }

        if(commit is null)
        {
            throw new ArgumentNullException(nameof(commit), "Commit must not be null.");
        }

        if(rollback is null)
        {
            throw new ArgumentNullException(nameof(rollback), "Rollback must not be null.");
        }

        try
        {
            TResult result = work();
            commit();
            return result;
        } catch
        {
            rollback();
            throw;
        }
    }
    #endregion
}
