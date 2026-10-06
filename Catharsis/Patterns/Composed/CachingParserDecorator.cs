using System.Buffers;
using Catharsis.Buffers;
using Catharsis.DataStructures;
using Catharsis.Services;
using Microsoft.Extensions.Logging;

namespace Catharsis.Patterns.Composed;

///<summary>
///Decorates an <see cref="ISequenceParser"/> with a bounded, least-recently-used cache keyed by the <em>content</em> of
///the input, so parsing the same bytes again skips the inner parser. Only worthwhile when the same inputs recur (for
///example repeated protocol headers) and the inner parser is expensive.
///</summary>
///<remarks>
public sealed partial class CachingParserDecorator : ISequenceParser
{
    #region Fields
    private readonly LruCache<ByteKey, CachedResult> _cache;
    private readonly Lock _gate = new();
    private readonly ISequenceParser _inner;
    private readonly ILogger? _logger;
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new <see cref="CachingParserDecorator"/>.
    ///</summary>
    ///<param name="inner">The parser to decorate.</param>
    ///<param name="capacity">How many distinct inputs to remember. Must be positive.</param>
    ///<param name="logger">Receives hit/miss entries, or <c>null</c> to log nothing.</param>
    ///<exception cref="ArgumentNullException"><paramref name="inner"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is not positive.</exception>
    public CachingParserDecorator(ISequenceParser inner, int capacity = 128, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _inner = inner;
        _logger = logger;
        _cache = new LruCache<ByteKey, CachedResult>(capacity);
    }
    #endregion

    #region Private methods
    [LoggerMessage(Level = LogLevel.Debug, Message = "Parser cache hit for {Length} bytes")]
    static partial void LogHit(ILogger logger, int length);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Parser cache miss for {Length} bytes")]
    static partial void LogMiss(ILogger logger, int length);
    #endregion

    #region Public methods
    ///<summary>
    ///Resets the inner parser. The cache is kept, because entries are keyed by content and remain valid.
    ///</summary>
    public void Reset()
    {
        lock(_gate)
        {
            _inner.Reset();
        }
    }

    ///<inheritdoc/>
    public SequenceParseStatus TryParse(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
    {
        if(sequence.Length > MaxCacheableLength)
        {
            lock(_gate)
            {
                return _inner.TryParse(in sequence, out consumed, out examined);
            }
        }

        ByteKey key = ByteKey.From(sequence);

        lock(_gate)
        {
            if(_cache.TryGetValue(key, out CachedResult cached))
            {
                Hits++;
                consumed = sequence.GetPosition(cached.ConsumedOffset);
                examined = sequence.GetPosition(cached.ExaminedOffset);

                if(_logger is not null)
                {
                    LogHit(_logger, (int)sequence.Length);
                }

                return cached.Status;
            }

            Misses++;

            if(_logger is not null)
            {
                LogMiss(_logger, (int)sequence.Length);
            }

            SequenceParseStatus status = _inner.TryParse(in sequence, out consumed, out examined);

            if(status != SequenceParseStatus.Cancelled)
            {
                _cache.AddOrUpdate(key, new CachedResult(status, sequence.Slice(sequence.Start, consumed).Length, sequence.Slice(sequence.Start, examined).Length));
            }

            return status;
        }
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets how many calls were answered from the cache.
    ///</summary>
    public long Hits { get; private set; }

        ///<summary>
///Gets the longest input, in bytes, that is eligible for caching.
///</summary>
    public static int MaxCacheableLength => 4096;

    ///<summary>
    ///Gets how many calls had to run the inner parser (excluding oversized inputs).
    ///</summary>
    public long Misses { get; private set; }
    #endregion

    private readonly record struct CachedResult(SequenceParseStatus Status, long ConsumedOffset, long ExaminedOffset);

    private readonly struct ByteKey : IEquatable<ByteKey>
    {
        #region Struct fields
        private readonly byte[] _bytes;
        private readonly int _hash;
        #endregion

        #region Constructors
        private ByteKey(byte[] bytes, int hash)
        {
            _bytes = bytes;
            _hash = hash;
        }
        #endregion

        #region Public methods
        public bool Equals(ByteKey other) => _bytes.AsSpan().SequenceEqual(other._bytes);

        public override bool Equals(object? obj) => obj is ByteKey other && Equals(other);

        public static ByteKey From(in ReadOnlySequence<byte> sequence)
        {
            byte[] bytes = sequence.ToArray();
            HashCode hash = new();
            hash.AddBytes(bytes);

            return new ByteKey(bytes, hash.ToHashCode());
        }

        public override int GetHashCode() => _hash;
        #endregion
    }
}
