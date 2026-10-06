using System.Threading.Tasks.Dataflow;

namespace Catharsis.Concurrency;

///<summary>
///A fluent builder chaining dataflow blocks (<see cref="BufferBlock{T}"/> → <see
///cref="TransformBlock{TInput,TOutput}"/> → ... → <see cref="ActionBlock{T}"/>), automatically linking each stage to
///the next with ///<see cref="DataflowLinkOptions.PropagateCompletion"/> set, so completing or faulting the head block
///cascades all the way through to the final stage without the caller wiring each link by hand.
///</summary>
///<typeparam name="TInput">The pipeline's original input type.</typeparam>
///<typeparam name="TCurrent">The output type of the last stage added so far.</typeparam>
public sealed class DataflowPipelineBuilder<TInput, TCurrent>
{
    #region Fields
    private readonly ITargetBlock<TInput> _head;
    private readonly ISourceBlock<TCurrent> _tail;
    #endregion

    #region Constructors
    internal DataflowPipelineBuilder(ITargetBlock<TInput> head, ISourceBlock<TCurrent> tail)
    {
        _head = head;
        _tail = tail;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Finishes the pipeline with an <see cref="ActionBlock{T}"/> as its terminal stage.
    ///</summary>
    ///<param name="action">The action to run for each item.</param>
    ///<param name="options">Options for the terminal stage. Defaults to <see cref="ExecutionDataflowBlockOptions"/> if not specified.</param>
    ///<returns>The completed pipeline: post items into it, then complete and await it.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public DataflowPipeline<TInput> ActionBlock(Action<TCurrent> action, ExecutionDataflowBlockOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        ActionBlock<TCurrent> block = new(action, options ?? new ExecutionDataflowBlockOptions());
        _tail.LinkTo(block, new DataflowLinkOptions { PropagateCompletion = true });

        return new DataflowPipeline<TInput>(_head, block.Completion);
    }

        ///<summary>
///Appends a <see cref="TransformBlock{TInput,TOutput}"/> stage, linked to receive everything from the current tail.
///</summary>
    ///<typeparam name="TNext">The new stage's output type.</typeparam>
    ///<param name="transform">The transform to apply to each item.</param>
    ///<param name="options">Options for the new stage. Defaults to <see cref="ExecutionDataflowBlockOptions"/> if not specified.</param>
    ///<returns>A builder for continuing the chain from the new stage's output type.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="transform"/> is <c>null</c>.</exception>
    public DataflowPipelineBuilder<TInput, TNext> Transform<TNext>(Func<TCurrent, TNext> transform, ExecutionDataflowBlockOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(transform);

        TransformBlock<TCurrent, TNext> block = new(transform, options ?? new ExecutionDataflowBlockOptions());
        _tail.LinkTo(block, new DataflowLinkOptions { PropagateCompletion = true });

        return new DataflowPipelineBuilder<TInput, TNext>(_head, block);
    }
    #endregion
}
