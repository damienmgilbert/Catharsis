using System.ComponentModel;
using System.Globalization;

namespace Catharsis.ComponentModel.Binding;

/// <summary>
/// Provides two-way property binding between objects that implement
/// <see cref="INotifyPropertyChanged"/>. When a bound property changes on
/// the source, the target property is automatically updated, and vice versa.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="Bind"/> to establish a two-way binding, or
/// <see cref="BindOneWay"/> for a one-way source-to-target binding.
/// Call <see cref="Unbind"/> or <see cref="UnbindAll"/> to remove bindings.
/// </para>
/// <para>
/// Property values are transferred using <see cref="TypeDescriptor"/> property
/// descriptors and <see cref="TypeConverter"/> for type coercion when the source
/// and target property types differ.
/// </para>
/// </remarks>
public sealed class ComponentModelBinder : IDisposable
{
    private readonly List<BindingEntry> _bindings = [];
    private bool _disposed;
    private bool _isSynchronizing;

    /// <summary>
    /// Gets the number of active bindings.
    /// </summary>
    public int Count => _bindings.Count;

    /// <summary>
    /// Gets or sets the culture used for type conversion. Defaults to
    /// <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Establishes a two-way binding between a source property and a target property.
    /// Changes to either property are propagated to the other.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="sourceProperty">The source property name.</param>
    /// <param name="target">The target object.</param>
    /// <param name="targetProperty">The target property name.</param>
    /// <exception cref="ArgumentNullException">
    /// Any argument is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// A specified property does not exist on its respective object.
    /// </exception>
    public void Bind(
        INotifyPropertyChanged source,
        string sourceProperty,
        INotifyPropertyChanged target,
        string targetProperty)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceProperty);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetProperty);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var sourceDescriptor = GetPropertyDescriptor(source, sourceProperty);
        var targetDescriptor = GetPropertyDescriptor(target, targetProperty);

        var entry = new BindingEntry(
            source, sourceDescriptor, target, targetDescriptor, isTwoWay: true);

        source.PropertyChanged += entry.OnSourceChanged;
        target.PropertyChanged += entry.OnTargetChanged;

        entry.Synchronize = (s, e) => SynchronizeProperty(entry, s, e);

        _bindings.Add(entry);

        // Initial sync: source -> target
        TransferValue(sourceDescriptor, source, targetDescriptor, target);
    }

    /// <summary>
    /// Establishes a one-way binding from source to target. Changes to the
    /// source property are propagated to the target, but not vice versa.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="sourceProperty">The source property name.</param>
    /// <param name="target">The target object.</param>
    /// <param name="targetProperty">The target property name.</param>
    /// <exception cref="ArgumentNullException">
    /// Any argument is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// A specified property does not exist on its respective object.
    /// </exception>
    public void BindOneWay(
        INotifyPropertyChanged source,
        string sourceProperty,
        object target,
        string targetProperty)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceProperty);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetProperty);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var sourceDescriptor = GetPropertyDescriptor(source, sourceProperty);
        var targetDescriptor = GetPropertyDescriptor(target, targetProperty);

        var entry = new BindingEntry(
            source, sourceDescriptor, target, targetDescriptor, isTwoWay: false);

        source.PropertyChanged += entry.OnSourceChanged;

        entry.Synchronize = (s, e) => SynchronizeProperty(entry, s, e);

        _bindings.Add(entry);

        // Initial sync: source -> target
        TransferValue(sourceDescriptor, source, targetDescriptor, target);
    }

    /// <summary>
    /// Removes all bindings involving the specified source and target objects.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    public void Unbind(object source, object target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        for (var i = _bindings.Count - 1; i >= 0; i--)
        {
            var entry = _bindings[i];

            if (ReferenceEquals(entry.Source, source) && ReferenceEquals(entry.Target, target))
            {
                DetachEntry(entry);
                _bindings.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Removes all active bindings.
    /// </summary>
    public void UnbindAll()
    {
        foreach (var entry in _bindings)
            DetachEntry(entry);

        _bindings.Clear();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        UnbindAll();
    }

    private void SynchronizeProperty(BindingEntry entry, object sender, PropertyChangedEventArgs e)
    {
        if (_isSynchronizing)
            return;

        _isSynchronizing = true;

        try
        {
            if (ReferenceEquals(sender, entry.Source) &&
                string.Equals(e.PropertyName, entry.SourceDescriptor.Name, StringComparison.Ordinal))
            {
                TransferValue(entry.SourceDescriptor, entry.Source, entry.TargetDescriptor, entry.Target);
            }
            else if (entry.IsTwoWay &&
                     ReferenceEquals(sender, entry.Target) &&
                     string.Equals(e.PropertyName, entry.TargetDescriptor.Name, StringComparison.Ordinal))
            {
                TransferValue(entry.TargetDescriptor, entry.Target, entry.SourceDescriptor, entry.Source);
            }
        }
        finally
        {
            _isSynchronizing = false;
        }
    }

    private void TransferValue(
        PropertyDescriptor fromProp, object fromObj,
        PropertyDescriptor toProp, object toObj)
    {
        if (toProp.IsReadOnly)
            return;

        var value = fromProp.GetValue(fromObj);

        if (value is null || toProp.PropertyType.IsInstanceOfType(value))
        {
            toProp.SetValue(toObj, value);
            return;
        }

        var converter = toProp.Converter;

        if (converter.CanConvertFrom(value.GetType()))
        {
            var converted = converter.ConvertFrom(null, Culture, value);
            toProp.SetValue(toObj, converted);
        }
    }

    private static PropertyDescriptor GetPropertyDescriptor(object obj, string propertyName)
    {
        var descriptor = TypeDescriptor.GetProperties(obj)[propertyName];

        return descriptor ?? throw new ArgumentException(
            $"Property '{propertyName}' not found on type '{obj.GetType().Name}'.",
            propertyName);
    }

    private static void DetachEntry(BindingEntry entry)
    {
        if (entry.Source is INotifyPropertyChanged sourceNpc)
            sourceNpc.PropertyChanged -= entry.OnSourceChanged;

        if (entry.IsTwoWay && entry.Target is INotifyPropertyChanged targetNpc)
            targetNpc.PropertyChanged -= entry.OnTargetChanged;
    }

    private sealed class BindingEntry(
        object source,
        PropertyDescriptor sourceDescriptor,
        object target,
        PropertyDescriptor targetDescriptor,
        bool isTwoWay)
    {
        public object Source { get; } = source;
        public PropertyDescriptor SourceDescriptor { get; } = sourceDescriptor;
        public object Target { get; } = target;
        public PropertyDescriptor TargetDescriptor { get; } = targetDescriptor;
        public bool IsTwoWay { get; } = isTwoWay;
        public Action<object, PropertyChangedEventArgs>? Synchronize { get; set; }

        public void OnSourceChanged(object? sender, PropertyChangedEventArgs e) =>
            Synchronize?.Invoke(Source, e);

        public void OnTargetChanged(object? sender, PropertyChangedEventArgs e) =>
            Synchronize?.Invoke(Target, e);
    }
}
