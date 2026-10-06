using System.ComponentModel;

namespace Catharsis.ComponentModel.Binding;

///<summary>
///Batches <see cref="INotifyPropertyChanged.PropertyChanged"/> notifications from a source object, collecting all
///distinct property names and raising a single notification per property when <see cref="Flush"/> is called or the
///batcher is disposed.
///</summary>
///<remarks>
public sealed class PropertyChangeBatcher : IDisposable
{
    #region Fields
    private int _batchDepth;
    private bool _disposed;
    private readonly HashSet<string> _pending = [ with(StringComparer.Ordinal) ];
    private readonly Action<string> _raisePropertyChanged;
    private readonly INotifyPropertyChanged _source;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new instance of <see cref="PropertyChangeBatcher"/>.
    ///</summary>
    ///<param name="source">
    ///The source object whose property change events are batched.
    ///</param>
    ///<param name="raisePropertyChanged">
    ///A callback that raises the batched property changed notification for a single property name.
    ///</param>
    ///<exception cref="ArgumentNullException">
    public PropertyChangeBatcher(INotifyPropertyChanged source, Action<string> raisePropertyChanged)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(raisePropertyChanged);

        _source = source;
        _raisePropertyChanged = raisePropertyChanged;
        _source.PropertyChanged += OnSourcePropertyChanged;
    }
    #endregion

    #region Private methods
    private void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(_disposed)
        {
            return;
        }

        string name = e.PropertyName ?? string.Empty;

        if(_batchDepth > 0)
        {
            _pending.Add(name);
        } else
        {
            _raisePropertyChanged(name);
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Begins a batching window. While batching is active, incoming property change notifications are accumulated rather
    ///than immediately forwarded.
    ///</summary>
    public void BeginBatch() => Interlocked.Increment(ref _batchDepth);

    ///<summary>
    ///Creates a disposable scope that calls <see cref="BeginBatch"/> on creation and <see cref="EndBatch"/> on
    ///disposal.
    ///</summary>
    ///<returns>A <see cref="PropertyChangeScope"/> wrapping a batch window.</returns>
    public PropertyChangeScope CreateScope()
    {
        BeginBatch();
        return new PropertyChangeScope(
               name =>
               {
                   _pending.Add(name);
                   EndBatch();
               });
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        _source.PropertyChanged -= OnSourcePropertyChanged;
        Flush();
    }

    ///<summary>
    ///Ends a batching window. If this is the outermost batch, <see cref="Flush"/> is called automatically.
    ///</summary>
    ///<exception cref="InvalidOperationException">
    public void EndBatch()
    {
        if(Interlocked.Decrement(ref _batchDepth) < 0)
        {
            Interlocked.Increment(ref _batchDepth);
            throw new InvalidOperationException("EndBatch was called without a matching BeginBatch.");
        }

        if(_batchDepth == 0)
        {
            Flush();
        }
    }

    ///<summary>
    ///Flushes all pending property change notifications, raising one notification per unique property name.
    ///</summary>
    public void Flush()
    {
        if(_pending.Count == 0)
        {
            return;
        }

        string[] names = [ .. _pending ];
        _pending.Clear();

        foreach(string name in names)
        {
            _raisePropertyChanged(name);
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets a value indicating whether the batcher is actively batching (i.e., <see cref="BeginBatch"/> has been called
    ///without a matching <see cref="EndBatch"/>).
    ///</summary>
    public bool IsBatching => _batchDepth > 0;

    ///<summary>
    ///Gets the number of unique pending property names.
    ///</summary>
    public int PendingCount => _pending.Count;
    #endregion
}
