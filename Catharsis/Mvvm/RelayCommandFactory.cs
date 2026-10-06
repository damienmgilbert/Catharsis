using CommunityToolkit.Mvvm.Input;

namespace Catharsis.Mvvm;

///<summary>
///Builds <see cref="IAsyncRelayCommand"/> instances with built-in busy-state reporting and exception routing, so view
///models don't need to hand-roll the same try/finally boilerplate around every async command.
///</summary>
public static class RelayCommandFactory
{
    #region Public methods

    ///<summary>
    ///Creates an <see cref="IAsyncRelayCommand"/> that reports busy state via <paramref name="onBusyChanged"/> and
    ///routes exceptions to <paramref name="onException"/> instead of letting them propagate as unobserved task
    ///exceptions.
    ///</summary>
    ///<param name="execute">The asynchronous action to run when the command executes.</param>
    ///<param name="onBusyChanged">Invoked with <c>true</c> before <paramref name="execute"/> starts and <c>false</c> after it completes.</param>
    ///<param name="onException">Invoked if <paramref name="execute"/> throws. If <c>null</c>, the exception propagates as usual.</param>
    ///<returns>A configured <see cref="IAsyncRelayCommand"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="execute"/> is <c>null</c>.</exception>
    public static IAsyncRelayCommand Create(Func<Task> execute, Action<bool>? onBusyChanged = null, Action<Exception>? onException = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        return new AsyncRelayCommand(
               async () =>
               {
                   onBusyChanged?.Invoke(true);

                   try
                   {
                       await execute();
                   } catch(Exception exception) when(onException is not null)
                   {
                       onException(exception);
                   } finally
                   {
                       onBusyChanged?.Invoke(false);
                   }
               });
    }

    ///<summary>
    ///Creates an <see cref="IAsyncRelayCommand{T}"/> that reports busy state via <paramref name="onBusyChanged"/> and
    ///routes exceptions to <paramref name="onException"/> instead of letting them propagate as unobserved task
    ///exceptions.
    ///</summary>
    ///<typeparam name="T">The type of the command's parameter.</typeparam>
    ///<param name="execute">The asynchronous action to run when the command executes.</param>
    ///<param name="onBusyChanged">Invoked with <c>true</c> before <paramref name="execute"/> starts and <c>false</c> after it completes.</param>
    ///<param name="onException">Invoked if <paramref name="execute"/> throws. If <c>null</c>, the exception propagates as usual.</param>
    ///<returns>A configured <see cref="IAsyncRelayCommand{T}"/>.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="execute"/> is <c>null</c>.</exception>
    public static IAsyncRelayCommand<T> Create<T>(Func<T?, Task> execute, Action<bool>? onBusyChanged = null, Action<Exception>? onException = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        return new AsyncRelayCommand<T>(
               async parameter =>
               {
                   onBusyChanged?.Invoke(true);

                   try
                   {
                       await execute(parameter);
                   } catch(Exception exception) when(onException is not null)
                   {
                       onException(exception);
                   } finally
                   {
                       onBusyChanged?.Invoke(false);
                   }
               });
    }
    #endregion
}
