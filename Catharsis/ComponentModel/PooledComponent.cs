using System.ComponentModel;

namespace Catharsis.ComponentModel;

/// <summary>
/// A <see cref="ComponentBase"/> designed for object-pooling scenarios.
/// Implements a reset-based lifecycle that allows components to be returned
/// to a pool and reused without reallocation.
/// </summary>
/// <remarks>
/// <para>
/// When a component is returned to the pool, <see cref="Reset"/> is called
/// to restore it to a clean state. Subclasses must override
/// <see cref="OnReset"/> to clear instance-specific state.
/// </para>
/// <para>
/// The <see cref="IsActive"/> property indicates whether the component is
/// currently leased from the pool.
/// </para>
/// </remarks>
public abstract class PooledComponent : ComponentBase
{
    private int _leaseVersion;

    /// <summary>
    /// Gets a value indicating whether the component is currently
    /// active (leased from the pool).
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the lease version, which is incremented each time the
    /// component is activated. Useful for detecting stale references.
    /// </summary>
    public int LeaseVersion => _leaseVersion;

    /// <summary>
    /// Activates the component for use after being retrieved from a pool.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The component is already active.
    /// </exception>
    public void Activate()
    {
        ThrowIfDisposed();

        if (IsActive)
            throw new InvalidOperationException("The component is already active.");

        _leaseVersion++;
        IsActive = true;
        OnActivate();
    }

    /// <summary>
    /// Resets the component to a clean state so it can be returned to a pool.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The component is not currently active.
    /// </exception>
    public void Reset()
    {
        ThrowIfDisposed();

        if (!IsActive)
            throw new InvalidOperationException("The component is not active.");

        OnReset();
        IsActive = false;
    }

    /// <summary>
    /// Called when the component is activated for a new lease.
    /// Override to perform initialization logic.
    /// </summary>
    protected virtual void OnActivate() { }

    /// <summary>
    /// Called when the component is being reset for pool return.
    /// Override to clear instance-specific state. The base implementation
    /// does nothing.
    /// </summary>
    protected virtual void OnReset() { }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing && IsActive)
        {
            OnReset();
            IsActive = false;
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if the component
    /// is not currently active.
    /// </summary>
    protected void ThrowIfNotActive()
    {
        ThrowIfDisposed();

        if (!IsActive)
            throw new InvalidOperationException("The component is not active.");
    }
}
