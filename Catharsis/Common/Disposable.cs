namespace Catharsis.Common;

///<summary>
///Provides helpers for constructing <see cref="IDisposable"/> instances without writing a dedicated class.
///</summary>
public static class Disposable
{
    #region Fields
    private static readonly IDisposable _empty = Create(
                                                 static() =>
    {
    });
    #endregion

    #region Public methods
    ///<summary>
    ///Combines multiple disposables into a single one that disposes all of them, in the order given, when disposed.
    ///</summary>
    ///<param name="disposables">The disposables to combine.</param>
    ///<returns>A single <see cref="IDisposable"/> that disposes every item in <paramref name="disposables"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="disposables"/> is <c>null</c>.</exception>
    public static IDisposable Combine(params IDisposable[] disposables)
    {
        ArgumentNullException.ThrowIfNull(disposables);

        IDisposable[] snapshot = [ .. disposables ];
        return Create(
               () =>
               {
                   foreach(IDisposable disposable in snapshot)
                   {
                       disposable.Dispose();
                   }
               });
    }

    ///<summary>
    ///Creates an <see cref="IDisposable"/> that invokes the specified delegate exactly once, on the first call to
    public static IDisposable Create(Action onDispose)
    {
        ArgumentNullException.ThrowIfNull(onDispose);
        return new ActionDisposable(onDispose);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets an <see cref="IDisposable"/> whose <see cref="IDisposable.Dispose"/> method does nothing.
    ///</summary>
    public static IDisposable Empty => _empty;
    #endregion

    private sealed class ActionDisposable(Action onDispose) : IDisposable
    {
        #region Fields
        private Action? _onDispose = onDispose;
        #endregion

        #region Public methods
        public void Dispose() => Interlocked.Exchange(ref _onDispose, null)?.Invoke();
        #endregion
    }
}
