using System.Runtime.CompilerServices;

namespace Catharsis.ComponentModel;

///<summary>
///An async counterpart to <see cref="ValidatableComponent"/>: adds <see cref="IAsyncDisposable"/> and an async
///validation hook for validation logic that itself needs to be asynchronous (e.g. a uniqueness check against a remote
///service).
///</summary>
public abstract class AsyncValidatableComponent : ValidatableComponent, IAsyncDisposable
{
    #region Protected methods

    ///<summary>
    ///Releases the unmanaged and, optionally, asynchronous resources used by this component. Override to release
    ///additional asynchronous resources; the default implementation does nothing.
    ///</summary>
    ///<returns>A task representing the asynchronous dispose operation.</returns>
    protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;

    ///<summary>
    ///Invokes <paramref name="setter"/> with <paramref name="value"/>, raises change notification for ///<paramref
    ///name="propertyName"/>, and then asynchronously validates the new value.
    ///</summary>
    ///<remarks>
    ///This takes a setter delegate rather than a <c>ref</c> backing field, unlike ///<see
    ///cref="ValidatableComponent.SetPropertyAndValidate{T}"/>, because C# does not allow <c>ref</c> parameters on an
    ///<c>async</c> method.
    ///</remarks>
    ///<typeparam name="T">The type of the property.</typeparam>
    ///<param name="value">The new value.</param>
    ///<param name="setter">The delegate that stores <paramref name="value"/> in the backing field.</param>
    ///<param name="cancellationToken">A token that can cancel the async validation.</param>
    ///<param name="propertyName">
    ///The name of the property. Automatically provided by the compiler.
    ///</param>
    ///<returns>A task representing the asynchronous validation operation.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="setter"/> is <c>null</c>.</exception>
    protected async Task SetPropertyAndValidateAsync<T>(T value, Action<T> setter, CancellationToken cancellationToken = default, [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(setter);

        setter(value);
        OnPropertyChanged(propertyName);
        await ValidatePropertyAsync(propertyName, value, cancellationToken);
    }

    ///<summary>
    ///Called to asynchronously validate a property value. Override to provide custom async validation logic. The
    ///default implementation does nothing.
    ///</summary>
    ///<param name="propertyName">The name of the property to validate.</param>
    ///<param name="value">The current value of the property.</param>
    ///<param name="cancellationToken">A token that can cancel the validation.</param>
    ///<returns>A task representing the asynchronous validation operation.</returns>
    protected virtual Task ValidatePropertyAsync(string? propertyName, object? value, CancellationToken cancellationToken = default) => Task.CompletedTask;
    #endregion

    #region Public methods
    ///<summary>
    ///Asynchronously releases the resources used by this component.
    ///</summary>
    ///<returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }
    #endregion
}
