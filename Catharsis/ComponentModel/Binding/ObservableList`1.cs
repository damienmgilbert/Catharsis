using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Catharsis.ComponentModel.Binding;

///<summary>
///An observable list that implements <see cref="INotifyCollectionChanged"/>, <see cref="INotifyPropertyChanged"/>, and
///<see cref="IBindingList"/> to support data-binding in UI frameworks.
///</summary>
///<typeparam name="T">The type of elements in the list.</typeparam>
///<remarks>
///<para><see cref="ObservableList{T}"/> extends <see cref="ObservableCollection{T}"/> with batch-add operations, range-
///remove operations, and suppression of change notifications during bulk updates via <see
///cref="SuppressNotifications"/>.</para>
///</remarks>
public class ObservableList<T> : ObservableCollection<T>
{
    #region Fields
    int _suppressionCount;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="ObservableList{T}"/>.
    ///</summary>
    public ObservableList()
    {
    }
    ///<summary>
    ///Initializes a new instance of <see cref="ObservableList{T}"/> with the specified items.
    ///</summary>
    ///<param name="collection">The items to copy into the list.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="collection"/> is <c>null</c>.
    ///</exception>
    public ObservableList(IEnumerable<T> collection) : base(collection)
    {
    }
    #endregion

    #region Protected methods
    ///<inheritdoc/>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if(_suppressionCount == 0)
        {
            base.OnCollectionChanged(e);
        }
    }

    ///<inheritdoc/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if(_suppressionCount == 0)
        {
            base.OnPropertyChanged(e);
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Adds all items from the specified collection. If notifications are not suppressed, a single <see
    ///cref="NotifyCollectionChangedAction.Reset"/> is raised after all items are added.
    ///</summary>
    ///<param name="items">The items to add.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="items"/> is <c>null</c>.
    ///</exception>
    public void AddRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        using(SuppressNotifications())
        {
            foreach(T item in items)
            {
                Items.Add(item);
            }
        }
    }

    ///<summary>
    ///Removes all items matching the specified predicate. Raises a single <see
    ///cref="NotifyCollectionChangedAction.Reset"/> after all removals.
    ///</summary>
    ///<param name="predicate">The condition to match.</param>
    ///<returns>The number of items removed.</returns>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="predicate"/> is <c>null</c>.
    ///</exception>
    public int RemoveAll(Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        int removed = 0;

        using(SuppressNotifications())
        {
            for(int i = Items.Count - 1; i >= 0; i--)
            {
                if(predicate(Items[i]))
                {
                    Items.RemoveAt(i);
                    removed++;
                }
            }
        }

        return removed;
    }

    ///<summary>
    ///Replaces the entire contents of the list with the specified collection. Raises a single <see
    ///cref="NotifyCollectionChangedAction.Reset"/>.
    ///</summary>
    ///<param name="items">The replacement items.</param>
    ///<exception cref="ArgumentNullException">
    ///<paramref name="items"/> is <c>null</c>.
    ///</exception>
    public void ReplaceAll(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        using(SuppressNotifications())
        {
            Items.Clear();

            foreach(T item in items)
            {
                Items.Add(item);
            }
        }
    }

    ///<summary>
    ///Suppresses <see cref="INotifyCollectionChanged.CollectionChanged"/> and <see
    ///cref="INotifyPropertyChanged.PropertyChanged"/> notifications until the returned scope is disposed. Supports
    ///nesting.
    ///</summary>
    ///<returns>
    ///An <see cref="IDisposable"/> that, when disposed, decrements the suppression counter and raises a <see
    ///cref="NotifyCollectionChangedAction.Reset"/> notification if the counter reaches zero.
    ///</returns>
    public IDisposable SuppressNotifications() { return new SuppressionScope(this); }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a value indicating whether change notifications are currently suppressed.
    ///</summary>
    public bool IsNotificationSuppressed => _suppressionCount > 0;
    #endregion

    sealed class SuppressionScope : IDisposable
    {
        #region Fields
        bool _disposed;
        readonly ObservableList<T> _owner;
        #endregion

        #region Constructors
        public SuppressionScope(ObservableList<T> owner)
        {
            _owner = owner;
            Interlocked.Increment(ref owner._suppressionCount);
        }
        #endregion

        #region Public methods
        public void Dispose()
        {
            if(_disposed)
            {
                return;
            }

            _disposed = true;

            if(Interlocked.Decrement(ref _owner._suppressionCount) == 0)
            {
                _owner.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                _owner.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                _owner.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        #endregion
    }
}
