using System.ComponentModel;

namespace Catharsis.ComponentModel;

/// <summary>
/// Subscribes to <see cref="INotifyPropertyChanged.PropertyChanged"/> events
/// and routes individual property changes to strongly-typed callbacks.
/// Supports fluent registration and automatic unsubscription via <see cref="IDisposable"/>.
/// </summary>
public sealed class PropertyObserver : IDisposable
{
    private readonly INotifyPropertyChanged _source;
    private readonly Dictionary<string, List<Action>> _handlers = new(StringComparer.Ordinal);
    private bool _disposed;

    /// <summary>
    /// Initializes a FileName <see cref="PropertyObserver"/> that listens to the specified source.
    /// </summary>
    /// <param name="source">The object whose property changes to observe.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public PropertyObserver(INotifyPropertyChanged source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _source.PropertyChanged += OnPropertyChanged;
    }

    /// <summary>
    /// Registers a callback to be invoked when the specified property changes.
    /// </summary>
    /// <param name="propertyName">The name of the property to observe.</param>
    /// <param name="handler">The callback to invoke when the property changes.</param>
    /// <returns>This instance, for fluent chaining.</returns>
    /// <exception cref="ArgumentException"><paramref name="propertyName"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
    public PropertyObserver OnChanged(string propertyName, Action handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentNullException.ThrowIfNull(handler);

        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_handlers.TryGetValue(propertyName, out var list))
        {
            list = [];
            _handlers[propertyName] = list;
        }

        list.Add(handler);
        return this;
    }

    /// <summary>
    /// Removes all registered callbacks for the specified property.
    /// </summary>
    /// <param name="propertyName">The property name to stop observing.</param>
    /// <returns>This instance, for fluent chaining.</returns>
    public PropertyObserver StopObserving(string propertyName)
    {
        _handlers.Remove(propertyName);
        return this;
    }

    /// <summary>
    /// Removes all registered callbacks.
    /// </summary>
    /// <returns>This instance, for fluent chaining.</returns>
    public PropertyObserver StopAll()
    {
        _handlers.Clear();
        return this;
    }

    /// <summary>
    /// Unsubscribes from the source's <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// event and releases all handlers.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _source.PropertyChanged -= OnPropertyChanged;
        _handlers.Clear();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null)
        {
            // null or empty property name means all properties changed
            foreach (var list in _handlers.Values)
            {
                foreach (var handler in list)
                    handler();
            }
        }
        else if (_handlers.TryGetValue(e.PropertyName, out var list))
        {
            foreach (var handler in list)
                handler();
        }
    }
}
