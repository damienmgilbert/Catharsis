using Catharsis.Buffers;
using Catharsis.Common;
using CommunityToolkit.Diagnostics;

namespace Catharsis.Patterns;

///<summary>
///Demonstrates a high-performance binary processing pipeline that chains multiple transformation stages using pooled
///buffers and span-based I/O.
///</summary>
public sealed class BinaryProcessingPipeline : IDisposable
{
    #region Fields
    bool _disposed;
    readonly List<Func<ReadOnlySpan<byte>, PooledBuffer<byte>>> _stages = [];
    #endregion

    #region Public methods
    ///<summary>
    ///Adds a transformation stage to the pipeline.
    ///</summary>
    ///<param name="stage">A function that transforms input bytes into a pooled output buffer.</param>
    ///<returns>This pipeline for fluent chaining.</returns>
    public BinaryProcessingPipeline AddStage(Func<ReadOnlySpan<byte>, PooledBuffer<byte>> stage)
    {
        Guard.IsNotNull(stage);
        _stages.Add(stage);
        return this;
    }

    ///<summary>
    ///Creates a sample byte reversal stage.
    ///</summary>
    ///<returns>A stage function.</returns>
    public static Func<ReadOnlySpan<byte>, PooledBuffer<byte>> CreateReverseStage()
    {
        return(ReadOnlySpan<byte> input) =>
        {
            PooledBuffer<byte> output = new PooledBuffer<byte>(input.Length);
            Span<byte> span = output.GetSpan(input.Length);
            for(int i = 0; i < input.Length; i++)
            {
                span[i] = input[input.Length - 1 - i];
            }

            output.Advance(input.Length);
            return output;
        };
    }

    ///<summary>
    ///Creates a sample XOR obfuscation stage that XORs each byte with a key.
    ///</summary>
    ///<param name="key">The XOR key byte.</param>
    ///<returns>A stage function.</returns>
    public static Func<ReadOnlySpan<byte>, PooledBuffer<byte>> CreateXorStage(byte key)
    {
        return(ReadOnlySpan<byte> input) =>
        {
            PooledBuffer<byte> output = new PooledBuffer<byte>(input.Length);
            Span<byte> span = output.GetSpan(input.Length);
            for(int i = 0; i < input.Length; i++)
            {
                span[i] = (byte)(input[i] ^ key);
            }

            output.Advance(input.Length);
            return output;
        };
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        _disposed = true;
        _stages.Clear();
    }

    ///<summary>
    ///Executes all stages sequentially, passing each stage's output as input to the next. Uses <see
    ///cref="ValueStopwatch"/> to measure total pipeline execution time.
    ///</summary>
    ///<param name="input">The initial input data.</param>
    ///<param name="elapsedMs">The total pipeline execution time in milliseconds.</param>
    ///<returns>The final output as a byte array.</returns>
    public byte[] Execute(ReadOnlySpan<byte> input, out double elapsedMs)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ValueStopwatch stopwatch = ValueStopwatch.StartNew();
        byte[] current = input.ToArray();

        foreach(Func<ReadOnlySpan<byte>, PooledBuffer<byte>> stage in _stages)
        {
            using PooledBuffer<byte> output = stage(current);
            current = output.WrittenSpan.ToArray();
        }

        elapsedMs = stopwatch.GetElapsedMilliseconds();
        return current;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of processing stages in the pipeline.
    ///</summary>
    public int StageCount => _stages.Count;
    #endregion
}
