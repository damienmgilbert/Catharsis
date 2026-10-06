namespace Catharsis.Common;

///<summary>
///Generates 64-bit identifiers that are strictly increasing across calls on the same instance, without depending on
public sealed class IdGenerator(DateTimeOffset? epoch = null)
{
    #region Constants
    private const long MaxSequence = (1L << SequenceBits) - 1;
    private const int SequenceBits = 12;
    #endregion

    #region Fields
    private readonly long _epochMilliseconds = (epoch ?? new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)).ToUnixTimeMilliseconds();
    private long _lastTimestamp = -1;
    private readonly Lock _lock = new();
    private long _sequence;
    #endregion

    #region Public methods
    ///<summary>
    ///Generates the next identifier. Guaranteed to be strictly greater than every identifier previously returned by
    ///this instance.
    ///</summary>
    ///<returns>A new, strictly increasing identifier.</returns>
    public long NextId()
    {
        lock(_lock)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _epochMilliseconds;

            if(timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & MaxSequence;

                if(_sequence == 0)
                {
                    while(timestamp <= _lastTimestamp)
                    {
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _epochMilliseconds;
                    }
                }
            } else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;
            return (timestamp << SequenceBits) | _sequence;
        }
    }
    #endregion
}
