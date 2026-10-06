namespace Catharsis.Concurrency;

///<summary>
///An async-friendly cyclic barrier, analogous to <see cref="Barrier"/> but awaitable without blocking a thread.
///Once every participant has called <see cref="SignalAndWaitAsync"/> for the current phase, all of them are
///released together and the barrier automatically resets for the next phase.
///</summary>
public sealed class AsyncBarrier
{
    #region Fields
    readonly Lock _gate = new();
    readonly int _participantCount;
    int _remaining;
    TaskCompletionSource _phase = new(TaskCreationOptions.RunContinuationsAsynchronously);
    #endregion

    #region Public methods
    ///<summary>
    ///Creates a barrier for the specified number of participants.
    ///</summary>
    ///<param name="participantCount">The number of participants that must signal before a phase completes.</param>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="participantCount"/> is less than 1.</exception>
    public AsyncBarrier(int participantCount)
    {
        if(participantCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(participantCount), "Participant count must be at least 1.");
        }

        _participantCount = participantCount;
        _remaining = participantCount;
    }

    ///<summary>
    ///Signals arrival at the current phase and waits for every other participant to arrive. The last participant to
    ///arrive releases everyone and starts the next phase.
    ///</summary>
    ///<param name="cancellationToken">A cancellation token that can abandon the wait.</param>
    ///<returns>A task that completes once every participant has arrived.</returns>
    public Task SignalAndWaitAsync(CancellationToken cancellationToken = default)
    {
        TaskCompletionSource phase;

        lock(_gate)
        {
            phase = _phase;
            _remaining--;

            if(_remaining == 0)
            {
                _remaining = _participantCount;
                _phase = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                phase.TrySetResult();
                return Task.CompletedTask;
            }
        }

        return phase.Task.WaitAsync(cancellationToken);
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of participants configured for this barrier.
    ///</summary>
    public int ParticipantCount => _participantCount;
    #endregion
}
