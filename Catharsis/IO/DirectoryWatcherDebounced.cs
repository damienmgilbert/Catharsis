using System.Collections.Concurrent;

namespace Catharsis.IO;

///<summary>
///Wraps a <see cref="FileSystemWatcher"/> and coalesces bursts of events for the same path into a single
///<see cref="Changed"/> notification, raised only once that path has stopped changing for the configured delay.
///This tames the notoriously bursty raw events a <see cref="FileSystemWatcher"/> raises for a single logical save
///(e.g. several <c>Changed</c> events as an editor writes a file in chunks).
///</summary>
///<param name="path">The directory to watch.</param>
///<param name="debounceDelay">How long a path must stay quiet before <see cref="Changed"/> fires for it.</param>
///<param name="filter">The file name filter passed to the underlying <see cref="FileSystemWatcher"/>.</param>
///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace.</exception>
public sealed class DirectoryWatcherDebounced(string path, TimeSpan debounceDelay, string filter = "*.*") : IDisposable
{
    #region Fields
    readonly ConcurrentDictionary<string, CancellationTokenSource> _pending = new();
    readonly FileSystemWatcher _watcher = CreateWatcher(path, filter);
    bool _disposed;
    #endregion

    #region Constructors
    static FileSystemWatcher CreateWatcher(string path, string filter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new FileSystemWatcher(path, filter) { EnableRaisingEvents = false };
    }
    #endregion

    #region Events
    ///<summary>
    ///Raised once a changed path has stopped changing for the configured debounce delay.
    ///</summary>
    public event EventHandler<FileSystemEventArgs>? Changed;
    #endregion

    #region Private methods
    async Task DebounceAsync(FileSystemEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(debounceDelay, cancellationToken).ConfigureAwait(false);
            _pending.TryRemove(args.FullPath, out _);
            Changed?.Invoke(this, args);
        }
        catch (OperationCanceledException)
        {
        }
    }

    void OnRawEvent(object sender, FileSystemEventArgs args)
    {
        CancellationTokenSource newSource = new();

        _pending.AddOrUpdate(args.FullPath, newSource, (_, existing) =>
        {
            existing.Cancel();
            existing.Dispose();
            return newSource;
        });

        _ = DebounceAsync(args, newSource.Token);
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Cancels every pending debounce, stops watching, and releases the underlying <see cref="FileSystemWatcher"/>.
    ///</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _watcher.EnableRaisingEvents = false;
        _watcher.Changed -= OnRawEvent;
        _watcher.Created -= OnRawEvent;
        _watcher.Deleted -= OnRawEvent;
        _watcher.Renamed -= OnRawEvent;
        _watcher.Dispose();

        foreach (CancellationTokenSource source in _pending.Values)
        {
            source.Cancel();
            source.Dispose();
        }

        _pending.Clear();
    }

    ///<summary>
    ///Starts watching and raising debounced <see cref="Changed"/> notifications.
    ///</summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _watcher.Changed += OnRawEvent;
        _watcher.Created += OnRawEvent;
        _watcher.Deleted += OnRawEvent;
        _watcher.Renamed += OnRawEvent;
        _watcher.EnableRaisingEvents = true;
    }
    #endregion
}
