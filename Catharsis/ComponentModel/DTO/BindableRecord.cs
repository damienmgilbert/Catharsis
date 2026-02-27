using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catharsis.ComponentModel.DTO;

/// <summary>
/// A record-based wrapper that adds <see cref="INotifyPropertyChanged"/> and
/// <see cref="INotifyPropertyChanging"/> support around an immutable
/// <typeparamref name="T"/> value, enabling data binding with records.
/// </summary>
/// <typeparam name="T">The record type to wrap.</typeparam>
/// <remarks>
/// <para>
/// Setting <see cref="Value"/> replaces the entire record and raises
/// property change notifications. Use <see cref="Update"/> to apply a
/// transformation function to the current value.
/// </para>
/// </remarks>
public class BindableRecord<T> : INotifyPropertyChanged, INotifyPropertyChanging
    where T : class
{
    private T _value;

    /// <summary>
    /// Initializes a new instance of <see cref="BindableRecord{T}"/>.
    /// </summary>
    /// <param name="value">The initial record value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <c>null</c>.
    /// </exception>
    public BindableRecord(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Gets or sets the underlying record value. Setting this property
    /// raises change notifications for <see cref="Value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// The value being set is <c>null</c>.
    /// </exception>
    public T Value
    {
        get => _value;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(_value, value))
                return;

            OnPropertyChanging();
            _value = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Applies a transformation function to the current value and sets the
    /// result as the new value.
    /// </summary>
    /// <param name="transform">
    /// A function that receives the current record and returns a new record.
    /// </param>
    /// <returns>The new record value.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="transform"/> is <c>null</c>.
    /// </exception>
    public T Update(Func<T, T> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);

        var newValue = transform(_value);
        Value = newValue;
        return newValue;
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Raises the <see cref="PropertyChanging"/> event.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    protected virtual void OnPropertyChanging([CallerMemberName] string? propertyName = null) =>
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

    /// <inheritdoc />
    public override string ToString() => _value.ToString() ?? string.Empty;
}
