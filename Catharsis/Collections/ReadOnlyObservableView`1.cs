using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Catharsis.Collections;

///<summary>
///A read-only view over an <see cref="ObservableCollection{T}"/> that forwards change notifications until
///explicitly <see cref="Freeze"/>d, at which point it stops observing the source and exposes a fixed snapshot of
///its contents at that moment.
///</summary>
///<typeparam name="T">The type of elements in the collection.</typeparam>
public sealed class ReadOnlyObservableView<T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    #region Fields
    readonly ObservableCollection<T> _source;
    IReadOnlyList<T>? _frozenSnapshot;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new view over <paramref name="source"/> and begins forwarding its change notifications.
    ///</summary>
    ///<param name="source">The observable collection to view.</param>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public ReadOnlyObservableView(ObservableCollection<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _source = source;
        _source.CollectionChanged += OnSourceCollectionChanged;
        ((INotifyPropertyChanged)_source).PropertyChanged += OnSourcePropertyChanged;
    }
    #endregion

    #region Private methods
    void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);
    void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
    #endregion

    #region Public methods
    ///<summary>
    ///Stops observing the source collection and fixes this view's contents to a snapshot of the source as it is
    ///right now. Idempotent: calling this more than once has no additional effect.
    ///</summary>
    public void Freeze()
    {
        if(IsFrozen)
        {
            return;
        }

        _frozenSnapshot = [.. _source];
        _source.CollectionChanged -= OnSourceCollectionChanged;
        ((INotifyPropertyChanged)_source).PropertyChanged -= OnSourcePropertyChanged;
    }

    ///<inheritdoc/>
    public IEnumerator<T> GetEnumerator() => (_frozenSnapshot ?? (IReadOnlyList<T>)_source).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Public properties
    ///<inheritdoc/>
    public int Count => (_frozenSnapshot ?? (IReadOnlyList<T>)_source).Count;

    ///<inheritdoc/>
    public T this[int index] => (_frozenSnapshot ?? (IReadOnlyList<T>)_source)[index];

    ///<summary>
    ///Whether <see cref="Freeze"/> has been called, meaning this view no longer reflects changes to the source.
    ///</summary>
    public bool IsFrozen => _frozenSnapshot is not null;
    #endregion

    #region Public events
    ///<inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    ///<inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;
    #endregion
}
