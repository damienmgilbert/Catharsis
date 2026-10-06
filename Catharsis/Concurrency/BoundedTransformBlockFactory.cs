using System.Threading.Tasks.Dataflow;

namespace Catharsis.Concurrency;

///<summary>
///Produces <see cref="TransformBlock{TInput,TOutput}"/> instances pre-configured with sane ///<see
///cref="ExecutionDataflowBlockOptions.MaxDegreeOfParallelism"/>/<see
///cref="ExecutionDataflowBlockOptions.BoundedCapacity"/> defaults for CPU-bound or I/O-bound work, instead of requiring
///every call site to reason about these settings itself.
///</summary>
public static class BoundedTransformBlockFactory
{
    #region Public methods

    ///<summary>
    ///Creates a block tuned for CPU-bound work: parallelism capped at <see cref="Environment.ProcessorCount"/> (more
    ///workers than cores would only add contention), and a bounded capacity that applies backpressure once several
    ///items per worker are already queued.
    ///</summary>
    ///<typeparam name="TInput">The block's input type.</typeparam>
    ///<typeparam name="TOutput">The block's output type.</typeparam>
    ///<param name="transform">The transform to apply to each item.</param>
    ///<param name="cancellationToken">A token that, when canceled, faults the block.</param>
    ///<exception cref="ArgumentNullException"><paramref name="transform"/> is <c>null</c>.</exception>
    public static TransformBlock<TInput, TOutput> CreateForCpuBoundWork<TInput, TOutput>(Func<TInput, TOutput> transform, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transform);

        return new TransformBlock<TInput, TOutput>(transform, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, BoundedCapacity = Environment.ProcessorCount * 4, CancellationToken = cancellationToken });
    }

    ///<summary>
    ///Creates a block tuned for I/O-bound work: parallelism well beyond <see cref="Environment.ProcessorCount"/> (I/O-
    ///bound tasks spend most of their time waiting on a completion, not consuming a core) and a proportionally larger
    ///bounded capacity.
    ///</summary>
    ///<typeparam name="TInput">The block's input type.</typeparam>
    ///<typeparam name="TOutput">The block's output type.</typeparam>
    ///<param name="transform">The asynchronous transform to apply to each item.</param>
    ///<param name="maxDegreeOfParallelism">The maximum number of items processed concurrently. Defaults to 32.</param>
    ///<param name="cancellationToken">A token that, when canceled, faults the block.</param>
    ///<exception cref="ArgumentNullException"><paramref name="transform"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentOutOfRangeException"><paramref name="maxDegreeOfParallelism"/> is less than 1.</exception>
    public static TransformBlock<TInput, TOutput> CreateForIoBoundWork<TInput, TOutput>(Func<TInput, Task<TOutput>> transform, int maxDegreeOfParallelism = 32, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transform);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        return new TransformBlock<TInput, TOutput>(transform, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, BoundedCapacity = maxDegreeOfParallelism * 4, CancellationToken = cancellationToken });
    }
    #endregion
}
