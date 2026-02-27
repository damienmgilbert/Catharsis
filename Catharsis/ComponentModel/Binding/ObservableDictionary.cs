using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Catharsis.ComponentModel.Binding;

/// <summary>
/// A dictionary that implements <see cref="INotifyCollectionChanged"/> and
/// <see cref="INotifyPropertyChanged"/>, raising notifications when entries
/// are added, removed, replaced, or the dictionary is cleared.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
/// <remarks>
/// <para>
/// This class wraps a standard <see cref="Dictionary{TKey, TValue}"/> and
/// raises <see cref="CollectionChanged"/> for each mutation. For bulk
/// updates, use <see cref="SuppressNotifications"/> to defer a single
/// <see cref="NotifyCollectionChangedAction.Reset"/> until the scope is disposed.
/// </para>
/// </remarks>
public class ObservableDictionary<TKey, TValue>
    : IDictionary<TKey, TValue>,
      IReadOnlyDictionary<TKey, TValue>,
      INotifyCollectionChanged,
      INotifyPropertyChanged
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary;
    private int _suppressionCount;

    /// <summary>
    /// Initializes a new instance of <see cref="ObservableDictionary{TKey, TValue}"/>.
    /// </summary>
    public ObservableDictionary()
    {
        _dictionary = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ObservableDictionary{TKey, TValue}"/>
    /// with the specified comparer.
    /// </summary>
    /// <param name="comparer">The key comparer.</param>
    public ObservableDictionary(IEqualityComparer<TKey> comparer)
    {
        _dictionary = new Dictionary<TKey, TValue>(comparer);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ObservableDictionary{TKey, TValue}"/>
    /// with entries copied from the specified dictionary.
    /// </summary>
    /// <param name="dictionary">The source dictionary.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dictionary"/> is <c>null</c>.
    /// </exception>
    public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        _dictionary = new Dictionary<TKey, TValue>(dictionary);
    }

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public int Count => _dictionary.Count;

    /// <inheritdoc />
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    /// <inheritdoc />
    public ICollection<TKey> Keys => _dictionary.Keys;

    /// <inheritdoc />
    public ICollection<TValue> Values => _dictionary.Values;

    /// <inheritdoc />
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys;

    /// <inheritdoc />
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values;

    /// <inheritdoc />
    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set
        {
            if (_dictionary.TryGetValue(key, out var oldValue))
            {
                if (EqualityComparer<TValue>.Default.Equals(oldValue, value))
                    return;

                _dictionary[key] = value;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<TKey, TValue>(key, value),
                    new KeyValuePair<TKey, TValue>(key, oldValue)));
            }
            else
            {
                _dictionary[key] = value;
                OnCountChanged();
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    new KeyValuePair<TKey, TValue>(key, value)));
            }
        }
    }

    /// <summary>
    /// Suppresses change notifications until the returned scope is disposed.
    /// A single <see cref="NotifyCollectionChangedAction.Reset"/> is raised
    /// when the outermost scope ends.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> suppression scope.</returns>
    public IDisposable SuppressNotifications() =>
        new SuppressionScope(this);

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value);
        OnCountChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            new KeyValuePair<TKey, TValue>(key, value)));
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        if (!_dictionary.TryGetValue(key, out var value))
            return false;

        _dictionary.Remove(key);
        OnCountChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove,
            new KeyValuePair<TKey, TValue>(key, value)));
        return true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        if (_dictionary.Count == 0)
            return;

        _dictionary.Clear();
        OnCountChanged();
        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <inheritdoc />
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    /// <inheritdoc />
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) =>
        _dictionary.TryGetValue(key, out value);

    /// <inheritdoc />
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) =>
        Add(item.Key, item.Value);

    /// <inheritdoc />
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) =>
        ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);

    /// <inheritdoc />
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
        ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);

    /// <inheritdoc />
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item))
            return false;

        OnCountChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, item));
        return true;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
        _dictionary.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (_suppressionCount == 0)
            CollectionChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (_suppressionCount == 0)
            PropertyChanged?.Invoke(this, e);
    }

    private void OnCountChanged()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Keys)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Values)));
    }

    private sealed class SuppressionScope : IDisposable
    {
        private readonly ObservableDictionary<TKey, TValue> _owner;
        private bool _disposed;

        public SuppressionScope(ObservableDictionary<TKey, TValue> owner)
        {
            _owner = owner;
            Interlocked.Increment(ref owner._suppressionCount);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (Interlocked.Decrement(ref _owner._suppressionCount) == 0)
            {
                _owner.OnCountChanged();
                _owner.OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}
