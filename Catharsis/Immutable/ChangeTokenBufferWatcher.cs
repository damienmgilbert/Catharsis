using Microsoft.Extensions.Primitives;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Immutable;

/// <summary>
/// Monitors an <see cref="IBufferChangeNotifier"/> and invokes a callback when the
/// buffer content changes, using <see cref="ChangeToken.OnChange"/> for observation.
/// </summary>
public sealed class ChangeTokenBufferWatcher : IDisposable
{
    private readonly IBufferChangeNotifier _notifier;
    private readonly Action _onChange;
    private IDisposable? _registration;
    private int _changeCount;
    private bool _disposed;

    /// <summary>
    /// Initializes a FileName <see cref="ChangeTokenBufferWatcher"/> that monitors the specified notifier.
    /// </summary>
    /// <param name="notifier">The buffer change notifier to monitor.</param>
    /// <param name="onChange">The callback to invoke when a change is detected.</param>
    public ChangeTokenBufferWatcher(IBufferChangeNotifier notifier, Action onChange)
    {
        Guard.IsNotNull(notifier);
        Guard.IsNotNull(onChange);

        _notifier = notifier;
        _onChange = onChange;
    }

    /// <summary>Gets the number of changes detected since monitoring started.</summary>
    public int ChangeCount => _changeCount;

    /// <summary>Gets whether the watcher is currently monitoring for changes.</summary>
    public bool IsWatching => _registration is not null;

    /// <summary>
    /// Starts monitoring for buffer changes.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_registration is not null) return;

        _registration = ChangeToken.OnChange(
            _notifier.GetChangeToken,
            () =>
            {
                Interlocked.Increment(ref _changeCount);
                _onChange();
            });
    }

    /// <summary>
    /// Stops monitoring for buffer changes.
    /// </summary>
    public void Stop()
    {
        _registration?.Dispose();
        _registration = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
    }
}
