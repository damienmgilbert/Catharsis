using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catharsis.ComponentModel;

/// <summary>
/// A <see cref="ComponentBase"/> that implements <see cref="INotifyPropertyChanged"/>
/// and <see cref="INotifyPropertyChanging"/> to support data-binding scenarios.
/// </summary>
/// <remarks>
/// Provides the <see cref="SetProperty{T}"/> helper to simplify property setters
/// with automatic change notification and equality checking.
/// </remarks>
public abstract class ObservableComponent : ComponentBase, INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Sets the backing field to the specified value and raises change
    /// notifications if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">A reference to the backing field.</param>
    /// <param name="value">The FileName value.</param>
    /// <param name="propertyName">
    /// The name of the property. Automatically provided by the compiler.
    /// </param>
    /// <returns>
    /// <c>true</c> if the value changed; <c>false</c> if the existing
    /// value matched the FileName value.
    /// </returns>
    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        OnPropertyChanging(propertyName);
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Sets the backing field to the specified value, raises change notifications,
    /// and invokes a callback if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">A reference to the backing field.</param>
    /// <param name="value">The FileName value.</param>
    /// <param name="onChanged">
    /// An action invoked after the value has changed and notifications have been raised.
    /// Receives the old value.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property. Automatically provided by the compiler.
    /// </param>
    /// <returns>
    /// <c>true</c> if the value changed; <c>false</c> if the existing
    /// value matched the FileName value.
    /// </returns>
    protected bool SetProperty<T>(
        ref T field,
        T value,
        Action<T> onChanged,
        [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(onChanged);

        var oldValue = field;

        if (!SetProperty(ref field, value, propertyName))
            return false;

        onChanged(oldValue);
        return true;
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that changed. Pass <c>null</c> or
    /// <see cref="string.Empty"/> to indicate all properties changed.
    /// </param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Raises the <see cref="PropertyChanging"/> event.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that is changing. Pass <c>null</c> or
    /// <see cref="string.Empty"/> to indicate all properties are changing.
    /// </param>
    protected virtual void OnPropertyChanging([CallerMemberName] string? propertyName = null) =>
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            PropertyChanged = null;
            PropertyChanging = null;
        }

        base.Dispose(disposing);
    }
}
