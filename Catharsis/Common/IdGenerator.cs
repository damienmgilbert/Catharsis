namespace Catharsis.Common;

///<summary>
///Generates 64-bit identifiers that are strictly increasing across calls on the same instance, without depending on
///<see cref="Guid"/> or a database sequence. Each identifier packs a millisecond timestamp (relative to a
///configurable epoch) into the high bits and a per-millisecond sequence number into the low bits, in the spirit of
///Twitter's Snowflake scheme.
///</summary>
///<param name="epoch">The reference point that timestamps are measured from, or <c>null</c> to use 2020-01-01 UTC.</param>
public sealed class IdGenerator(DateTimeOffset? epoch = null)
{
    #region Fields
    const int SequenceBits = 12;
    const long MaxSequence = (1L << SequenceBits) - 1;

    readonly long _epochMilliseconds = (epoch ?? new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)).ToUnixTimeMilliseconds();
    readonly Lock _lock = new();
    long _lastTimestamp = -1;
    long _sequence;
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
