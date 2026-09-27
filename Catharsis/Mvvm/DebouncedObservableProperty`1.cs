using CommunityToolkit.Mvvm.ComponentModel;

namespace Catharsis.Mvvm;

///<summary>
///An observable property that coalesces rapid updates: <see cref="Value"/> updates and notifies immediately on every
///set, but <see cref="DebouncedValueChanged"/> fires only once the value has stopped changing for the configured
///delay — the classic "search as you type" debounce.
///</summary>
///<typeparam name="T">The type of the property's value.</typeparam>
///<param name="initialValue">The initial value.</param>
///<param name="delay">How long the value must stay unchanged before <see cref="DebouncedValueChanged"/> fires.</param>
public sealed class DebouncedObservableProperty<T>(T initialValue, TimeSpan delay) : ObservableObject, IDisposable
{
    #region Fields
    CancellationTokenSource? _debounceSource;
    bool _disposed;
    T _value = initialValue;
    #endregion

    #region Events
    ///<summary>
    ///Raised once <see cref="Value"/> has remained unchanged for the configured delay.
    ///</summary>
    public event EventHandler<T>? DebouncedValueChanged;
    #endregion

    #region Private methods
    async Task DebounceAsync(T value, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
            DebouncedValueChanged?.Invoke(this, value);
        } catch(OperationCanceledException)
        {
        }
    }

    void ScheduleDebounce(T value)
    {
        _debounceSource?.Cancel();
        _debounceSource?.Dispose();

        CancellationTokenSource source = new();
        _debounceSource = source;

        _ = DebounceAsync(value, source.Token);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Cancels any pending debounce and releases resources. Does not raise a final <see cref="DebouncedValueChanged"/>.
    ///</summary>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        _debounceSource?.Cancel();
        _debounceSource?.Dispose();
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets or sets the current value. Setting it notifies <see cref="ObservableObject.PropertyChanged"/> immediately
    ///and (re)starts the debounce timer for <see cref="DebouncedValueChanged"/>.
    ///</summary>
    public T Value
    {
        get => _value;
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if(SetProperty(ref _value, value))
            {
                ScheduleDebounce(value);
            }
        }
    }
    #endregion
}
