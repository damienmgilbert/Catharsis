using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Channels;

namespace Catharsis.Advanced;

///<summary>
///Pumps data written to a <see cref="System.IO.Pipelines.Pipe"/>'s <see cref="PipeWriter"/> into a ///<see
///cref="MemoryBackedChannel{T}"/> of pooled segments, bridging pipeline-based producers into channel-based consumers
///while preserving backpressure: the pump only reads more from the pipe once the channel has room for another segment.
///</summary>
public sealed class PipelineChannelBridge : IAsyncDisposable
{
    #region Fields
    private readonly MemoryBackedChannel<byte> _channel;
    private bool _disposed;
    private readonly Pipe _pipe = new();
    private readonly Task _pumpTask;
    private readonly CancellationTokenSource _stoppingSource = new();
    #endregion

    #region Constructors
    ///<summary>
    ///Initializes a new bridge and immediately starts pumping from the pipe into the channel.
    ///</summary>
    ///<param name="channelCapacity">The channel's maximum number of buffered segments. Use 0 for unbounded.</param>
    public PipelineChannelBridge(int channelCapacity = 0)
    {
        _channel = new MemoryBackedChannel<byte>(channelCapacity);
        _pumpTask = PumpAsync(_stoppingSource.Token);
    }
    #endregion

    #region Private methods
    private async Task PumpAsync(CancellationToken stoppingToken)
    {
        try
        {
            while(true)
            {
                ReadResult result = await _pipe.Reader.ReadAsync(stoppingToken).ConfigureAwait(false);
                ReadOnlySequence<byte> buffer = result.Buffer;

                foreach(ReadOnlyMemory<byte> segment in buffer)
                {
                    await _channel.WriteAsync(segment, stoppingToken).ConfigureAwait(false);
                }

                _pipe.Reader.AdvanceTo(buffer.End);

                if(result.IsCompleted)
                {
                    break;
                }
            }
        } catch(OperationCanceledException)
        {
        } finally
        {
            await _pipe.Reader.CompleteAsync().ConfigureAwait(false);
            _channel.Complete();
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Stops the pump, completes the pipe reader, and disposes the channel.
    ///</summary>
    public async ValueTask DisposeAsync()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;

        await _stoppingSource.CancelAsync().ConfigureAwait(false);

        try
        {
            await _pumpTask.ConfigureAwait(false);
        } catch(OperationCanceledException)
        {
        }

        _stoppingSource.Dispose();
        _channel.Dispose();
    }
    #endregion

    #region Public properties
    ///<summary>
    ///The channel's reader side, yielding pooled segments as they are pumped from the pipe.
    ///</summary>
    public ChannelReader<MemoryBackedChannel<byte>.OwnedSegment> Reader => _channel.Reader;

    ///<summary>
    ///The pipe's writer side. Complete this when no more data will be written.
    ///</summary>
    public PipeWriter Writer => _pipe.Writer;
    #endregion
}
